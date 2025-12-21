using System.ComponentModel.DataAnnotations;

namespace Proiect.Models
{
    public class AllowedExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] extensions;
        public AllowedExtensionsAttribute(string[] extensions)
        {
            this.extensions = extensions;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                var extension = Path.GetExtension(file.FileName).ToLower();
                if (!extensions.Contains(extension))
                {
                    return new ValidationResult($"Extensia {extension} nu este permisă, folosește: {string.Join(", ", extensions)}");
                }
            }
            return ValidationResult.Success;
        }
    }
}
