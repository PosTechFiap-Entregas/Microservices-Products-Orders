using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orders.API.Extensions;
using Orders.Application.Services;
using Orders.Application.Services.Interface;
using Orders.Application.Services.Service;
using Orders.Domain.Interfaces;
using Orders.Domain.Interfaces.Repository;
using Orders.Infrastructure.Data;
using Orders.Infrastructure.HttpClients;
using Orders.Infrastructure.Repositories;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// HttpClient para comunicação com Products API
var productsApiUrl = builder.Configuration["ProductsApi:BaseUrl"] ?? "http://localhost:5001";
builder.Services.AddHttpClient<IProductsHttpClient, ProductsHttpClient>(client =>
{
    client.BaseAddress = new Uri(productsApiUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// HttpClient para comunicação com Payment API
var paymentApiUrl = builder.Configuration["PaymentApi:BaseUrl"] ?? "http://localhost:8083";
builder.Services.AddHttpClient<IPaymentHttpClient, PaymentHttpClient>(client =>
{
    client.BaseAddress = new Uri(paymentApiUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Dependency Injection
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Orders API",
        Version = "v1",
        Description = "API de gerenciamento de pedidos - Microserviço"
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<OrdersDbContext>();

var app = builder.Build();

app.ApplyMigrations();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    //app.UseSwagger();
    //app.UseSwaggerUI();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();