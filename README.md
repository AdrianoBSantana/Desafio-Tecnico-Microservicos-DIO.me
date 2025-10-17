# Sistema de E-commerce com Microserviços

Este projeto implementa uma arquitetura de microserviços para gerenciamento de estoque e vendas de uma plataforma de e-commerce, utilizando .NET Core, RabbitMQ, JWT e API Gateway.

## Arquitetura

O sistema é composto por:

- **API Gateway**: Ponto de entrada único com autenticação JWT e roteamento
- **Microserviço de Estoque**: Gerencia produtos e controle de estoque
- **Microserviço de Vendas**: Gerencia pedidos e interage com o serviço de estoque
- **RabbitMQ**: Comunicação assíncrona entre microserviços
- **SQLite**: Banco de dados para cada microserviço

## Tecnologias Utilizadas

- .NET 9.0
- Entity Framework Core
- RabbitMQ
- JWT (JSON Web Tokens)
- Ocelot (API Gateway)
- SQLite
- xUnit (Testes)
- Swagger/OpenAPI

## Estrutura do Projeto

```
src/
├── Gateway.API/           # API Gateway com autenticação JWT
├── Estoque.API/          # Microserviço de gestão de estoque
├── Vendas.API/           # Microserviço de gestão de vendas
├── Shared/               # Bibliotecas compartilhadas
├── Estoque.API.Tests/    # Testes unitários do Estoque
└── Vendas.API.Tests/     # Testes unitários do Vendas
```

## Funcionalidades Implementadas

### Microserviço de Estoque
- ✅ Cadastro de produtos
- ✅ Consulta de produtos
- ✅ Atualização de estoque
- ✅ Busca de produtos por nome
- ✅ Consulta de produtos com estoque baixo
- ✅ Autenticação JWT
- ✅ Comunicação RabbitMQ
- ✅ Health checks
- ✅ Logging detalhado

### Microserviço de Vendas
- ✅ Criação de pedidos com validação de estoque
- ✅ Consulta de pedidos
- ✅ Atualização de status de pedidos
- ✅ Relatórios de vendas
- ✅ Autenticação JWT
- ✅ Comunicação com Estoque via HTTP
- ✅ Notificações RabbitMQ
- ✅ Health checks
- ✅ Logging detalhado

### API Gateway
- ✅ Roteamento para microserviços
- ✅ Autenticação JWT centralizada
- ✅ CORS configurado
- ✅ Endpoint de login

## Pré-requisitos

- .NET 9.0 SDK
- RabbitMQ Server
- Visual Studio 2022 ou VS Code

## Como Executar

### 1. Instalar RabbitMQ

**Windows:**
```bash
# Via Chocolatey
choco install rabbitmq

# Ou baixar de: https://www.rabbitmq.com/download.html
```

**Linux/macOS:**
```bash
# Ubuntu/Debian
sudo apt-get install rabbitmq-server

# macOS
brew install rabbitmq
```

Inicie o RabbitMQ:
```bash
# Windows
net start RabbitMQ

# Linux/macOS
sudo systemctl start rabbitmq-server
# ou
brew services start rabbitmq
```

### 2. Executar os Microserviços

Abra 4 terminais e execute cada serviço:

**Terminal 1 - API Gateway (Porta 5000):**
```bash
cd src/Gateway.API
dotnet run
```

**Terminal 2 - Estoque API (Porta 5065):**
```bash
cd src/Estoque.API
dotnet run
```

**Terminal 3 - Vendas API (Porta 5066):**
```bash
cd src/Vendas.API
dotnet run
```

### 3. Acessar as APIs

- **API Gateway**: http://localhost:5000
- **Estoque API**: http://localhost:5065
- **Vendas API**: http://localhost:5066
- **Swagger Gateway**: http://localhost:5000/swagger
- **Swagger Estoque**: http://localhost:5065/swagger
- **Swagger Vendas**: http://localhost:5066/swagger

### 4. Health Checks

- **Estoque Health**: http://localhost:5065/health
- **Vendas Health**: http://localhost:5066/health

## Como Usar

### 1. Autenticação

Primeiro, faça login no Gateway para obter um token JWT:

```bash
POST http://localhost:5000/api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "admin123"
}
```

**Usuários disponíveis:**
- `admin` / `admin123` (Admin)
- `vendas` / `vendas123` (Vendas)
- `estoque` / `estoque123` (Estoque)

### 2. Usar o Token

Adicione o token no header de todas as requisições:
```
Authorization: Bearer {seu-token-aqui}
```

### 3. Operações Básicas

**Cadastrar Produto (via Gateway):**
```bash
POST http://localhost:5000/estoque/api/produtos
Authorization: Bearer {token}

{
  "nome": "Produto Teste",
  "descricao": "Descrição do produto",
  "preco": 29.99,
  "quantidade": 100
}
```

**Criar Pedido (via Gateway):**
```bash
POST http://localhost:5000/vendas/api/pedidos
Authorization: Bearer {token}

{
  "itens": [
    {
      "produtoId": 1,
      "quantidade": 2,
      "precoUnitario": 29.99
    }
  ]
}
```

## Executar Testes

```bash
# Executar todos os testes
dotnet test

# Executar testes específicos
dotnet test src/Estoque.API.Tests/
dotnet test src/Vendas.API.Tests/

# Com cobertura de código
dotnet test --collect:"XPlat Code Coverage"
```

## Fluxo de Comunicação

1. **Cliente** → **API Gateway** (com JWT)
2. **API Gateway** → **Microserviço** (rota baseada na URL)
3. **Vendas** → **Estoque** (HTTP para validação)
4. **Vendas** → **RabbitMQ** (notificação assíncrona)
5. **Estoque** ← **RabbitMQ** (atualização de estoque)

## Monitoramento

### Logs
Os logs detalhados são gerados automaticamente, incluindo:
- Requisições e respostas HTTP
- Tempo de processamento
- Erros e exceções
- Comunicação RabbitMQ

### Health Checks
Verifique a saúde dos serviços:
- Database connectivity
- RabbitMQ connectivity
- Status geral da aplicação

## Extras Implementados

- ✅ **Testes Unitários**: Cobertura dos controllers principais
- ✅ **Logging Detalhado**: Middleware personalizado para rastreamento
- ✅ **Health Checks**: Monitoramento de dependências
- ✅ **Documentação**: Swagger/OpenAPI em todos os serviços
- ✅ **Validações**: Validação de entrada e regras de negócio
- ✅ **Tratamento de Erros**: Respostas padronizadas
- ✅ **Escalabilidade**: Arquitetura preparada para múltiplas instâncias

## Próximos Passos

Para expandir o sistema, considere:

1. **Banco de Dados**: Migrar para SQL Server ou PostgreSQL
2. **Containerização**: Docker e Kubernetes
3. **Service Discovery**: Consul ou Eureka
4. **Circuit Breaker**: Polly para resiliência
5. **Métricas**: Prometheus e Grafana
6. **CI/CD**: Azure DevOps ou GitHub Actions
7. **Segurança**: Rate limiting e validação adicional

## Troubleshooting

### RabbitMQ não conecta
```bash
# Verificar se está rodando
rabbitmq-diagnostics status

# Verificar portas
netstat -an | findstr 5672
```

### Banco de dados não inicializa
```bash
# Recriar migrações
dotnet ef database drop
dotnet ef database update
```

### Portas em uso
Altere as portas nos arquivos `Properties/launchSettings.json` de cada projeto.

## Contribuição

1. Fork o projeto
2. Crie uma branch para sua feature
3. Commit suas mudanças
4. Push para a branch
5. Abra um Pull Request


