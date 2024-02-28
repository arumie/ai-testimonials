using AiTestimonials.Models;
using AiTestimonials.Repository;
using AiTestimonials.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace AiTestimonials.Api;

public static class AiTestimonialsApi
{
public static void RegisterAiTestimonialsEndpoints(this IEndpointRouteBuilder routes)
    {
        var route = routes.MapGroup("/api/v1/ai-testimonials").WithOpenApi();
        route.MapPost("/generate", PostGenerateAiTestimonialAsync);
        route.MapGet("", GetTestimonialsAsync);
    }

    private static async Task<Ok<TestimonialResult>> PostGenerateAiTestimonialAsync(string name, string skills, [FromHeader(Name = "OPENAI_KEY")] string? openAIKey, AiTestimonialsService service, VercelPostgresRepository repo)
    {
        service.SetupOpenAIService(openAIKey);
        var res = await service.GenerateAiTestimonialAsync(name, skills);
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