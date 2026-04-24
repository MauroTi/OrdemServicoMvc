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

builder.Services.AddScoped<DbConnectionFactory>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IClienteService, ClienteService>();

var app = builder.Build();

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
