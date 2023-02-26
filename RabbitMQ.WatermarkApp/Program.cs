using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.WatermarkApp;
using RabbitMQ.WatermarkApp.BackgroundServices;
using RabbitMQ.WatermarkApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseInMemoryDatabase(databaseName: "productDb");
});

//DI rabbitmq ConnectionFactory
builder.Services.AddSingleton(sp => {
    var connectionString = builder.Configuration.GetConnectionString("RabbitMQ");
    return new ConnectionFactory()
    {
        Uri = new Uri(connectionString),
        DispatchConsumersAsync = true
    };
});

builder.Services.AddSingleton<RabbitMQClientService>();
builder.Services.AddSingleton<RabbitMQPublisher>();
//add background consumer-sub service
builder.Services.AddHostedService<ImageWatermarkProcessBackgroundService>();   



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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
