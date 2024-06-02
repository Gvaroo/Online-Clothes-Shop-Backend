using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OnlineShopApp.Data;
using OnlineShopApp.Services.EmailSender;
using OnlineShopApp.Services.Implementations;
using OnlineShopApp.Services.Interfaces;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.AspNetCore.Http;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using FluentValidation;
using System.Reflection;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using OnlineShopApp.Utilities;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

builder.Services.AddSwaggerGen(options =>
{
	options.SwaggerDoc("v1", new OpenApiInfo { Title = "Online Clothes Shop API", Version = "v1" });

	var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
	var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
	options.IncludeXmlComments(xmlPath);
	options.AddSecurityDefinition("oauth2", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
	{
		Description = "Standard authorization header using the bearer scheme, e.g \"bearer {token} \"",
		In = Microsoft.OpenApi.Models.ParameterLocation.Header,
		Name = "Authorization",
		Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey
	});
	options.OperationFilter<SecurityRequirementsOperationFilter>();
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
}, ServiceLifetime.Scoped);

// Add Hangfire services
builder.Services.AddHangfire(config => config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));


// Check Redis connection
bool isRedisAvailable = await RedisHelper.IsRedisAvailableAsync("localhost:6379");

// Conditionally add Redis or In-memory cache
if (isRedisAvailable)
{
	builder.Services.AddStackExchangeRedisCache(options =>
	{
		options.Configuration = "localhost:6379";
		options.InstanceName = "OnlineShopApp:";
	});
	Console.WriteLine("Using Redis cache.");
}
else
{
	builder.Services.AddDistributedMemoryCache();
	Console.WriteLine("Using in-memory cache.");
}


// Add  Fluent Validation support

builder.Services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly()));


builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ITokenValidationService, TokenValidationService>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<TokenValidationService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		options.Events = new()
		{
			OnMessageReceived = context =>
			{
				// Get the token from a cookie
				context.Token = context.Request.Cookies["access_token"];

				return Task.CompletedTask;
			}
		};
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuerSigningKey = true,
			IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value)),
			ValidateIssuer = false,
			//ValidIssuer = builder.Configuration.GetSection("AppSettings:Issuer").Value,
			ValidateAudience = false,
			//ValidAudience = builder.Configuration.GetSection("AppSettings:Audience").Value,

		};
	}

	)
	.AddGoogle(opt =>
	{
		opt.ClientId = "820471012227-a8mlcifobut26bds76gga72td53p5dtr.apps.googleusercontent.com";
		opt.ClientSecret = "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";
	}); ;


builder.Services.AddCors(options =>
{
	options.AddPolicy(name: MyAllowSpecificOrigins,
					  policy =>
					  {
						  policy
						  .WithOrigins(builder.Configuration.GetSection("WebAppUrl:Development").Value)
						  .AllowAnyHeader()						  
						  .AllowAnyMethod()
						  .AllowCredentials();

					  });
});



builder.Services.AddAutoMapper(typeof(Program).Assembly);
builder.Services.AddHttpContextAccessor();
var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}


// Use Hangfire middleware
app.UseHangfireDashboard();


app.UseHttpsRedirection();
app.UseRouting();
app.UseCors(MyAllowSpecificOrigins);


app.UseAuthentication();
app.UseAuthorization();
// Start the Hangfire background processing server
app.UseHangfireServer();
app.MapControllers();

app.Run();
