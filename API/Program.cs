        ".webm",
        ".mov"
    };

// Compatibility with old media URLs

app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value ?? "";

    if (
        context.Request.Method == HttpMethods.Get ||
        context.Request.Method == HttpMethods.Head
    )
    {
        var requestedFileName = Path.GetFileName(path);
        var extension = Path.GetExtension(requestedFileName);

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

// Public image and video files

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = mediaProvider,
    RequestPath = "/Images"
});

// =====================================================
// 16. React frontend
// =====================================================

app.UseDefaultFiles();
app.UseStaticFiles();

// =====================================================
// 17. Swagger
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
// 18. Middleware
// =====================================================

app.UseRouting();

app.UseCors("ReactPolicy");

app.UseAuthentication();

app.UseAuthorization();

// API controllers
app.MapControllers();

// Return 404 for unknown API endpoints,
// rather than returning React's index.html.
app.Map("/api/{**path}", () => Results.NotFound())
   .AllowAnonymous();

// React SPA fallback
app.MapFallbackToFile("index.html")
   .AllowAnonymous();

// =====================================================
// 19. Start
// =====================================================

app.Run();
