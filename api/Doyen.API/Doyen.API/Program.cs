using Doyen.API;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});
builder.Services.AddEndpointsApiExplorer();
string allowSpecificOrigins = "AllowAllHeaders";
var origins = new string[] {
                "http://localhost:18039"
            };
builder.Services.AddCors(options =>
{
    options.AddPolicy(allowSpecificOrigins,
    builder =>
    {
        builder.WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
builder.Services.AddRouting(options => options.LowercaseUrls = true);
string appTitle = "Doyen Expert Finder";
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = appTitle,
        Version = "v1",
        Description = appTitle + " REST API",
        Contact = new OpenApiContact
        {
            Name = "TBD",
            Email = "TBD@todo.com",
            Url = new Uri("https://todo.com"),
        },
    });

    c.CustomOperationIds(apiDesc =>
    {
        return apiDesc.TryGetMethodInfo(out MethodInfo methodInfo) ? methodInfo.Name : null;
    });
});

// Dependency Injection
builder.Services.AddSingleton(Startup.GetElasticsearchSettings(builder));

var app = builder.Build();
app.UseSwagger();
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI();
}
else
{
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
}
app.UseCors(x => x
.AllowAnyMethod()
.AllowAnyHeader()
.SetIsOriginAllowed(origin => true)
.AllowCredentials());
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
