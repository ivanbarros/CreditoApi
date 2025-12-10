# Script para executar testes do CreditoAPI
# PowerShell Script

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  CreditoAPI - Executando Testes" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Navegar para o diretório de testes
$testPath = Join-Path $PSScriptRoot "CreditoAPI.Tests"

if (-Not (Test-Path $testPath)) {
    Write-Host "Erro: Diretório de testes não encontrado!" -ForegroundColor Red
    Write-Host "Procurando em: $testPath" -ForegroundColor Yellow
    exit 1
}

Set-Location $testPath

Write-Host "Restaurando pacotes..." -ForegroundColor Yellow
dotnet restore

Write-Host ""
Write-Host "Executando testes..." -ForegroundColor Yellow
Write-Host ""

# Executar testes com detalhes
dotnet test --logger "console;verbosity=detailed" --collect:"XPlat Code Coverage"

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  Todos os testes passaram! ✓" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "  Alguns testes falharam! ✗" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    exit 1
}

# Voltar ao diretório original
Set-Location $PSScriptRoot

Write-Host ""
Write-Host "Resumo dos Testes:" -ForegroundColor Cyan
Write-Host "- Testes de Serviços: CreditoServiceTests" -ForegroundColor White
Write-Host "- Testes de Repositórios: CreditoRepositoryTests" -ForegroundColor White
Write-Host "- Testes de Controladores: CreditosControllerTests" -ForegroundColor White
Write-Host ""
