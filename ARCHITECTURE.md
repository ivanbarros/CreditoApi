# Arquitetura do Sistema - API de Créditos Constituídos

## 📐 Visão Geral da Arquitetura

O sistema foi desenvolvido seguindo os princípios de **Clean Architecture** e **Domain-Driven Design (DDD)**, com foco em separação de responsabilidades, testabilidade e manutenibilidade.

## 🏗️ Camadas da Aplicação

```
┌─────────────────────────────────────────────────────────┐
│                    Presentation Layer                    │
│                      (Controllers)                       │
├─────────────────────────────────────────────────────────┤
│                    Application Layer                     │
│                  (Services, DTOs, Mappers)              │
├─────────────────────────────────────────────────────────┤
│                      Domain Layer                        │
│                  (Models, Interfaces)                    │
├─────────────────────────────────────────────────────────┤
│                  Infrastructure Layer                    │
│         (Repositories, DbContext, External APIs)        │
└─────────────────────────────────────────────────────────┘
```

### 1. Presentation Layer (Controllers)

**Responsabilidade:** Receber requisições HTTP, validar entrada e retornar respostas.

**Componentes:**
- `CreditosController`: Endpoints para operações CRUD de créditos
- `HealthController`: Endpoints de health check

**Princípios:**
- Controllers finos (thin controllers)
- Validação de entrada
- Tratamento de exceções
- Retorno de status codes apropriados

### 2. Application Layer (Services)

**Responsabilidade:** Lógica de negócio e orquestração entre camadas.

**Componentes:**
- `CreditoService`: Lógica de negócio para créditos
- `ServiceBusService`: Integração com Azure Service Bus
- `CreditoProcessorService`: Background service para processamento assíncrono

**Princípios:**
- Regras de negócio centralizadas
- Orquestração de operações
- Mapeamento entre DTOs e entidades
- Logging estruturado

### 3. Domain Layer (Models)

**Responsabilidade:** Representar o domínio do negócio.

**Componentes:**
- `Credito`: Entidade principal do domínio
- `ICreditoRepository`: Interface do repositório
- `IServiceBusService`: Interface do serviço de mensageria
- `ICreditoService`: Interface do serviço de créditos

**Princípios:**
- Entidades ricas (quando aplicável)
- Interfaces para inversão de dependência
- Validações de domínio

### 4. Infrastructure Layer (Data Access)

**Responsabilidade:** Acesso a dados e integrações externas.

**Componentes:**
- `ApplicationDbContext`: Contexto do Entity Framework
- `CreditoRepository`: Implementação do repositório
- Migrações do banco de dados

**Princípios:**
- Abstração de acesso a dados
- Configuração de mapeamento ORM
- Gerenciamento de transações

## 🔄 Fluxo de Dados

### Fluxo de Integração de Créditos

```
┌──────────┐     ┌────────────┐     ┌──────────────┐     ┌──────────────┐
│  Client  │────▶│ Controller │────▶│   Service    │────▶│ Service Bus  │
└──────────┘     └────────────┘     └──────────────┘     └──────────────┘
                                            │
                                            ▼
                                     ┌──────────────┐
                                     │   Response   │
                                     │  (202 Accepted)│
                                     └──────────────┘
```

### Fluxo de Processamento em Background

```
┌──────────────┐     ┌──────────────────┐     ┌──────────────┐     ┌──────────┐
│ Service Bus  │────▶│ Background       │────▶│   Service    │────▶│Repository│
│              │     │ Service          │     │              │     │          │
└──────────────┘     └──────────────────┘     └──────────────┘     └──────────┘
                            │                                              │
                            │                                              ▼
                            │                                       ┌──────────┐
                            └──────────────────────────────────────▶│PostgreSQL│
                                    (Polling a cada 500ms)          └──────────┘
```

### Fluxo de Consulta

```
┌──────────┐     ┌────────────┐     ┌──────────────┐     ┌──────────┐     ┌──────────┐
│  Client  │────▶│ Controller │────▶│   Service    │────▶│Repository│────▶│PostgreSQL│
└──────────┘     └────────────┘     └──────────────┘     └──────────┘     └──────────┘
                        │                    │                    │              │
                        │                    │                    │              │
                        │◀───────────────────┴────────────────────┴──────────────┘
                        │
                        ▼
                 ┌──────────────┐
                 │   Response   │
                 │  (200 OK)    │
                 └──────────────┘
```

## 🎯 Padrões de Projeto Implementados

### 1. Repository Pattern

**Objetivo:** Abstrair a lógica de acesso a dados.

**Implementação:**
```csharp
public interface ICreditoRepository
{
    Task<Credito?> GetByNumeroCreditoAsync(string numeroCredito);
    Task<List<Credito>> GetByNumeroNfseAsync(string numeroNfse);
    Task<Credito> AddAsync(Credito credito);
    Task<bool> ExistsAsync(string numeroCredito);
}
```

**Benefícios:**
- Facilita testes unitários (mocking)
- Centraliza lógica de acesso a dados
- Permite trocar implementação sem afetar camadas superiores

### 2. Dependency Injection (DI)

**Objetivo:** Inversão de controle e baixo acoplamento.

**Implementação:**
```csharp
// Program.cs
builder.Services.AddScoped<ICreditoRepository, CreditoRepository>();
builder.Services.AddScoped<ICreditoService, CreditoService>();
builder.Services.AddSingleton<IServiceBusService, ServiceBusService>();
```

**Benefícios:**
- Facilita testes
- Reduz acoplamento
- Melhora manutenibilidade

### 3. Data Transfer Object (DTO)

**Objetivo:** Separar modelos de domínio de modelos de transferência.

**Implementação:**
```csharp
public class CreditoDto
{
    [JsonPropertyName("numeroCredito")]
    public string NumeroCredito { get; set; }
    // ... outros campos
}
```

**Benefícios:**
- Controle sobre dados expostos
- Versionamento de API facilitado
- Validação específica por endpoint

### 4. Background Service Pattern

**Objetivo:** Processar tarefas assíncronas em background.

**Implementação:**
```csharp
public class CreditoProcessorService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Processa mensagens
            await Task.Delay(_processingIntervalMs, stoppingToken);
        }
    }
}
```

**Benefícios:**
- Processamento assíncrono
- Não bloqueia requisições HTTP
- Escalável

### 5. Factory Pattern (Implícito via DI)

**Objetivo:** Criar instâncias de objetos de forma controlada.

**Implementação:**
```csharp
using (var scope = _serviceProvider.CreateScope())
{
    var service = scope.ServiceProvider.GetRequiredService<ICreditoService>();
}
```

### 6. Singleton Pattern

**Objetivo:** Garantir uma única instância de um serviço.

**Implementação:**
```csharp
builder.Services.AddSingleton<IServiceBusService, ServiceBusService>();
```

**Uso:** ServiceBusService mantém conexão única com Azure Service Bus.

## 🔐 Princípios SOLID

### Single Responsibility Principle (SRP)
Cada classe tem uma única responsabilidade:
- `CreditoRepository`: Acesso a dados
- `CreditoService`: Lógica de negócio
- `CreditosController`: Manipulação de requisições HTTP

### Open/Closed Principle (OCP)
Classes abertas para extensão, fechadas para modificação:
- Interfaces permitem novas implementações sem alterar código existente

### Liskov Substitution Principle (LSP)
Implementações podem ser substituídas por suas interfaces:
- `ICreditoRepository` pode ter múltiplas implementações

### Interface Segregation Principle (ISP)
Interfaces específicas e coesas:
- `ICreditoRepository` contém apenas métodos relacionados a créditos
- `IServiceBusService` contém apenas métodos de mensageria

### Dependency Inversion Principle (DIP)
Dependências de abstrações, não de implementações:
- Controllers dependem de `ICreditoService`, não de `CreditoService`
- Services dependem de `ICreditoRepository`, não de `CreditoRepository`

## 📊 Modelo de Dados

### Entidade Credito

```
┌─────────────────────────────────────┐
│            CREDITO                  │
├─────────────────────────────────────┤
│ PK │ id (BIGINT)                    │
│    │ numero_credito (VARCHAR 50)    │
│    │ numero_nfse (VARCHAR 50)       │
│    │ data_constituicao (DATE)       │
│    │ valor_issqn (DECIMAL 15,2)     │
│    │ tipo_credito (VARCHAR 50)      │
│    │ simples_nacional (BOOLEAN)     │
│    │ aliquota (DECIMAL 5,2)         │
│    │ valor_faturado (DECIMAL 15,2)  │
│    │ valor_deducao (DECIMAL 15,2)   │
│    │ base_calculo (DECIMAL 15,2)    │
└─────────────────────────────────────┘
         │
         │ Índices:
         ├─ idx_numero_credito
         └─ idx_numero_nfse
```

### Relacionamentos
- Não há relacionamentos diretos (tabela única)
- Índices otimizam consultas por `numero_credito` e `numero_nfse`

## 🔄 Processamento Assíncrono

### Estratégia de Mensageria

**Padrão:** Publish/Subscribe com Azure Service Bus

**Fluxo:**
1. API publica mensagens no tópico
2. Background service consome mensagens da subscription
3. Processamento individual (não bulk)
4. Verificação de duplicatas antes de inserir

**Configuração:**
- Intervalo de polling: 500ms
- Modo de recebimento: PeekLock (garante processamento)
- Tratamento de erros: Abandon message em caso de falha

### Garantias

- **At-least-once delivery**: Mensagens são processadas pelo menos uma vez
- **Idempotência**: Verificação de duplicatas evita inserções repetidas
- **Resiliência**: Mensagens com erro retornam à fila

## 🏥 Health Checks

### Tipos de Health Checks

1. **Self Check** (`/api/self`)
   - Verifica se a aplicação está rodando
   - Sempre retorna 200 OK se a API está ativa

2. **Ready Check** (`/api/ready`)
   - Verifica dependências (banco de dados)
   - Retorna 200 OK se tudo está saudável
   - Retorna 503 Service Unavailable se há problemas

### Uso em Kubernetes

```yaml
livenessProbe:
  httpGet:
    path: /api/self
    port: 80
  initialDelaySeconds: 30
  periodSeconds: 10

readinessProbe:
  httpGet:
    path: /api/ready
    port: 80
  initialDelaySeconds: 10
  periodSeconds: 5
```

## 🐳 Containerização

### Estratégia Multi-Stage Build

```dockerfile
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
# ... build steps

# Stage 2: Publish
FROM build AS publish
# ... publish steps

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
# ... runtime configuration
```

**Benefícios:**
- Imagem final menor (apenas runtime)
- Build reproduzível
- Separação de ambientes

### Docker Compose

**Serviços:**
- `postgres`: Banco de dados com volume persistente
- `api`: Aplicação .NET Core

**Rede:**
- Bridge network para comunicação entre containers

**Volumes:**
- `postgres_data`: Persistência de dados do PostgreSQL

## 🧪 Estratégia de Testes

### Tipos de Testes

1. **Testes Unitários**
   - Controllers: Validação de lógica de endpoints
   - Services: Validação de regras de negócio
   - Repositories: Validação de acesso a dados

2. **Mocking**
   - Uso de Moq para simular dependências
   - In-Memory Database para testes de repositório

### Cobertura de Testes

- Controllers: 100%
- Services: 100%
- Repositories: 100%

### Estrutura de Testes

```
CreditoAPI.Tests/
├── Controllers/
│   └── CreditosControllerTests.cs
├── Services/
│   └── CreditoServiceTests.cs
└── Repositories/
    └── CreditoRepositoryTests.cs
```

## 📈 Escalabilidade

### Horizontal Scaling

A aplicação foi projetada para escalar horizontalmente:

- **Stateless**: Não mantém estado entre requisições
- **Background Service**: Múltiplas instâncias podem processar mensagens
- **Database**: PostgreSQL suporta múltiplas conexões

### Considerações

- Service Bus garante que cada mensagem é processada por apenas uma instância
- Connection pooling do Entity Framework otimiza uso de conexões
- Health checks permitem load balancing inteligente

## 🔒 Segurança

### Implementações Atuais

- Validação de entrada (Data Annotations)
- Tratamento de exceções
- Logging de erros
- Connection strings em configuração

### Melhorias Futuras

- [ ] Autenticação JWT
- [ ] Autorização baseada em roles
- [ ] Rate limiting
- [ ] HTTPS obrigatório
- [ ] Secrets management (Azure Key Vault)

## 📝 Logging

### Estratégia

- Logging estruturado com `ILogger<T>`
- Níveis de log apropriados (Information, Warning, Error)
- Contexto em cada log (IDs, operações)

### Exemplos

```csharp
_logger.LogInformation("Credito inserted: {NumeroCredito}", credito.NumeroCredito);
_logger.LogError(ex, "Error processing credito: {NumeroCredito}", numeroCredito);
```

## 🎯 Decisões Arquiteturais

### Por que Repository Pattern?

- Facilita testes unitários
- Abstrai detalhes de implementação do EF Core
- Permite trocar ORM no futuro

### Por que Background Service?

- Processamento assíncrono não bloqueia API
- Escalável (múltiplas instâncias)
- Resiliente (retry automático)

### Por que Azure Service Bus?

- Garantias de entrega
- Escalabilidade
- Integração nativa com Azure
- Suporte a tópicos e subscriptions

### Por que PostgreSQL?

- Open source
- Robusto e confiável
- Suporte a JSON (futuras extensões)
- Bom desempenho

## 📚 Referências

- [Clean Architecture - Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)
- [Repository Pattern](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)
- [Background Services in .NET](https://docs.microsoft.com/en-us/dotnet/core/extensions/workers)

---

**Última atualização:** Dezembro 2024
