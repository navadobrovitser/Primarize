using API.Extensions;
using AutoMapper;
using DAL.Entities;
using DAL.IRepository;
using DTO.Comment;
using Microsoft.AspNetCore.Mvc;
[Route("api/[controller]")]
[ApiController]
public class CommentsController : ControllerBase
{
    private readonly ICommentRepository _commentRepository;
    private readonly IMapper _mapper;

    public CommentsController(ICommentRepository commentRepository, IMapper mapper)
    {
        _commentRepository = commentRepository;
        _mapper = mapper;
    }

    // הוספת תגובה חדשה (מקשרת ל-PostId ולמשתמש)
    [HttpPost]
    public IActionResult CreateComment([FromBody] CreateCommentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // מיפוי ה-DTO ישירות לישות ה-Comment
        var comment = _mapper.Map<Comment>(dto);

        // שיוך מזהה המשתמש (לצורכי בדיקה כרגע ניתן להגדיר ערך קבוע, למשל 1)
        comment.UserId = 1;
        comment.CreatedAt = DateTime.UtcNow;

        _commentRepository.Add(comment);

        return Ok(new { message = "התגובה נוספה בהצלחה!", data = comment });
    }

    // עדכון תגובה קיימת לפי מזהה
    [HttpPut("{id}")]
    public IActionResult UpdateComment(int id, [FromBody] UpdateCommentDto dto)
    {
        var existingComment = _commentRepository.GetById(id);
        if (existingComment == null)
        {
            return NotFound(new { message = "התגובה המבוקשת לא נמצאה." });
        }
        int currentUserId = User.GetUserId();

        // בדיקת בעלות: האם המשתמש המחובר הוא כותב התגובה?
        if (existingComment.UserId != currentUserId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "אינך מורשה לערוך תגובה זו." });
        }

        // עדכון תוכן התגובה בלבד (או שדות רלוונטיים נוספים)
        existingComment.Content = dto.Content;

        _commentRepository.Update(existingComment);

        return Ok(new { message = "התגובה עודכנה בהצלחה!" });
    }

    // מחיקת תגובה לפי מזהה
    [HttpDelete("{id}")]
    public IActionResult DeleteComment(int id)
    {
        var existingComment = _commentRepository.GetById(id);
        if (existingComment == null)
        {
            return NotFound(new { message = "התגובה המבוקשת לא נמצאה." });
        }
        int currentUserId = User.GetUserId();

        // מותר למחוק רק אם המשתמש הוא כותב התגובה OR מנהל
        bool isAdmin = User.IsInRole("Admin");
        if (existingComment.UserId != currentUserId && !isAdmin)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "אינך מורשה למחוק תגובה זו." });
        }
        _commentRepository.Delete(id);

        return Ok(new { message = "התגובה נמחקה בהצלחה!" });
    }

    // קבלת רשימת כל התגובות במערכת----מי אמר שצריך, נא לבדוק את זה
    [HttpGet]
    public IActionResult GetAllComments()
    {
        var comments = _commentRepository.GetAll();
        var commentDtos = _mapper.Map<List<CommentDto>>(comments);
        return Ok(commentDtos);
    }

    [HttpGet("{id}")]
    public IActionResult GetCommentById(int id)
    {
        var comment = _commentRepository.GetById(id);
        if (comment == null)
        {
            return NotFound(new { message = "התגובה המבוקשת לא נמצאה." });
        }

        var commentDto = _mapper.Map<CommentDto>(comment);
        return Ok(commentDto);
    }
}