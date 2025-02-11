using System.ComponentModel;
using Microsoft.Extensions.AI;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var ollamaEndpoint = builder.Configuration["AI__Ollama__Endpoint"] ?? "http://localhost:11434";
var chatModel = builder.Configuration["AI__Ollama__ChatModel"] ?? "llama3.1";

IChatClient client = new OllamaChatClient(ollamaEndpoint, modelId: chatModel)
    .AsBuilder()
    .UseFunctionInvocation()
    .Build();

[Description("Gets the weather")]
string GetWeather() => Random.Shared.NextDouble() > 0.5 ? "It's sunny" : "It's raining";

[Description("Gets the location")]
string GetLocation() => "Vienna, Austria";

[Description("Gets the weather and location")]
string GetWeatherAndLocation() => $"{GetWeather()} in {GetLocation()}";

[Description("Get Who am I")]
string GetMe() => "Lionel";

var chatOptions = new ChatOptions
{
    Tools =
    [
        AIFunctionFactory.Create(GetWeather),
        AIFunctionFactory.Create(GetLocation),
        AIFunctionFactory.Create(GetWeatherAndLocation),
        AIFunctionFactory.Create(GetMe),
    ]
};

builder.Services.AddChatClient(client);

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi().CacheOutput();

app.MapScalarApiReference();

app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();

app.MapPost(
    "/chat",
    async (IChatClient client, string message) =>
        await client.CompleteAsync(message, chatOptions, cancellationToken: default)
);

app.Run();
