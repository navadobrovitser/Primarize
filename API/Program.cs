using BLL.IServices;
using BLL.Mapping;
using BLL.Services;
using DAL;
using DAL.Entities;
using DAL.IRepository;
using DAL.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. קריאת מחרוזת החיבור ורישום ה-DbContext (פעם אחת בלבד)
var connStr = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connStr));

// 2. הוספת קונטרולרים ו-Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 3. רישום ה-Repositories וה-Services (ללא כפילויות)
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICandidateRepository, CandidateRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IUserVoteRepository, UserVoteRepository>();
builder.Services.AddScoped<IAnonymousVoteRepository, AnonymousVoteRepository>();
builder.Services.AddScoped<IPostLikeRepository, PostLikeRepository>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICandidateService, CandidateService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IVotingService, VotingService>();
builder.Services.AddScoped<IAnonymousVoteService, AnonymousVoteService>();
builder.Services.AddScoped<IPostLikeService, PostLikeService>();

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// 4. הגדרת אימות (Authentication) והרשאות (Authorization)
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException("Jwt:Key is missing. Set the environment variable Jwt__Key.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],

            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// 5. הגדרת שירות תמונות ומדיה
var imagesFolder = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, builder.Configuration["ImagesFolder"] ?? "../Images"));
Directory.CreateDirectory(imagesFolder);
builder.Services.AddScoped<IImageService>(provider => new ImageService(imagesFolder));

// 6. רישום AutoMapper
builder.Services.AddAutoMapper(cfg => {
    cfg.AddProfile<BLL.Mapping.MappingProfile>();
});

// 7. הגדרות גודל בקשה ל-Kestrel ו-IIS
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 104857600; // 100MB
});

builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 104857600; // 100MB
});

var app = builder.Build();

// 8. הגדרת קבצים סטטיים
var imagesFolderFullPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, builder.Configuration["ImagesFolder"] ?? "../Images"));
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(imagesFolderFullPath),
    RequestPath = "/Images"
});

// 9. צינור העיבוד (Pipeline)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Primerize API V1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();