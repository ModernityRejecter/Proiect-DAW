using System.ComponentModel.DataAnnotations;

namespace Proiect.Models
{
    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly int maxFileSize;
        public MaxFileSizeAttribute(int maxFileSize)
        {
            this.maxFileSize = maxFileSize;
        }
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if(value is IFormFile file && file.Length > this.maxFileSize)
            {
                return new ValidationResult($"Fișierul încărcat este prea mare, dimensiunea maximă permisă este {this.maxFileSize / (1024 * 1024)} MB.");
            }
            return ValidationResult.Success;
        }
    }
}
