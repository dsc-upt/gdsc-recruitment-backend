using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using RecruitmentBackend.Areas.Identity;
using RecruitmentBackend.Data;
using RecruitmentBackend.Features.Users;
using RecruitmentBackend.Utilities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var configuration = builder.Configuration;
var services = builder.Services;
var connectionString = configuration.GetConnectionString("Default");

services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
services.AddDatabaseDeveloperPageExceptionFilter();
services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true)
   .AddEntityFrameworkStores<ApplicationDbContext>();
services.AddRazorPages();
services.AddServerSideBlazor();
services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<User>>();
services.AddAuthentication().AddGoogle(options =>
{
    options.ClientId = configuration["Google:ClientId"];
    options.ClientSecret = configuration["Google:ClientSecret"];
    options.ClaimActions.MapJsonKey(CustomClaimTypes.Picture, "picture", "url");
    options.ClaimActions.MapJsonKey(CustomClaimTypes.EmailVerified, "verified_email", "bool");
    // options.ClaimActions.Add(new CustomClaimAction("list json data", "json data"));
    // options.Scope.Add("https://www.googleapis.com/auth/user.phonenumbers.read");
    // options.Events.OnCreatingTicket = async context =>
    // {
    //     var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
    //     request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    //     request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
    //
    //     var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead,
    //         context.HttpContext.RequestAborted);
    //     response.EnsureSuccessStatusCode();
    //
    //     var user = JObject.Parse(await response.Content.ReadAsStringAsync());
    //     user.WriteJson();
    //
    //     // context.RunClaimActions(user);
    // };
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
