using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Register_Login_Otp.Data;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.




builder.Services.AddDbContextPool<ApplicationDbContext>(options =>
{
    var connetionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(connetionString, ServerVersion.AutoDetect(connetionString));
});



builder.Services.AddCors(option =>
{
    option.AddPolicy(name: "MyPolicy",
        builder =>
        {
            builder.WithOrigins("http://localhost:3000/")
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
});





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
app.UseCors("MyPolicy");


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
