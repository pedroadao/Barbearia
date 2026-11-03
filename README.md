# Barbearia API

API REST para gerenciamento de uma barbearia, desenvolvida para fins de estudo com .NET 10. Permite o cadastro de barbeiros e clientes e a alocação de clientes em horários disponíveis na agenda dos barbeiros.

## Tech Stack

- .NET 10 SDK / C# 14
- ASP.NET Core Web API (Controllers)
- Entity Framework Core 10 + Npgsql (PostgreSQL)
- FluentValidation
- Swagger / OpenAPI
- xUnit + FluentAssertions (testes)
- Docker / Docker Compose

## Arquitetura

```
src/
├── Core/
│   ├── Domain/           # Entidades, Result/Error, constantes de domínio
│   └── Application/      # DTOs e mapeamentos para DTO
├── Infrastructure/
│   ├── Dao/               # Acesso a dados (uma classe por entidade)
│   ├── Service/           # Regras de orquestração chamadas pelas Controllers
│   └── Mappings/          # Configuração do EF Core (IEntityTypeConfiguration)
│   └── Migrations/        # Migrations do EF Core
└── API/
    ├── Controllers/        # Endpoints HTTP
    └── Extensions/         # Mapeamento de Result -> IActionResult
tests/
└── Barbearia.Tests/        # Testes de Controller -> Service -> DAO -> EF Core
```

Fluxo de uma requisição: `Controller` → `Service` → `Dao` → `AppDbContext` (EF Core/Postgres).

### Modelo de dados

Existe uma única tabela `usuario`, usada tanto para **Barbeiro** quanto para **Cliente**. A diferenciação é feita pelo campo `IdTipoUsuario`, que referencia `tipo_usuario` (`Descricao` = "Barbeiro" ou "Cliente"). `Barbeiro` e `Cliente` são subclasses de `Usuario` em C#, mas não possuem mapeamento próprio no EF Core.

- `status` — Id, Descricao (ex.: Ativo/Inativo)
- `tipo_usuario` — Id, Descricao, IdStatus
- `usuario` — Id, Nome, Email, Telefone, Instagram, DataNascimento, IdTipoUsuario
- `calendario` — Id, IdBarbeiro, Data, HoraInicio, HoraFim, IdCliente (nulo = horário livre), Disponivel

## Como executar

### Com Docker (recomendado)

1. Copie `.env.developer` (ambiente de desenvolvimento) ou `.env` (configuração "de produção" local) para o arquivo lido pelo Compose, ou aponte o `--env-file` diretamente:

```bash
docker compose --env-file .env.developer up -d --build
```

2. A API fica disponível em `http://localhost:${API_PORT}` (porta definida no arquivo `.env` usado) e o Swagger em `/swagger`.

3. Para derrubar os containers:

```bash
docker compose down
```

### Localmente (sem Docker)

Requer um Postgres acessível e as variáveis de ambiente (ou `appsettings.Development.json`) configuradas com a connection string.

```bash
dotnet restore
dotnet build
dotnet ef database update --project src/Infrastructure --startup-project src/API
dotnet run --project src/API
```

## Variáveis de ambiente

| Variável | Descrição |
|---|---|
| `POSTGRES_DB` | Nome do banco de dados |
| `POSTGRES_USER` | Usuário do Postgres |
| `POSTGRES_PASSWORD` | Senha do Postgres |
| `POSTGRES_PORT` | Porta exposta do Postgres no host |
| `ASPNETCORE_ENVIRONMENT` | Ambiente do ASP.NET Core (ex.: `Development`) |
| `API_PORT` | Porta exposta da API no host |

`.env` e `.env.developer` não são versionados (estão no `.gitignore`); crie-os localmente a partir das variáveis acima.

## Endpoints principais

| Recurso | Rota | Métodos |
|---|---|---|
| Barbeiros | `/barbeiros` | `GET`, `GET /{id}`, `POST`, `PUT /{id}`, `DELETE /{id}` |
| Clientes | `/clientes` | `GET`, `GET /{id}`, `POST`, `PUT /{id}`, `DELETE /{id}` |
| Agendamentos | `/agendamentos` | `GET`, `GET /{id}`, `POST`, `DELETE /{id}` |

Documentação interativa disponível via Swagger em `/swagger` com a API em execução.

## Testes

```bash
dotnet test
```

Os testes cobrem o fluxo Controller → Service → DAO → EF Core usando o provider **InMemory** do EF Core, sem gravar dados permanentemente no Postgres real.

## Restrições do projeto

Este repositório segue as diretrizes definidas em `CLAUDE.md` (não versionado), incluindo: não alterar Migrations existentes, não adicionar pacotes NuGet sem autorização explícita, não expor Entidades de Domínio diretamente nos Endpoints, e tratar warnings como erros de build.
