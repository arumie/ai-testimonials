namespace AiTestimonials.Models;

public record TestimonialEntity
{
    public required string Id { get; init; }
    public required TestimonialStatus Status { get; init; }
    public required TestimonialInput Input { get; init; }
    public TestimonialResult? Testimonial { get; init; }
}

public record TestimonialResult
{
    public string? Testimonial { get; init; }
    public string? TestifierName { get; init; }
    public string? TestifierCompany { get; init; }
    public string? TestifierPosition { get; init; }
    public string? LogoUrl { get; set; }
    public string? LogoB64 { get; set; }
}

public record TestimonialInput
{
    public required string Name { get; init; }
    public required string Skills { get; init; }
}

public record Identity
{
    public string Id { get; init; }
}

public enum TestimonialStatus
{
    PENDING = 0,
    SAVED = 1,
    SUCCESSFUL = 2,
    FAILED = 3
}