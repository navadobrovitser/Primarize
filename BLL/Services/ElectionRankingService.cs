
using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BLL.Services
{
    public class ElectionRankingService
    {
        public List<int> CalculateRanking(
            List<Candidate> candidates,
            List<AnonymousVote> votes,
            string? tieBreakSeed = null)
        {
            if (candidates == null)
                throw new ArgumentNullException(nameof(candidates));

            if (votes == null)
                throw new ArgumentNullException(nameof(votes));

            // אם לא סופק מפתח, נוצר מפתח חדש להרצה
            tieBreakSeed ??= Guid.NewGuid().ToString("N");

            var candidateIds = candidates
                .Select(c => c.CandidateId)
                .ToList();

            if (candidateIds.Count != candidateIds.Distinct().Count())
                throw new ArgumentException("קיימים מזהי מועמדים כפולים.");

            var validCandidateIds = candidateIds.ToHashSet();

            // בדיקת תקינות פתקי ההצבעה
            foreach (var vote in votes)
            {
                if (!validCandidateIds.Contains(vote.CandidateId))
                    throw new ArgumentException("נמצא דירוג למועמד שאינו קיים.");

                if (vote.Priority < 1 || vote.Priority > 5)
                    throw new ArgumentException("נמצאה עדיפות שאינה בטווח 1 עד 5.");

                if (string.IsNullOrWhiteSpace(vote.UserRandomVoteId))
                    throw new ArgumentException("נמצא פתק ללא מזהה.");
            }

            var ballots = votes
                .GroupBy(v => v.UserRandomVoteId)
                .OrderBy(g => g.Key, StringComparer.Ordinal)
                .Select(g =>
                {
                    var orderedVotes = g
                        .OrderBy(v => v.Priority)
                        .ToList();

                    if (orderedVotes.Select(v => v.Priority).Distinct().Count()
                        != orderedVotes.Count)
                        throw new ArgumentException("נמצאו עדיפויות כפולות בפתק.");

                    if (orderedVotes.Select(v => v.CandidateId).Distinct().Count()
                        != orderedVotes.Count)
                        throw new ArgumentException("נמצא מועמד כפול בפתק.");

                    for (int i = 0; i < orderedVotes.Count; i++)
                    {
                        if (orderedVotes[i].Priority != i + 1)
                            throw new ArgumentException("סדר העדיפויות בפתק אינו רציף.");
                    }

                    return orderedVotes
                        .Select(v => v.CandidateId)
                        .ToList();
                })
                .ToList();

            var ranking = new List<int>();

            var remaining = candidateIds
                .Where(id => votes.Any(v => v.CandidateId == id))
                .OrderBy(id => id)
                .ToList();

            while (remaining.Count > 0)
            {
                int position = ranking.Count + 1;

                int winner = FindWinner(
                    remaining,
                    ballots,
                    votes,
                    tieBreakSeed,
                    position);

                ranking.Add(winner);
                remaining.Remove(winner);
            }

            // מועמדים שלא הופיעו באף פתק
            var withoutVotes = candidateIds
                .Except(ranking)
                .OrderBy(id => id)
                .ToList();

            withoutVotes = withoutVotes
                .OrderBy(id => TieBreakValue(
                    tieBreakSeed,
                    "unranked",
                    id.ToString()))
                .ThenBy(id => id)
                .ToList();

            ranking.AddRange(withoutVotes);

            return ranking;
        }

        private int FindWinner(
            List<int> remainingCandidates,
            List<List<int>> ballots,
            List<AnonymousVote> originalVotes,
            string seed,
            int position)
        {
            var active = remainingCandidates.ToHashSet();

            var previousRounds = new List<Dictionary<int, int>>();

            int round = 0;

            while (active.Count > 1)
            {
                round++;

                var counts = active.ToDictionary(id => id, id => 0);

                int activeBallots = 0;

                foreach (var ballot in ballots)
                {
                    int selected = ballot.FirstOrDefault(
                        id => active.Contains(id));

                    if (selected != 0)
                    {
                        counts[selected]++;
                        activeBallots++;
                    }
                }

                // אין יותר פתקים פעילים
                if (activeBallots == 0)
                {
                    return ResolveByOriginalPriorities(
                        active,
                        originalVotes,
                        seed,
                        position);
                }

                // רוב מוחלט מתוך הפתקים הפעילים
                foreach (var item in counts.OrderBy(x => x.Key))
                {
                    if (item.Value * 2 > activeBallots)
                        return item.Key;
                }

                int minimum = counts.Values.Min();

                var tied = counts
                    .Where(x => x.Value == minimum)
                    .Select(x => x.Key)
                    .OrderBy(id => id)
                    .ToList();

                int eliminated;

                if (tied.Count == 1)
                {
                    eliminated = tied[0];
                }
                else
                {
                    eliminated = ResolveEliminationTie(
                        tied,
                        previousRounds,
                        seed,
                        position,
                        round);
                }

                previousRounds.Add(
                    new Dictionary<int, int>(counts));

                active.Remove(eliminated);
            }

            return active.Single();
        }

        private int ResolveEliminationTie(
            List<int> tiedCandidates,
            List<Dictionary<int, int>> previousRounds,
            string seed,
            int position,
            int round)
        {
            var tied = tiedCandidates.ToList();

            for (int i = previousRounds.Count - 1; i >= 0; i--)
            {
                int minimum = tied.Min(id =>
                    previousRounds[i].GetValueOrDefault(id));

                tied = tied
                    .Where(id =>
                        previousRounds[i].GetValueOrDefault(id) == minimum)
                    .ToList();

                if (tied.Count == 1)
                    return tied[0];
            }

            // הגרלה ניתנת לשחזור באמצעות מפתח קבוע
            return tied
                .OrderBy(id => TieBreakValue(
                    seed,
                    $"elimination-{position}-{round}",
                    id.ToString()))
                .ThenBy(id => id)
                .First();
        }

        private int ResolveByOriginalPriorities(
            HashSet<int> candidates,
            List<AnonymousVote> votes,
            string seed,
            int position)
        {
            var tied = candidates.OrderBy(id => id).ToList();

            for (int priority = 1; priority <= 5; priority++)
            {
                int currentPriority = priority;

                int maximum = tied.Max(id =>
                    votes.Count(v =>
                        v.CandidateId == id &&
                        v.Priority == currentPriority));

                tied = tied
                    .Where(id =>
                        votes.Count(v =>
                            v.CandidateId == id &&
                            v.Priority == currentPriority) == maximum)
                    .ToList();

                if (tied.Count == 1)
                    return tied[0];
            }

            return tied
                .OrderBy(id => TieBreakValue(
                    seed,
                    $"fallback-{position}",
                    id.ToString()))
                .ThenBy(id => id)
                .First();
        }

        private string TieBreakValue(
            string seed,
            string context,
            string candidateId)
        {
            string input = $"{seed}|{context}|{candidateId}";

            byte[] bytes = SHA256.HashData(
                Encoding.UTF8.GetBytes(input));

            return Convert.ToHexString(bytes);
        }
    }
}
