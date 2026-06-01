# Agenda de Consultas

Aplicacao full stack para gestao de pacientes e consultas, com JWT, MongoDB e controle de acesso por perfil (`admin` e `usuario`).

## Stack

- Backend: ASP.NET Core Web API (.NET 10)
- Banco: MongoDB
- Frontend: HTML + CSS + JavaScript (fetch)
- Auth: JWT + BCrypt

## Estrutura do repositorio

- `backend/AgendaConsultas.Api`: API e frontend estatico
- `backend/AgendaConsultas.Tests`: testes unitarios de servicos
- `frontend`: arquivos do front (`index.html`, `app.js`, `styles.css`)
- `docker-compose.yml`: MongoDB local
- `SOLID.md`: resumo da aplicacao dos principios SOLID

## Fluxo funcional atual

### Autenticacao no front

- Um formulario unico com dois modos:
  - `Entrar`: pede `email` e `senha`
  - `Criar conta`: pede `email`, `senha`, `nome`, `cpf`, `telefone`
- No cadastro pelo front, o perfil e sempre `usuario`.

### Comportamento por role

- `admin`:
  - ve todos os pacientes e consultas
  - cria paciente (com usuario existente ou novo)
  - cria/remove consultas
  - lista/remove usuarios
  - atualiza role de usuario
- `usuario`:
  - ve somente o proprio paciente
  - ve somente as proprias consultas
  - marca consulta para si na aba `Pacientes`
  - nao ve formularios/modais administrativos

### Criacao de paciente por admin

No modal `Novo paciente` (aba Pacientes), admin pode:

1. usar email de usuario ja existente (sem senha)
2. informar email novo + senha (minimo 6), e a API cria usuario `usuario` e paciente no mesmo fluxo

## Permissoes da API

| Endpoint | Admin | Usuario |
|---|---|---|
| `POST /api/auth/register` | sim | sim |
| `POST /api/auth/login` | sim | sim |
| `PUT /api/auth/role` | sim | nao |
| `GET /api/pacientes` | todos | somente o proprio |
| `GET /api/pacientes/{id}` | sim | somente se for o proprio |
| `POST /api/pacientes` | sim | sim (email forcado para o proprio token) |
| `PUT /api/pacientes/{id}` | sim | nao |
| `DELETE /api/pacientes/{id}` | sim | nao |
| `GET /api/consultas` | todas | somente as proprias |
| `GET /api/consultas/paciente/{id}` | sim | somente o proprio paciente |
| `POST /api/consultas` | sim | sim (somente para si) |
| `PUT /api/consultas/{id}` | sim | nao |
| `DELETE /api/consultas/{id}` | sim | nao |
| `GET /api/consultas/slots` | sim | sim (anonimo tambem) |
| `GET /api/usuarios` | sim | nao |
| `DELETE /api/usuarios/{id}` | sim | nao |

## Horarios e fuso

- Horarios padrao: `08:00`, `09:00`, `10:00`, `11:00`, `13:00`, `14:00`, `15:00`, `16:00`, `17:00`.
- A API recebe horario no contexto Brasil (`America/Sao_Paulo`) e salva em UTC.
- O campo `dataBrasil` e gerado para exibicao no front.
- Slots: `GET /api/consultas/slots?date=yyyy-MM-dd`.

## Requisitos

- .NET SDK 10
- Docker Desktop (ou MongoDB local)

## Configuracao

`appsettings.json` (padrao):

- `Mongo:ConnectionString = mongodb://localhost:27017`
- `Mongo:DatabaseName = AgendaConsultasDb`
- `Jwt:Issuer = AgendaConsultas.Api`
- `Jwt:Audience = AgendaConsultas.Api`
- `Jwt:ExpirationMinutes = 60`

Em desenvolvimento, a chave JWT padrao vem em `appsettings.Development.json`.
Em ambientes reais, use variavel de ambiente:

```powershell
$env:Jwt__Key="sua-chave-com-32-ou-mais-caracteres"
```

## Como rodar

1. Suba o MongoDB:

```powershell
docker-compose up -d
```

2. Rode a API:

```powershell
dotnet run --project backend/AgendaConsultas.Api
```

3. Abra:

- Frontend: `http://localhost:5000/`
- Swagger: `http://localhost:5000/swagger` (Development)

## Exemplos de payload

### Cadastro de usuario (front)

`POST /api/auth/register`

```json
{
  "email": "usuario@email.com",
  "password": "123456",
  "role": "usuario",
  "nome": "Ana Silva",
  "cpf": "12345678901",
  "telefone": "11999999999"
}
```

### Login

`POST /api/auth/login`

```json
{
  "email": "usuario@email.com",
  "password": "123456"
}
```

### Criar paciente (admin, criando usuario novo junto)

`POST /api/pacientes`

```json
{
  "nome": "Carlos Pereira",
  "cpf": "98765432100",
  "telefone": "11988887777",
  "email": "carlos@email.com",
  "password": "123456"
}
```

### Criar consulta

`POST /api/consultas`

```json
{
  "pacienteId": "ID_DO_PACIENTE",
  "data": "2026-06-01T08:00:00",
  "especialidade": "clinico",
  "status": "agendada"
}
```

## Testes

Rodar tudo:

```powershell
dotnet test AgendaConsultas.slnx
```

Somente testes unitarios:

```powershell
dotnet test backend/AgendaConsultas.Tests
```

Mais detalhes: `backend/AgendaConsultas.Tests/README.md`.

## Erros comuns

- `400 Bad Request`:
  - cadastro sem `nome/cpf/telefone` para role `usuario`
  - CPF ou telefone fora do formato esperado
  - criacao de paciente com email novo sem senha
  - horario de consulta fora dos horarios padrao
- `401 Unauthorized`: token ausente, invalido ou expirado
- `403 Forbidden`: usuario tentando acessar dados de outro paciente
