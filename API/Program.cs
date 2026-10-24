using DAL.IRepository;
using BLL.IServices;
using BLL.Mapping;
using DAL.Repository;
using BLL.Services;
using DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System.Text;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);


var connStr = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' is missing. " +
        "Set it with User Secrets or an environment variable.");

builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlServer(connStr));
// הוספת שירותים למיכל ה-DI
builder.Services.AddControllers();

// הגדרת תמיכה ב-Swagger (Swashbuckle)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// רישום הריפוזיטוריז

// רישום ה-Repositories
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

// הגדרת תיקיית המדיה והזרקת השירות בצורה נקייה
var imagesFolder = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, builder.Configuration["ImagesFolder"] ?? "../Images"));
Directory.CreateDirectory(imagesFolder);
builder.Services.AddScoped<IImageService>(provider => new ImageService(imagesFolder));

// רישום ה-AutoMapper
builder.Services.AddAutoMapper(cfg => {
    cfg.AddProfile<BLL.Mapping.MappingProfile>();
});

// רישום ה-DbContext למסד הנתונים
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 104857600; // 100MB (או כל גודל אחר שתרצי בבייטים)
});

// אם את משתמשת ב-IIS:
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 104857600; // 100MB
});

var app = builder.Build();

// הגדרת תיקיית הקבצים הסטטיים לגישה לדפדפן
var imagesFolderFullPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, builder.Configuration["ImagesFolder"] ?? "../Images"));
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(imagesFolderFullPath),
    RequestPath = "/Images"
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Primerize API V1");
        c.RoutePrefix = string.Empty; // מגדיר את ה-Swagger שיופיע ישירות בכתובת הבסיס בהרצת השרת
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();