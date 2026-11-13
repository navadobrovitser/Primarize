
using BLL.Exceptions;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;

namespace BLL.Services
{
    public interface IImageService
    {
        string Save(IFormFile file);
        string SaveImage(IFormFile file);
        string SaveVideo(IFormFile file);
        void Delete(string fileName);
    }

    public class ImageService : IImageService
    {
        private readonly string _folder;

        public ImageService(string folder)
        {
            _folder = folder;

            if (!Directory.Exists(_folder))
            {
                Directory.CreateDirectory(_folder);
            }
        }

        // שמירת תמונה - המתודה הקיימת
        public string Save(IFormFile file)
        {
            return SaveImage(file);
        }

        // שמירת תמונה
        public string SaveImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new BusinessRuleException(
                    "לא נבחר קובץ תמונה."
                );
            }

            var ext = Path.GetExtension(
                file.FileName
            ).ToLowerInvariant();

            var allowed = new[]
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

            if (!allowed.Contains(ext))
            {
                throw new BusinessRuleException(
                    "מותר להעלות תמונות מסוג JPG, JPEG, PNG או WEBP בלבד."
                );
            }

            if (file.Length > 10 * 1024 * 1024)
            {
                throw new BusinessRuleException(
                    "גודל התמונה לא יכול לעלות על 10MB."
                );
            }

            // בדיקת חתימת הקובץ
            using (var stream = file.OpenReadStream())
            {
                var header = new byte[12];
                var bytesRead = stream.Read(
                    header,
                    0,
                    header.Length
                );

                bool isJpeg =
                    bytesRead >= 3 &&
                    header[0] == 0xFF &&
                    header[1] == 0xD8 &&
                    header[2] == 0xFF;

                bool isPng =
                    bytesRead >= 8 &&
                    header[0] == 0x89 &&
                    header[1] == 0x50 &&
                    header[2] == 0x4E &&
                    header[3] == 0x47 &&
                    header[4] == 0x0D &&
                    header[5] == 0x0A &&
                    header[6] == 0x1A &&
                    header[7] == 0x0A;

                bool isWebp =
                    bytesRead >= 12 &&
                    header[0] == (byte)'R' &&
                    header[1] == (byte)'I' &&
                    header[2] == (byte)'F' &&
                    header[3] == (byte)'F' &&
                    header[8] == (byte)'W' &&
                    header[9] == (byte)'E' &&
                    header[10] == (byte)'B' &&
                    header[11] == (byte)'P';

                bool validImage = ext switch
                {
                    ".jpg" or ".jpeg" => isJpeg,
                    ".png" => isPng,
                    ".webp" => isWebp,
                    _ => false
                };

                if (!validImage)
                {
                    throw new BusinessRuleException(
                        "הקובץ שנבחר אינו תמונה תקינה מהסוג המבוקש."
                    );
                }
            }

            var fileName = Guid.NewGuid() + ext;
            var fullPath = Path.Combine(
                _folder,
                fileName
            );

            using (var stream = File.Create(fullPath))
            {
                file.CopyTo(stream);
            }

            return fileName;
        }

        // שמירת סרטון
        public string SaveVideo(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new BusinessRuleException(
                    "לא נבחר קובץ סרטון."
                );
            }

            var ext = Path.GetExtension(
                file.FileName
            ).ToLowerInvariant();

            var allowed = new[]
            {
                ".mp4",
                ".webm",
                ".mov"
            };

            if (!allowed.Contains(ext))
            {
                throw new BusinessRuleException(
                    "מותר להעלות סרטונים מסוג MP4, WEBM או MOV בלבד."
                );
            }

            if (file.Length > 20 * 1024 * 1024)
            {
                throw new BusinessRuleException(
                    "גודל הסרטון לא יכול לעלות על 20MB."
                );
            }

            var fileName = Guid.NewGuid() + ext;
            var fullPath = Path.Combine(
                _folder,
                fileName
            );

            using (var stream = File.Create(fullPath))
            {
                file.CopyTo(stream);
            }

            return fileName;
        }

        // מחיקת קובץ
        public void Delete(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return;
            }

            // מניעת שימוש בנתיבים חיצוניים
            var safeFileName = Path.GetFileName(fileName);

            if (safeFileName != fileName)
            {
                throw new BusinessRuleException(
                    "שם הקובץ אינו תקין."
                );
            }

            var path = Path.Combine(
                _folder,
                safeFileName
            );

            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
