using AiTestimonials.Api;
using AiTestimonials.Options;
using AiTestimonials.Repository;
using AiTestimonials.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<ServiceOptions>()
    .Bind(builder.Configuration.GetSection(ServiceOptions.Section))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<AiTestimonialsService>();
builder.Services.AddScoped<VercelPostgresRepository>();

var app = builder.Build();

app.RegisterAiTestimonialsEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();
