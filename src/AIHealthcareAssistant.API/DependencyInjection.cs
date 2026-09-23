using System.Reflection;
using System.Text.Json.Serialization;
using AIHealthcareAssistant.API.Middleware;
using AIHealthcareAssistant.Application.Common.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;

namespace AIHealthcareAssistant.API;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(
        this IServiceCollection services)
    {
        services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(
                new JsonStringEnumConverter());
        })
        .ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
        .Where(x => x.Value?.Errors.Count > 0)
        .SelectMany(x => x.Value!.Errors.Select(error =>
        new ApiValidationError
        {
            Field = x.Key,
            Message = string.IsNullOrWhiteSpace(error.ErrorMessage)
        ? "The provided value is invalid."
        : error.ErrorMessage
        }))
        .ToList();

                var response = ApiErrorResponse.Validation(
                            "Validation failed.",
                            errors);

                return new BadRequestObjectResult(response);
            };
        });

        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            var xmlFile =
                $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

            var xmlPath = Path.Combine(
                AppContext.BaseDirectory,
                xmlFile);

            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT token"
            });

            options.AddSecurityRequirement(document =>
                new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                });
        });



        return services;
    }

    public static IApplicationBuilder UseApi(
        this IApplicationBuilder app)
    {
       

        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseRouting();

        return app;
    }

    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        return app;
    }
}
