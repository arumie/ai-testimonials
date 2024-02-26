using System.Text.Json;
using AiTestimonials.Models;
using AiTestimonials.Options;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Managers;
using OpenAI.ObjectModels.RequestModels;
using static OpenAI.ObjectModels.StaticValues.ImageStatics.ImageDetailTypes;
using static OpenAI.ObjectModels.StaticValues.ImageStatics;

namespace AiTestimonials.Services;

public class AiTestimonialsService
{
    private readonly ILogger<AiTestimonialsService> _logger;

    private OpenAIService _openaiService;

    public AiTestimonialsService(IOptions<ServiceOptions> serviceOptions, ILogger<AiTestimonialsService> logger)
    {
        _logger = logger;
        _openaiService = new OpenAIService(new OpenAiOptions()
        {
            ApiKey = serviceOptions.Value.OpenAiApiKey
        });

        _openaiService.SetDefaultModelId("gpt-4");

    }

    public async Task<TestimonialResult> GenerateAiTestimonialAsync()
    {
        var testimonial = await CreateTestimonialAsync("David Zachariae, a Frontend Developer, AI Engineer.");
        return await GenerateCompanyLogoAsync(testimonial);

    }

    private async Task<TestimonialResult> CreateTestimonialAsync(string prompt)
    {
        var res = await _openaiService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest()
        {
            Messages = new List<ChatMessage>
            {
                ChatMessage.FromSystem("You generate random short testimonials from fake IT people and companies where you mention fake projects. You output is valid JSON format: { Testimonial: string, TestifierName: string, TestifierCompany: string, TestifierPosition: string }"),
                ChatMessage.FromUser($"Create a random testimonial from a company praising the following developer: {prompt}")
            },
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
        string[] colors = ["Red", "Green", "Teal", "Yellow", "Orange"];
        var colorScheme = colors[new Random().NextInt64(0, 4)];
        var res = await _openaiService.CreateImage(new ImageCreateRequest()
        {
            Model = "dall-e-3",
            Prompt = $"Generate a company logo for the IT company with the following name: '${testimonial.TestifierCompany}'. The logos coloscheme should be {colorScheme}",
            N = 1,
            Size = Size.Size1024,
            Quality = Quality.Hd,
            Style = Style.Natural,
        });

        if (res.Successful)
        {

            testimonial.LogoUrl = res.Results.First().Url;

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