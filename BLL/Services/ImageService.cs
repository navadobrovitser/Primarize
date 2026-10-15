using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;

namespace BLL.Services
{
    public interface IImageService
    {
        string Save(IFormFile file);
        string SaveVideo(IFormFile file);
        void Delete(string fileName);
    }

    public class ImageService : IImageService
    {
        private readonly string _folder;

        public ImageService(string folder)
        {
            _folder = folder;
        }

        // שמירת תמונה ובדיקת תקינות
        public string Save(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLower();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (!allowed.Contains(ext))
                throw new ArgumentException("מותר להעלות רק קבצי תמונה מסוג jpg, png או webp");

            if (file.Length > 5 * 1024 * 1024)
                throw new ArgumentException("קובץ התמונה גדול מדי (מקסימום 5MB)");

            var fileName = Guid.NewGuid() + ext; // יצירת שם ייחודי למניעת התנגשויות

            using (var stream = File.Create(Path.Combine(_folder, fileName)))
            {
                file.CopyTo(stream);
            }

            return fileName; // החזרת שם הקובץ בלבד לשמירה במסד הנתונים[cite: 1]
        }

        // שמירת סרטון הסבר ובדיקת תקינות
        public string SaveVideo(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLower();
            var allowed = new[] { ".mp4", ".webm", ".mov" };
            if (!allowed.Contains(ext))
                throw new ArgumentException("מותר להעלות סרטונים בפורמט MP4, WEBM או MOV בלבד");

            if (file.Length > 50 * 1024 * 1024)
                throw new ArgumentException("קובץ הסרטון גדול מדי (מקסימום 50MB)");

            var fileName = Guid.NewGuid() + ext;

            using (var stream = File.Create(Path.Combine(_folder, fileName)))
            {
                file.CopyTo(stream);
            }

            return fileName;
        }

        // מחיקת קובץ פיזי מהשרת בעת עדכון או מחיקת מועמד[cite: 1]
        public void Delete(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return;

            var path = Path.Combine(_folder, fileName);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}