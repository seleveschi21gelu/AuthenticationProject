using CompanyEmployees.Entities.DataTransferObjects;
using CompanyEmployees.Entities.Helpers;
using CompanyEmployees.Entities.Models;
using CompanyEmployees.Extensions;
using CompanyEmployees.Repository;
using EmailSenderProject.Interfaces;
using EmailSenderProject.Models;
using EmailService;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.ConfigureCors();
builder.Services.ConfigureIISIntegration();
builder.Services.ConfigureSqlContext(builder.Configuration);
builder.Services.ConfigureRepositoryManager();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        builder =>
        {
            //you can configure your custom policy
            builder.AllowAnyOrigin()
                                .AllowAnyHeader()
                                .AllowAnyMethod();
        });
});

builder.Services.AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
                .AddCertificate();

builder.Services.AddIdentity<User, IdentityRole>(opt =>
{
    opt.Password.RequiredLength = 7;
    opt.Password.RequireDigit = false;

    opt.User.RequireUniqueEmail = true;
    opt.SignIn.RequireConfirmedEmail = true;

    opt.Tokens.EmailConfirmationTokenProvider = "emailconfirmation";

    opt.Lockout.AllowedForNewUsers = true;
    opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    opt.Lockout.MaxFailedAccessAttempts = 5;
}).AddEntityFrameworkStores<RepositoryContext>()
  .AddDefaultTokenProviders()
  .AddTokenProvider<EmailConfirmationTokenProvider<User>>("emailconfirmation");

builder.Services.Configure<DataProtectionTokenProviderOptions>(opt => opt.TokenLifespan = TimeSpan.FromHours(2));

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["validIssuer"],
        ValidAudience = jwtSettings["validAudience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.GetSection("securityKey").Value)),
    };
});

builder.Services.Configure<DataProtectionTokenProviderOptions>(opt => opt.TokenLifespan = TimeSpan.FromHours(2));

builder.Services.Configure<EmailConfirmationTokenProviderOptions>(opt =>
    opt.TokenLifespan = TimeSpan.FromDays(3));

var emailConfig = builder.Configuration.GetSection("EmailConfiguration")
                                       .Get<EmailConfiguration>();

builder.Services.AddSingleton(emailConfig);
builder.Services.AddScoped<IEmailSender, EmailSender>();

builder.Services.AddScoped<JwtHandler>();
builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
