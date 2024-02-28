namespace AiTestimonials.Models;

public record TestimonialResult
{
    public string? Testimonial { get; init; }
    public string? TestifierName { get; init; }
    public string? TestifierCompany { get; init; }
    public string? TestifierPosition { get; init; }
    public string? LogoUrl { get; set; }
    public string? LogoB64 { get; set; }
}