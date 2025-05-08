using NetCoreAPI.MyMiddleware;
var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMyMiddleWare();// tự định nghĩ middleware


app.Run(async context =>
{
    await context.Response.WriteAsync("Chuong trinh se dung tai day va tra ve ket qua");
});
//app.UseMiddleware<MiddleWare>();

app.UseAuthorization();

app.MapControllers();

app.Run();
