using System.ComponentModel.DataAnnotations;
using OpenAI;

namespace AiTestimonials.Options;

public class ServiceOptions : IValidatableObject
{
    public const string Section = "AiTestimonials";

    [Required]
    public string OpenAiApiKey { get; set; } = default!;

    [Required]
    public string PostgresHost { get; set; } = default!;

    [Required]
    public string PostgresUser { get; set; } = default!;

    [Required]
    public string PostgresDatabase { get; set; } = default!;

    [Required]
    public string PostgresPassword { get; set; } = default!;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (OpenAiApiKey == null)
        {
            yield return new ValidationResult("No ApiKey for OpenAI specified");
        }
        if (PostgresHost == null)
        {
            yield return new ValidationResult("No PostgresHost for DB specified");
        }
        if (PostgresUser == null)
        {
            yield return new ValidationResult("No PostgresUser for DB specified");
        }
        if (PostgresDatabase == null)
        {
            yield return new ValidationResult("No PostgresDatabase for DB specified");
        }
        if (PostgresPassword == null)
        {
            yield return new ValidationResult("No PostgresPassword for DB specified");
        }
    }
}
