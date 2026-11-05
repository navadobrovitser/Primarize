using BLL.IServices;
using DAL.IRepository;
using DAL.Entities;
using DTO.Voting;
using AutoMapper;
using BLL.Exceptions;
using System.Collections.Generic;

namespace BLL.Services
{
    public class AnonymousVoteService : IAnonymousVoteService
    {
        private readonly IAnonymousVoteRepository _anonymousVoteRepository;
        private readonly IMapper _mapper;

        public AnonymousVoteService(IAnonymousVoteRepository anonymousVoteRepository, IMapper mapper)
        {
            _anonymousVoteRepository = anonymousVoteRepository;
            _mapper = mapper;
        }

        // שליפת כל ההצבעות האנונימיות והמרתן ל-DTO
        public IEnumerable<AnonymousVoteDto> GetAllAnonymousVotes()
        {
            var votes = _anonymousVoteRepository.GetAll();
            return _mapper.Map<IEnumerable<AnonymousVoteDto>>(votes);
        }

        // שליפת הצבעה אנונימית לפי מזהה
        public AnonymousVoteDto GetById(int id)
        {
            var vote = _anonymousVoteRepository.GetById(id); 
            if (vote == null)
            {
                throw new NotFoundException($"Anonymous vote with id {id} was not found");
            }
            return _mapper.Map<AnonymousVoteDto>(vote);
        }

        // הוספת הצבעה אנונימית חדשה מתוך ה-DTO
        public void AddAnonymousVote(CreateAnonymousVoteDto model)
        {
            var vote = _mapper.Map<AnonymousVote>(model);
            _anonymousVoteRepository.Add(vote); 
        }
    }
}