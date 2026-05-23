using App.SharedUI;
using App.SharedUI.HostApi;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;

namespace SadathEMS.AppMobile;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
     builder.Services.Configure<HostApiOptions>(options =>
		{
			options.BaseUrl = builder.Configuration[$"{HostApiOptions.SectionName}:BaseUrl"] ?? "https://localhost:7185";
		});
      builder.Services.AddScoped(serviceProvider =>
		{
			var options = serviceProvider.GetRequiredService<IOptions<HostApiOptions>>().Value;
			return new HttpClient { BaseAddress = new Uri(options.BaseUrl) };
		});
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();
		builder.Services.AddSharedUi(builder.Configuration);

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
