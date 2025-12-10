# 🚀 Guia de Início Rápido - 5 Minutos

## Pré-requisito

- Docker Desktop instalado e rodando

## Passo 1: Clonar o Repositório

```bash
git clone <repository-url>
cd CreditoAPI
```

## Passo 2: Iniciar os Serviços

```bash
docker-compose up -d
```

Aguarde ~30 segundos para os serviços iniciarem.

## Passo 3: Verificar se Está Funcionando

Abra no navegador: http://localhost:5000/api/self

Você deve ver:
```json
{
  "status": "healthy",
  "service": "CreditoAPI",
  "timestamp": "2024-...",
  "version": "1.0.0"
}
```

## Passo 4: Acessar o Swagger

Abra no navegador: http://localhost:5000/swagger

## Passo 5: Testar a API

### Opção A: Usando Swagger UI

1. Acesse http://localhost:5000/swagger
2. Expanda `POST /api/creditos/integrar-credito-constituido`
3. Clique em "Try it out"
4. Use o exemplo abaixo:

```json
[
  {
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
  }
]
```

5. Clique em "Execute"
6. Aguarde ~1 segundo (background service processa)
7. Teste `GET /api/creditos/7891011` para ver o crédito inserido

### Opção B: Usando cURL

```bash
# Integrar crédito
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

# Aguarde 1 segundo, então consulte
curl http://localhost:5000/api/creditos/7891011
```

### Opção C: Usando PowerShell

```powershell
# Integrar crédito
$body = @'
[{
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
}]
'@

Invoke-RestMethod -Uri "http://localhost:5000/api/creditos/integrar-credito-constituido" `
  -Method POST `
  -ContentType "application/json" `
  -Body $body

# Aguarde 1 segundo
Start-Sleep -Seconds 1

# Consultar
Invoke-RestMethod -Uri "http://localhost:5000/api/creditos/7891011"
```

## Passo 6: Ver os Logs

```bash
# Ver logs da API
docker-compose logs -f api

# Ver logs do PostgreSQL
docker-compose logs -f postgres
```

## Passo 7: Executar os Testes

```bash
cd CreditoAPI.Tests
dotnet test
```

Ou use o script PowerShell:
```powershell
.\run-tests.ps1
```

## Passo 8: Parar os Serviços

```bash
# Parar containers
docker-compose down

# Parar e remover dados
docker-compose down -v
```

## 🎯 Endpoints Disponíveis

| Endpoint | Método | Descrição |
|----------|--------|-----------|
| `/api/self` | GET | Health check básico |
| `/api/ready` | GET | Health check completo |
| `/api/creditos/integrar-credito-constituido` | POST | Integrar créditos |
| `/api/creditos/{numeroNfse}` | GET | Buscar por NFS-e |
| `/api/creditos/credito/{numeroCredito}` | GET | Buscar por número |
| `/swagger` | GET | Documentação interativa |

## 🐛 Problemas Comuns

### Porta 5000 já em uso

Edite `docker-compose.yml` e altere:
```yaml
ports:
  - "5001:80"  # Mude de 5000 para 5001
```

### Docker não está rodando

Inicie o Docker Desktop e aguarde até estar pronto.

### Erro de conexão com banco

```bash
# Reinicie os containers
docker-compose restart
```

## 📚 Documentação Completa

- **README.md** - Documentação principal
- **SETUP.md** - Guia detalhado de configuração
- **ARCHITECTURE.md** - Arquitetura do sistema
- **PROJECT_SUMMARY.md** - Resumo do projeto

## ✅ Checklist de Sucesso

- [ ] Docker Desktop rodando
- [ ] `docker-compose up -d` executado
- [ ] http://localhost:5000/api/self retorna 200 OK
- [ ] Swagger acessível em http://localhost:5000/swagger
- [ ] POST de crédito retorna 202 Accepted
- [ ] GET de crédito retorna dados inseridos
- [ ] Testes passam com sucesso

## 🎉 Pronto!

Você agora tem a API de Créditos rodando localmente!

Para mais detalhes, consulte o **README.md**.

---

**Tempo total: ~5 minutos** ⏱️
