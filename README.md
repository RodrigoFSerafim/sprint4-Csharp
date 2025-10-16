# BetControlAPI

**BetControlAPI** é uma API REST desenvolvida em **ASP.NET Core 9** para o **controle de apostas por usuário**, com **limites mensais**, **estatísticas automáticas** e **integração com um serviço externo de câmbio (exchangerate.host)**.
A aplicação utiliza **Entity Framework Core** com **SQLite** e oferece **documentação interativa via Swagger**.

---

## Integrantes da Equipe

| **Nome**                   | **RM**   |
|-----------------------------|----------|
| Rodrigo Fernandes Serafim  | RM550816 |
| João Antonio Rihan         | RM99656  |
| Adriano Lopes              | RM98574  |
| Henrique de Brito          | RM98831  |
| Rodrigo Lima               | RM98326  |

---

## ⚙️ Visão geral e arquitetura

**Stack principal:**

* ASP.NET Core Web API (Minimal Hosting)
* Entity Framework Core (EF Core)
* SQLite

**Camadas e estrutura:**

* **Controllers:** `Controllers/`
  (`UsuariosController`, `ApostasController`, `LimitesController`)
* **Modelos:** `Models/`
  (`Usuario`, `Aposta`, `Limite`)
* **Contexto de dados:** `Data/AppDbContext.cs`
* **Documentação:** Swagger UI habilitado em `/swagger`
  *(a rota raiz `/` redireciona automaticamente para o Swagger)*

---

## 🧾 Requisitos

* .NET SDK **9.0+**
* (Opcional) EF Core CLI:

  ```bash
  dotnet tool install --global dotnet-ef
  ```

---

## 🚀 Configuração e execução

1. **Clonar o repositório**

   ```bash
   git clone https://github.com/RodrigoFSerafim/sprint4-Csharp.git
   ```

2. **Verificar a connection string** `"DefaultConnection"` no arquivo `appsettings.json`
   *(por padrão, utiliza o banco `betcontrol.db` na raiz do projeto).*

3. **Restaurar dependências e executar**

   ```bash
   dotnet restore
   dotnet run
   ```

4. **Acessar a documentação interativa**

   ```
   http://localhost:5094/swagger
   ```

   *(ou na porta exibida no console)*

---

## 🧱 Modelos de dados

### **Usuario**

| Campo    | Tipo                               | Descrição                              |
| -------- | ---------------------------------- | -------------------------------------- |
| Id       | int                                | Identificador único                    |
| Nome     | string                             | Obrigatório, até 120 caracteres        |
| Email    | string                             | Obrigatório, único, até 160 caracteres |
| Saldo    | decimal                            | ≥ 0                                    |
| Relações | muitas `Apostas`, muitos `Limites` |                                        |

### **Aposta**

| Campo     | Tipo     | Descrição                      |
| --------- | -------- | ------------------------------ |
| Id        | int      | Identificador                  |
| UsuarioId | int (FK) | Usuário da aposta              |
| Valor     | decimal  | ≥ 0.01                         |
| Tipo      | string   | Obrigatório, até 60 caracteres |
| Data      | DateTime | Default: UTC Now               |
| Ganhou    | bool     | Indica se venceu               |

### **Limite**

| Campo             | Tipo                           | Descrição         |
| ----------------- | ------------------------------ | ----------------- |
| Id                | int                            | Identificador     |
| UsuarioId         | int (FK)                       | Usuário do limite |
| ValorMaximoMensal | decimal                        | ≥ 0               |
| ValorAtual        | decimal                        | ≥ 0               |
| MesReferencia     | string                         | `yyyy-MM`         |
| Índice único      | (`UsuarioId`, `MesReferencia`) |                   |

---

## 🌐 Endpoints principais

### **Usuarios** (`/api/usuarios`)

* `GET /` — lista usuários
* `GET /{id}` — busca usuário por ID
* `POST /` — cria novo usuário
* `PUT /{id}` — atualiza usuário
* `DELETE /{id}` — remove usuário
* `GET /excederam-limite/{mes}` — usuários que excederam o limite mensal (`yyyy-MM`)

### **Apostas** (`/api/apostas`)

* `GET /` — lista apostas
* `GET /{id}` — busca aposta por ID
* `POST /` — cria nova aposta
* `PUT /{id}` — atualiza aposta
* `DELETE /{id}` — remove aposta
* `GET /media` — calcula valor médio das apostas
* `GET /acima-da-media` — retorna apostas acima da média
* `GET /{id}/valor-usd` — converte valor da aposta para USD (via exchangerate.host)

### **Limites** (`/api/limites`)

* `GET /` — lista limites
* `GET /{id}` — busca limite por ID
* `POST /` — cria novo limite
* `PUT /{id}` — atualiza limite
* `DELETE /{id}` — remove limite

---

## 📊 Regras de negócio e integrações

* **Atualização automática do limite:**

    * Ao criar uma `Aposta`, o sistema soma o valor ao `Limite` do mês correspondente (`yyyy-MM`).
    * Ao atualizar uma `Aposta`, ajusta a diferença no `ValorAtual`.
    * Ao excluir, subtrai o valor (sem permitir negativo).

* **Criação de Limite:**

    * Se `MesReferencia` não for informado, assume o mês atual (UTC).
    * Inicializa `ValorAtual` com a soma das apostas do usuário no mês.

* **Restrições e índices:**

    * `Email` de `Usuario` é único.
    * (`UsuarioId`, `MesReferencia`) em `Limite` é único.

* **Integração externa (câmbio):**

    * Endpoint `/api/apostas/{id}/valor-usd` consulta `exchangerate.host` e retorna valor convertido de BRL→USD.

---

## 📡 Exemplos de requisições (cURL)

**Criar usuário**

```bash
curl -X POST http://localhost:5094/api/usuarios \
  -H "Content-Type: application/json" \
  -d '{
    "nome": "Ana Silva",
    "email": "ana.silva@example.com",
    "saldo": 1000
  }'
```

**Criar limite (mês atual)**

```bash
curl -X POST http://localhost:5094/api/limites \
  -H "Content-Type: application/json" \
  -d '{
    "usuarioId": 1,
    "valorMaximoMensal": 500
  }'
```

**Criar aposta**

```bash
curl -X POST http://localhost:5094/api/apostas \
  -H "Content-Type: application/json" \
  -d '{
    "usuarioId": 1,
    "valor": 120.5,
    "tipo": "futebol",
    "data": "2025-10-15T12:00:00Z",
    "ganhou": false
  }'
```

**Listar usuários que excederam o limite**

```bash
curl http://localhost:5094/api/usuarios/excederam-limite/2025-10
```

**Consultar valor em USD de uma aposta**

```bash
curl http://localhost:5094/api/apostas/1/valor-usd
```

---

## 🗃️ Migrações e banco de dados

* **Banco padrão:** SQLite (`betcontrol.db`)
* **Migrações:** localizadas em `Data/Migrations/`

**Comandos úteis:**

```bash
dotnet ef migrations add <NomeDaMigracao>
dotnet ef database update
```

**Recriar o banco local:**

```bash
rm betcontrol.db
dotnet ef database update
```

---

## 📝 Notas

* Todas as datas usam **UTC**.
* Respostas seguem boas práticas HTTP:

    * `400` / `404` em erros de validação ou inexistência
    * `502` em falhas de serviço externo (exchangerate.host)
* Projeto ideal para estudos de **boas práticas REST**, **camadas limpas**, **integrações externas** e **migrações EF Core**.

