# Exemplos de Uso da API

Este documento contém exemplos práticos de como usar a API do sistema de microserviços.

## 1. Autenticação

### Login
```bash
curl -X POST "http://localhost:5000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "admin123"
  }'
```

**Resposta:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiration": "2024-01-15T10:30:00Z",
  "username": "admin"
}
```

## 2. Microserviço de Estoque

### Cadastrar Produto
```bash
curl -X POST "http://localhost:5000/estoque/api/produtos" \
  -H "Authorization: Bearer SEU_TOKEN_AQUI" \
  -H "Content-Type: application/json" \
  -d '{
    "nome": "Smartphone Samsung",
    "descricao": "Smartphone Samsung Galaxy S24",
    "preco": 1299.99,
    "quantidade": 50
  }'
```

### Listar Produtos
```bash
curl -X GET "http://localhost:5000/estoque/api/produtos" \
  -H "Authorization: Bearer SEU_TOKEN_AQUI"
```

### Buscar Produtos por Nome
```bash
curl -X GET "http://localhost:5000/estoque/api/produtos/buscar?nome=samsung" \
  -H "Authorization: Bearer SEU_TOKEN_AQUI"
```

### Consultar Produtos com Estoque Baixo
```bash
curl -X GET "http://localhost:5000/estoque/api/produtos/estoque-baixo?limite=10" \
  -H "Authorization: Bearer SEU_TOKEN_AQUI"
```

### Atualizar Estoque
```bash
curl -X PUT "http://localhost:5000/estoque/api/produtos/1/atualizar-estoque" \
  -H "Authorization: Bearer SEU_TOKEN_AQUI" \
  -H "Content-Type: application/json" \
  -d '{
    "quantidade": 75
  }'
```

## 3. Microserviço de Vendas

### Criar Pedido
```bash
curl -X POST "http://localhost:5000/vendas/api/pedidos" \
  -H "Authorization: Bearer SEU_TOKEN_AQUI" \
  -H "Content-Type: application/json" \
  -d '{
    "itens": [
      {
        "produtoId": 1,
        "quantidade": 2,
        "precoUnitario": 1299.99
      },
      {
        "produtoId": 2,
        "quantidade": 1,
        "precoUnitario": 599.99
      }
    ]
  }'
```

### Listar Pedidos
```bash
curl -X GET "http://localhost:5000/vendas/api/pedidos" \
  -H "Authorization: Bearer SEU_TOKEN_AQUI"
```

### Consultar Pedido por ID
```bash
curl -X GET "http://localhost:5000/vendas/api/pedidos/1" \
  -H "Authorization: Bearer SEU_TOKEN_AQUI"
```

### Consultar Pedidos por Status
```bash
curl -X GET "http://localhost:5000/vendas/api/pedidos/status/Aprovado" \
  -H "Authorization: Bearer SEU_TOKEN_AQUI"
```

### Atualizar Status do Pedido
```bash
curl -X PUT "http://localhost:5000/vendas/api/pedidos/1/status" \
  -H "Authorization: Bearer SEU_TOKEN_AQUI" \
  -H "Content-Type: application/json" \
  -d '{
    "status": "Enviado"
  }'
```

### Gerar Relatório de Vendas
```bash
curl -X GET "http://localhost:5000/vendas/api/pedidos/relatorio?dataInicio=2024-01-01&dataFim=2024-01-31" \
  -H "Authorization: Bearer SEU_TOKEN_AQUI"
```

## 4. Health Checks

### Verificar Saúde do Estoque
```bash
curl -X GET "http://localhost:5065/health"
```

### Verificar Saúde das Vendas
```bash
curl -X GET "http://localhost:5066/health"
```

## 5. Exemplos com JavaScript/Fetch

### Login e Uso da API
```javascript
// Função para fazer login
async function login(username, password) {
  const response = await fetch('http://localhost:5000/api/auth/login', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ username, password })
  });
  
  const data = await response.json();
  return data.token;
}

// Função para fazer requisições autenticadas
async function authenticatedRequest(url, options = {}) {
  const token = localStorage.getItem('token'); // Assumindo que o token está salvo
  
  const response = await fetch(url, {
    ...options,
    headers: {
      ...options.headers,
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
  });
  
  return response.json();
}

// Exemplo de uso
async function exemploCompleto() {
  // 1. Login
  const token = await login('admin', 'admin123');
  localStorage.setItem('token', token);
  
  // 2. Cadastrar produto
  const produto = await authenticatedRequest('http://localhost:5000/estoque/api/produtos', {
    method: 'POST',
    body: JSON.stringify({
      nome: 'Notebook Dell',
      descricao: 'Notebook Dell Inspiron 15',
      preco: 2499.99,
      quantidade: 20
    })
  });
  
  console.log('Produto cadastrado:', produto);
  
  // 3. Criar pedido
  const pedido = await authenticatedRequest('http://localhost:5000/vendas/api/pedidos', {
    method: 'POST',
    body: JSON.stringify({
      itens: [{
        produtoId: produto.id,
        quantidade: 1,
        precoUnitario: produto.preco
      }]
    })
  });
  
  console.log('Pedido criado:', pedido);
}
```

## 6. Exemplos com Postman

### Collection para Importar

```json
{
  "info": {
    "name": "E-commerce Microservices",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "variable": [
    {
      "key": "baseUrl",
      "value": "http://localhost:5000"
    },
    {
      "key": "token",
      "value": ""
    }
  ],
  "item": [
    {
      "name": "Auth",
      "item": [
        {
          "name": "Login",
          "request": {
            "method": "POST",
            "header": [
              {
                "key": "Content-Type",
                "value": "application/json"
              }
            ],
            "body": {
              "mode": "raw",
              "raw": "{\n  \"username\": \"admin\",\n  \"password\": \"admin123\"\n}"
            },
            "url": {
              "raw": "{{baseUrl}}/api/auth/login",
              "host": ["{{baseUrl}}"],
              "path": ["api", "auth", "login"]
            }
          }
        }
      ]
    },
    {
      "name": "Estoque",
      "item": [
        {
          "name": "Listar Produtos",
          "request": {
            "method": "GET",
            "header": [
              {
                "key": "Authorization",
                "value": "Bearer {{token}}"
              }
            ],
            "url": {
              "raw": "{{baseUrl}}/estoque/api/produtos",
              "host": ["{{baseUrl}}"],
              "path": ["estoque", "api", "produtos"]
            }
          }
        }
      ]
    }
  ]
}
```

## 7. Fluxo Completo de Exemplo

```bash
# 1. Login
TOKEN=$(curl -s -X POST "http://localhost:5000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}' | \
  jq -r '.token')

echo "Token obtido: $TOKEN"

# 2. Cadastrar produto
PRODUTO_ID=$(curl -s -X POST "http://localhost:5000/estoque/api/produtos" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "nome": "Mouse Gamer",
    "descricao": "Mouse gamer RGB",
    "preco": 89.99,
    "quantidade": 100
  }' | jq -r '.id')

echo "Produto cadastrado com ID: $PRODUTO_ID"

# 3. Criar pedido
PEDIDO_ID=$(curl -s -X POST "http://localhost:5000/vendas/api/pedidos" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{
    \"itens\": [{
      \"produtoId\": $PRODUTO_ID,
      \"quantidade\": 5,
      \"precoUnitario\": 89.99
    }]
  }" | jq -r '.id')

echo "Pedido criado com ID: $PEDIDO_ID"

# 4. Verificar estoque atualizado
curl -s -X GET "http://localhost:5000/estoque/api/produtos/$PRODUTO_ID" \
  -H "Authorization: Bearer $TOKEN" | jq '.quantidade'

# 5. Gerar relatório
curl -s -X GET "http://localhost:5000/vendas/api/pedidos/relatorio" \
  -H "Authorization: Bearer $TOKEN" | jq '.'
```

## 8. Códigos de Status HTTP

- **200 OK**: Operação realizada com sucesso
- **201 Created**: Recurso criado com sucesso
- **400 Bad Request**: Dados inválidos ou regra de negócio violada
- **401 Unauthorized**: Token JWT inválido ou ausente
- **404 Not Found**: Recurso não encontrado
- **500 Internal Server Error**: Erro interno do servidor

## 9. Tratamento de Erros

### Exemplo de Resposta de Erro
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "traceId": "0HMQ8VJJA3J2D:00000001",
  "errors": {
    "Nome": [
      "O campo Nome é obrigatório."
    ]
  }
}
```

### Exemplo de Erro de Estoque Insuficiente
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Estoque insuficiente para o produto Smartphone Samsung. Disponível: 10, Solicitado: 15"
}
```


