# API de Creditos Constituidos - Versao 2.0 com CQRS

## Novidades da Versao 2.0

Esta versao implementa CQRS (Command Query Responsibility Segregation) e diversos Design Patterns avancados.

## Arquitetura CQRS

### O que mudou?

**Versao 1.0 (Tradicional)**
```
Controller -> Service -> Repository -> Database
```

**Versao 2.0 (CQRS)**
```
Controller -> MediatR -> Handler -> Repository -> Database
                |
                +-> Command/Query
```

### Beneficios

1. **Separacao de Responsabilidades** - Leitura e escrita independentes
2. **Escalabilidade** - Pode escalar cada lado separadamente
3. **Testabilidade** - Handlers podem ser testados isoladamente
4. **Manutenibilidade** - Codigo mais organizado
5. **Performance** - Otimizacoes especificas

## Novos Componentes

### Commands (Escrita)

- `IntegrarCreditosCommand` - Integra lista de creditos
- `ProcessarCreditoCommand` - Processa credito individual

### Queries (Leitura)

- `GetCreditoByNumeroQuery` - Busca credito por numero
- `GetCreditosByNfseQuery` - Busca creditos por NFS-e

### Handlers

Cada Command/Query tem seu Handler dedicado:
- `IntegrarCreditosCommandHandler`
- `ProcessarCreditoCommandHandler`
- `GetCreditoByNumeroQueryHandler`
- `GetCreditosByNfseQueryHandler`

### Validators

FluentValidation para validacao robusta:
- `IntegrarCreditosCommandValidator`
- `CreditoDtoValidator`

## Design Patterns Implementados

### 1. CQRS Pattern
Separacao de comandos e consultas

### 2. Mediator Pattern
MediatR para desacoplar componentes

### 3. Repository Pattern
Abstracao de acesso a dados

### 4. Unit of Work Pattern
Gerenciamento de transacoes

### 5. Specification Pattern
Logica de consulta reutilizavel

### 6. Dependency Injection
Inversao de controle

### 7. Factory Pattern
Criacao de objetos via DI

### 8. Singleton Pattern
ServiceBusService como singleton

### 9. DTO Pattern
Separacao de modelos

## Principios SOLID

### S - Single Responsibility
Cada classe tem uma unica responsabilidade

### O - Open/Closed
Aberto para extensao, fechado para modificacao

### L - Liskov Substitution
Interfaces substituiveis

### I - Interface Segregation
Interfaces especificas e coesas

### D - Dependency Inversion
Dependencias de abstracoes

## Estrutura de Pastas

```
CreditoAPI/
├── Application/
│   ├── Commands/           # CQRS Commands
│   ├── Queries/            # CQRS Queries
│   ├── Handlers/           # Command/Query Handlers
│   └── Validators/         # FluentValidation
├── Infrastructure/
│   ├── UnitOfWork/         # Unit of Work Pattern
│   └── Specifications/     # Specification Pattern
├── Controllers/            # API Controllers
├── Models/                 # Domain Entities
├── DTOs/                   # Data Transfer Objects
├── Repositories/           # Repository Pattern
├── Services/               # Business Services
└── BackgroundServices/     # Background Workers
```

## Tecnologias Adicionadas

- **MediatR 12.2.0** - Implementacao do Mediator Pattern
- **FluentValidation 11.9.0** - Validacao expressiva
- **.NET 8.0** - Framework atualizado
- **Entity Framework Core 8.0** - ORM atualizado

## Como Usar

### Exemplo de Command

```csharp
// Controller
var command = new IntegrarCreditosCommand(creditos);
var result = await _mediator.Send(command);
```

### Exemplo de Query

```csharp
// Controller
var query = new GetCreditoByNumeroQuery(numeroCredito);
var credito = await _mediator.Send(query);
```

## Executar o Projeto

### Docker
```bash
docker-compose up -d
```

### Local
```bash
dotnet restore
dotnet build
dotnet run
```

### Testes
```bash
cd CreditoAPI.Tests
dotnet test
```

## Endpoints

Todos os endpoints continuam os mesmos:

- POST /api/creditos/integrar-credito-constituido
- GET /api/creditos/{numeroNfse}
- GET /api/creditos/credito/{numeroCredito}
- GET /api/self
- GET /api/ready

## Swagger

Acesse a documentacao interativa:
```
http://localhost:5000/swagger
```

## Documentacao Adicional

- **CQRS_AND_PATTERNS.md** - Detalhes dos padroes implementados
- **ARCHITECTURE.md** - Arquitetura do sistema
- **README.md** - Documentacao principal

## Comparacao de Versoes

| Aspecto | v1.0 | v2.0 |
|---------|------|------|
| Arquitetura | Tradicional | CQRS |
| Acoplamento | Medio | Baixo |
| Testabilidade | Boa | Excelente |
| Escalabilidade | Boa | Excelente |
| Manutenibilidade | Boa | Excelente |
| Design Patterns | 6 | 9 |
| SOLID | Sim | Sim (Avancado) |

## Melhorias Futuras

- Event Sourcing
- Saga Pattern
- Circuit Breaker
- Retry Policy com Polly
- Caching com Redis
- API Gateway

## Conclusao

A versao 2.0 eleva o projeto a um nivel enterprise com arquitetura CQRS, multiplos Design Patterns e conformidade total com SOLID.

---

Desenvolvido com as melhores praticas de arquitetura de software.
