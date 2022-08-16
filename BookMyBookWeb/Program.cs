using BookMyBook_DataAccess;
using BookMyBook_DataAccess.Repository;
using BookMyBook_DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using BookMyBook_Utility;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<BookMyBook_DataAccess.ApplicationDBContext>
    (
        options => options.UseSqlServer
        (
            builder.Configuration.GetConnectionString("DefaultConnection"),
            builder =>
                {
                    builder.EnableRetryOnFailure(10, TimeSpan.FromSeconds(10), null);
                }
                
        ).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)     
        
    );
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("StripeSettings"));
builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddDefaultTokenProviders()
    .AddEntityFrameworkStores<BookMyBook_DataAccess.ApplicationDBContext>();

builder.Services.AddScoped<IUnitofwork, Unitofwork>();
builder.Services.AddSingleton<IEmailSender, EmailSender>();
//builder.Services.AddTransient<IUnitofwork, Unitofwork>();
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";

});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
StripeConfiguration.ApiKey = builder.Configuration.GetSection("StripeSettings:SecretKey").Get<string>();

app.UseAuthentication();;

app.UseAuthorization();
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

app.Run();
