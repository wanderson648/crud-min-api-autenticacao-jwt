using CatalogoApi.ApiEndpoints;
using CatalogoApi.AppServicesExtensions;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container .//--> m�todo ConfigureServices()

builder.AddApiSwagger();
builder.AddPersistence();
builder.Services.AddCors();
builder.AddAutenticationJwt();


var app = builder.Build();
// Cofigure the HTTP request pipeline.//--> m�todo Configure()


// ----------------------- endpoint para autentica��o de login -------------------------------
app.MapAutenticacaoEndpoints();

// -----------------------  endpoints categorias -----------------------------
app.MapCategoriasEndpoints();

// -----------------------  endpoints produtos -----------------------------
app.MapProdutosEndpoints();



var environment = app.Environment;
app.UseExceptionHandling(environment)
    .UseSwaggerMiddleware()
    .UseAppCors();




app.UseAuthentication();
app.UseAuthorization();
app.Run();