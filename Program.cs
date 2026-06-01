using Serilog;
using Api.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("secrets.json", optional: true, reloadOnChange: true);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Host.UseSerilog((context, configuration) =>
{
    var betterStackEndpoint = context.Configuration["BetterStack:Endpoint"];
    var sourceToken = context.Configuration["BetterStack:SourceToken"];

    configuration
        .MinimumLevel.Error();

    if (!string.IsNullOrWhiteSpace(sourceToken) && !string.IsNullOrWhiteSpace(betterStackEndpoint))
    {
        configuration.WriteTo.BetterStack(
            sourceToken: sourceToken,
            betterStackEndpoint: betterStackEndpoint
        );
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
