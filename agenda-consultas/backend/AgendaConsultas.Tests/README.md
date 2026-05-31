# AgendaConsultas.Tests

Este projeto contem testes unitarios da camada de servicos da API. Ele nao usa MongoDB, pois os repositorios sao simulados em memoria para testar apenas as regras de negocio.

## O que e testado

PacienteService

- cria paciente com dados validos
- falha ao criar paciente com email invalido
- falha ao criar paciente com email duplicado
- falha ao criar paciente com CPF duplicado
- falha ao atualizar paciente inexistente
- falha ao remover paciente inexistente
- atualiza dados do paciente

ConsultaService

- cria consulta para paciente existente
- falha ao criar consulta quando o paciente nao existe
- falha ao criar consulta fora do horario padrao
- falha ao criar consulta quando o horario ja esta ocupado
- permite criar consulta quando a anterior esta cancelada
- preenche campo dataBrasil (horario do Brasil)
- marca horarios ocupados no endpoint de slots

## Como funciona

1) O teste cria repositorios em memoria (listas internas).
2) O servico recebe esses repositorios via construtor.
3) O metodo do servico e executado.
4) O teste valida o resultado ou a excecao esperada.

Arquivo principal:

- UnitTest1.cs: testes e repositorios em memoria.

## Observacoes

- Horarios padronizados usam o fuso do Brasil e sao validados no service.
- O banco armazena data em UTC, mas a API gera dataBrasil com offset -03:00.

## Como executar

Na raiz do repositorio:

```
dotnet test AgendaConsultas.slnx
```

Somente os testes:

```
dotnet test backend/AgendaConsultas.Tests
```
