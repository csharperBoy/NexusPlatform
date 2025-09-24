using Auth.Infrastructure.DependencyInjection;
using Auth.Presentation.DependencyInjection;
using Auth.Application.DependencyInjection;
using Core.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);


// configuration
var configuration = builder.Configuration;

builder.Services.AddCoreInfrastructure();
builder.Services.AddAuthApplication();
builder.Services.AddAuthInfrastructure(configuration);

builder.Services.AddAuthPresentation();



builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.Services.SeedAuthModuleAsync();
app.Run();
