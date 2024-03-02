using AiTestimonials.Api;
using AiTestimonials.Options;
using AiTestimonials.Repository;
using AiTestimonials.Services;
using AiTestimonials.Authorization;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<ServiceOptions>()
    .Bind(builder.Configuration.GetSection(ServiceOptions.Section))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<IApiKeyValidation, ApiKeyValidation>();
builder.Services.AddScoped<AiTestimonialsService>();
builder.Services.AddScoped<VercelPostgresRepository>();

builder.Services.AddAuthorization();
builder.Services.AddAuthentication();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition(AuthConstants.ApiKeyName, new OpenApiSecurityScheme
    {
        Description = "ApiKey must appear in header",
        Type = SecuritySchemeType.ApiKey,
        Name = AuthConstants.ApiKeyHeaderName,
        In = ParameterLocation.Header,
        Scheme = "ApiKeyScheme"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
        {
            {
                new OpenApiSecurityScheme()
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = AuthConstants.ApiKeyName
                    },
                    In = ParameterLocation.Header
                },
                new List<string>()
            }
        });
});

var app = builder.Build();

app.RegisterAiTestimonialsEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(new SwaggerOptions() { SerializeAsV2 = true, RouteTemplate = "swagger/{documentName}/swaggerV2.{json|yaml}" });
    app.UseSwagger(new SwaggerOptions() );
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<ApiKeyMiddleware>();
app.UseAuthorization();

app.Run();
