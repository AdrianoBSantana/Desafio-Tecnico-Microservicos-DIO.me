# Arquitetura do Sistema de Microserviços

## Visão Geral

Este sistema implementa uma arquitetura de microserviços para e-commerce com os seguintes componentes:

## Diagrama da Arquitetura

```
┌─────────────────────────────────────────────────────────────────┐
│                           CLIENTE                               │
└─────────────────────┬───────────────────────────────────────────┘
                      │ HTTP/HTTPS + JWT
                      ▼
┌─────────────────────────────────────────────────────────────────┐
│                      API GATEWAY                               │
│                   (Porta 5000)                                 │
│  • Autenticação JWT                                            │
│  • Roteamento                                                  │
│  • CORS                                                        │
│  • Logging                                                     │
└─────────────────────┬───────────────────────────────────────────┘
                      │
         ┌────────────┴────────────┐
         ▼                         ▼
┌─────────────────┐        ┌─────────────────┐
│   ESTOQUE API   │        │   VENDAS API    │
│  (Porta 5065)   │        │  (Porta 5066)   │
│                 │        │                 │
│ • CRUD Produtos │        │ • CRUD Pedidos  │
│ • Validação     │        │ • Validação     │
│ • Busca         │        │ • Relatórios    │
│ • Estoque Baixo │        │ • Status        │
└─────────┬───────┘        └─────────┬───────┘
          │                          │
          │ HTTP (Validação)         │
          │                          │
          ▼                          ▼
┌─────────────────────────────────────────────────────────────────┐
│                    RABBITMQ                                     │
│                (Porta 5672)                                     │
│  • Comunicação Assíncrona                                       │
│  • Notificações de Venda                                        │
│  • Atualização de Estoque                                       │
└─────────────────────────────────────────────────────────────────┘
          ▲                          ▲
          │                          │
          └──────────┬───────────────┘
                     │
┌─────────────────────────────────────────────────────────────────┐
│                  BANCOS DE DADOS                               │
│                                                                 │
│  ┌─────────────────┐              ┌─────────────────┐          │
│  │  EstoqueDB.db   │              │   VendasDB.db   │          │
│  │   (SQLite)      │              │    (SQLite)     │          │
│  │                 │              │                 │          │
│  │ • Produtos      │              │ • Pedidos       │          │
│  │ • Estoque       │              │ • Itens Pedido  │          │
│  └─────────────────┘              └─────────────────┘          │
└─────────────────────────────────────────────────────────────────┘
```

## Componentes Detalhados

### 1. API Gateway (Gateway.API)
**Porta:** 5000
**Tecnologia:** ASP.NET Core + Ocelot

**Responsabilidades:**
- Ponto de entrada único para todas as requisições
- Autenticação JWT centralizada
- Roteamento para microserviços
- CORS configurado
- Logging de requisições

**Endpoints:**
- `POST /api/auth/login` - Autenticação
- `GET /estoque/{*}` - Proxy para Estoque API
- `GET /vendas/{*}` - Proxy para Vendas API

### 2. Microserviço de Estoque (Estoque.API)
**Porta:** 5065
**Tecnologia:** ASP.NET Core + Entity Framework

**Responsabilidades:**
- Gestão de produtos
- Controle de estoque
- Validação de disponibilidade
- Busca e filtros

**Endpoints:**
- `GET /api/produtos` - Listar produtos
- `GET /api/produtos/{id}` - Obter produto
- `POST /api/produtos` - Criar produto
- `PUT /api/produtos/{id}` - Atualizar produto
- `DELETE /api/produtos/{id}` - Deletar produto
- `GET /api/produtos/buscar?nome={nome}` - Buscar produtos
- `GET /api/produtos/estoque-baixo` - Produtos com estoque baixo
- `PUT /api/produtos/{id}/atualizar-estoque` - Atualizar estoque
- `GET /health` - Health check

### 3. Microserviço de Vendas (Vendas.API)
**Porta:** 5066
**Tecnologia:** ASP.NET Core + Entity Framework

**Responsabilidades:**
- Gestão de pedidos
- Validação de estoque
- Processamento de vendas
- Relatórios

**Endpoints:**
- `GET /api/pedidos` - Listar pedidos
- `GET /api/pedidos/{id}` - Obter pedido
- `POST /api/pedidos` - Criar pedido
- `GET /api/pedidos/status/{status}` - Pedidos por status
- `PUT /api/pedidos/{id}/status` - Atualizar status
- `GET /api/pedidos/relatorio` - Relatório de vendas
- `GET /health` - Health check

### 4. RabbitMQ
**Porta:** 5672 (AMQP), 15672 (Management UI)
**Tecnologia:** RabbitMQ Server

**Responsabilidades:**
- Comunicação assíncrona entre microserviços
- Notificações de vendas
- Atualização de estoque
- Decoupling de serviços

**Filas:**
- `venda_notifications` - Notificações de vendas

### 5. Bancos de Dados
**Tecnologia:** SQLite

**EstoqueDB.db:**
- Tabela: `Produtos`
  - Id (PK)
  - Nome
  - Descricao
  - Preco
  - Quantidade

**VendasDB.db:**
- Tabela: `Pedidos`
  - Id (PK)
  - DataPedido
  - Status
  - ValorTotal
- Tabela: `ItemPedidos`
  - Id (PK)
  - PedidoId (FK)
  - ProdutoId
  - Quantidade
  - PrecoUnitario

## Fluxo de Comunicação

### 1. Autenticação
```
Cliente → Gateway → JWT Token
```

### 2. Consulta de Produtos
```
Cliente → Gateway → Estoque API → SQLite
```

### 3. Criação de Pedido
```
Cliente → Gateway → Vendas API → Estoque API (validação)
         ↓
    RabbitMQ → Estoque API (atualização assíncrona)
```

### 4. Notificação de Venda
```
Vendas API → RabbitMQ → Estoque API → SQLite
```

## Padrões de Design Implementados

### 1. API Gateway Pattern
- Ponto de entrada único
- Autenticação centralizada
- Roteamento transparente

### 2. Microservices Pattern
- Serviços independentes
- Bancos de dados separados
- Deploy independente

### 3. Event-Driven Architecture
- Comunicação assíncrona via RabbitMQ
- Desacoplamento de serviços
- Resiliência

### 4. CQRS (Command Query Responsibility Segregation)
- Comandos: POST, PUT, DELETE
- Queries: GET
- Separação de responsabilidades

### 5. Circuit Breaker Pattern
- Health checks implementados
- Monitoramento de dependências
- Falha graciosa

## Segurança

### 1. Autenticação JWT
- Tokens com expiração
- Claims de usuário e role
- Validação em todos os endpoints

### 2. Autorização
- Roles: Admin, Vendas, Estoque
- Controle de acesso baseado em roles
- Endpoints protegidos

### 3. Validação
- Validação de entrada
- Sanitização de dados
- Tratamento de erros

## Monitoramento

### 1. Logging
- Middleware personalizado
- Logs estruturados
- Rastreamento de requisições

### 2. Health Checks
- Database connectivity
- RabbitMQ connectivity
- Status da aplicação

### 3. Métricas
- Tempo de resposta
- Contagem de requisições
- Erros e exceções

## Escalabilidade

### 1. Horizontal Scaling
- Microserviços independentes
- Load balancing via Gateway
- Stateless services

### 2. Database Scaling
- Bancos separados por serviço
- Possibilidade de replicação
- Migração para SQL Server/PostgreSQL

### 3. Message Queue Scaling
- RabbitMQ clustering
- Multiple consumers
- Dead letter queues

## Deployment

### 1. Desenvolvimento
- SQLite para simplicidade
- RabbitMQ via Docker
- Swagger para documentação

### 2. Produção
- SQL Server/PostgreSQL
- RabbitMQ cluster
- Containerização (Docker)
- Orquestração (Kubernetes)

## Considerações de Performance

### 1. Caching
- HTTP caching headers
- In-memory caching
- Redis para cache distribuído

### 2. Database Optimization
- Índices apropriados
- Connection pooling
- Query optimization

### 3. Message Queue Optimization
- Message batching
- Compression
- Dead letter handling

## Resiliência

### 1. Fault Tolerance
- Circuit breaker pattern
- Retry policies
- Timeout handling

### 2. Data Consistency
- Eventual consistency
- Saga pattern
- Compensation transactions

### 3. Service Discovery
- Health checks
- Load balancing
- Failover mechanisms


