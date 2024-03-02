namespace AiTestimonials.Authorization;

public interface IApiKeyValidation
{
    bool IsValidApiKey(string userApiKey);
}
