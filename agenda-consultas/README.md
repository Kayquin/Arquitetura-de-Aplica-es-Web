# Agenda de Consultas

Aplicação full stack para gestão de pacientes e consultas, com JWT, MongoDB e controle de acesso por perfil (`admin` e `usuario`).

## Stack

- Backend: ASP.NET Core Web API (.NET 10)
- Banco: MongoDB
- Frontend: HTML + CSS + JavaScript (fetch)
- Autenticação: JWT + BCrypt

## Estrutura do repositório

- `backend/AgendaConsultas.Api`: API e frontend estático
- `backend/AgendaConsultas.Tests`: testes unitários de serviços
- `frontend`: arquivos do front (`index.html`, `app.js`, `styles.css`)
- `docker-compose.yml`: MongoDB local
- `SOLID.md`: resumo da aplicação dos princípios SOLID

## Fluxo funcional atual

### Autenticação no front

- Um formulário único com dois modos:
  - `Entrar`: pede `email` e `senha`
  - `Criar conta`: pede `email`, `senha`, `nome`, `cpf`, `telefone`
- No cadastro pelo front, o perfil é sempre `usuario`.

### Comportamento por perfil

- `admin`:
  - vê todos os pacientes e consultas
  - cria paciente (com usuário existente ou novo)
  - cria/remove consultas
  - lista/remove usuários
  - atualiza perfil de usuário
- `usuario`:
  - vê somente o próprio paciente
  - vê somente as próprias consultas
  - marca consulta para si na aba `Pacientes`
  - não vê formulários/modais administrativos

### Criação de paciente por admin

No modal `Novo paciente` (aba Pacientes), admin pode:

1. usar email de usuário já existente (sem senha)
2. informar email novo + senha (mínimo 6), e a API cria usuário `usuario` e paciente no mesmo fluxo

## Permissões da API

| Endpoint | Admin | Usuário |
|---|---|---|
| `POST /api/auth/register` | sim | sim |
| `POST /api/auth/login` | sim | sim |
| `PUT /api/auth/role` | sim | não |
| `GET /api/pacientes` | todos | somente o próprio |
| `GET /api/pacientes/{id}` | sim | somente se for o próprio |
| `POST /api/pacientes` | sim | sim (email forçado para o próprio token) |
| `PUT /api/pacientes/{id}` | sim | não |
| `DELETE /api/pacientes/{id}` | sim | não |
| `GET /api/consultas` | todas | somente as próprias |
| `GET /api/consultas/paciente/{id}` | sim | somente o próprio paciente |
| `POST /api/consultas` | sim | sim (somente para si) |
| `PUT /api/consultas/{id}` | sim | não |
| `DELETE /api/consultas/{id}` | sim | não |
| `GET /api/consultas/slots` | sim | sim (anônimo também) |
| `GET /api/usuarios` | sim | não |
| `DELETE /api/usuarios/{id}` | sim | não |

## Horários e fuso

- Horários padrão: `08:00`, `09:00`, `10:00`, `11:00`, `13:00`, `14:00`, `15:00`, `16:00`, `17:00`.
- A API recebe horário no contexto Brasil (`America/Sao_Paulo`) e salva em UTC.
- O campo `dataBrasil` é gerado para exibição no front.
- Slots: `GET /api/consultas/slots?date=yyyy-MM-dd`.

## Requisitos

- .NET SDK 10
- Docker Desktop (ou MongoDB local)

## Configuração

`appsettings.json` (padrão):

- `Mongo:ConnectionString = mongodb://localhost:27017`
- `Mongo:DatabaseName = AgendaConsultasDb`
- `Jwt:Issuer = AgendaConsultas.Api`
- `Jwt:Audience = AgendaConsultas.Api`
- `Jwt:ExpirationMinutes = 60`

Em desenvolvimento, a chave JWT padrão vem em `appsettings.Development.json`.
Em ambientes reais, use variável de ambiente:

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

### Cadastro de usuário (front)

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

### Criar paciente (admin, criando usuário novo junto)

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

Somente testes unitários:

```powershell
dotnet test backend/AgendaConsultas.Tests
```

Mais detalhes: `backend/AgendaConsultas.Tests/README.md`.

## Erros comuns

- `400 Bad Request`:
  - cadastro sem `nome/cpf/telefone` para perfil `usuario`
  - CPF ou telefone fora do formato esperado
  - criação de paciente com email novo sem senha
  - horário de consulta fora dos horários padrão
- `401 Unauthorized`: token ausente, inválido ou expirado
- `403 Forbidden`: usuário tentando acessar dados de outro paciente
