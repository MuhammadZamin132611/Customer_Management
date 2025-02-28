using AutoMapper;
using LearnAPI.Container;
using LearnAPI.Helper;
using LearnAPI.Modal;
using LearnAPI.Repos;
using LearnAPI.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<ICustomerService, CustomerService>();
builder.Services.AddTransient<IRefreshHandler, RefreshHandler>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IUserRoleService, UserRoleService>();
builder.Services.AddDbContext<LearndataContext>(o 
    => o.UseSqlServer(builder.Configuration.GetConnectionString("apicon")));

//builder.Services.AddAuthentication("BasicAuthentication").AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

var _authkey = builder.Configuration.GetValue<string>("JwtSettings:securitykey");
builder.Services.AddAuthentication(item =>
{
    item.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    item.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(item =>
{
    item.RequireHttpsMetadata = true;
    item.SaveToken = true;
    item.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authkey)),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero,
    };
});

var automapper = new MapperConfiguration(item => item.AddProfile(new AutoMapperHandler()));
IMapper mapper = automapper.CreateMapper();
builder.Services.AddSingleton(mapper);
builder.Services.AddCors(option
    => option.AddPolicy("corspolicy", build =>
    {
        build.WithOrigins("http://localhost:4200/", "https://localhost:4200/","*")
        .AllowAnyMethod().AllowAnyHeader();
    }));
builder.Services.AddCors(option
    => option.AddPolicy("corspolicy1", build =>
    {
        build.WithOrigins("http://localhost:4200/", "https://localhost:4200/","*")
        .AllowAnyMethod().AllowAnyHeader();
    }));
builder.Services.AddCors(option
    => option.AddDefaultPolicy(build =>
    {
        build.WithOrigins("*")
        .AllowAnyMethod().AllowAnyHeader();
    }));

builder.Services.AddRateLimiter(_ => _.AddFixedWindowLimiter(policyName:"fixedwindow", options =>
{
    options.Window = TimeSpan.FromMinutes(60);
    options.PermitLimit = 1;
    options.QueueLimit = 0;
    options.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
}).RejectionStatusCode=401);

string logpath = builder.Configuration.GetSection("Logging:Logpath").Value;
var _logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("microsoft", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.File(logpath)
    .CreateLogger();
builder.Services.AddSerilog(_logger);

var _jwtsetting = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(_jwtsetting);

var app = builder.Build();

//app.MapGet("/minimalapi", ()=>"Mohd Zmain");
//app.MapGet("/getchannel", (string channelname) => "Welcome to " + channelname);


app.UseRateLimiter();
// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseStaticFiles();

app.UseCors();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
