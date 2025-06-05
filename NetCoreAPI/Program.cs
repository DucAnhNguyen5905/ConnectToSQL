using DataAccessNetcore.EFCore;
using DataAccessNetcore.IRepository;
using DataAccessNetcore.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NetCoreAPI.MyMiddleware;
var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
// Add services to the container.
builder.Services.AddDbContext<CsharpCobanDBContext>(options =>
               options.UseSqlServer(configuration.GetConnectionString("Ten_ConnStr")));
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMyMiddleWare();// tự định nghĩ middleware


/*app.Run(async context =>
{
    await context.Response.WriteAsync("Chuong trinh se dung tai day va tra ve ket qua");
});*/

app.UseAuthorization();

app.MapControllers();

app.Run();
