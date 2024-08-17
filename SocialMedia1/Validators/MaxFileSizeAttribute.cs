using SocialMedia1.Controllers;
using System.ComponentModel.DataAnnotations;

namespace SocialMedia1.Validators
{
    public class MaxFileSizeAttribute: ValidationAttribute {
        private readonly long _maxFileSize;

        public MaxFileSizeAttribute(long maxFileSize) {
            _maxFileSize = maxFileSize;
        }
        protected override ValidationResult IsValid
            (object value, ValidationContext validationContext) {
            var file = value as IFormFile;
            if (file != null && file.Length > _maxFileSize) {
                return new ValidationResult(GetErrorMessage());
            }
            return ValidationResult.Success;
        }
        public string GetErrorMessage() {
            return $"Maximum allowed file size is {_maxFileSize} bytes.";
        }
    }
}
