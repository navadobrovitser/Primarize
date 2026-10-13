using DAL.Entities;
using DTO.Candidates;
using DTO.Likes;
using DTO.Posts;
using DTO.Users;
using DTO.Voting;
using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using System.Linq;
namespace BLL.Mapping
{

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
       

            // ==========================================
            // מיפויי משתמש (User)
            // ==========================================

            // מיפוי פשוט מטבלת משתמש במסד הנתונים אל אובייקט ה-DTO שחוזר ללקוח
            CreateMap<User, UserDto>();

            // מיפוי מטבלת משתמש אל גרסת תקציר/רשימה של פרטי המשתמש
            CreateMap<User, UserSummaryDto>();

            // מיפוי מנתוני הרשמה שמגיעים מהלקוח אל ישות המשתמש במסד הנתונים
            CreateMap<RegisterUserDto, User>()
                // מתעלמים ממזהה המשתמש כי מסד הנתונים יוצר אותו לבד באופן אוטומטי
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                // קובעים שתאריך ההצטרפות למערכת יהיה בדיוק הזמן הנוכחי ב-UTC
                .ForMember(dest => dest.PartyJoinDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                // מתעלמים מרשימת הלייקים כי משתמש חדש מתחיל בלי לייקים
                .ForMember(dest => dest.PostLikes, opt => opt.Ignore())
                // מתעלמים מרשימת התגובות כי משתמש חדש מתחיל בלי תגובות
                .ForMember(dest => dest.Comments, opt => opt.Ignore());

            // ==========================================
            // מיפויי מועמד (Candidate)
            // ==========================================

            // מיפוי פרטי מועמד מלאים, כולל שליפת השם שלו מתוך טבלת המשתמש המקושר אליו
            CreateMap<Candidate, CandidateDto>()
                .ForMember(dest => dest.Name,
                    opt => opt.MapFrom(src => src.User.Name));

            // מיפוי מועמד לגרסת רשימה, כולל שליפת השם מתוך טבלת המשתמש המקושר
            CreateMap<Candidate, CandidateListItemDto>()
                .ForMember(dest => dest.Name,
                    opt => opt.MapFrom(src => src.User.Name));

            // מיפוי נתוני יצירת מועמד חדש אל ישות המסד
            CreateMap<CreateCandidateDto, Candidate>()
                // מתעלמים ממזהה המועמד שנוצר אוטומטית בדאטה-בייס
                .ForMember(dest => dest.CandidateId, opt => opt.Ignore())
                // מתעלמים ממזהה המשתמש שיקושר בהמשך על ידי הקוד
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                // מתעלמים מאובייקט המשתמש עצמו בשלב זה
                .ForMember(dest => dest.User, opt => opt.Ignore())
                // מתעלמים מרשימת הפוסטים כי למועמד חדש אין עדיין פוסטים
                .ForMember(dest => dest.Posts, opt => opt.Ignore());

            // ==========================================
            // מיפויי פוסט (Post)
            // ==========================================

            // מיפוי פוסט מהמסד אל ה-DTO שמוצג במסך ללקוח
            CreateMap<Post, PostDto>()
                // ממירים את סוג התוכן (Enum) למחרוזת טקסט רגילה
                .ForMember(dest => dest.ContentType,
                    opt => opt.MapFrom(src => src.ContentType.ToString()))
                // סופרים אוטומטית רק את הלייקים שסוגם הוא "Like"
                .ForMember(dest => dest.LikesCount,
                    opt => opt.MapFrom(src =>
                        src.PostLikes.Count(l => l.Type == LikeType.Like)))
                // סופרים אוטומטית רק את הדיסלייקים שסוגם הוא "Dislike"
                .ForMember(dest => dest.DislikesCount,
                    opt => opt.MapFrom(src =>
                        src.PostLikes.Count(l => l.Type == LikeType.Dislike)))
                // סופרים את סך כל התגובות שיש לפוסט הזה
                .ForMember(dest => dest.CommentsCount,
                    opt => opt.MapFrom(src => src.Comments.Count));

            // מיפוי יצירת פוסט חדש
            CreateMap<CreatePostDto, Post>()
                // מתעלמים ממזהה הפוסט שנוצר אוטומטית
                .ForMember(dest => dest.PostId, opt => opt.Ignore())
                // מתעלמים ממזהה המועמד והקשרים שיוגדרו בקוד
                .ForMember(dest => dest.CandidateId, opt => opt.Ignore())
                .ForMember(dest => dest.Candidate, opt => opt.Ignore())
                .ForMember(dest => dest.PostLikes, opt => opt.Ignore())
                .ForMember(dest => dest.Comments, opt => opt.Ignore())
                // קובעים אוטומטית את תאריך יצירת הפוסט לזמן הנוכחי
                .ForMember(dest => dest.CreatedAt,
                    opt => opt.MapFrom(src => DateTime.UtcNow));

            // מיפוי עדכון פוסט קיים
            CreateMap<UpdatePostDto, Post>()
                // מתעלמים ממזהים ומקשרים כדי שלא יידרסו בטעות בעדכון
                .ForMember(dest => dest.PostId, opt => opt.Ignore())
                .ForMember(dest => dest.CandidateId, opt => opt.Ignore())
                .ForMember(dest => dest.Candidate, opt => opt.Ignore())
                .ForMember(dest => dest.PostLikes, opt => opt.Ignore())
                .ForMember(dest => dest.Comments, opt => opt.Ignore())
                // מתעלמים מתאריך היצירה כדי שהוא יישאר המקורי ולא יתאפס
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

            // ==========================================
            // מיפוי לייקים (Like)
            // ==========================================

            // מיפוי בקשה למתן לייק או דיסלייק מטעם משתמש
            CreateMap<SetPostLikeDto, PostLike>()
                // מתעלמים ממזהים ומקשרים מערכתיים שישובצו על ידי הקוד
                .ForMember(dest => dest.PostLikeId, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.Post, opt => opt.Ignore())
                // ממירים את מחרוזת סוג הלייק שמגיעה מהלקוח חזרה לטיפוס ה-Enum של המערכת
                .ForMember(dest => dest.Type,
                    opt => opt.MapFrom(src =>
                        Enum.Parse<LikeType>(src.Type, true)));
        }
    }
}
