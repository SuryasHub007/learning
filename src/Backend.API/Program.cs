using Backend.API.Extensions;
using Backend.API.Middleware;
using Backend.Application;
using Backend.Infrastructure;
using Backend.Infrastructure.Persistence;
using Backend.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddApiControllers()
    .AddSwaggerDocumentation()
    .AddJwtAuthentication(builder.Configuration)
    .AddConfiguredCors(builder.Configuration);

builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    await DevelopmentDataSeeder.SeedAsync(dbContext, passwordHasher);
}

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        foreach (var url in app.Urls)
        {
            // ANSI codes: bright blue and underlined, so the link stands out in the terminal.
            Console.WriteLine($"Swagger UI: \u001b[94;4m{url}/swagger\u001b[0m");
        }
    });
}
else
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseCors(ServiceCollectionExtensions.CorsPolicyName);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();