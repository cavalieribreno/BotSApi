using System.Text;
using BotSaaS.Api.Channels;
using BotSaaS.Api.Channels.Telegram;
using BotSaaS.Api.Core.Auth;
using BotSaaS.Api.Core.Companies;
using BotSaaS.Api.Core.Conversations;
using BotSaaS.Api.Capabilities.Appointments;
using BotSaaS.Api.Capabilities.Professionals;
using BotSaaS.Api.Core.Users;
using BotSaaS.Api.Shared.AI;
using BotSaaS.Api.Shared.Database;
using BotSaaS.Api.Shared.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Read Env
DotNetEnv.Env.Load();

// Dependencies 
builder.Services.AddHttpClient<IAiClient, GroqAiClient>();
builder.Services.AddScoped<DbSession>();
builder.Services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddSingleton<IDatabase, MySqlDatabase>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICompaniesRepository, CompaniesRepository>();
builder.Services.AddScoped<ICompaniesService, CompaniesService>();
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IConversationService, ConversationService>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IProfessionalRepository, ProfessionalRepository>();
builder.Services.AddScoped<IProfessionalService, ProfessionalService>();
builder.Services.AddScoped<IAvailabilityPolicy, GenericAvailabilityPolicy>();
builder.Services.AddScoped<IChatTool, CreateAppointmentTool>();
builder.Services.AddScoped<IChatTool, GetAppointmentsTool>();
builder.Services.AddScoped<IChatTool, GetProfessionalsTool>();

// Outbound channel sender via Typed HttpClient
builder.Services.AddHttpClient<IChannelSender, TelegramChannelSender>();

// Background workers
builder.Services.AddHostedService<TelegramPollingService>();
builder.Services.AddHostedService<ReminderBackgroundService>();

// CORS: allow the Angular dev app (localhost:4200) to call the API.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// JWT auth: validates token signature/issuer/audience/expiry on protected routes.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false; // map claim with names "sub", "email"
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
            ValidateAudience = true,
            ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET")!)),
            ValidateLifetime = true
        };
    });

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
