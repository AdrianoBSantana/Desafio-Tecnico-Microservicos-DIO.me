#!/bin/bash

# Script para iniciar todos os serviços do sistema de microserviços
# Execute este script no terminal Linux/macOS

echo "🚀 Iniciando Sistema de Microserviços E-commerce"
echo "==============================================="

# Verificar se o .NET está instalado
echo "📋 Verificando pré-requisitos..."
if command -v dotnet &> /dev/null; then
    DOTNET_VERSION=$(dotnet --version)
    echo "✅ .NET versão: $DOTNET_VERSION"
else
    echo "❌ .NET não encontrado. Instale o .NET 9.0 SDK"
    exit 1
fi

# Verificar se o RabbitMQ está rodando
echo "📋 Verificando RabbitMQ..."
if systemctl is-active --quiet rabbitmq-server 2>/dev/null; then
    echo "✅ RabbitMQ está rodando"
elif brew services list | grep -q "rabbitmq.*started" 2>/dev/null; then
    echo "✅ RabbitMQ está rodando (Homebrew)"
else
    echo "⚠️  RabbitMQ não está rodando. Tentando iniciar..."
    if command -v systemctl &> /dev/null; then
        sudo systemctl start rabbitmq-server
    elif command -v brew &> /dev/null; then
        brew services start rabbitmq
    else
        echo "⚠️  RabbitMQ não encontrado. Verifique se está instalado."
    fi
    sleep 5
fi

# Função para iniciar serviço em background
start_service() {
    local title="$1"
    local directory="$2"
    local port="$3"
    
    echo "🔧 Iniciando $title (Porta $port)..."
    cd "$directory" && dotnet run &
    sleep 3
}

echo "🔧 Iniciando serviços..."

# Iniciar Gateway API
start_service "API Gateway" "src/Gateway.API" "5000"

# Iniciar Estoque API  
start_service "Estoque API" "src/Estoque.API" "5065"

# Iniciar Vendas API
start_service "Vendas API" "src/Vendas.API" "5066"

sleep 5

echo ""
echo "✅ Todos os serviços foram iniciados!"
echo ""
echo "📋 URLs disponíveis:"
echo "  • API Gateway:     http://localhost:5000"
echo "  • Estoque API:     http://localhost:5065"
echo "  • Vendas API:      http://localhost:5066"
echo ""
echo "📚 Documentação:"
echo "  • Gateway Swagger: http://localhost:5000/swagger"
echo "  • Estoque Swagger: http://localhost:5065/swagger"
echo "  • Vendas Swagger:  http://localhost:5066/swagger"
echo ""
echo "🔍 Health Checks:"
echo "  • Estoque Health:  http://localhost:5065/health"
echo "  • Vendas Health:   http://localhost:5066/health"
echo ""
echo "🔐 Credenciais para teste:"
echo "  • Admin:  admin / admin123"
echo "  • Vendas: vendas / vendas123"
echo "  • Estoque: estoque / estoque123"
echo ""
echo "💡 Dica: Use o arquivo EXEMPLOS_USO.md para ver exemplos de requisições!"
echo ""
echo "Pressione Ctrl+C para parar todos os serviços..."

# Aguardar interrupção
trap 'echo ""; echo "🛑 Parando todos os serviços..."; pkill -f "dotnet run"; echo "✅ Serviços parados!"; exit 0' INT

# Manter script rodando
while true; do
    sleep 1
done


