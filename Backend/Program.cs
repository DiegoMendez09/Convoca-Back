using Backend.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var startup = new Backend.Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);

var app = builder.Build();
startup.Configure(app, app.Environment);
await DatabaseInitializer.InitializeAsync(app.Services);

app.Run();
