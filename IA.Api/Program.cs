using IA.Api.Application.Contracts;
using IA.Api.Application.Services;
using IA.Api.Infrastructure.OpenAI;
using IA.Api.Infrastructure.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ISumService, SumService>();
builder.Services.Configure<OpenAiOptions>(builder.Configuration.GetSection(OpenAiOptions.SectionName));
builder.Services.AddHttpClient<IOpenAiService, OpenAiService>();
builder.Services.AddHttpClient<IOpenAiResponsesService, OpenAiResponsesService>();
builder.Services.AddHttpClient<IOpenAiTranscriptionService, OpenAiTranscriptionService>();
builder.Services.AddHttpClient<IOpenAiWorkflowService, OpenAiWorkflowService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
