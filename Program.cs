using Banking_IVR.Services;
using Banking_IVR.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddHealthChecks();
builder.Services
    .AddOptions<IvrOptions>()
    .Bind(builder.Configuration.GetSection(IvrOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
var ivrOptions = builder.Configuration.GetSection(IvrOptions.SectionName).Get<IvrOptions>() ?? new IvrOptions();

if (string.Equals(ivrOptions.PersistenceMode, "PostgreSql", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddDbContext<BankingIvrDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
    builder.Services.AddScoped<ISessionService, PostgresSessionService>();
}
else
{
    builder.Services.AddSingleton<ISessionService, InMemorySessionService>();
}

builder.Services.AddSingleton<ITranslationService, TranslationService>();
builder.Services.AddSingleton<IBankingService, BankingService>();

var app = builder.Build();

if (string.Equals(ivrOptions.PersistenceMode, "PostgreSql", StringComparison.OrdinalIgnoreCase))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<BankingIvrDbContext>();
    dbContext.Database.Migrate();
}

app.UseExceptionHandler();
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/scalar", options =>
    {
        options.WithTitle("Banking IVR API");
    });
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
var contentTypeProvider = new FileExtensionContentTypeProvider();
contentTypeProvider.Mappings[".aiff"] = "audio/aiff";
contentTypeProvider.Mappings[".aif"] = "audio/aiff";
contentTypeProvider.Mappings[".wav"] = "audio/wav";
contentTypeProvider.Mappings[".mp3"] = "audio/mpeg";
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = contentTypeProvider
});
app.MapHealthChecks("/health");
app.MapGet("/", () => Results.Redirect(app.Environment.IsDevelopment() ? "/scalar" : "/health"));
app.MapControllers();

app.Run();
