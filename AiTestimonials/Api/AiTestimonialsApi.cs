using AiTestimonials.Models;
using AiTestimonials.Repository;
using AiTestimonials.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AiTestimonials.Api;

public static class AiTestimonialsApi
{
public static void RegisterAiTestimonialsEndpoints(this IEndpointRouteBuilder routes)
    {
        var route = routes.MapGroup("/api/v1/ai-testimonials").WithOpenApi();
        route.MapPost("/generate", PostGenerateAiTestimonialAsync);
        route.MapGet("", GetTestimonialsAsync);
    }

    private static async Task<Ok<TestimonialResult>> PostGenerateAiTestimonialAsync(AiTestimonialsService service, VercelPostgresRepository repo)
    {
        var res = await service.GenerateAiTestimonialAsync();
        await repo.CreatTestimonialsTableAsync();
        await repo.AddTestimonialAsync(res);
        return TypedResults.Ok(res);
    }

    private static async Task<Ok<List<TestimonialResult>>> GetTestimonialsAsync(VercelPostgresRepository repo)
    {
        var res = await repo.GetTestimonialsAsync();
        return TypedResults.Ok(res);
    }
}