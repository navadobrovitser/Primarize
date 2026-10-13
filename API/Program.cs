using BLL.IRepository;
using BLL.Mapping;
using BLL.Repository;
using BLL.Services;
using DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICandidateRepository, CandidateRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IPostLikeRepository, PostLikeRepository>();
builder.Services.AddScoped<IUserVoteRepository, UserVoteRepository>();
builder.Services.AddScoped<IAnonymousVoteRepository, AnonymousVoteRepository>();

// הגדרת תיקיית המדיה והזרקת השירות בצורה נקייה
var imagesFolder = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, builder.Configuration["ImagesFolder"] ?? "../Images"));
Directory.CreateDirectory(imagesFolder);
builder.Services.AddScoped<IImageService>(provider => new ImageService(imagesFolder));

// רישום ה-AutoMapper בצורה נפרדת ותקינה
builder.Services.AddAutoMapper(cfg => {
    cfg.AddProfile<BLL.Mapping.MappingProfile>();
});

// רישום ה-DbContext למסד הנתונים
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();