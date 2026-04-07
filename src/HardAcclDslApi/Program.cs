using HardAcclDslApi.Services;
using HardAcclDslApi.Models.Ast;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new AstNodeJsonConverter());
    });
builder.Services.AddSingleton<AntlrLuaParserService>();
builder.Services.AddSingleton<LuaToIR>();
builder.Services.AddSingleton<VisualScriptGraphToAstMapper>();
builder.Services.AddSingleton<AstToLuaScribanRenderer>();
builder.Services.AddSingleton<LuaExecutionService>();
var corsAllowedOrigin = builder.Configuration["CORS_ALLOWED_ORIGIN"] ?? "*";
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        if (corsAllowedOrigin == "*")
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
            return;
        }

        policy.WithOrigins(corsAllowedOrigin).AllowAnyHeader().AllowAnyMethod();
    });
});
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

app.UseHttpsRedirection();

app.UseCors("frontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
