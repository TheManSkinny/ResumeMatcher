using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ResumeMatcher.Data;
using ResumeMatcher.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// 🔗 Add database connection
builder.Services.AddDbContext<ResumeMatcherDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 🤖 Add ML.NET resume matching service
builder.Services.AddScoped<ResumeMatchingService>();

// 📄 Add document processing service
builder.Services.AddScoped<DocumentProcessingService>();

// 📊 Add analytics service
builder.Services.AddScoped<AnalyticsService>();

// 📋 Add application management service
builder.Services.AddScoped<ApplicationService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
