using System.ComponentModel.DataAnnotations;
using OpenAI;

namespace AiTestimonials.Options;

public class ServiceOptions : IValidatableObject
{
    public const string Section = "AiTestimonials";

    [Required]
    public string OpenAiApiKey { get; set; } = default!;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (OpenAiApiKey == null)
        {
            yield return new ValidationResult("No ApiKey for OpenAI specified");
        }
    }
}
