using System.ComponentModel.DataAnnotations;
using OpenAI;

namespace AiTestimonials.Options;

public class ServiceOptions : IValidatableObject
{
    public const string Section = "AiTestimonials";
    
    public string OpenAiApiKey { get; set; } = default!;

    [Required]
    public string PostgresConnectionString { get; set; } = default!;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (PostgresConnectionString == null)
        {
            yield return new ValidationResult("No PostgresConnectionString for DB specified. Should be on the form: 'Host={host};Username={user};Password={pwd};Database={db}'");
        }
    }
}
