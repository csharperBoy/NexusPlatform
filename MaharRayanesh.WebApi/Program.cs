using Auth.Infrastructure.DependencyInjection;
using Auth.Presentation.DependencyInjection;
using Core.Infrastructure.DependencyInjection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


// configuration
var configuration = builder.Configuration;

builder.Services.AddAuthInfrastructure(configuration);
builder.Services.AddAuthPresentation();

// Add services to the container.
//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
/*
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(connectionString, b => b.MigrationsAssembly("Auth.Infrastructure")));

builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseSqlServer(connectionString, b => b.MigrationsAssembly("Notification.Infrastructure")));
*/
// Add Core Infrastructure (generic repo, etc.)
builder.Services.AddCoreInfrastructure();


builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
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

app.Run();
