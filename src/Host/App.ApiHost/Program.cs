using BuildingBlocks.Core;
using BuildingBlocks.Infrastructure;
using BuildingBlocks.Workflow;
using Company.App.Hse.Endpoints;
using Company.App.Identity.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<IWorkflowRuntime, NoOpWorkflowRuntime>();
builder.Services.AddScoped<ITenantProvider>(_ => new HeaderTenantProvider("default", builder.Configuration.GetConnectionString("DefaultConnection") ?? builder.Configuration.GetConnectionString("PostgreSqlConnection") ?? "Host=localhost;Database=SadathEMS;Username=postgres;Password=postgres"));
builder.Services.AddHseModule();
builder.Services.AddIdentityModule(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new { Solution = "SadathEMS", Host = "App.ApiHost" }));
app.MapHseEndpoints();
app.MapIdentityEndpoints();

app.Run();
