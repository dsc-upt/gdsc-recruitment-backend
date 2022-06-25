using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using RecruitmentBackend.Areas.Identity;
using RecruitmentBackend.Data;
using RecruitmentBackend.Features.Users;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var configuration = builder.Configuration;
var services = builder.Services;
var connectionString = configuration.GetConnectionString("Default");

services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
services.AddDatabaseDeveloperPageExceptionFilter();
services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();
services.AddRazorPages();
services.AddServerSideBlazor();
services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<User>>();
services.AddAuthentication().AddGoogle(options =>
{
    options.ClientId = configuration["Google:ClientId"];
    options.ClientSecret = configuration["Google:ClientSecret"];
    options.Events.OnTicketReceived = context =>
    {
        Console.WriteLine(context.Response.Body);
        return Task.CompletedTask;
    };
    options.ClaimActions.MapJsonKey("urn:google:picture", "picture", "url");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
