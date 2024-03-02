namespace AiTestimonials.Authorization;

internal sealed class ApiKeyValidation(IConfiguration configuration) : IApiKeyValidation
{
    public bool IsValidApiKey(string userApiKey)
    {
        if (string.IsNullOrWhiteSpace(userApiKey))
        {
            return false;
        }

        var apiKey = configuration.GetValue<string>(AuthConstants.ApiKeyName);
        return apiKey != null && apiKey == userApiKey;
    }
}
