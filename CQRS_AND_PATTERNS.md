# CQRS e Design Patterns Implementados

## Visao Geral

Este documento descreve todos os padroes de projeto e principios SOLID implementados no projeto CreditoAPI.

## CQRS (Command Query Responsibility Segregation)

### O que e CQRS?

CQRS e um padrao arquitetural que separa operacoes de leitura (Queries) de operacoes de escrita (Commands).

### Implementacao no Projeto

#### Commands (Escrita)

Localizacao: Application/Commands/

1. IntegrarCreditosCommand - Integra lista de creditos
2. ProcessarCreditoCommand - Processa credito individual

#### Queries (Leitura)

Localizacao: Application/Queries/

1. GetCreditoByNumeroQuery - Busca credito por numero
2. GetCreditosByNfseQuery - Busca creditos por NFS-e

#### Handlers

Localizacao: Application/Handlers/

Cada Command e Query tem seu proprio Handler que implementa a logica de negocio.

### Beneficios do CQRS

- Separacao de Responsabilidades
- Escalabilidade
- Manutenibilidade
- Testabilidade
- Performance

## MediatR Pattern

MediatR implementa o padrao Mediator, que reduz o acoplamento entre componentes.

### Uso no Controller

```csharp
// Antes (sem CQRS)
var result = await _creditoService.IntegrarCreditosAsync(creditos);

// Depois (com CQRS + MediatR)
var command = new IntegrarCreditosCommand(creditos);
var result = await _mediator.Send(command);
```

## Design Patterns Implementados

### 1. Repository Pattern

Localizacao: Repositories/

Abstrai a camada de acesso a dados.

```csharp
public interface ICreditoRepository
{
    Task<Credito?> GetByNumeroCreditoAsync(string numeroCredito);
    Task<List<Credito>> GetByNumeroNfseAsync(string numeroNfse);
    Task<Credito> AddAsync(Credito credito);
    Task<bool> ExistsAsync(string numeroCredito);
}
```

### 2. Unit of Work Pattern

Localizacao: Infrastructure/UnitOfWork/

Gerencia transacoes e coordena multiplos repositorios.

```csharp
public interface IUnitOfWork : IDisposable
{
    ICreditoRepository Creditos { get; }
    Task<int> CommitAsync();
    Task RollbackAsync();
}
```

### 3. Specification Pattern

Localizacao: Infrastructure/Specifications/

Encapsula logica de consulta reutilizavel.

```csharp
public class CreditoByNumeroSpecification : BaseSpecification<Credito>
{
    public CreditoByNumeroSpecification(string numeroCredito)
        : base(c => c.NumeroCredito == numeroCredito)
    {
    }
}
```

### 4. Mediator Pattern

Implementado via MediatR para CQRS.

### 5. Dependency Injection Pattern

Todas as dependencias sao injetadas via construtor.

### 6. Factory Pattern

Implementado implicitamente via DI Container.

### 7. Singleton Pattern

ServiceBusService e registrado como Singleton.

### 8. DTO Pattern

Separacao entre modelos de dominio e transferencia de dados.

## Principios SOLID

### S - Single Responsibility Principle

Cada classe tem uma unica responsabilidade:
- Handlers: Processam um unico comando ou query
- Repositories: Acesso a dados de uma entidade
- Controllers: Apenas roteamento de requisicoes

### O - Open/Closed Principle

Classes abertas para extensao, fechadas para modificacao:
- Specifications podem ser estendidas sem modificar codigo existente
- Novos handlers podem ser adicionados sem alterar existentes

### L - Liskov Substitution Principle

Implementacoes podem ser substituidas por suas interfaces:
- ICreditoRepository pode ter multiplas implementacoes
- IUnitOfWork pode ser substituido por mock em testes

### I - Interface Segregation Principle

Interfaces especificas e coesas:
- ISpecification - Apenas logica de consulta
- IUnitOfWork - Apenas gerenciamento de transacoes
- ICreditoRepository - Apenas operacoes de credito

### D - Dependency Inversion Principle

Dependencias de abstracoes, nao de implementacoes:
- Controllers dependem de IMediator
- Handlers dependem de ICreditoRepository
- Services dependem de interfaces

## FluentValidation

Localizacao: Application/Validators/

Validacao robusta e expressiva de comandos.

```csharp
public class IntegrarCreditosCommandValidator : AbstractValidator<IntegrarCreditosCommand>
{
    public IntegrarCreditosCommandValidator()
    {
        RuleFor(x => x.Creditos)
            .NotNull()
            .NotEmpty();
            
        RuleForEach(x => x.Creditos)
            .SetValidator(new CreditoDtoValidator());
    }
}
```

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
├── Controllers/            # API Controllers (thin)
├── Models/                 # Domain Entities
├── DTOs/                   # Data Transfer Objects
├── Repositories/           # Repository Pattern
├── Services/               # Business Services
└── BackgroundServices/     # Background Workers
```

## Fluxo de Requisicao

### Command Flow (Escrita)

1. Controller recebe requisicao
2. Cria Command
3. Envia Command via MediatR
4. Handler processa Command
5. Repository persiste dados
6. Retorna resultado

### Query Flow (Leitura)

1. Controller recebe requisicao
2. Cria Query
3. Envia Query via MediatR
4. Handler processa Query
5. Repository busca dados
6. Retorna DTOs

## Beneficios da Arquitetura

1. Codigo Limpo e Organizado
2. Alta Testabilidade
3. Baixo Acoplamento
4. Alta Coesao
5. Facil Manutencao
6. Escalabilidade
7. Extensibilidade
8. Conformidade com SOLID

## Proximos Passos

- Implementar Caching com Redis
- Adicionar Event Sourcing
- Implementar Saga Pattern para transacoes distribuidas
- Adicionar Circuit Breaker Pattern
- Implementar Retry Policy com Polly

---

Desenvolvido seguindo as melhores praticas de arquitetura de software.
