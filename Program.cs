using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SentinelDoCloudinho.Services; // ou seu namespace correto
using SentinelDoCloudinho.Services.Hubs;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSignalR();
builder.Services.AddScoped<SecurityTestService>();
builder.Services.AddHttpClient();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapHub<TestHub>("/testHub");
app.MapFallbackToPage("/_Host");

app.Run();

