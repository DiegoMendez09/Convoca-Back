using AutoMapper;
using Backend.Clients;
using Backend.Mappings.Profiles;
using Backend.Middleware;
using Backend.Models.Database;
using Backend.Options;
using Backend.Repositories.Implementations;
using Backend.Repositories.Interfaces;
using Backend.Services;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Backend;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddOpenApi();

        services.AddOptions<SecopOptions>()
            .Bind(_configuration.GetSection(SecopOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.BaseUrl), "Secop:BaseUrl es requerido.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.DatasetId), "Secop:DatasetId es requerido.")
            .Validate(options => options.TimeoutSeconds > 0, "Secop:TimeoutSeconds debe ser mayor a cero.")
            .ValidateOnStart();

        var allowedOrigins = _configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        if (allowedOrigins.Length == 0)
            throw new InvalidOperationException("Cors:AllowedOrigins es requerido.");

        services.AddCors(options =>
        {
            options.AddPolicy(CorsSettings.PolicyName, policy =>
            {
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithExposedHeaders(CorsSettings.ExportTruncatedHeader, CorsSettings.ExportCountHeader);
            });
        });

        services.AddHttpClient<ISecopApiClient, SecopApiClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<SecopOptions>>().Value;
            var baseUrl = options.BaseUrl.EndsWith('/') ? options.BaseUrl : options.BaseUrl + "/";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("SECOPII/1.0");

            if (!string.IsNullOrWhiteSpace(options.AppToken))
                client.DefaultRequestHeaders.Add("X-App-Token", options.AppToken);
        });

        services.AddScoped<IFavoriteService, FavoriteService>();

        var connectionString = _configuration.GetConnectionString("SecopDb");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddDbContext<SecopDbContext>(options => options.UseSqlServer(connectionString));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }

        services.AddAutoMapper(typeof(ProcessProfile));
        services.AddScoped<IProcessService, ProcessService>();
    }

    public void Configure(WebApplication app, IWebHostEnvironment environment)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        if (environment.IsDevelopment())
            app.MapOpenApi();
        else
            app.UseHttpsRedirection();

        app.UseCors(CorsSettings.PolicyName);
        app.UseAuthorization();
        app.MapControllers();
    }
}
