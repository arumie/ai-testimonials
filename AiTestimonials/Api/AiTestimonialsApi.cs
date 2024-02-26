using AiTestimonials.Models;
using AiTestimonials.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AiTestimonials.Api;

public static class AiTestimonialsApi
{
public static void RegisterAiTestimonialsEndpoints(this IEndpointRouteBuilder routes)
    {
        var route = routes.MapGroup("/api/v1/ai-testimonials").WithOpenApi();
        route.MapPost("/generate", PostGenerateAiTestimonialAsync);
    }

    private static async Task<Ok<TestimonialResult>> PostGenerateAiTestimonialAsync(AiTestimonialsService service)
    {
        var res = await service.GenerateAiTestimonialAsync();
        return TypedResults.Ok(res);
    }
}