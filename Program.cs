using CatalogoApi.Context;
using CatalogoApi.Models;
using CatalogoApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// registrando serviço de token
builder.Services.AddSingleton<ITokenService>(new TokenService());


// validação do token
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey
            (Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });


builder.Services.AddAuthorization();


var app = builder.Build();

// ----------------------- endpoint para login -------------------------------
app.MapPost("/login", [AllowAnonymous] (UserModel userModel, ITokenService tokenService) =>
{
    if (userModel is null)
    {
        return Results.BadRequest("Login inválido");
    }

    if(userModel.UserName == "macoratti" && userModel.Password == "numsey#123")
    {
        var tokenString = tokenService.GetToken(app.Configuration["Jwt:Key"],
            app.Configuration["Jwt:Issuer"],
            app.Configuration["Jwt:Audience"], userModel);

        return Results.Ok(new { token = tokenString });
    } else
    {
        return Results.BadRequest("Login inválido");
    }
}).Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status200OK)
.WithName("Login")
.WithTags("Autenticacao");

// -----------------------  endpoints categorias -----------------------------

app.MapPost("/categorias", async (Categoria categoria, AppDbContext db) =>
{
    db.Categorias.Add(categoria);
    await db.SaveChangesAsync();
    return Results.Created($"/categorias/{categoria.CategoriaId}", categoria);

});


app.MapGet("/categorias", async (AppDbContext db) => await db.Categorias.ToListAsync())
    .RequireAuthorization();

app.MapGet("/categorias/{id:int}", async (int id, AppDbContext db) =>
{
    return await db.Categorias.FindAsync(id)
     is Categoria categoria ? Results.Ok(categoria) : Results.NotFound();
});

app.MapPut("/categorias/{id:int}", async (int id, Categoria categoria, AppDbContext db) =>
{
    if(categoria.CategoriaId != id)
    {
        return Results.BadRequest();
    }

    var categoriaDb = await db.Categorias.FindAsync(id);
    if(categoriaDb == null) return Results.NotFound();

    categoriaDb.Nome = categoria.Nome;
    categoriaDb.Descricao = categoria.Descricao;
    
    await db.SaveChangesAsync();
    return Results.Ok(categoriaDb);
});

app.MapDelete("/categorias/{id:int}", async (int id, AppDbContext db) => {
    var categoria = await db.Categorias.FindAsync(id);
    if(categoria is null)
    {
        return Results.NotFound();
    }

    db.Categorias.Remove(categoria);
    await db.SaveChangesAsync();

    return Results.NoContent();
});


// -----------------------  endpoints produtos -----------------------------

app.MapPost("/produtos", async (Produto produto, AppDbContext db) =>
{
    db.Produtos.Add(produto);
    await db.SaveChangesAsync();

    return Results.Created($"/produtos/{produto.ProdutoId}", produto);
});

app.MapGet("/produtos", async (AppDbContext db) => await db.Produtos.ToListAsync())
    .RequireAuthorization();

app.MapGet("/produtos/{id:int}", async (int id, AppDbContext db) =>
{
    return await db.Produtos.FindAsync(id) 
        is Produto produto ? Results.Ok(produto) : Results.NotFound();
});

app.MapPut("/produtos/{id:int}", async (int id, Produto produto, AppDbContext db) =>
{
   if(produto.ProdutoId != id)
    {
        return Results.BadRequest();
    }

    var produtoDb = await db.Produtos.FindAsync(id);
    if (produtoDb is null) return Results.NotFound();

    produtoDb.Nome = produto.Nome;
    produtoDb.Descricao = produto.Descricao;
    produtoDb.Preco = produto.Preco;
    produtoDb.Imagem = produto.Imagem;
    produtoDb.DataCompra = produto.DataCompra;
    produtoDb.Estoque = produto.Estoque;
    produtoDb.CategoriaId = produto.CategoriaId;

    await db.SaveChangesAsync();

    return Results.Ok(produtoDb);
});


app.MapDelete("/produtos/{id:int}", async(int id, AppDbContext db) =>
{
    var produto = await db.Produtos.FindAsync(id);
    if(produto is null)
    {
        return Results.NotFound();
    }

    db.Produtos.Remove(produto);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

// ----------------------------------- fim endpoints ------------------------------


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseAuthentication();
app.UseAuthorization();
app.Run();