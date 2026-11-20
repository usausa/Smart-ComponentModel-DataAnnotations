using Example.Web.Infrastructure;

using Smart.ComponentModel.DataAnnotations.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllersWithViews()
    .AddDataAnnotationsLocalization(static options => options.DataAnnotationLocalizerProvider = static (_, _) => JapaneseMessages.Localizer)
    .AddDefaultMessageLocalization()
    .AddValidationAttributeAdapters();

var app = builder.Build();

app.MapStaticAssets();
app.MapDefaultControllerRoute().WithStaticAssets();

app.Run();
