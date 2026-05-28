# Agenda de Consultas

Aplicacao web simples para agenda de consultas medicas. O dominio usa duas entidades relacionadas: Pacientes e Consultas. A API oferece CRUD completo, documentacao Swagger e autenticacao JWT com perfis admin/usuario.

## Stack

- Backend: .NET 10 Web API
- Banco: MongoDB
- Frontend: HTML + JavaScript (fetch)

## Requisitos

- .NET SDK 10.0
- Docker (para MongoDB)

## Variaveis de ambiente

Exemplos (PowerShell):

```
$env:Mongo__ConnectionString="mongodb://localhost:27017"
$env:Mongo__DatabaseName="AgendaConsultasDb"
$env:Jwt__Key="CHANGE_ME_USE_ENV_VAR"
$env:Jwt__Issuer="AgendaConsultas.Api"
$env:Jwt__Audience="AgendaConsultas.Api"
$env:Jwt__ExpirationMinutes="60"
```

## Como executar

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

## Endpoints principais

- /api/pacientes (CRUD completo)
- /api/consultas (CRUD completo)
- /api/auth/register
- /api/auth/login

## Postman (exemplos de requests)

Defina a base URL como:

```
http://localhost:5000
```

### Auth - Register

- Metodo: POST
- URL: /api/auth/register
- Body (raw JSON):

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
- Body (raw JSON):

```
{
	"email": "admin@email.com",
	"password": "123456"
}
```

Copie o token e use no header:

```
Authorization: Bearer <TOKEN>
```

### Pacientes - Criar

- Metodo: POST
- URL: /api/pacientes
- Body (raw JSON):

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
- Body (raw JSON):

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
- Body (raw JSON):

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
- Body (raw JSON):

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
- Body (raw JSON):

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

## Regras de acesso

- Update e delete exigem role admin.
- O role pode ser definido no registro (usuario ou admin).

## Testes

```
dotnet test AgendaConsultas.slnx
```
