using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace DTO.Validation
{
    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly int _maxMb;

        public MaxFileSizeAttribute(int maxMb)
        {
            _maxMb = maxMb;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file && file.Length > _maxMb * 1024 * 1024)
                return new ValidationResult($"גודל הקובץ אינו יכול לעבור {_maxMb}MB");

            return ValidationResult.Success;
        }
    }
}