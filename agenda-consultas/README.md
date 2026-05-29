# Agenda de Consultas

Aplicacao web para agenda de consultas medicas. O dominio usa duas entidades relacionadas (Pacientes e Consultas). A API oferece CRUD completo, Swagger e autenticacao JWT com perfis admin/usuario.

## Funcionalidades

- CRUD de pacientes
- CRUD de consultas
- Registro e login com JWT
- Perfis admin e usuario
- Swagger para documentacao
- Frontend simples com navegacao assincrona

## Stack

- Backend: .NET 10 Web API
- Banco: MongoDB
- Frontend: HTML + JavaScript (fetch)

## Estrutura

- backend/AgendaConsultas.Api -> API
- backend/AgendaConsultas.Tests -> testes unitarios
- frontend -> pagina web
- docker-compose.yml -> MongoDB local

## Requisitos

- .NET SDK 10.0
- Docker Desktop (para MongoDB)

## Variaveis de ambiente (opcional)

Se quiser sobrescrever valores locais, use:

```
$env:Mongo__ConnectionString="mongodb://localhost:27017"
$env:Mongo__DatabaseName="AgendaConsultasDb"
$env:Jwt__Key="super-secret-key-32-chars-minimum!!"
$env:Jwt__Issuer="AgendaConsultas.Api"
$env:Jwt__Audience="AgendaConsultas.Api"
$env:Jwt__ExpirationMinutes="60"
```

Observacao: a chave JWT precisa ter 32+ caracteres.

## Passo a passo

1) Suba o MongoDB:

```
docker-compose up -d
```

2) Rode a API:

```
dotnet run --project backend/AgendaConsultas.Api
```

3) Acesse:

- Swagger: http://localhost:5000/swagger
- Frontend: http://localhost:5000/

## Fluxo rapido (Postman)

Base URL:

```
http://localhost:5000
```

1) Registrar admin

POST /api/auth/register

```
{
	"email": "admin@email.com",
	"password": "123456",
	"role": "admin"
}
```

2) Login

POST /api/auth/login

```
{
	"email": "admin@email.com",
	"password": "123456"
}
```

Guarde o token e use no header:

```
Authorization: Bearer <TOKEN>
```

3) Criar paciente

POST /api/pacientes

```
{
	"nome": "Ana Silva",
	"cpf": "12345678901",
	"telefone": "11999999999",
	"email": "ana@email.com"
}
```

4) Criar consulta

POST /api/consultas

```
{
	"pacienteId": "<ID_PACIENTE>",
	"data": "2026-05-28T14:00:00Z",
	"especialidade": "clinico",
	"status": "agendada"
}
```

## Endpoints principais

- /api/pacientes (CRUD completo)
- /api/consultas (CRUD completo)
- /api/auth/register
- /api/auth/login
- /api/auth/role (admin)
- /api/usuarios (admin)

## Regras de acesso

- Update e delete exigem role admin.
- Depois de atualizar role, o usuario deve fazer login novamente.

## Postman (lista completa)

### Auth - Register

- Metodo: POST
- URL: /api/auth/register

```
{
	"email": "admin@email.com",
	"password": "123456",
	"role": "admin"
}
```

### Auth - Login

- Metodo: POST
- URL: /api/auth/login

```
{
	"email": "admin@email.com",
	"password": "123456"
}
```

### Pacientes - Criar

- Metodo: POST
- URL: /api/pacientes

```
{
	"nome": "Ana Silva",
	"cpf": "12345678901",
	"telefone": "11999999999",
	"email": "ana@email.com"
}
```

### Pacientes - Listar

- Metodo: GET
- URL: /api/pacientes

### Pacientes - Atualizar (admin)

- Metodo: PUT
- URL: /api/pacientes/{id}
- Header: Authorization: Bearer <TOKEN>

```
{
	"nome": "Ana Silva",
	"cpf": "12345678901",
	"telefone": "11999999999",
	"email": "ana@email.com"
}
```

### Pacientes - Deletar (admin)

- Metodo: DELETE
- URL: /api/pacientes/{id}
- Header: Authorization: Bearer <TOKEN>

### Consultas - Criar

- Metodo: POST
- URL: /api/consultas

```
{
	"pacienteId": "<ID_PACIENTE>",
	"data": "2026-05-28T14:00:00Z",
	"especialidade": "clinico",
	"status": "agendada"
}
```

### Consultas - Listar por paciente

- Metodo: GET
- URL: /api/consultas/paciente/{pacienteId}

### Consultas - Atualizar (admin)

- Metodo: PUT
- URL: /api/consultas/{id}
- Header: Authorization: Bearer <TOKEN>

```
{
	"pacienteId": "<ID_PACIENTE>",
	"data": "2026-05-28T14:00:00Z",
	"especialidade": "clinico",
	"status": "concluida"
}
```

### Consultas - Deletar (admin)

- Metodo: DELETE
- URL: /api/consultas/{id}
- Header: Authorization: Bearer <TOKEN>

### Usuarios - Atualizar role (admin)

- Metodo: PUT
- URL: /api/auth/role
- Header: Authorization: Bearer <TOKEN>

```
{
	"email": "usuario@email.com",
	"role": "admin"
}
```

### Usuarios - Listar (admin)

- Metodo: GET
- URL: /api/usuarios
- Header: Authorization: Bearer <TOKEN>

### Usuarios - Deletar (admin)

- Metodo: DELETE
- URL: /api/usuarios/{id}
- Header: Authorization: Bearer <TOKEN>

## Testes

```
dotnet test AgendaConsultas.slnx
```

Detalhes dos testes:

- backend/AgendaConsultas.Tests/README.md

## Problemas comuns

- 415: verifique Content-Type = application/json no Postman.
- 401: token ausente ou gerado antes de trocar a chave JWT.
- 400: valide CPF (11 digitos) e email valido.
