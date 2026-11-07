# Barbearia API

API REST para gerenciamento de uma barbearia, desenvolvida para fins de estudo com .NET 10. Permite o cadastro de barbeiros e clientes e a alocação de clientes em horários disponíveis na agenda dos barbeiros.

## Tech Stack

- .NET 10 SDK / C# 14
- ASP.NET Core Web API (Controllers)
- Entity Framework Core 10 + Npgsql (PostgreSQL)
- FluentValidation
- Autenticação JWT Bearer (OAuth 2.0/OIDC) via projeto `Authentication`
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
    ├── Authorization/      # Roles (ex.: Administrador)
    ├── Controllers/        # Endpoints HTTP
    └── Extensions/         # Mapeamento de Result -> IActionResult
tests/
└── Barbearia.Tests/        # Testes de Controller -> Service -> DAO -> EF Core
```

Fluxo de uma requisição: `Controller` → `Service` → `Dao` → `AppDbContext` (EF Core/Postgres).

### Modelo de dados

Existe uma única tabela `usuario`, usada tanto para **Barbeiro** quanto para **Cliente**. A diferenciação é feita pelo campo `IdTipoUsuario`, que referencia `tipo_usuario` (`Descricao` = "Barbeiro", "Cliente" ou "Administrador"). `Barbeiro` e `Cliente` são subclasses de `Usuario` em C#, mas não possuem mapeamento próprio no EF Core.

- `status` — Id, Descricao (ex.: Ativo/Inativo)
- `tipo_usuario` — Id, Descricao, IdStatus
- `usuario` — Id, Nome, Email, Telefone, Instagram, DataNascimento, IdTipoUsuario, AuthUserId (id do usuário no Authentication), MostrarCadastroSempre
- `calendario` — Id, IdBarbeiro, Data, HoraInicio, HoraFim, IdCliente (nulo = horário livre), Disponivel

## Autenticação e autorização

A API não emite tokens: ela valida JWTs emitidos pelo projeto **Authentication** (`../Authentication`), que roda em outro `docker-compose`. Todos os controllers exigem `[Authorize]`.

- Configuração em `Authentication:Authority`, `Authentication:Audience` (`authentication-api`) e, opcionalmente, `Authentication:Issuer` (URL pela qual o navegador acessa o Authentication, presente no `iss` do token; padrão = Authority).
- O vínculo entre o token e o cadastro local é feito pelo claim `sub`, gravado em `usuario.auth_user_id`.
- A policy `Administrador` exige a role `Administrador` no token e protege: `GET /clientes/{id}`, `DELETE /clientes/{id}` e `GET /barbeiros/{id}`, `POST`, `PUT` e `DELETE` de `/barbeiros`. As demais rotas aceitam qualquer usuário autenticado.
- CORS liberado para o frontend (`http://localhost:5174` e `http://localhost:8082`).
- O certificado de desenvolvimento do Authentication é autoassinado; não armazene segredos no repositório.

## Como executar

### Com Docker (recomendado)

O `docker-compose.yml` deste repositório sobe três serviços:

- `postgres` — banco de dados Postgres.
- `api` — a API .NET, construída a partir do `Dockerfile` deste repositório.
- `web` — o frontend React (repositório irmão `BarbeariaReact`, localizado em `../BarbeariaReact`), construído com Vite e servido via nginx.

1. Copie `.env.developer` (ambiente de desenvolvimento) ou `.env` (configuração "de produção" local) para o arquivo lido pelo Compose, ou aponte o `--env-file` diretamente:

```bash
docker compose --env-file .env.developer up -d --build
```

2. Serviços disponíveis após a subida:
   - API em `http://localhost:${API_PORT}` (Swagger em `/swagger`).
   - Frontend em `http://localhost:${WEB_PORT}`.

3. Para derrubar os containers:

```bash
docker compose down
```

> O serviço `web` espera encontrar o repositório `BarbeariaReact` no diretório irmão (`../BarbeariaReact`). Se o frontend não estiver clonado nesse local, remova ou ajuste o serviço `web` no `docker-compose.yml`, ou clone-o ao lado deste repositório antes de subir o Compose.

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
| `WEB_PORT` | Porta exposta do frontend (nginx) no host |

As configurações `Authentication__Authority`, `Authentication__Audience` e `Authentication__Issuer` são definidas diretamente no `docker-compose.yml`.

`.env` e `.env.developer` não são versionados (estão no `.gitignore`); crie-os localmente a partir das variáveis acima.

## Endpoints principais

| Recurso | Rota | Métodos |
|---|---|---|
| Barbeiros | `/barbeiros` | `GET`, `GET /{id}` (Admin), `POST` (Admin), `PUT /{id}` (Admin), `DELETE /{id}` (Admin) |
| Clientes | `/clientes` | `GET`, `GET /me`, `GET /{id}` (Admin), `POST`, `PUT /{id}`, `DELETE /{id}` (Admin) |
| Agendamentos | `/agendamentos` | `GET`, `GET /{id}`, `POST`, `DELETE /{id}` |

Todas as rotas exigem `Authorization: Bearer <token>`; "(Admin)" exige a role `Administrador`.

Documentação interativa disponível via Swagger em `/swagger` com a API em execução.

## Testes

```bash
dotnet test
```

Os testes cobrem o fluxo Controller → Service → DAO → EF Core usando o provider **InMemory** do EF Core, sem gravar dados permanentemente no Postgres real.
