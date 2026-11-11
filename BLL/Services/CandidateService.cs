using AutoMapper;
using BLL.IServices;
using DAL.Entities;
using DAL.IRepository;
using DTO.Candidates;
using BLL.Exceptions;
using System;
using System.Collections.Generic;

namespace BLL.Services
{
    public class CandidateService : ICandidateService
    {
        private readonly ICandidateRepository _candidateRepository;
        private readonly IUserRepository _userRepository;
        private readonly IImageService _imageService;
        private readonly IMapper _mapper;

        public CandidateService(
            ICandidateRepository candidateRepository,
            IUserRepository userRepository,
            IImageService imageService,
            IMapper mapper)
        {
            _candidateRepository = candidateRepository;
            _userRepository = userRepository;
            _imageService = imageService;
            _mapper = mapper;
        }

        public IEnumerable<CandidateListItemDto> GetAllCandidates()
        {
            var candidates = _candidateRepository.GetAll();
            return _mapper.Map<IEnumerable<CandidateListItemDto>>(candidates);
        }

        public CandidateDto GetCandidateById(string id)
        {
            var candidate = _candidateRepository.GetByUserIdentityNumber(id);

            if (candidate == null)
            {
                throw new NotFoundException(
                    $"Candidate with identity number {id} was not found"
                );
            }

            return _mapper.Map<CandidateDto>(candidate);
        }
        public CandidateEligibilityDto CheckEligibility(int userId)
        {
            var user = _userRepository.GetById(userId);

            if (user == null)
                throw new NotFoundException("המשתמש לא נמצא");

            bool alreadyCandidate =
                _candidateRepository.GetByUserId(userId) != null;

            bool hasJoinDate = user.PartyJoinDate != default;

            DateTime? eligibleFrom = hasJoinDate
                ? user.PartyJoinDate.Date.AddYears(4)
                : null;
            // זמני לצורך בדיקות בלבד
            bool eligible = !alreadyCandidate;
            //bool eligible =
            //    eligibleFrom.HasValue &&
            //    DateTime.UtcNow.Date >= eligibleFrom.Value &&
            //    !alreadyCandidate;

            return new CandidateEligibilityDto
            {
                Eligible = eligible,
                PartyJoinDate = hasJoinDate
                    ? user.PartyJoinDate
                    : null,
                EligibleFrom = eligibleFrom,
                AlreadyCandidate = alreadyCandidate
            };
        }

        public CandidateDto CreateCandidate( int currentUserId,
      CreateCandidateDto model)
     
        {
            var user = _userRepository.GetById(currentUserId);

            if (user == null)
                throw new NotFoundException("המשתמש לא נמצא");

            // בדיקת ותק של ארבע שנים
            //if (user.PartyJoinDate == default ||
            //    user.PartyJoinDate.Date.AddYears(4) > DateTime.UtcNow.Date)
            //{
            //    throw new BusinessRuleException(
            //        "נדרשות לפחות ארבע שנות ותק להגשת מועמדות"
            //    );
            //}

            // בדיקה שהמשתמש עדיין לא מועמד
            var existingCandidate =
                _candidateRepository.GetByUserId(currentUserId);

            if (existingCandidate != null)
                throw new ConflictException("כבר הגשת מועמדות");

            var candidate = _mapper.Map<Candidate>(model);

            // שיוך המועמד למשתמש המחובר
            candidate.UserId = user.UserId;

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                candidate.ProfileImageUrl =
                    _imageService.Save(model.ImageFile);
            }

            if (model.VideoFile != null && model.VideoFile.Length > 0)
            {
                candidate.ExplanationVideoUrl =
                    _imageService.SaveVideo(model.VideoFile);
            }

            var createdCandidate = _candidateRepository.Add(candidate);

            return _mapper.Map<CandidateDto>(createdCandidate);
        }
        public void UpdateCandidate(int currentUserId, UpdateCandidateDto model)
        {
            // חיפוש המועמד ששייך למשתמש המחובר מהטוקן
            var candidate = _candidateRepository.GetByUserId(currentUserId);
            if (candidate == null)
            {
                throw new NotFoundException("המועמד לא נמצא.");
            }

            _mapper.Map(model, candidate);

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                candidate.ProfileImageUrl = _imageService.Save(model.ImageFile);
            }

            if (model.VideoFile != null && model.VideoFile.Length > 0)
            {
                candidate.ExplanationVideoUrl = _imageService.SaveVideo(model.VideoFile);
            }

            _candidateRepository.Update(candidate);
        }

        public IEnumerable<CandidateDto> GetCandidatesForMatch()
        {
            var candidates = _candidateRepository.GetAll();

            return _mapper.Map<IEnumerable<CandidateDto>>(candidates);
        }

        public void DeleteCandidate(int currentUserId)
        {
            var candidate = _candidateRepository.GetByUserId(currentUserId);
            if (candidate == null)
            {
                throw new NotFoundException("המועמד לא נמצא.");
            }

            _candidateRepository.Delete(candidate.CandidateId);
        }
    }
}