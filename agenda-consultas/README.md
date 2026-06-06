# 🏥 Agenda de Consultas

> Sistema full stack para gestão de pacientes e consultas médicas, com autenticação JWT, controle de acesso por perfil e painel web responsivo.

![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)
![MongoDB](https://img.shields.io/badge/MongoDB-7-47A248?logo=mongodb)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker)
![JavaScript](https://img.shields.io/badge/Frontend-Vanilla%20JS-F7DF1E?logo=javascript)

---

## 📋 Sumário

- [Visão geral](#-visão-geral)
- [Stack](#-stack)
- [Estrutura do repositório](#-estrutura-do-repositório)
- [Funcionalidades](#-funcionalidades)
- [Permissões da API](#-permissões-da-api)
- [Como rodar](#-como-rodar)
- [Configuração](#️-configuração)
- [Exemplos de payload](#-exemplos-de-payload)
- [Horários e fuso horário](#-horários-e-fuso-horário)
- [Testes](#-testes)
- [Erros comuns](#-erros-comuns)

---

## 🔍 Visão geral

O **Agenda de Consultas** é uma aplicação web voltada para pequenas clínicas. Permite:

- **Cadastrar e autenticar usuários** com perfis `admin` e `usuario`
- **Gerenciar pacientes** com vínculo direto ao usuário do sistema
- **Agendar consultas** com controle de horários, especialidade e status
- **Painel administrativo** completo para admins; visão pessoal restrita para usuários comuns

---

## 🛠 Stack

| Camada | Tecnologia |
|---|---|
| Backend | ASP.NET Core Web API (.NET 10) |
| Banco de dados | MongoDB |
| Frontend | HTML + CSS + JavaScript (Fetch API) |
| Autenticação | JWT + BCrypt |
| Infraestrutura | Docker Compose |

---

## 📁 Estrutura do repositório

```
agenda-consultas/
├── backend/
│   ├── AgendaConsultas.Api/        # API REST + frontend estático servido
│   │   ├── Controllers/            # Endpoints HTTP
│   │   ├── Services/               # Regras de negócio
│   │   ├── Repositories/           # Acesso ao MongoDB
│   │   ├── Models/                 # Entidades do domínio
│   │   ├── Dtos/                   # Objetos de transferência
│   │   └── Settings/               # Configurações tipadas
│   └── AgendaConsultas.Tests/      # Testes unitários de serviços
├── frontend/
│   ├── index.html                  # SPA principal
│   ├── app.js                      # Lógica de UI e chamadas à API
│   └── styles.css                  # Estilização
├── docker-compose.yml              # MongoDB local
├── SOLID.md                        # Aplicação dos princípios SOLID
└── AgendaConsultas.slnx            # Solution file
```

---

## ✨ Funcionalidades

### 🔐 Autenticação

- Formulário único com dois modos: **Entrar** e **Criar conta**
- Registro pelo front sempre cria perfil `usuario`
- Admin pode promover qualquer usuário via `PUT /api/auth/role`

### 👨‍⚕️ Perfil `admin`

- Visualiza **todos** os pacientes e consultas
- Cria pacientes vinculando a um usuário existente **ou** criando um novo usuário no mesmo fluxo
- Cria, edita e remove consultas
- Lista, remove e atualiza o perfil de usuários

### 👤 Perfil `usuario`

- Visualiza **apenas** o próprio paciente e as próprias consultas
- Agenda consultas para si mesmo na aba **Pacientes**
- Não tem acesso a formulários e modais administrativos

### 📅 Agendamento

- Slots disponíveis: `08:00`, `09:00`, `10:00`, `11:00`, `13:00`, `14:00`, `15:00`, `16:00`, `17:00`
- Consulta em tempo real de horários disponíveis via `GET /api/consultas/slots?date=yyyy-MM-dd`
- Status possíveis: `agendada`, `concluida`, `cancelada`

---

## 🔒 Permissões da API

| Endpoint | Admin | Usuário |
|---|:---:|:---:|
| `POST /api/auth/register` | ✅ | ✅ |
| `POST /api/auth/login` | ✅ | ✅ |
| `PUT /api/auth/role` | ✅ | ❌ |
| `GET /api/pacientes` | todos | somente o próprio |
| `GET /api/pacientes/{id}` | ✅ | somente o próprio |
| `POST /api/pacientes` | ✅ | ✅ (email forçado pelo token) |
| `PUT /api/pacientes/{id}` | ✅ | ❌ |
| `DELETE /api/pacientes/{id}` | ✅ | ❌ |
| `GET /api/consultas` | todas | somente as próprias |
| `GET /api/consultas/paciente/{id}` | ✅ | somente o próprio |
| `POST /api/consultas` | ✅ | ✅ (somente para si) |
| `PUT /api/consultas/{id}` | ✅ | ❌ |
| `DELETE /api/consultas/{id}` | ✅ | ❌ |
| `GET /api/consultas/slots` | ✅ | ✅ (anônimo também) |
| `GET /api/usuarios` | ✅ | ❌ |
| `DELETE /api/usuarios/{id}` | ✅ | ❌ |

---

## 🚀 Como rodar

### Pré-requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (ou MongoDB local na porta `27017`)

### Passo a passo

**1. Clone o repositório**

```powershell
git clone <url-do-repositorio>
cd agenda-consultas
```

**2. Suba o MongoDB com Docker**

```powershell
docker-compose up -d
```

**3. Inicie a API**

```powershell
dotnet run --project backend/AgendaConsultas.Api
```

**4. Acesse no navegador**

| Interface | URL |
|---|---|
| Frontend | http://localhost:5000/ |
| Swagger (dev) | http://localhost:5000/swagger |

---

## 🧾 Versionamento da API e Swagger (V1 + V2)

A API expõe duas versões no Swagger UI (`/swagger`), coexistindo lado a lado:

- **V1**: versão anterior, preservada para compatibilidade.
- **V2**: versão atual com melhorias.

No seletor do Swagger você verá:

- `Agenda Consultas API v1` → `/swagger/v1/swagger.json`
- `Agenda Consultas API v2` → `/swagger/v2/swagger.json`

### Melhorias aplicadas nesta entrega

- Implementado versionamento formal com rotas:
  - `api/v1/...`
  - `api/v2/...`
- Mantida compatibilidade da V1 para os fluxos legados.
- Criados controllers V2 para:
  - Auth
  - Pacientes
  - Consultas
  - Usuários
- Melhorias de Usuários na V2:
  - `GET /api/v2/usuarios/{id}`
  - `PUT /api/v2/usuarios/{id}`
  - `PATCH /api/v2/usuarios/{id}`
  - além de `GET /api/v2/usuarios` e `DELETE /api/v2/usuarios/{id}`

### Melhorias no Frontend

- Versionamento automático de chamadas `/api/...` para `/api/{versão}/...` (default: `v2`).
- Correção para evitar requests protegidas sem login.
- Abas protegidas corretamente no estado deslogado.
- Ajuste da aba **Consultas**:
  - Admin visualiza todas as consultas e não vê formulário de marcação.
  - Usuário comum visualiza/agenda apenas no contexto do próprio paciente selecionado.

## ⚙️ Configuração

As configurações padrão ficam em `appsettings.json`:

| Chave | Valor padrão |
|---|---|
| `Mongo:ConnectionString` | `mongodb://localhost:27017` |
| `Mongo:DatabaseName` | `AgendaConsultasDb` |
| `Jwt:Issuer` | `AgendaConsultas.Api` |
| `Jwt:Audience` | `AgendaConsultas.Api` |
| `Jwt:ExpirationMinutes` | `60` |

### Chave JWT

Em **desenvolvimento**, a chave JWT já vem definida em `appsettings.Development.json`.

Em **produção**, defina via variável de ambiente:

```powershell
# PowerShell
$env:Jwt__Key="sua-chave-secreta-com-32-ou-mais-caracteres"
```

```bash
# Linux / macOS
export Jwt__Key="sua-chave-secreta-com-32-ou-mais-caracteres"
```

---

## 📦 Exemplos de payload

### Cadastro de usuário

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

### Criar paciente (admin — criando usuário junto)

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

> **Dica:** para vincular a um usuário já existente, envie apenas o `email` (sem `password`).

### Agendar consulta

`POST /api/consultas`

```json
{
  "pacienteId": "ID_DO_PACIENTE",
  "data": "2026-06-10T08:00:00",
  "especialidade": "Clínico Geral",
  "status": "agendada"
}
```

### Atualizar perfil de usuário (admin)

`PUT /api/auth/role`

```json
{
  "email": "usuario@email.com",
  "role": "admin"
}
```

---

## 🕐 Horários e fuso horário

- **Horários disponíveis:** `08:00`, `09:00`, `10:00`, `11:00`, `13:00`, `14:00`, `15:00`, `16:00`, `17:00`
- A API recebe datas no fuso `America/Sao_Paulo` e armazena em **UTC** no banco
- O campo `dataBrasil` é gerado automaticamente para exibição no frontend
- Consultar slots livres: `GET /api/consultas/slots?date=2026-06-10`

---

## 🧪 Testes

**Todos os projetos da solution:**

```powershell
dotnet test AgendaConsultas.slnx
```

**Somente testes unitários:**

```powershell
dotnet test backend/AgendaConsultas.Tests
```

> Mais detalhes sobre os casos de teste: [`backend/AgendaConsultas.Tests/README.md`](backend/AgendaConsultas.Tests/README.md)

---

## ⚠️ Erros comuns

| Código | Causa provável |
|---|---|
| `400 Bad Request` | Cadastro sem `nome`, `cpf` ou `telefone` para perfil `usuario` |
| `400 Bad Request` | CPF ou telefone fora do formato esperado |
| `400 Bad Request` | Criação de paciente com email novo mas sem `password` |
| `400 Bad Request` | Horário de consulta fora dos slots permitidos |
| `401 Unauthorized` | Token ausente, inválido ou expirado |
| `403 Forbidden` | Usuário tentando acessar dados de outro paciente |

---

## 📐 Princípios SOLID

A aplicação foi desenvolvida aplicando os princípios SOLID. Veja o resumo detalhado em [`SOLID.md`](SOLID.md).
