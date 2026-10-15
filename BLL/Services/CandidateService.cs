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

        public CandidateDto? GetCandidateById(int id)
        {
            var candidate = _candidateRepository.GetById(id);
            if (candidate == null) return null;

            return _mapper.Map<CandidateDto>(candidate);
        }

        public CandidateDto CreateCandidate(CreateCandidateDto model, int userId)
        {
            // חוק עסקי: כל מועמד חייב להיות משתמש קיים במערכת
            var user = _userRepository.GetById(userId);
            if (user == null)
            {
                throw new ArgumentException("המשתמש המשויך למועמד אינו קיים במערכת.");
            }

            var candidate = _mapper.Map<Candidate>(model);
            candidate.UserId = userId;

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

        public void UpdateCandidate(int id, CreateCandidateDto model)
        {
            var candidate = _candidateRepository.GetById(id);
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

        public void DeleteCandidate(int id)
        {
            var candidate = _candidateRepository.GetById(id);
            if (candidate == null)
            {
                throw new KeyNotFoundException("המועמד לא נמצא.");
            }
            _candidateRepository.Delete(id);
        }
    }
}