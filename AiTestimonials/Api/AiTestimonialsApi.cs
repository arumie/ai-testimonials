using AiTestimonials.Models;
using AiTestimonials.Repository;
using AiTestimonials.Services;
using AiTestimonials.Util;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace AiTestimonials.Api;

public static class AiTestimonialsApi
{
    public static void RegisterAiTestimonialsEndpoints(this IEndpointRouteBuilder routes)
    {
        var route = routes.MapGroup("/api/v1/ai-testimonials").WithOpenApi();
        route.MapPost("/generate", PostGenerateAiTestimonialAsync);
        route.MapPost("/redo", PostRedoTestimonialsAsync);
        route.MapPost("/save", PostSaveTestimonialsAsync);
        route.MapGet("", GetTestimonialsAsync);
    }

    private static async Task<Ok<Identity>> PostGenerateAiTestimonialAsync(TestimonialInput input, [FromHeader(Name = "OPENAI_KEY")] string? openAIKey, AiTestimonialsService service, VercelPostgresRepository repo)
    {
        service.SetupOpenAIService(openAIKey);
        var testimonialId = (await repo.CreateNewTestimonialAsync(input)).ToString();
        try
        {
            GenerateAiTestimonialAsync(testimonialId, input.Name, input.Skills, service, repo).Forget();
            return TypedResults.Ok(new Identity() { Id = testimonialId.ToString() });
        }
        catch
        {
            await repo.UpdateTestimonialAsync(TestimonialStatus.FAILED, testimonialId);
            throw;
        }
    }

    private static async Task<IResult> PostRedoTestimonialsAsync(string id, AiTestimonialsService service, VercelPostgresRepository repo)
    {
        var entity = await repo.GetTestimonialsEntityAsync(id);
        if (entity != null && entity.Status != TestimonialStatus.SAVED && entity.Input != null)
        {
            await repo.UpdateTestimonialAsync(TestimonialStatus.PENDING, id);
            GenerateAiTestimonialAsync(id, entity.Input.Name, entity.Input.Skills, service, repo).Forget();
        }
        else
        {
            return TypedResults.BadRequest();
        };
        return TypedResults.Ok();
    }

    private static async Task<Ok> PostSaveTestimonialsAsync(string id, VercelPostgresRepository repo)
    {
        await repo.UpdateTestimonialAsync(TestimonialStatus.SAVED, id);
        return TypedResults.Ok();
    }

    private static async Task<Ok<List<TestimonialResult>>> GetTestimonialsAsync(string? id, VercelPostgresRepository repo)
    {
        var res = await repo.GetTestimonialsAsync(id);
        return TypedResults.Ok(res);
    }

    private static async Task GenerateAiTestimonialAsync(string id, string name, string skills, AiTestimonialsService service, VercelPostgresRepository repo)
    {
        try
        {
            var res = await service.GenerateAiTestimonialAsync(name, skills);
            await repo.CreatTestimonialsTableAsync();
            await repo.AddTestimonialAsync(res, id);
            await repo.UpdateTestimonialAsync(TestimonialStatus.SUCCESSFUL, id);
        }
        catch
        {
            await repo.UpdateTestimonialAsync(TestimonialStatus.FAILED, id);

        }
    }
}