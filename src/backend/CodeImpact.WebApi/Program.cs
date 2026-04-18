using CodeImpact.WebApi.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWebApiServices(builder.Configuration);
// Add CORS for frontend development (Vite default origin)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhostVite", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowLocalhostVite");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
