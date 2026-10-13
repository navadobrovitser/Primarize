using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Services
{
   
    public class FileService
        {

            private readonly string _imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

            // השירות מקבל Stream רגיל ושם מקורי, ולא IFormFile
            public string SaveImage(Stream fileStream, string originalFileName)
            {
                if (fileStream == null || fileStream.Length == 0)
                    return string.Empty;

                if (!Directory.Exists(_imagesFolder))
                {
                    Directory.CreateDirectory(_imagesFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(originalFileName);
                string filePath = Path.Combine(_imagesFolder, uniqueFileName);

                using (var destinationStream = new FileStream(filePath, FileMode.Create))
                {
                    fileStream.CopyTo(destinationStream);
                }

                return $"/images/{uniqueFileName}";
            }
        }
    }

