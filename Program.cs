using OrdemServicoMvc.Data;
using OrdemServicoMvc.Filters;
using OrdemServicoMvc.Repositories;
using OrdemServicoMvc.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllersWithViews(
        options =>
        {
            options.Filters.Add<GlobalExceptionFilter>();
        });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<DatabaseServiceStartupOptions>(
    builder.Configuration.GetSection("DatabaseServiceStartup"));
builder.Services.AddScoped<DbConnectionFactory>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddSingleton<DatabaseWindowsServiceStarter>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var databaseStarter = scope.ServiceProvider.GetRequiredService<DatabaseWindowsServiceStarter>();
    await databaseStarter.EnsureStartedAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Swagger
app.UseSwagger();
app.UseSwaggerUI(
    c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "OrdemServico API v1");
        c.RoutePrefix = "swagger";
    });

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(name: "default", pattern: "{controller=Clientes}/{action=Index}/{id?}");

app.Run();
