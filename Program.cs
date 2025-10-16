using BetControlAPI.Data;
using Microsoft.EntityFrameworkCore;

// Configuração do host da aplicação
var builder = WebApplication.CreateBuilder(args);

// Configuração dos serviços
// Adiciona suporte a controllers da API
builder.Services.AddControllers();

// Adiciona exploração de endpoints para documentação
builder.Services.AddEndpointsApiExplorer();

// Adiciona geração de documentação Swagger/OpenAPI
builder.Services.AddSwaggerGen();

// Configuração do Entity Framework com SQLite
// Connection string vem do appsettings.json
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Adiciona factory para HttpClient (usado na conversão de moeda)
builder.Services.AddHttpClient();

// Construção da aplicação
var app = builder.Build();

// Configuração do pipeline de middleware

// Habilita documentação Swagger (disponível em /swagger)
app.UseSwagger();

// Habilita interface Swagger UI
app.UseSwaggerUI();

// Mapeia controllers para rotas da API
app.MapControllers();

// Middleware customizado: redireciona raiz para documentação Swagger
// Facilita acesso à documentação da API
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/" || context.Request.Path == "/index.html")
    {
        context.Response.Redirect("/swagger");
        return;
    }
    await next();
});

// Inicia a aplicação
app.Run();
