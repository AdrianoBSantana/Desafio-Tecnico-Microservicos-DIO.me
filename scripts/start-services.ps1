# Script para iniciar todos os serviços do sistema de microserviços
# Execute este script no PowerShell

Write-Host "🚀 Iniciando Sistema de Microserviços E-commerce" -ForegroundColor Green
Write-Host "===============================================" -ForegroundColor Green

# Verificar se o .NET está instalado
Write-Host "📋 Verificando pré-requisitos..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET versão: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ .NET não encontrado. Instale o .NET 9.0 SDK" -ForegroundColor Red
    exit 1
}

# Verificar se o RabbitMQ está rodando
Write-Host "📋 Verificando RabbitMQ..." -ForegroundColor Yellow
try {
    $rabbitmqStatus = Get-Service -Name "RabbitMQ" -ErrorAction SilentlyContinue
    if ($rabbitmqStatus -and $rabbitmqStatus.Status -eq "Running") {
        Write-Host "✅ RabbitMQ está rodando" -ForegroundColor Green
    } else {
        Write-Host "⚠️  RabbitMQ não está rodando. Iniciando..." -ForegroundColor Yellow
        Start-Service -Name "RabbitMQ" -ErrorAction SilentlyContinue
        Start-Sleep -Seconds 5
    }
} catch {
    Write-Host "⚠️  RabbitMQ não encontrado como serviço. Verifique se está instalado." -ForegroundColor Yellow
}

# Função para abrir nova janela do PowerShell
function Start-NewTerminal {
    param(
        [string]$Title,
        [string]$WorkingDirectory,
        [string]$Command
    )
    
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$WorkingDirectory'; Write-Host '$Title' -ForegroundColor Cyan; $Command"
}

Write-Host "🔧 Iniciando serviços..." -ForegroundColor Yellow

# Iniciar Gateway API
Write-Host "🌐 Iniciando API Gateway (Porta 5000)..." -ForegroundColor Cyan
Start-NewTerminal -Title "API Gateway" -WorkingDirectory "src/Gateway.API" -Command "dotnet run"

Start-Sleep -Seconds 3

# Iniciar Estoque API
Write-Host "📦 Iniciando Estoque API (Porta 5065)..." -ForegroundColor Cyan
Start-NewTerminal -Title "Estoque API" -WorkingDirectory "src/Estoque.API" -Command "dotnet run"

Start-Sleep -Seconds 3

# Iniciar Vendas API
Write-Host "🛒 Iniciando Vendas API (Porta 5066)..." -ForegroundColor Cyan
Start-NewTerminal -Title "Vendas API" -WorkingDirectory "src/Vendas.API" -Command "dotnet run"

Start-Sleep -Seconds 5

Write-Host ""
Write-Host "✅ Todos os serviços foram iniciados!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 URLs disponíveis:" -ForegroundColor Yellow
Write-Host "  • API Gateway:     http://localhost:5000" -ForegroundColor White
Write-Host "  • Estoque API:     http://localhost:5065" -ForegroundColor White
Write-Host "  • Vendas API:      http://localhost:5066" -ForegroundColor White
Write-Host ""
Write-Host "📚 Documentação:" -ForegroundColor Yellow
Write-Host "  • Gateway Swagger: http://localhost:5000/swagger" -ForegroundColor White
Write-Host "  • Estoque Swagger: http://localhost:5065/swagger" -ForegroundColor White
Write-Host "  • Vendas Swagger:  http://localhost:5066/swagger" -ForegroundColor White
Write-Host ""
Write-Host "🔍 Health Checks:" -ForegroundColor Yellow
Write-Host "  • Estoque Health:  http://localhost:5065/health" -ForegroundColor White
Write-Host "  • Vendas Health:   http://localhost:5066/health" -ForegroundColor White
Write-Host ""
Write-Host "🔐 Credenciais para teste:" -ForegroundColor Yellow
Write-Host "  • Admin:  admin / admin123" -ForegroundColor White
Write-Host "  • Vendas: vendas / vendas123" -ForegroundColor White
Write-Host "  • Estoque: estoque / estoque123" -ForegroundColor White
Write-Host ""
Write-Host "💡 Dica: Use o arquivo EXEMPLOS_USO.md para ver exemplos de requisições!" -ForegroundColor Cyan
Write-Host ""
Write-Host "Pressione qualquer tecla para parar todos os serviços..." -ForegroundColor Red
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

# Parar todos os processos dotnet
Write-Host ""
Write-Host "🛑 Parando todos os serviços..." -ForegroundColor Yellow
Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Write-Host "✅ Serviços parados!" -ForegroundColor Green


