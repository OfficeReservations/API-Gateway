using CabinetBooking;
using Gateway.Api.Schema;
using Grpc.Net.Client;
using HotChocolate.AspNetCore;
using Microsoft.AspNetCore.Server.Kestrel.Core;

const int ApiPort = 8080;
const string GraphQLHttpUrl = "/graphql/http";

var builder = WebApplication.CreateBuilder(args);

// Добавляем доступ к HttpContext
builder.Services.AddHttpContextAccessor();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddSingleton(services =>
{
    var channel = GrpcChannel.ForAddress("http://localhost:28710");
    return new CabinetBookingService.CabinetBookingServiceClient(channel);
});


// Настройка Kestrel
builder.WebHost.UseKestrel(options =>
{
    options.AddServerHeader = false; // Отключаем заголовок сервера
    options.ListenAnyIP(ApiPort, opt => opt.Protocols = HttpProtocols.Http1AndHttp2); // Слушаем на всех IP-адресах
});

// Добавляем GraphQL сервер
builder.Services.AddGraphQLServer()
    .AddQueryType<Query>() // Указываем корневой тип Query
    .AddMutationType<Mutation>() // Указываем тип Mutation
    .InitializeOnStartup(); // Инициализация при старте

var app = builder.Build();

app.UseCors("AllowAll");
// Настройка маршрутизации
app.UseRouting();

// Настройка GraphQL HTTP endpoint
app.MapGraphQLHttp(GraphQLHttpUrl)
    .AllowAnonymous() // Разрешаем анонимный доступ
    .WithOptions(new GraphQLHttpOptions
    {
        EnableGetRequests = false, // Отключаем GET-запросы
    });



// Настройка дополнительных endpoint'ов
app.UseWhen(ctx => ctx.Request.Host.Port == ApiPort, intApp =>
{
    intApp.UseEndpoints(endpoints =>
    {
        // Endpoint для получения схемы GraphQL
        endpoints.MapGraphQLSchema("/graphql/schema").AllowAnonymous();

        // Endpoint для GraphQL UI (Nitro)
        endpoints.MapNitroApp("/graphql/ui").WithOptions(new GraphQLToolOptions
        {
            Title = "Nitro - Platform",
            GraphQLEndpoint = GraphQLHttpUrl
        });

        // Простой endpoint для проверки работоспособности
        endpoints.MapGet("/check", () => "locked and loaded").AllowAnonymous().ExcludeFromDescription();
    });
});

app.Run();