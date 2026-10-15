using AutoMapper;
using BLL.IServices;
using DAL.Entities;
using DAL.IRepository;
using DTO.Candidates;
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

        public CandidateDto? GetCandidateById(string id)
        {
            var candidate = _candidateRepository.GetByUserIdentityNumber(id);
            if (candidate == null) return null;

            return _mapper.Map<CandidateDto>(candidate);
        }

        public CandidateDto CreateCandidate(CreateCandidateDto model)
        {
            // חיפוש המשתמש לפי תעודת הזהות (Id) שנשלחה ב-DTO
            var user = _userRepository.GetByIdNumber(model.Id);
            if (user == null)
            {
                throw new ArgumentException("המשתמש עם מספר תעודת הזהות הזה אינו קיים במערכת, ולכן לא ניתן להגדירו כמועמד.");
            }

            // בדיקה חדשה: האם כבר קיים מועמד רשום במערכת עבור משתמש זה
            var existingCandidate = _candidateRepository.GetByUserId(user.UserId);
            if (existingCandidate != null)
            {
                throw new ArgumentException("כבר קיים מועמד רשום במערכת עבור תעודת זהות זו.");
            }

            var candidate = _mapper.Map<Candidate>(model);

            // שיוך ה-UserId המספרי שנמצא אל המועמד
            candidate.UserId = user.UserId;

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                candidate.ProfileImageUrl = _imageService.Save(model.ImageFile);
            }

            if (model.VideoFile != null && model.VideoFile.Length > 0)
            {
                candidate.ExplanationVideoUrl = _imageService.SaveVideo(model.VideoFile);
            }

            var createdCandidate = _candidateRepository.Add(candidate);
            return _mapper.Map<CandidateDto>(createdCandidate);
        }

        public void UpdateCandidate(string id, UpdateCandidateDto model)
        {
            var candidate = _candidateRepository.GetByUserIdentityNumber(id);
            if (candidate == null)
            {
                throw new KeyNotFoundException("המועמד לא נמצא.");
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
        public void DeleteCandidate(string id)
        {
            var candidate = _candidateRepository.GetByUserIdentityNumber(id);
            if (candidate == null)
            {
                throw new KeyNotFoundException("המועמד לא נמצא.");
            }
            // מחיקה לפי מזהה המועמד (CandidateId) של הרשומה שנמצאה
            _candidateRepository.Delete(candidate.CandidateId);
        }
    }
}