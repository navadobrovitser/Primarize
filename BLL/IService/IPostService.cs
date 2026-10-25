using DTO.Posts;
using System.Collections.Generic;

namespace BLL.IServices
{
    public interface IPostService
    {
        IEnumerable<PostDto> GetAllPosts();
        PostDto? GetPostById(int id);
        void UpdatePost(int id, UpdatePostDto model, int currentUserId);

        void DeletePost(int id, int currentUserId);
        PostDto CreatePost(CreatePostDto model, int currentUserId);

        IEnumerable<PostDto> GetPostsByCandidateId(int currentUserId);

    }
}