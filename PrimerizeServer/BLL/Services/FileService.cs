using Microsoft.AspNetCore.Http;
using System;
using System.IO;

namespace PrimerizeServer.BLL.Services
{
    public class FileService
    {

        // הגדרת נתיב התיקייה לשמירת התמונות בשרת
        private readonly string _imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

        public string SaveImage(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return string.Empty;

            // וידוא שתיקיית היעד קיימת בשרת
            if (!Directory.Exists(_imagesFolder))
            {
                Directory.CreateDirectory(_imagesFolder);
            }

            // יצירת שם ייחודי לקובץ למניעת התנגשויות
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
            string filePath = Path.Combine(_imagesFolder, uniqueFileName);

            // שמירת הקובץ פיזית בשרת בצורה סינכרונית
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                imageFile.CopyTo(fileStream); // שימוש בהעתקה רגילה ללא Async
            }

            // החזרת הנתיב היחסי שיישמר במסד הנתונים
            return $"/images/{uniqueFileName}";
        }
    }
}

