
using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BLL.Services
{//האלגוריתם
    public class ElectionRankingService
    {
        // =====================================================
        // 1. חישוב הדירוג הסופי של המועמדים
        //
        // האלגוריתם מבוסס על Instant Runoff Voting (IRV).
        // בכל שלב נבחר המועמד המוביל מבין המועמדים שנותרו.
        // =====================================================

        public List<int> CalculateRanking(
            List<Candidate> candidates,
            List<AnonymousVote> votes,
            string? tieBreakSeed = null)
        {
            if (candidates == null)
                throw new ArgumentNullException(nameof(candidates));

            if (votes == null)
                throw new ArgumentNullException(nameof(votes));

            // מפתח המשמש להכרעה עקבית במקרי שוויון אם הוא לא התקבל בפונקציה
            tieBreakSeed ??= Guid.NewGuid().ToString("N");

            var candidateIds = candidates
                .Select(c => c.CandidateId)
                .ToList();

            if (candidateIds.Count != candidateIds.Distinct().Count())
                throw new ArgumentException("קיימים מזהי מועמדים כפולים.");

            var validCandidateIds = candidateIds.ToHashSet();

            // =====================================================
            // 2. אימות תקינות נתוני ההצבעה
            //
            // בדיקת קיום מועמדים, טווח העדיפויות
            // ותקינות המזהה האנונימי של כל פתק.
            // =====================================================

            foreach (var vote in votes)
            {
                if (!validCandidateIds.Contains(vote.CandidateId))
                    throw new ArgumentException("נמצא דירוג למועמד שאינו קיים.");

                if (vote.Priority < 1 || vote.Priority > 5)
                    throw new ArgumentException("נמצאה עדיפות שאינה בטווח 1 עד 5.");

                if (string.IsNullOrWhiteSpace(vote.UserRandomVoteId))
                    throw new ArgumentException("נמצא פתק ללא מזהה.");
            }

            // =====================================================
            // 3. בניית פתקי הצבעה מדורגים
            //
            // קיבוץ הרשומות לפי מזהה פתק אנונימי.
            // כל פתק מיוצג כרשימת מועמדים לפי סדר העדיפות.
            // =====================================================

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

            // =====================================================
            // 4. בניית הדירוג הסופי
            //
            // בחירה חוזרת של המועמד המוביל והסרתו
            // מרשימת המועמדים לצורך קביעת המקום הבא.
            // =====================================================

            //הרשימה הסופית שתחזור בסוף
            var ranking = new List<int>();
            // רשימת המועמדים שעדין  אפשר לדרג  כלומר שמשהו בחר בהם
            var remaining = candidateIds
                .Where(id => votes.Any(v => v.CandidateId == id))
                .OrderBy(id => id)
                .ToList();
///זה האופן הלולאתי של IRV
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

            // מועמדים שלא דורגו באף פתק משובצים בסוף הרשימה
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

        // =====================================================
        // 5. מציאת המועמד המוביל - IRV
        //
        // בכל סבב נספרת ההעדפה הגבוהה ביותר שעדיין פעילה.
        // מועמד בעל רוב מוחלט נבחר.
        // אחרת, המועמד בעל מספר הקולות הנמוך ביותר מודח.
        // =====================================================
        
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

                // טיפול במקרה שלא נותרו פתקים פעילים
                if (activeBallots == 0)
                {
                    return ResolveByOriginalPriorities(
                        active,
                        originalVotes,
                        seed,
                        position);
                }

                // בחירת מועמד שקיבל יותר ממחצית הקולות הפעילים
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

        // =====================================================
        // 6. הכרעה בשוויון בין מועמדים להדחה
        //
        // תחילה נבדקות תוצאות הסבבים הקודמים.
        // אם השוויון נמשך, ההכרעה מתבצעת באמצעות SHA256
        // ומפתח קבוע המאפשר לשחזר את התוצאה.
        // =====================================================

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

            // הכרעה פסאודו-אקראית הניתנת לשחזור
            return tied
                .OrderBy(id => TieBreakValue(
                    seed,
                    $"elimination-{position}-{round}",
                    id.ToString()))
                .ThenBy(id => id)
                .First();
        }

        // =====================================================
        // 7. הכרעה לפי העדיפויות המקוריות
        //
        // כאשר אין פתקים פעילים, נבדקת כמות הדירוגים
        // שקיבל כל מועמד בכל רמת עדיפות, מהגבוהה לנמוכה.
        // =====================================================

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

        // =====================================================
        // 8. יצירת ערך להכרעה בשוויון
        //
        // שימוש ב-SHA256 ליצירת ערך דטרמיניסטי
        // המבוסס על המפתח, ההקשר ומזהה המועמד.
        // =====================================================

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
