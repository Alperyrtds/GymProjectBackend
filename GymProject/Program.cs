using System.Text;
using GymProject.Config;
using GymProject.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using XLocalizer.Translate.MyMemoryTranslate;
using XLocalizer.Translate;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Bearer Authentication with JWT Token",
        Type = SecuritySchemeType.Http
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            new List<string>()
        }
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateActor = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            // Tüm origin'lere izin ver (React web için)
            policy.SetIsOriginAllowed(origin => 
            {
                // Localhost ve tüm IP adreslerine izin ver
                return true;
            })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // JWT token için credentials gerekli
        });
    
    // Production için belirli origin'lere izin vermek isterseniz:
    // options.AddPolicy("Production",
    //     policy =>
    //     {
    //         policy.WithOrigins("https://yourdomain.com", "https://www.yourdomain.com")
    //               .AllowAnyHeader()
    //               .AllowAnyMethod()
    //               .AllowCredentials();
    //     });
});
builder.Services.AddAuthorization();
builder.Services.AddDbContext<AlperyurtdasGymProjectContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("GymTracking"));
});
builder.Services.AddHttpClient<ITranslator, MyMemoryTranslateService>();

// Push Notification Services
builder.Services.AddHttpClient<GymProject.Services.PushNotificationService>();
builder.Services.AddScoped<GymProject.Services.PushNotificationService>();
builder.Services.AddScoped<GymProject.Services.PushTokenService>();

// Translation Service
builder.Services.AddHttpClient<GymProject.Services.TranslationService>();
builder.Services.AddScoped<GymProject.Services.TranslationService>();

DependencyInjection.Configure(builder.Services);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CORS middleware'i Authentication ve Authorization'dan ÖNCE olmalı
app.UseCors("AllowAll");

//app.UseHttpsRedirection(); // React web için HTTP'ye izin ver

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
