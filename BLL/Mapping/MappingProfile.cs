using AutoMapper;
using DAL.Entities;
using DTO.Candidates;
using DTO.Comment;
using DTO.Likes;
using DTO.Posts;
using DTO.Users;
using DTO.Voting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BLL.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User
            CreateMap<User, UserDto>();
            CreateMap<User, UserSummaryDto>();

            CreateMap<RegisterUserDto, User>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.Ignore()) // התעלמות כדי שישתמש בערך ברירת המחדל "User"
                .ForMember(dest => dest.PartyJoinDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.PostLikes, opt => opt.Ignore())
                .ForMember(dest => dest.Comments, opt => opt.Ignore());

            // Candidate
            CreateMap<Candidate, CandidateDto>()
                .ForMember(dest => dest.Name,
                    opt => opt.MapFrom(src => src.User.Name))
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => src.User.Id));

            CreateMap<Candidate, CandidateListItemDto>()
                .ForMember(dest => dest.Name,
                    opt => opt.MapFrom(src => src.User.Name))
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => src.User.Id));

            CreateMap<CreateCandidateDto, Candidate>()
                .ForMember(dest => dest.CandidateId, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Posts, opt => opt.Ignore())
                .ForMember(dest => dest.ProfileImageUrl, opt => opt.Ignore())
                .ForMember(dest => dest.ExplanationVideoUrl, opt => opt.Ignore());

            // Post
            CreateMap<Post, PostDto>()
                .ForMember(dest => dest.ContentType,
                    opt => opt.MapFrom(src => src.ContentType.ToString()))
                .ForMember(dest => dest.LikesCount,
                    opt => opt.MapFrom(src =>
                        src.PostLikes.Count(l => l.Type == LikeType.Like)))
                .ForMember(dest => dest.DislikesCount,
                    opt => opt.MapFrom(src =>
                        src.PostLikes.Count(l => l.Type == LikeType.Dislike)))
                .ForMember(dest => dest.CommentsCount,
                    opt => opt.MapFrom(src => src.Comments.Count));

            CreateMap<CreatePostDto, Post>()
                .ForMember(dest => dest.PostId, opt => opt.Ignore())
                .ForMember(dest => dest.Candidate, opt => opt.Ignore())
                .ForMember(dest => dest.PostLikes, opt => opt.Ignore())
                .ForMember(dest => dest.Comments, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.ContentType, opt => opt.MapFrom(src => Enum.Parse<PostContentType>(src.ContentType, true)));

            CreateMap<UpdateCandidateDto, Candidate>()
                .ForMember(dest => dest.CandidateId, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Posts, opt => opt.Ignore())
                .ForMember(dest => dest.ProfileImageUrl, opt => opt.Ignore())
                .ForMember(dest => dest.ExplanationVideoUrl, opt => opt.Ignore());

            CreateMap<UpdatePostDto, Post>()
                .ForMember(dest => dest.PostId, opt => opt.Ignore())
                .ForMember(dest => dest.CandidateId, opt => opt.Ignore())
                .ForMember(dest => dest.Candidate, opt => opt.Ignore())
                .ForMember(dest => dest.PostLikes, opt => opt.Ignore())
                .ForMember(dest => dest.Comments, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ContentType,
                    opt => opt.MapFrom(src => Enum.Parse<PostContentType>(src.ContentType, true)));

            // Like
            CreateMap<SetPostLikeDto, PostLike>()
                .ForMember(dest => dest.PostLikeId, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.Post, opt => opt.Ignore())
                .ForMember(dest => dest.Type,
                    opt => opt.MapFrom(src =>
                        Enum.Parse<LikeType>(src.Type, true)));

            CreateMap<PostLike, PostLikeDto>()
                .ForMember(dest => dest.Type,
                    opt => opt.MapFrom(src => src.Type.ToString()));

            // Comment
            CreateMap<CreateCommentDto, Comment>()
                .ForMember(dest => dest.CommentId, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Post, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<UpdateCommentDto, Comment>()
                .ForMember(dest => dest.CommentId, opt => opt.Ignore())
                .ForMember(dest => dest.PostId, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Post, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

            CreateMap<Comment, CommentDto>()
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => src.User != null ? src.User.Name : string.Empty));

            // AnonymousVote
            // AnonymousVote
            CreateMap<AnonymousVote, AnonymousVoteDto>();

            CreateMap<CreateAnonymousVoteDto, AnonymousVote>()
                .ForMember(dest => dest.AnonymousVoteId, opt => opt.Ignore())
                .ForMember(dest => dest.Candidate, opt => opt.Ignore());
        }
    }
}