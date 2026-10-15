using DTO.Posts;
using System.Collections.Generic;

namespace BLL.IServices
{
    public interface IPostService
    {
        IEnumerable<PostDto> GetAllPosts();
        PostDto? GetPostById(int id);
        PostDto CreatePost(CreatePostDto model);
        void UpdatePost(int id, UpdatePostDto model);
        void DeletePost(int id);
    }
}