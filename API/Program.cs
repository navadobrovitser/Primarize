
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
using OpenAI;
using System.ClientModel;
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
// 2. חיבור למסד הנתונים
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
// 3. שירותי אימות דו-שלבי ושליחת מייל
// =====================================================

// זיכרון מטמון עבור קודי האימות
builder.Services.AddMemoryCache();

// שירות שליחת מיילים
builder.Services.AddScoped<EmailService>();

// שירות אימות דו-שלבי
builder.Services.AddScoped<AuthService>();

// =====================================================
// 4. Controllers ו-Swagger
// =====================================================

builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your JWT token"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// =====================================================
// 5. Repositories
// =====================================================
builder.Services.AddDataProtection();

builder.Services.AddScoped<PasswordSetupTokenService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICandidateRepository, CandidateRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IUserVoteRepository, UserVoteRepository>();
builder.Services.AddScoped<IAnonymousVoteRepository, AnonymousVoteRepository>();
builder.Services.AddScoped<IPostLikeRepository, PostLikeRepository>();

// =====================================================
// 6. Services
// =====================================================

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<ElectionStateService>();
builder.Services.AddScoped<ICandidateService, CandidateService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IVotingService, VotingService>();
builder.Services.AddScoped<IAnonymousVoteService, AnonymousVoteService>();
builder.Services.AddScoped<IPostLikeService, PostLikeService>();

builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddScoped<
    IPasswordHasher<User>,
    PasswordHasher<User>
>();

// =====================================================
// 7. JWT Authentication
// =====================================================

var jwtKey = builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException(
        "Jwt:Key is missing. Set the environment variable Jwt__Key."
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
    });

// =====================================================
// 8. Authorization
// =====================================================

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy =
        new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
});

// =====================================================
// 9. CORS - חיבור React לשרת
// =====================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "http://localhost:5174",
                "http://localhost:5175"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// =====================================================
// 10. שירות תמונות ומדיה
// =====================================================

var imagesFolder = Path.GetFullPath(
    Path.Combine(
        builder.Environment.ContentRootPath,
        builder.Configuration["ImagesFolder"] ?? "../Images"
    )
);

Directory.CreateDirectory(imagesFolder);

builder.Services.AddScoped<IImageService>(
    provider => new ImageService(imagesFolder)
);

// =====================================================
// 11. AutoMapper
// =====================================================

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<BLL.Mapping.MappingProfile>();
});

// =====================================================
// 12. הגדרות גודל בקשות
// =====================================================

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 104857600;
});

builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 104857600;
});
builder.Services.AddScoped<
    ICandidateMatchService,
    CandidateMatchService>();

// =====================================================
// 13. בניית האפליקציה
// =================================================





var app = builder.Build();





// =====================================================
// 14. הגדרת קבצים סטטיים
// =====================================================

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(imagesFolder),
    RequestPath = "/Images"
});

// =====================================================
// 15. Swagger
// =====================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "Primerize API V1"
        );

        c.RoutePrefix = string.Empty;
    });
}

// =====================================================
// 16. Middleware Pipeline
// =====================================================

// הפניה ל-HTTPS
app.UseHttpsRedirection();

// ניתוב בקשות
app.UseRouting();

// מאפשר בקשות מ-React
// חייב להופיע לפני Authentication ו-Authorization
app.UseCors("ReactPolicy");

// זיהוי משתמש לפי JWT
app.UseAuthentication();

// בדיקת הרשאות
app.UseAuthorization();

// מיפוי Controllers
app.MapControllers();

// =====================================================
// 17. הפעלת השרת
// =====================================================

app.Run();
