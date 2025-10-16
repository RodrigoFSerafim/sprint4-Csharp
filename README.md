BetControlAPI

Uma API REST em ASP.NET Core para controle de apostas por usuário, com limites mensais, estatísticas e integração a serviço externo de câmbio. O projeto usa .NET 9, Entity Framework Core e SQLite, e expõe documentação interativa via Swagger.

Sumário
- Visão geral e arquitetura
- Requisitos
- Configuração e execução
- Modelos de dados
- Endpoints
- Regras de negócio e integrações
- Exemplos de requisições
- Migrações e banco de dados
- Deploy e produção

Visão geral e arquitetura
- Stack: ASP.NET Core Web API (Minimal Hosting), EF Core, SQLite.
- Camadas principais:
  - Controllers em `Controllers/` (`Usuarios`, `Apostas`, `Limites`).
  - Modelos em `Models/` (`Usuario`, `Aposta`, `Limite`).
  - Contexto de dados em `Data/AppDbContext.cs`.
- Documentação interativa: Swagger UI habilitado em `/swagger`. A raiz (`/`) redireciona para o Swagger.

Requisitos
- .NET SDK 9.0+
- (Opcional) EF Core CLI para comandos de migração: `dotnet tool install --global dotnet-ef`

Configuração e execução
1) Clonar o repositório e entrar na pasta do projeto.
2) Verificar a connection string "DefaultConnection" em `appsettings.json` (SQLite padrão: arquivo `betcontrol.db` na raiz).
3) Restaurar e executar:
   - `dotnet restore`
   - `dotnet run`
4) Acessar a documentação: `http://localhost:5000/swagger` (ou a porta exibida no console).

Modelos de dados
- Usuario (`Models/Usuario.cs`)
  - `Id` (int)
  - `Nome` (string, obrigatório, até 120)
  - `Email` (string, obrigatório, formato e único, até 160)
  - `Saldo` (decimal, ≥ 0)
  - Relacionamentos: muitas `Apostas`, muitos `Limites`

- Aposta (`Models/Aposta.cs`)
  - `Id` (int)
  - `UsuarioId` (int, FK)
  - `Valor` (decimal, ≥ 0.01)
  - `Tipo` (string, obrigatório, até 60) — ex.: futebol, cassino
  - `Data` (DateTime, default UTC now)
  - `Ganhou` (bool)

- Limite (`Models/Limite.cs`)
  - `Id` (int)
  - `UsuarioId` (int, FK)
  - `ValorMaximoMensal` (decimal, ≥ 0)
  - `ValorAtual` (decimal, ≥ 0)
  - `MesReferencia` (string, obrigatório, formato `yyyy-MM`)
  - Índice único composto: (`UsuarioId`, `MesReferencia`)

Endpoints
- Usuarios (`/api/usuarios`)
  - GET `/` — listar usuários
  - GET `/{id}` — obter usuário por id
  - POST `/` — criar usuário
  - PUT `/{id}` — atualizar usuário
  - DELETE `/{id}` — remover usuário
  - GET `/excederam-limite/{mes}` — usuários cujo gasto no mês (`yyyy-MM`) excedeu o limite

- Apostas (`/api/apostas`)
  - GET `/` — listar apostas
  - GET `/{id}` — obter aposta por id
  - POST `/` — criar aposta
  - PUT `/{id}` — atualizar aposta
  - DELETE `/{id}` — remover aposta
  - GET `/media` — valor médio das apostas
  - GET `/acima-da-media` — apostas com valor acima da média
  - GET `/{id}/valor-usd` — converte o valor da aposta para USD via serviço externo

- Limites (`/api/limites`)
  - GET `/` — listar limites
  - GET `/{id}` — obter limite por id
  - POST `/` — criar limite
  - PUT `/{id}` — atualizar limite
  - DELETE `/{id}` — remover limite

Regras de negócio e integrações
- Atualização automática de `ValorAtual` em `Limite`:
  - Ao criar uma `Aposta`, o sistema identifica o mês de referência (`yyyy-MM`) a partir de `Aposta.Data` e soma o `Valor` ao `Limite` correspondente do usuário, se existir.
  - Ao atualizar uma `Aposta` dentro do mesmo mês/usuário, ajusta a diferença no `ValorAtual`.
  - Ao excluir uma `Aposta`, subtrai seu `Valor` do `ValorAtual` (não deixando negativo).
- Criação de `Limite`:
  - Se `MesReferencia` não informado, usa o mês atual em UTC (`yyyy-MM`).
  - Inicializa `ValorAtual` com a soma das apostas do usuário naquele mês.
- Restrições e índices:
  - `Email` de `Usuario` é único.
  - (`UsuarioId`, `MesReferencia`) de `Limite` é único.
- Integração externa (câmbio):
  - Endpoint `/api/apostas/{id}/valor-usd` consulta `exchangerate.host` para obter BRL→USD e retorna o valor convertido.

Exemplos de requisições (cURL)
Criar usuário
```
curl -X POST http://localhost:5000/api/usuarios \
  -H "Content-Type: application/json" \
  -d '{
    "nome": "Ana Silva",
    "email": "ana.silva@example.com",
    "saldo": 1000
  }'
```

Criar limite (mês atual por padrão)
```
curl -X POST http://localhost:5000/api/limites \
  -H "Content-Type: application/json" \
  -d '{
    "usuarioId": 1,
    "valorMaximoMensal": 500
  }'
```

Criar aposta
```
curl -X POST http://localhost:5000/api/apostas \
  -H "Content-Type: application/json" \
  -d '{
    "usuarioId": 1,
    "valor": 120.5,
    "tipo": "futebol",
    "data": "2025-10-15T12:00:00Z",
    "ganhou": false
  }'
```

Usuários que excederam o limite (para um mês)
```
curl http://localhost:5000/api/usuarios/excederam-limite/2025-10
```

Valor em USD de uma aposta
```
curl http://localhost:5000/api/apostas/1/valor-usd
```

Migrações e banco de dados
- Banco padrão: SQLite (`betcontrol.db`).
- Migrações existentes em `Data/Migrations/` (ex.: `InitialCreate`). Caso precise recriar/aplicar:
  - Adicionar nova migração: `dotnet ef migrations add <NomeDaMigracao>`
  - Atualizar banco local: `dotnet ef database update`
- Para resetar o banco local, delete `betcontrol.db` e execute `dotnet ef database update` novamente.

Deploy e produção
- Arquivos de configuração por ambiente: `appsettings.Development.json`, `appsettings.Production.json`.
- Publicação: pasta `publish/` contém binários gerados; utilize `dotnet publish -c Release` para gerar uma nova versão.
- O Swagger está habilitado por padrão; em produção, avalie restringir acesso conforme necessidade.

Notas
- A API usa UTC para datas de referência de mês.
- Endpoints retornam 400/404 conforme validações e 502 em falhas de serviços externos.


