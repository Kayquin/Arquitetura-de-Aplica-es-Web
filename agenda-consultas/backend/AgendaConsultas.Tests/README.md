# AgendaConsultas.Tests

Testes unitarios da camada de servicos da API, focados em regra de negocio.
Nao usa MongoDB real: repositorios sao simulados em memoria.

## Escopo coberto

### `PacienteService`

- cria paciente com dados validos
- bloqueia email invalido
- bloqueia email duplicado
- bloqueia CPF duplicado
- bloqueia update de paciente inexistente
- bloqueia delete de paciente inexistente
- atualiza paciente existente

### `ConsultaService`

- cria consulta para paciente existente
- bloqueia criacao sem paciente valido
- bloqueia horario fora do padrao
- bloqueia horario ja ocupado
- permite horario quando consulta anterior esta cancelada
- preenche `dataBrasil` no formato esperado
- marca slots ocupados na consulta de disponibilidade

## Estrategia dos testes

1. Arrange: instancia repositorios em memoria e service.
2. Act: executa o metodo alvo.
3. Assert: valida retorno ou excecao esperada.

Arquivo principal: `UnitTest1.cs`.

## Observacoes importantes

- Horarios seguem o fuso do Brasil (`America/Sao_Paulo` / `E. South America Standard Time`).
- Persistencia de `data` ocorre em UTC.
- `dataBrasil` e campo auxiliar de exibicao.

## Como executar

Na raiz do repositorio:

```powershell
dotnet test AgendaConsultas.slnx
```

Somente este projeto:

```powershell
dotnet test backend/AgendaConsultas.Tests
```
