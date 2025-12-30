using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using ShopSphere.Application.Caching;
using ShopSphere.Application.Messaging;
using ShopSphere.Application.Repositories;
using ShopSphere.Application.Services;
using ShopSphere.Infrastructure.Messaging;
using ShopSphere.Infrastructure.Repositories;
using ShopSphere.Infrastructure.db;
using ShopSphere.Worker.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));
builder.Services.Configure<CacheOptions>(builder.Configuration.GetSection("Cache"));

builder.Services.AddDbContext<ShopSphereDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IOutboxRepository, OutboxRepository>();
builder.Services.AddScoped<IProcessedMessageRepository, ProcessedMessageRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();
builder.Services.AddScoped<CheckoutService>();

builder.Services.AddHostedService<OutboxDispatcher>();
builder.Services.AddHostedService<OrderPaymentConsumer>();

var host = builder.Build();
await host.RunAsync();
