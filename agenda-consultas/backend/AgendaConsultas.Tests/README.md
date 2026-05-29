# AgendaConsultas.Tests

Este projeto contem testes unitarios da camada de servicos da API. Ele nao usa MongoDB, pois os repositorios sao simulados em memoria para testar apenas as regras de negocio.

## O que e testado

PacienteService

- cria paciente com dados validos
- falha ao criar paciente com email invalido

ConsultaService

- cria consulta para paciente existente
- falha ao criar consulta quando o paciente nao existe

## Como funciona

1) O teste cria repositorios em memoria (listas internas).
2) O servico recebe esses repositorios via construtor.
3) O metodo do servico e executado.
4) O teste valida o resultado ou a excecao esperada.

Arquivo principal:

- UnitTest1.cs: testes e repositorios em memoria.

## Como executar

Na raiz do repositorio:

```
dotnet test AgendaConsultas.slnx
```

Somente os testes:

```
dotnet test backend/AgendaConsultas.Tests
```
