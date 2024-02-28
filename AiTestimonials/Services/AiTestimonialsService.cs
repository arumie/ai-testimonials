using System.Text.Json;
using AiTestimonials.Models;
using AiTestimonials.Options;
using MathNet.Numerics.Random;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Managers;
using OpenAI.ObjectModels.RequestModels;
using static OpenAI.ObjectModels.StaticValues.ImageStatics;

namespace AiTestimonials.Services;

public class AiTestimonialsService(IOptions<ServiceOptions> serviceOptions, ILogger<AiTestimonialsService> logger)
{
    private readonly ILogger<AiTestimonialsService> _logger = logger;
    private readonly ServiceOptions _serviceOptions = serviceOptions.Value;

    private OpenAIService? openaiService;

    public void SetupOpenAIService(string? apiKey)
    {

        var key = apiKey ?? _serviceOptions?.OpenAiApiKey;
        if ( key != null)
        {
            openaiService = new OpenAIService(new OpenAiOptions()
            {
                ApiKey = key
            });

            openaiService.SetDefaultModelId("gpt-4");
        } else {
            throw new Exception("No API key for OpenAI specified");
        }

    }

    public async Task<TestimonialResult> GenerateAiTestimonialAsync(string name, string skills)
    {
        var testimonial = await CreateTestimonialAsync(name, skills);
        return await GenerateCompanyLogoAsync(testimonial);

    }

    private async Task<TestimonialResult> CreateTestimonialAsync(string developer, string work)
    {
        if (openaiService == null)
        {
            throw new Exception("OpenAIService not initialized");
        }   

        var res = await openaiService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest()
        {
            Messages =
            [
                ChatMessage.FromSystem("You generate random short testimonials from fake IT people and companies where you mention fake projects. You output is valid JSON format: { Testimonial: string, TestifierName: string, TestifierCompany: string, TestifierPosition: string }"),
                ChatMessage.FromUser($"Create a random testimonial from a company praising the following software developer \"{developer}\", for his great work with \"{work}\"")
            ],
            MaxTokens = 1000,
            Temperature = 0.9f

        });
        if (res.Successful)
        {
            var content = res.Choices.First().Message.Content?.Replace("\n", "") ?? "{}";
            return JsonSerializer.Deserialize<TestimonialResult>(content) ?? new TestimonialResult();
        }
        else
        {
            if (res.Error == null)
            {
                throw new Exception("Unknown Error");
            }
            throw new Exception(res.Error.Message);
        }


    }

    private async Task<TestimonialResult> GenerateCompanyLogoAsync(TestimonialResult testimonial)
    {
        if (openaiService == null)
        {
            throw new Exception("OpenAIService not initialized");
        } 

        var random = new Random();
        string[] styles = ["mascot", "pictorial", "abstract", "emblem", "lettermark", "simple"];
        string[] designers = ["Saul Bass", "Paul Rand", "Piet Mondrian"];
        string[] genres = ["Abstract Expessionism", "Crystal Cubism", "Pop Art"];
        string[] techniques = ["gradient", "outline"];
        var style = styles[random.NextInt64(0, styles.Length - 1)];
        var useDesigner = random.NextBoolean();
        var useGenre = random.NextBoolean();
        var useTechnique = random.NextBoolean();
        var designer = useDesigner ? $", by {designers[random.NextInt64(0, designers.Length - 1)]}" : "";
        var genre = useGenre ? $", {genres[random.NextInt64(0, genres.Length - 1)]}" : "";
        var technique = useTechnique ? $", {techniques[random.NextInt64(0, techniques.Length - 1)]}" : "";
        
        var prompt = $"A {style} company logo for the company {testimonial.TestifierCompany}, vector{technique}{genre}{designer}'";
        var res = await openaiService.CreateImage(new ImageCreateRequest()
        {
            Model = "dall-e-3",
            Prompt = prompt,
            N = 1,
            Size = Size.Size1024,
            Quality = Quality.Hd,
            Style = Style.Natural,
            ResponseFormat = "b64_json"
        });

        if (res.Successful)
        {
            testimonial.LogoB64 = res.Results.First().B64;

            return testimonial;
        }
        else
        {
            if (res.Error == null)
            {
                throw new Exception("Unknown Error");
            }
            throw new Exception(res.Error.Message);
        }
    }


}