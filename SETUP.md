# Guia de Configuração Rápida

## 🚀 Início Rápido com Docker (Recomendado)

### 1. Pré-requisitos
- Docker Desktop instalado e rodando
- Git (para clonar o repositório)

### 2. Clonar e Executar

```bash
# Clone o repositório
git clone <repository-url>
cd CreditoAPI

# Inicie todos os serviços
docker-compose up -d

# Verifique se os containers estão rodando
docker-compose ps

# Acompanhe os logs
docker-compose logs -f api
```

### 3. Testar a API

Acesse no navegador:
- Swagger UI: http://localhost:5000/swagger
- Health Check: http://localhost:5000/api/self

### 4. Parar os Serviços

```bash
# Parar containers
docker-compose down

# Parar e remover dados do banco
docker-compose down -v
```

## 💻 Configuração Local (Desenvolvimento)

### 1. Pré-requisitos

- .NET 6.0 SDK ou superior
- PostgreSQL 14+
- IDE (Visual Studio 2022, VS Code ou Rider)

### 2. Instalar PostgreSQL

**Windows (usando Chocolatey):**
```powershell
choco install postgresql
```

**Linux:**
```bash
sudo apt-get install postgresql-14
```

**macOS:**
```bash
brew install postgresql@14
```

### 3. Configurar Banco de Dados

```sql
-- Conecte ao PostgreSQL
psql -U postgres

-- Crie o banco de dados
CREATE DATABASE creditodb;

-- Crie o usuário (se necessário)
CREATE USER postgres WITH PASSWORD 'postgres';

-- Conceda permissões
GRANT ALL PRIVILEGES ON DATABASE creditodb TO postgres;
```

### 4. Configurar Connection String

Edite `CreditoAPI/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=creditodb;Username=postgres;Password=postgres"
  }
}
```

### 5. Restaurar Pacotes e Aplicar Migrações

```bash
cd CreditoAPI
dotnet restore
dotnet ef database update
```

### 6. Executar a API

```bash
dotnet run
```

A API estará disponível em:
- HTTPS: https://localhost:7000
- HTTP: http://localhost:5000
- Swagger: https://localhost:7000/swagger

## 🔧 Configuração do Azure Service Bus (Opcional)

### Opção 1: Usar Azure Service Bus Real

1. **Criar Namespace no Azure:**
   - Acesse o Portal do Azure
   - Crie um Service Bus Namespace
   - Anote a Connection String

2. **Criar Tópico:**
   - Nome: `integrar-credito-constituido-entry`
   - Crie uma Subscription para o tópico

3. **Configurar na API:**

Edite `appsettings.json`:

```json
{
  "ServiceBus": {
    "ConnectionString": "Endpoint=sb://seu-namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=sua-chave",
    "TopicName": "integrar-credito-constituido-entry",
    "SubscriptionName": "credito-processor"
  }
}
```

### Opção 2: Modo Mock (Desenvolvimento)

Se você não configurar uma connection string válida, a API funcionará em modo mock:
- As mensagens não serão enviadas ao Service Bus
- O background service não processará mensagens
- Os endpoints de consulta funcionarão normalmente

## 🧪 Executar Testes

```bash
# Navegar para o projeto de testes
cd CreditoAPI.Tests

# Executar todos os testes
dotnet test

# Executar com detalhes
dotnet test --logger "console;verbosity=detailed"

# Executar com cobertura
dotnet test /p:CollectCoverage=true
```

## 📊 Verificar se Está Funcionando

### 1. Health Checks

```bash
# Self check
curl http://localhost:5000/api/self

# Ready check (verifica banco de dados)
curl http://localhost:5000/api/ready
```

### 2. Testar Integração de Créditos

```bash
curl -X POST http://localhost:5000/api/creditos/integrar-credito-constituido \
  -H "Content-Type: application/json" \
  -d '[{
    "numeroCredito": "123456",
    "numeroNfse": "7891011",
    "dataConstituicao": "2024-02-25",
    "valorIssqn": 1500.75,
    "tipoCredito": "ISSQN",
    "simplesNacional": "Sim",
    "aliquota": 5.0,
    "valorFaturado": 30000.00,
    "valorDeducao": 5000.00,
    "baseCalculo": 25000.00
  }]'
```

### 3. Consultar Créditos

```bash
# Por NFS-e
curl http://localhost:5000/api/creditos/7891011

# Por número do crédito
curl http://localhost:5000/api/creditos/credito/123456
```

## 🐛 Troubleshooting

### Problema: Erro de conexão com PostgreSQL

**Solução:**
```bash
# Verificar se o PostgreSQL está rodando
docker ps | grep postgres

# Ou no Windows
Get-Service postgresql*

# Reiniciar o PostgreSQL
docker-compose restart postgres
```

### Problema: Porta 5000 já em uso

**Solução:**
Edite `docker-compose.yml` e altere a porta:
```yaml
ports:
  - "5001:80"  # Altere de 5000 para 5001
```

### Problema: Migrações não aplicadas

**Solução:**
```bash
cd CreditoAPI
dotnet ef database drop --force
dotnet ef database update
```

### Problema: Service Bus não configurado

**Solução:**
A API funciona sem o Service Bus configurado (modo mock). Para produção, configure conforme a seção "Configuração do Azure Service Bus".

## 📝 Variáveis de Ambiente

Você pode configurar via variáveis de ambiente:

```bash
# Windows PowerShell
$env:ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=creditodb;Username=postgres;Password=postgres"
$env:ServiceBus__ConnectionString="sua-connection-string"

# Linux/macOS
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=creditodb;Username=postgres;Password=postgres"
export ServiceBus__ConnectionString="sua-connection-string"
```

## 🔍 Logs e Monitoramento

### Ver logs do Docker

```bash
# Logs da API
docker-compose logs -f api

# Logs do PostgreSQL
docker-compose logs -f postgres

# Todos os logs
docker-compose logs -f
```

### Ver logs locais

Os logs aparecem no console quando você executa `dotnet run`.

## 📚 Recursos Adicionais

- [Documentação .NET Core](https://docs.microsoft.com/dotnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- [Azure Service Bus](https://docs.microsoft.com/azure/service-bus-messaging/)
- [Docker Documentation](https://docs.docker.com/)

## ✅ Checklist de Configuração

- [ ] Docker Desktop instalado e rodando
- [ ] Repositório clonado
- [ ] `docker-compose up -d` executado com sucesso
- [ ] Health check retorna 200 OK
- [ ] Swagger UI acessível
- [ ] Testes executados com sucesso
- [ ] Endpoints testados e funcionando

## 🎯 Próximos Passos

1. Explore a documentação Swagger em http://localhost:5000/swagger
2. Teste os endpoints usando o arquivo `test-requests.http`
3. Execute os testes unitários
4. Configure o Azure Service Bus para ambiente de produção
5. Customize as configurações conforme necessário

---

**Dúvidas?** Consulte o README.md principal ou abra uma issue no repositório.
