using App.SharedUI;
using App.SharedUI.HostApi;
using Company.App.Identity.Endpoints;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;
using SadathEMS.AppWeb.Components;
using SadathEMS.AppWeb.HostApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorizationCore();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSharedUi(builder.Configuration);
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.Configure<HostApiOptions>(builder.Configuration.GetSection(HostApiOptions.SectionName));
builder.Services.AddHttpClient("HostApi", (serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<HostApiOptions>>().Value;
    if (!string.IsNullOrWhiteSpace(options.BaseUrl) && Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var configuredBaseUri))
    {
        client.BaseAddress = configuredBaseUri;
    }
});
builder.Services.AddScoped<HostApiClient>();
builder.Services.AddScoped<HostApiAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(serviceProvider =>
    serviceProvider.GetRequiredService<HostApiAuthenticationStateProvider>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapIdentityEndpoints();
app.MapRazorComponents<SadathEMS.AppWeb.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();
