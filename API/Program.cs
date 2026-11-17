
using API.Filters;
using API.Services;
using BLL.IServices;
using BLL.Mapping;
using BLL.Services;
using DAL;
using DAL.Entities;
using DAL.IRepository;
using DAL.Repository;
using GenerativeAI.Microsoft;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// =====================================================
// 1. Gemini AI
// =====================================================

var apiKey = builder.Configuration["Gemini:ApiKey"]
    ?? throw new InvalidOperationException(
        "Gemini:ApiKey is missing"
    );

IChatClient innerClient = new GenerativeAIChatClient(
    apiKey,
    "gemini-2.5-flash-lite"
);

builder.Services.AddChatClient(innerClient)
    .UseFunctionInvocation();

builder.Services.AddScoped<IAgentService, AgentService>();

// =====================================================
// 2. Database
// =====================================================

var connStr = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' is missing."
    );

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connStr)
);

// =====================================================
// 3. Authentication services
// =====================================================

builder.Services.AddMemoryCache();

builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<AuthService>();

builder.Services.AddDataProtection();

builder.Services.AddScoped<PasswordSetupTokenService>();
builder.Services.AddScoped<PasswordResetTokenService>();

// =====================================================
// 4. Controllers + Swagger
// =====================================================

builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type =
                Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In =
                Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "Enter your JWT token"
        }
    );

    options.AddSecurityRequirement(
        new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference =
                        new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type =
                                Microsoft.OpenApi.Models.ReferenceType
                                    .SecurityScheme,
                            Id = "Bearer"
                        }
                },
                Array.Empty<string>()
            }
        }
    );
});

// =====================================================
// 5. Repositories
// =====================================================

builder.Services.AddScoped<
    IUserRepository,
    UserRepository
>();

builder.Services.AddScoped<
    ICandidateRepository,
    CandidateRepository
>();

builder.Services.AddScoped<
    IPostRepository,
    PostRepository
>();

builder.Services.AddScoped<
    ICommentRepository,
    CommentRepository
>();

builder.Services.AddScoped<
    IUserVoteRepository,
    UserVoteRepository
>();

builder.Services.AddScoped<
    IAnonymousVoteRepository,
    AnonymousVoteRepository
>();

builder.Services.AddScoped<
    IPostLikeRepository,
    PostLikeRepository
>();

// =====================================================
// 6. Business services
// =====================================================

builder.Services.AddScoped<
    IUserService,
    UserService
>();

builder.Services.AddSingleton<ElectionStateService>();

builder.Services.AddScoped<
    ICandidateService,
    CandidateService
>();

builder.Services.AddScoped<
    IPostService,
    PostService
>();

builder.Services.AddScoped<
    ICommentService,
    CommentService
>();

builder.Services.AddScoped<
    IVotingService,
    VotingService
>();

builder.Services.AddScoped<
    IAnonymousVoteService,
    AnonymousVoteService
>();

builder.Services.AddScoped<
    IPostLikeService,
    PostLikeService
>();

builder.Services.AddScoped<
    ITokenService,
    TokenService
>();

builder.Services.AddScoped<
    ICandidateMatchService,
    CandidateMatchService
>();

builder.Services.AddScoped<
    IPasswordHasher<User>,
    PasswordHasher<User>
>();

builder.Services.AddScoped<
    IRegulationImportService,
    RegulationImportService
>();

// =====================================================
// שירות Embeddings עבור צ'אט התקנון
//
// השירות משתמש במודל Gemini ייעודי ליצירת וקטורים.
// הוא נפרד משירות Gemini הקיים ליצירת תשובות.
//
// HttpClient מאפשר לשלוח בקשות ל-Gemini API.
// מפתח ה-API נלקח מההגדרות הקיימות של השרת.
// =====================================================

// רישום HttpClientFactory לניהול חיבורי HTTP.

builder.Services.AddHttpClient();

// רישום שירות ה-Embeddings באמצעות הממשק שלו.

builder.Services.AddScoped<
    IRegulationEmbeddingService,
    RegulationEmbeddingService
>(serviceProvider =>
{
    var httpClientFactory = serviceProvider
        .GetRequiredService<IHttpClientFactory>();

    var httpClient = httpClientFactory.CreateClient();

    httpClient.Timeout = TimeSpan.FromSeconds(60);

    return new RegulationEmbeddingService(
        httpClient,
        apiKey
    );
});

// =====================================================
// שירות בניית מאגר הווקטורים של התקנון
//
// עובר על סעיפי גרסת התקנון,
// יוצר Embeddings ושומר אותם במסד הנתונים.
// =====================================================

builder.Services.AddScoped<
    IRegulationIndexService,
    RegulationIndexService
>();

// =====================================================
// שירות חיפוש סמנטי בתקנון המפורסם
//
// ממיר שאלות לווקטורים ומשווה אותם לסעיפי התקנון.
// =====================================================

builder.Services.AddScoped<
    IRegulationSearchService,
    RegulationSearchService
>();

// =====================================================
// שירות צ'אט RAG של התקנון
// =====================================================

builder.Services.AddScoped<
    IRegulationChatService,
    RegulationChatService
>();

// =====================================================
// ניהול גרסאות וטיוטות של התקנון
// =====================================================

builder.Services.AddScoped<
    IRegulationManagementService,
    RegulationManagementService
>();

// =====================================================
// 7. JWT
// =====================================================

var jwtKey = builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException(
        "Jwt:Key is missing. Set Jwt__Key."
    );
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,

                ValidIssuer =
                    builder.Configuration["Jwt:Issuer"],

                ValidateAudience = true,

                ValidAudience =
                    builder.Configuration["Jwt:Audience"],

                ValidateLifetime = true,

                ClockSkew = TimeSpan.Zero,

                ValidateIssuerSigningKey = true,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)
                    )
            };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var userIdClaim =
                    context.Principal?.FindFirst(
                        ClaimTypes.NameIdentifier
                    )?.Value;

                if (!int.TryParse(
                    userIdClaim,
                    out int userId))
                {
                    context.Fail("Invalid user token.");
                    return;
                }

                var dbContext =
                    context.HttpContext.RequestServices
                        .GetRequiredService<AppDbContext>();

                var user = await dbContext.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        u => u.UserId == userId
                    );

                if (
                    user == null ||
                    user.IsDeleted ||
                    !user.IsApproved
                )
                {
                    context.Fail(
                        "User account is inactive."
                    );
                }
            }
        };
    });

// =====================================================
// 8. Authorization
// =====================================================

// כל Endpoint מוגן כברירת מחדל.
// גישה ללא Token מותרת רק כאשר הוגדר
// AllowAnonymous במפורש.

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy =
        new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
});

// =====================================================
// 9. CORS - Local + MonsterASP
// =====================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "http://localhost:5174",
                "http://localhost:5175",
                "http://localhost:5176",
                "https://localhost:5173",
                "https://primerize.runasp.net",
                "http://primerize.runasp.net"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// =====================================================
// 10. Image and video storage
// =====================================================

var imagesFolder = Path.GetFullPath(
    Path.Combine(
        builder.Environment.ContentRootPath,
        builder.Configuration["ImagesFolder"] ?? "../Images"
    )
);

Directory.CreateDirectory(imagesFolder);

builder.Services.AddScoped<IImageService>(
    _ => new ImageService(imagesFolder)
);

// =====================================================
// 11. AutoMapper
// =====================================================

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<MappingProfile>();
});

// =====================================================
// 12. Upload limits
// =====================================================

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize =
        100 * 1024 * 1024;
});

builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize =
        100 * 1024 * 1024;
});

// =====================================================
// 13. Build
// =====================================================

var app = builder.Build();

// =====================================================
// 14. HTTP / HTTPS
// =====================================================

// כרגע השרת שלך מוגדר לפעול ב-HTTP.
//
// לא מפעילים הפניית HTTPS כפויה כל עוד
// לא הוגדר HTTPS תקין באתר האחסון.
//
// לשימוש אמיתי עם סיסמאות ו-JWT
// יש להגדיר HTTPS ולהפעיל הפניה אליו.

// app.UseHttpsRedirection();

// =====================================================
// 15. Static media files
// =====================================================

// קבצים שנשמרים בתיקיית Images נגישים דרך:
// /Images/file.jpg
//
// הם ציבוריים ואינם דורשים JWT.

var mediaProvider =
    new PhysicalFileProvider(imagesFolder);

var mediaExtensions =
    new HashSet<string>(
        StringComparer.OrdinalIgnoreCase
    )
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp",
        ".mp4",
        ".webm",
        ".mov"
    };

// Compatibility with old media URLs
// Example: /photo.jpg -> /Images/photo.jpg
// תאימות לכתובות ישנות שמכילות רק שם קובץ.
// אין צורך לשלוח JWT עבור קובצי מדיה ציבוריים.

app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value ?? "";

    if (
        context.Request.Method == HttpMethods.Get ||
        context.Request.Method == HttpMethods.Head
    )
    {
        var requestedFileName =
            Path.GetFileName(path);

        var extension =
            Path.GetExtension(requestedFileName);

        var isRootFile =
            path.StartsWith("/") &&
            path.LastIndexOf('/') == 0;

        if (
            isRootFile &&
            mediaExtensions.Contains(extension)
        )
        {
            var physicalPath = Path.Combine(
                imagesFolder,
                requestedFileName
            );

            if (File.Exists(physicalPath))
            {
                context.Response.Redirect(
                    "/Images/" +
                    Uri.EscapeDataString(requestedFileName),
                    permanent: false
                );

                return;
            }
        }
    }

    await next();
});

// הגשת התמונות והסרטונים לפני מנגנון ההרשאות.

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = mediaProvider,
    RequestPath = "/Images"
});

// =====================================================
// 16. Swagger
// =====================================================

// נגיש לצורך בדיקות הפרויקט.
// לפני שימוש ציבורי אמיתי יש להגביל את הגישה.

app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint(
        "/swagger/v1/swagger.json",
        "Primerize API V1"
    );

    // פתיחת כתובת השרת תציג Swagger.
    c.RoutePrefix = string.Empty;
});

// =====================================================
// 17. Middleware
// =====================================================

app.UseRouting();

app.UseCors("ReactPolicy");

app.UseAuthentication();

app.UseAuthorization();

// =====================================================
// 18. API controllers
// =====================================================

// שומרים על כל ה-Controllers וההרשאות
// שמוגדרות על כל פעולה.

app.MapControllers();

// =====================================================
// 19. Health check
// =====================================================

// Endpoint ציבורי לבדיקת זמינות השרת.
// לא דורש JWT ולא ניגש למסד הנתונים.

app.MapGet("/health", () =>
    Results.Ok(new
    {
        status = "ok",
        service = "Primerize API"
    })
).AllowAnonymous();

// =====================================================
// 20. Unknown API endpoints
// =====================================================

// כתובת API שאינה קיימת מחזירה 404
// ולא 401 רק בגלל שהיא אינה מוכרת.

app.Map("/api/{**path}", () =>
    Results.NotFound()
).AllowAnonymous();

// =====================================================
// 21. Unknown routes
// =====================================================

// השרת הזה מיועד ל-API בלבד.
// אתר React נפרס באתר נפרד.
//
// כתובת שלא קיימת מחזירה 404.
// לא מחזירים בטעות קובץ JavaScript
// או את index.html של React.

app.MapFallback(() =>
    Results.NotFound()
).AllowAnonymous();

// =====================================================
// 22. Start
// =====================================================

app.Run();
