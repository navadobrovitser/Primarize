using AutoMapper;
using DAL.Entities;
using DAL.IRepository;
using Microsoft.AspNetCore.Mvc;
using DTO.Comment;
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

        _commentRepository.Delete(id);

        return Ok(new { message = "התגובה נמחקה בהצלחה!" });
    }

    [HttpPut("{id}")]
    public IActionResult UpdateComment(int id, [FromBody] UpdateCommentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // שליפת התגובה הקיימת ממסד הנתונים
        var existingComment = _commentRepository.GetById(id);
        if (existingComment == null)
        {
            return NotFound(new { message = "התגובה המבוקשת לא נמצאה." });
        }

        // עדכון הנתונים באמצעות ה-Mapping שהוגדר או עדכון ישיר של השדה
        _mapper.Map(dto, existingComment);

        // לחלופין, אם מעדיפים עדכון ידני ישיר:
        // existingComment.Content = dto.Content;

        _commentRepository.Update(existingComment);

        return Ok(new { message = "התגובה עודכנה בהצלחה!", data = existingComment });
    }
}