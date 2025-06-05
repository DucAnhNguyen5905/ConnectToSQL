using DataAccess.Netcore.EfCore;
using DataAccess.Netcore.GenericRepository;
using DataAccess.Netcore.IGenericRepository;
using DataAccess.Netcore.IRepository;
using DataAccess.Netcore.Repository;
using DataAccess.Netcore.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using DataAccess.Netcore.DO;
using NETCORE.API.MiddleWare;

using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
// Add services to the container.
builder.Services.AddDbContext<CSharpCoBanDbContext>(options =>
               options.UseSqlServer(configuration.GetConnectionString("Ten_ConnStr")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:ValidIssuer"],
        ValidAudience = builder.Configuration["Jwt:ValidAudience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))
    };
});
builder.Services.AddStackExchangeRedisCache(options => { options.Configuration = configuration["RedisCacheUrl"]; });

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IAccountRepository,AccountRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAccountGenericRepository, AccountGenericRepository>();
builder.Services.AddScoped<IFunctionRepository, FunctionRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

});
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "DucAnh API", Version = "v1" });
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthentication();

app.UseAuthorization();

builder.Services.AddDirectoryBrowser();

//app.UseMiddleware<MyMiddleWare>();

app.UseStaticFiles(new StaticFileOptions
{ 
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Headers",
            "Origin, X-Request-With, Content-Type, Accept" );
    },
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "Files")),
    RequestPath = "/Files"
});

var fileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "Files"));
var requestPath = "/Files";
app.UseDirectoryBrowser(new DirectoryBrowserOptions
{
    FileProvider = fileProvider,
    RequestPath = requestPath
});
app.UseStaticFiles();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "..", "Static-Files", "wwwroot")),
    RequestPath = "/staticfiles"
});

app.MapControllers();
app.Run();

