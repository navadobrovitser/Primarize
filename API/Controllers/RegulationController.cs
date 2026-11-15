using BLL.IServices;
using DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using DTO.RegulationManagement;
namespace API.Controllers
{
    [ApiController]
    [Route("api/regulation")]
    [Authorize]
    public class RegulationController : ControllerBase
    {
        // השירות שאחראי לקרוא את התקנון
        // ולשמור את סעיפיו במסד הנתונים.
        private readonly IRegulationImportService _regulationService;

        // מאפשר לנו למצוא את תיקיית הפרויקט API
        // בלי להשתמש בנתיב קבוע במחשב.
        private readonly IWebHostEnvironment _environment;

        // שירות יצירת הווקטורים עבור סעיפי התקנון.
        private readonly IRegulationIndexService _indexService;


        // מאפשר לבדוק את הרשאות המשתמש
        // לפי הנתונים העדכניים במסד הנתונים.
        private readonly AppDbContext _dbContext;
        // שירות לניהול טיוטות ועריכת סעיפי התקנון.
        private readonly IRegulationManagementService _managementService;
        public RegulationController(
           IRegulationImportService regulationService,
           IRegulationIndexService indexService,
           IWebHostEnvironment environment,
           IRegulationManagementService managementService,

           AppDbContext dbContext)
        {
            _regulationService = regulationService;
            _indexService = indexService;
            _environment = environment;
            _managementService = managementService;
            _dbContext = dbContext;
        }
        // =====================================================
        // ייבוא גרסת תקנון חדשה כטיוטה
        //
        // POST: api/regulation/import-draft?versionNumber=1.0
        //
        // הפעולה מותרת למנהל האתר בלבד.
        // היא אינה מפרסמת את התקנון ואינה יוצרת Embeddings.
        // =====================================================

        [HttpPost("import-draft")]
        public async Task<IActionResult> ImportDraft(
            [FromQuery] string versionNumber,
            CancellationToken cancellationToken)
        {
            // =================================================
            // 1. זיהוי המשתמש המחובר
            // =================================================

            string? userIdValue = User.FindFirst(
                ClaimTypes.NameIdentifier
            )?.Value;

            if (!int.TryParse(userIdValue, out int userId))
            {
                return Unauthorized(new
                {
                    message = "לא ניתן לזהות את המשתמש המחובר."
                });
            }

            // =================================================
            // 2. בדיקת הרשאת מנהל מול מסד הנתונים
            //
            // לא מסתמכים רק על התפקיד שהיה בטוקן
            // בזמן ההתחברות, אלא בודקים את המצב הנוכחי.
            // =================================================

            bool isAdmin = await _dbContext.Users
                .AsNoTracking()
                .AnyAsync(
                    u => u.UserId == userId
                         && u.Role == "Admin"
                         && u.IsApproved
                         && !u.IsDeleted,
                    cancellationToken
                );

            if (!isAdmin)
            {
                return StatusCode(403, new
                {
                    message = "רק מנהל האתר רשאי לנהל את התקנון."
                });
            }

            // =================================================
            // 3. איתור קובץ התקנון בתיקיית API
            //
            // ContentRootPath הוא נתיב השורש של השרת.
            // ממנו מגיעים לתיקייה Regulations ולקובץ.
            // =================================================

            string filePath = Path.Combine(
                _environment.ContentRootPath,
                "Regulations",
                "regulation.md"
            );

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(new
                {
                    message = "קובץ התקנון לא נמצא בשרת."
                });
            }

            // =================================================
            // 4. הפעלת שירות ייבוא התקנון
            //
            // השירות קורא את הקובץ, מחלק אותו לסעיפים
            // ושומר גרסה חדשה במצב Draft.
            // =================================================

            try
            {
                int versionId = await _regulationService
                    .ImportDraftAsync(
                        filePath,
                        versionNumber,
                        cancellationToken
                    );

                // החזרת פרטי הגרסה שנוצרה בהצלחה.
                return Ok(new
                {
                    message = "טיוטת התקנון יובאה בהצלחה.",
                    versionId,
                    versionNumber,
                    status = "Draft"
                });
            }
            catch (InvalidOperationException ex)
            {
                // למשל: מספר גרסה שכבר קיים,
                // או קובץ תקנון שלא זוהו בו סעיפים.
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                // למשל: מספר גרסה ריק או ארוך מדי.
                return BadRequest(new
                {
                    message = ex.Message
                });
            }

        }

        // =====================================================
        // בדיקת קובץ התקנון לפני ייבוא
        //
        // הפעולה קוראת את הקובץ ומחזירה מידע על הסעיפים.
        // היא אינה שומרת נתונים במסד הנתונים.
        // למנהל האתר בלבד.
        // =====================================================

        [HttpGet("preview")]
        public async Task<IActionResult> PreviewRegulation(
            CancellationToken cancellationToken)
        {
            // זיהוי המשתמש המחובר ובדיקת הרשאת מנהל.
            string? userIdValue = User.FindFirst(
                ClaimTypes.NameIdentifier
            )?.Value;

            if (!int.TryParse(userIdValue, out int userId))
            {
                return Unauthorized();
            }

            bool isAdmin = await _dbContext.Users
                .AsNoTracking()
                .AnyAsync(
                    u => u.UserId == userId
                         && u.Role == "Admin"
                         && u.IsApproved
                         && !u.IsDeleted,
                    cancellationToken
                );

            if (!isAdmin)
            {
                return Forbid();
            }

            // איתור הקובץ בתיקיית Regulations של פרויקט API.
            string filePath = Path.Combine(
                _environment.ContentRootPath,
                "Regulations",
                "regulation.md"
            );

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(new
                {
                    message = "קובץ התקנון לא נמצא."
                });
            }

            // קריאת הסעיפים בלי לשמור אותם במסד.
            var sections = await _regulationService
                .ReadSectionsAsync(filePath);

            return Ok(new
            {
                totalSections = sections.Count,

                firstSection = sections.FirstOrDefault()?.SectionNumber,

                lastSection = sections.LastOrDefault()?.SectionNumber,

                sectionNumbers = sections.Select(
                    s => s.SectionNumber
                ).ToList()
            });
        }
        // =====================================================
        // יצירת Embeddings עבור גרסת תקנון
        //
        // POST: api/regulation/generate-embeddings
        //
        // הפעולה זמינה למנהל בלבד.
        // בכל הפעלה נוצרים עד 10 וקטורים.
        // =====================================================

        [HttpPost("generate-embeddings")]
        public async Task<IActionResult> GenerateEmbeddings(
            [FromQuery] int versionId,
            [FromQuery] int batchSize = 5,
            CancellationToken cancellationToken = default)
        {
            // =================================================
            // 1. זיהוי המשתמש המחובר
            // =================================================

            string? userIdValue = User.FindFirst(
                ClaimTypes.NameIdentifier
            )?.Value;

            if (!int.TryParse(userIdValue, out int userId))
            {
                return Unauthorized();
            }

            // =================================================
            // 2. בדיקת הרשאת מנהל מול מסד הנתונים
            // =================================================

            bool isAdmin = await _dbContext.Users
                .AsNoTracking()
                .AnyAsync(
                    u => u.UserId == userId
                         && u.Role == "Admin"
                         && u.IsApproved
                         && !u.IsDeleted,
                    cancellationToken);

            if (!isAdmin)
            {
                return StatusCode(403, new
                {
                    message = "רק מנהל האתר רשאי ליצור Embeddings."
                });
            }

            // =================================================
            // 3. הפעלת שירות יצירת הווקטורים
            // =================================================

            try
            {
                int generatedCount = await _indexService
                    .GenerateEmbeddingsAsync(
                        versionId,
                        batchSize,
                        cancellationToken);

                return Ok(new
                {
                    message = "פעולת יצירת ה-Embeddings הסתיימה.",
                    versionId,
                    generatedCount
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }
        // =====================================================
        // השלמה אוטומטית של Embeddings לגרסת תקנון
        //
        // הפעולה זמינה למנהל בלבד.
        // היא מפעילה את שירות האינדוקס במנות של 5 סעיפים,
        // עד להשלמת כל הסעיפים שעדיין חסרים להם וקטורים.
        //
        // אם התהליך נעצר, אפשר להפעיל אותו שוב:
        // סעיפים שכבר נשמרו לא יעובדו מחדש.
        // =====================================================

        [HttpPost("complete-embeddings")]
        public async Task<IActionResult> CompleteEmbeddings(
            [FromQuery] int versionId,
            CancellationToken cancellationToken)
        {
            // =================================================
            // 1. זיהוי המשתמש המחובר
            // =================================================

            string? userIdValue = User.FindFirst(
                ClaimTypes.NameIdentifier
            )?.Value;

            if (!int.TryParse(userIdValue, out int userId))
            {
                return Unauthorized();
            }

            // =================================================
            // 2. בדיקה שהמשתמש הוא מנהל פעיל
            // =================================================

            bool isAdmin = await _dbContext.Users
                .AsNoTracking()
                .AnyAsync(
                    u => u.UserId == userId
                         && u.Role == "Admin"
                         && u.IsApproved
                         && !u.IsDeleted,
                    cancellationToken);

            if (!isAdmin)
            {
                return StatusCode(403, new
                {
                    message = "רק מנהל האתר רשאי ליצור Embeddings."
                });
            }

            // =================================================
            // 3. בדיקת גרסת התקנון
            // =================================================

            var version = await _dbContext.RegulationVersions
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    v => v.VersionId == versionId,
                    cancellationToken);

            if (version == null)
            {
                return NotFound(new
                {
                    message = "גרסת התקנון לא נמצאה."
                });
            }

            if (version.Status != "Draft")
            {
                return BadRequest(new
                {
                    message = "ניתן ליצור Embeddings רק לטיוטה."
                });
            }

            // =================================================
            // 4. ספירת הווקטורים החסרים
            // =================================================

            int remaining = await _dbContext.RegulationChunks
                .CountAsync(
                    c => c.VersionId == versionId
                         && c.Embedding == null,
                    cancellationToken);

            int generatedTotal = 0;

            // =================================================
            // 5. השלמת הווקטורים במנות
            //
            // בכל סבב מעבדים עד 5 סעיפים.
            // השהיה בין המנות מפחיתה מעט את קצב הבקשות,
            // אך אינה מבטיחה עמידה בכל מכסות Gemini.
            // =================================================

            try
            {
                while (remaining > 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    int generated = await _indexService
                        .GenerateEmbeddingsAsync(
                            versionId,
                            batchSize: 5,
                            cancellationToken: cancellationToken);

                    if (generated == 0)
                    {
                        break;
                    }

                    generatedTotal += generated;

                    remaining = await _dbContext.RegulationChunks
                        .CountAsync(
                            c => c.VersionId == versionId
                                 && c.Embedding == null,
                            cancellationToken);

                    // השהיה קצרה בין המנות.
                    if (remaining > 0)
                    {
                        await Task.Delay(
                            TimeSpan.FromSeconds(3),
                            cancellationToken);
                    }
                }
            }
            catch (HttpRequestException)
            {
                // ייתכן ש-Gemini החזיר שגיאה או שהחיבור נכשל.
                // כל הווקטורים שנוצרו לפני השגיאה כבר נשמרו.
                return StatusCode(503, new
                {
                    message =
                        "היצירה נעצרה בעקבות שגיאה בתקשורת עם Gemini. " +
                        "ניתן להפעיל שוב כדי להמשיך.",
                    generatedThisRun = generatedTotal,
                    remaining
                });
            }

            // =================================================
            // 6. החזרת סיכום ההתקדמות
            // =================================================

            return Ok(new
            {
                message = remaining == 0
                    ? "כל ה-Embeddings נוצרו בהצלחה!"
                    : "התהליך הסתיים, אך נותרו סעיפים לעיבוד.",

                versionId,
                generatedThisRun = generatedTotal,
                remaining,
                completed = remaining == 0
            });
        }
        // =====================================================
        // פרסום גרסת תקנון
        //
        // הפעולה זמינה למנהל בלבד.
        // הפרסום מתבצע רק לאחר שכל סעיפי הגרסה
        // קיבלו Embeddings תקינים.
        //
        // גרסאות קודמות נשמרות במסד לצורכי היסטוריה.
        // =====================================================

        // =====================================================
        // פרסום גרסת תקנון חדשה
        //
        // POST: api/regulation/publish?versionId=2
        //
        // 1. בדיקת הרשאת מנהל.
        // 2. בדיקה שהגרסה היא טיוטה.
        // 3. בדיקה שכל הסעיפים מכילים Embeddings.
        // 4. העברת הגרסה המפורסמת הקודמת לארכיון.
        // 5. פרסום הגרסה החדשה.
        //
        // השינויים מתבצעים באותה טרנזקציה.
        // =====================================================

        [HttpPost("publish")]
        public async Task<IActionResult> PublishRegulation(
            [FromQuery] int versionId,
            CancellationToken cancellationToken)
        {
            // =================================================
            // 1. בדיקת הרשאת מנהל
            // =================================================

            if (!await IsCurrentUserAdminAsync(cancellationToken))
            {
                return StatusCode(403, new
                {
                    message = "רק מנהל האתר רשאי לפרסם תקנון."
                });
            }

            // =================================================
            // 2. פתיחת טרנזקציה
            //
            // כל שינויי הסטטוס נשמרים יחד.
            // אם השמירה נכשלת, לא יבוצע פרסום חלקי.
            // =================================================

            await using var transaction =
                await _dbContext.Database.BeginTransactionAsync(
                    System.Data.IsolationLevel.Serializable,
                    cancellationToken);

            // =================================================
            // 3. איתור הטיוטה
            // =================================================

            var version = await _dbContext.RegulationVersions
                .FirstOrDefaultAsync(
                    v => v.VersionId == versionId,
                    cancellationToken);

            if (version == null)
            {
                return NotFound(new
                {
                    message = "גרסת התקנון לא נמצאה."
                });
            }

            if (version.Status != "Draft")
            {
                return BadRequest(new
                {
                    message = "ניתן לפרסם רק גרסת טיוטה."
                });
            }

            // =================================================
            // 4. בדיקה שכל סעיפי הטיוטה מוכנים
            // =================================================

            int totalChunks = await _dbContext.RegulationChunks
                .CountAsync(
                    c => c.VersionId == versionId,
                    cancellationToken);

            int missingEmbeddings = await _dbContext.RegulationChunks
                .CountAsync(
                    c => c.VersionId == versionId &&
                         (c.Embedding == null || c.Embedding == ""),
                    cancellationToken);

            if (totalChunks == 0 || missingEmbeddings > 0)
            {
                return BadRequest(new
                {
                    message = "לא ניתן לפרסם: מאגר הידע אינו מוכן.",
                    totalChunks,
                    missingEmbeddings
                });
            }

            // =================================================
            // 5. איתור הגרסאות המפורסמות הקיימות
            //
            // מעבירים אותן לארכיון.
            // הסעיפים וה-Embeddings שלהן נשמרים במסד.
            // =================================================

            var previousPublishedVersions =
                await _dbContext.RegulationVersions
                    .Where(v =>
                        v.Status == "Published" &&
                        v.VersionId != versionId)
                    .ToListAsync(cancellationToken);

            foreach (var previousVersion in previousPublishedVersions)
            {
                previousVersion.Status = "Archived";
            }

            // =================================================
            // 6. פרסום הגרסה החדשה
            // =================================================

            version.Status = "Published";
            version.PublishedAt = DateTime.UtcNow;

            // =================================================
            // 7. שמירת כל השינויים יחד
            // =================================================

            await _dbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            // =================================================
            // 8. החזרת תוצאה ל-React
            // =================================================

            return Ok(new
            {
                message = "גרסת התקנון פורסמה בהצלחה.",
                versionId = version.VersionId,
                versionNumber = version.VersionNumber,
                status = version.Status,
                publishedAt = version.PublishedAt,
                totalChunks,
                archivedVersions = previousPublishedVersions.Count
            });
        }
        // =====================================================
        // בדיקת הרשאת מנהל
        //
        // פונקציית עזר משותפת לפעולות ניהול הטיוטות.
        // בודקים את התפקיד העדכני במסד ולא רק בטוקן.
        // =====================================================

        private async Task<bool> IsCurrentUserAdminAsync(
            CancellationToken cancellationToken)
        {
            string? userIdValue = User.FindFirst(
                ClaimTypes.NameIdentifier
            )?.Value;

            if (!int.TryParse(userIdValue, out int userId))
            {
                return false;
            }

            return await _dbContext.Users
                .AsNoTracking()
                .AnyAsync(
                    u => u.UserId == userId
                         && u.Role == "Admin"
                         && u.IsApproved
                         && !u.IsDeleted,
                    cancellationToken);
        }

        // =====================================================
        // יצירת טיוטה מתוך הגרסה המפורסמת
        //
        // POST: api/regulation/create-draft?versionNumber=1.1
        //
        // מעתיקים את התקנון הקיים לגרסת טיוטה חדשה.
        // הצ'אט ממשיך להשתמש בגרסה המפורסמת.
        // =====================================================

        [HttpPost("create-draft")]
        public async Task<IActionResult> CreateDraft(
            [FromQuery] string versionNumber,
            CancellationToken cancellationToken)
        {
            if (!await IsCurrentUserAdminAsync(cancellationToken))
            {
                return StatusCode(403, new
                {
                    message = "רק מנהל האתר רשאי לנהל טיוטות."
                });
            }

            try
            {
                int versionId = await _managementService
                    .CreateDraftFromPublishedAsync(
                        versionNumber,
                        cancellationToken);

                return Ok(new
                {
                    message = "טיוטת התקנון נוצרה בהצלחה.",
                    versionId,
                    versionNumber,
                    status = "Draft"
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =====================================================
        // שליפת סעיפי גרסת תקנון
        //
        // GET: api/regulation/versions/2/sections
        //
        // מחזירים את כל הסעיפים לפי הסדר,
        // כדי להציג אותם במסך עריכת התקנון.
        // =====================================================

        [HttpGet("versions/{versionId:int}/sections")]
        public async Task<IActionResult> GetVersionSections(
            int versionId,
            CancellationToken cancellationToken)
        {
            if (!await IsCurrentUserAdminAsync(cancellationToken))
            {
                return StatusCode(403, new
                {
                    message = "רק מנהל האתר רשאי לצפות במסך הניהול."
                });
            }

            try
            {
                var sections = await _managementService
                    .GetSectionsAsync(
                        versionId,
                        cancellationToken);

                return Ok(sections);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =====================================================
        // עדכון תוכן של סעיף בטיוטה
        //
        // PUT: api/regulation/versions/2/sections/4.16
        //
        // הפעולה מעדכנת את התוכן ומבטלת את ה-Embedding
        // הישן, כדי שניצור וקטור חדש לפני הפרסום.
        // =====================================================

        [HttpPut("versions/{versionId:int}/sections/{sectionNumber}")]
        public async Task<IActionResult> UpdateDraftSection(
            int versionId,
            string sectionNumber,
            [FromBody] UpdateRegulationSectionRequest request,
            CancellationToken cancellationToken)
        {
            if (!await IsCurrentUserAdminAsync(cancellationToken))
            {
                return StatusCode(403, new
                {
                    message = "רק מנהל האתר רשאי לערוך את התקנון."
                });
            }

            if (request == null ||
                string.IsNullOrWhiteSpace(request.Content))
            {
                return BadRequest(new
                {
                    message = "יש להזין תוכן לסעיף."
                });
            }

            try
            {
                await _managementService.UpdateSectionAsync(
                    versionId,
                    sectionNumber,
                    request.Content,
                    cancellationToken);

                return Ok(new
                {
                    message = "הסעיף עודכן בהצלחה.",
                    versionId,
                    sectionNumber
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        // =====================================================
        // שליפת רשימת גרסאות התקנון
        //
        // GET: api/regulation/versions
        //
        // מחזירים את הגרסאות עם מספר הסעיפים
        // ומספר ה-Embeddings שעדיין חסרים.
        //
        // רק מנהל פעיל רשאי לקבל את המידע.
        // =====================================================

        [HttpGet("versions")]
        public async Task<IActionResult> GetVersions(
            CancellationToken cancellationToken)
        {
            // =================================================
            // 1. בדיקת הרשאת מנהל
            // =================================================

            if (!await IsCurrentUserAdminAsync(cancellationToken))
            {
                return StatusCode(403, new
                {
                    message = "רק מנהל האתר רשאי לצפות בגרסאות התקנון."
                });
            }

            // =================================================
            // 2. שליפת כל הגרסאות ומצב ההכנה שלהן
            // =================================================

            var versions = await _dbContext.RegulationVersions
                .AsNoTracking()
                .Select(v => new
                {
                    versionId = v.VersionId,
                    versionNumber = v.VersionNumber,
                    status = v.Status,
                    publishedAt = v.PublishedAt,

                    totalSections = _dbContext.RegulationChunks.Count(
                        c => c.VersionId == v.VersionId),

                    missingEmbeddings = _dbContext.RegulationChunks.Count(
                        c => c.VersionId == v.VersionId &&
                             (c.Embedding == null || c.Embedding == ""))
                })
                .OrderByDescending(v => v.versionId)
                .ToListAsync(cancellationToken);

            // =================================================
            // 3. החזרת הרשימה ל-React
            // =================================================

            return Ok(versions);
        }

        // =====================================================
        // שימוש חוזר ב-Embeddings של סעיפים שלא השתנו
        //
        // POST: api/regulation/reuse-embeddings?versionId=2
        //
        // הפעולה זמינה למנהל בלבד.
        // אינה שולחת בקשות ל-Gemini.
        // =====================================================

        [HttpPost("reuse-embeddings")]
        public async Task<IActionResult> ReuseEmbeddings(
            [FromQuery] int versionId,
            CancellationToken cancellationToken)
        {
            // בדיקת הרשאת מנהל.
            if (!await IsCurrentUserAdminAsync(cancellationToken))
            {
                return StatusCode(403, new
                {
                    message = "רק מנהל האתר רשאי להכין את התקנון."
                });
            }

            try
            {
                // העתקת הווקטורים של הסעיפים הזהים.
                int reusedCount =
                    await _managementService.ReuseEmbeddingsAsync(
                        versionId,
                        cancellationToken);

                // ספירת הסעיפים שעדיין חסרים להם וקטורים.
                int remaining = await _dbContext.RegulationChunks
                    .CountAsync(
                        c => c.VersionId == versionId &&
                             (c.Embedding == null || c.Embedding == ""),
                        cancellationToken);

                return Ok(new
                {
                    message = "בדיקת השימוש החוזר ב-Embeddings הסתיימה.",
                    versionId,
                    reusedCount,
                    remaining
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }
    }

}