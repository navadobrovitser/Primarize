using BLL.IRepository;
using BLL.Repository;
using BLL.Mapping;
using BLL.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//נאוה
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
builder.Services.AddAutoMapper(cfg => {
    cfg.AddProfile<BLL.Mapping.MappingProfile>();
    var imagesFolder = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, builder.Configuration["ImagesFolder"] ?? "../Images"));
    Directory.CreateDirectory(imagesFolder);
    builder.Services.AddScoped<IImageService>(provider =>
    new ImageService(Path.Combine(builder.Environment.ContentRootPath, "../Images")));

    builder.Services.AddSingleton<IImageService>(new ImageService(imagesFolder));
}); 
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
