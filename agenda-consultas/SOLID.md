# SOLID

## SRP (Single Responsibility Principle)

- Controllers apenas recebem requisicoes e devolvem respostas.
- Services concentram regras de negocio.
- Repositories isolam o acesso ao MongoDB.
- Exemplos: backend/AgendaConsultas.Api/Controllers, backend/AgendaConsultas.Api/Services, backend/AgendaConsultas.Api/Repositories.

## DIP (Dependency Inversion Principle)

- Services dependem de interfaces, nao de implementacoes concretas.
- Exemplo: IPacienteService usa IPacienteRepository em backend/AgendaConsultas.Api/Services/PacienteService.cs.

## ISP (Interface Segregation Principle)

- Interfaces separadas por responsabilidade (paciente, consulta, usuario).
- Exemplo: backend/AgendaConsultas.Api/Repositories/IPacienteRepository.cs e IConsultaRepository.cs.
