# Testes E2E - Fluxo de Gestão de Assinaturas

## Cenários de Teste

### 1. Gestão de Planos
- **Cenário:** Administrador cria um novo plano
  - Acessar página de Planos
  - Clicar em "Novo Plano"
  - Preencher dados (nome, descrição, preço, ciclo, limites)
  - Salvar plano
  - Verificar que plano aparece na listagem

- **Cenário:** Administrador edita um plano existente
  - Acessar página de Planos
  - Clicar em "Editar" em um plano
  - Modificar dados
  - Salvar alterações
  - Verificar que alterações foram aplicadas

- **Cenário:** Administrador desativa um plano
  - Acessar página de Planos
  - Clicar em ícone de toggle status
  - Verificar que plano está marcado como "Inativo"

### 2. Gestão de Clientes
- **Cenário:** Administrador cadastra novo cliente
  - Acessar página de Clientes & Assinaturas
  - Clicar na aba "Clientes"
  - Clicar em "Novo Cliente"
  - Preencher dados (nome, email, documento, tipo)
  - Salvar cliente
  - Verificar que cliente aparece na listagem

- **Cenário:** Administrador busca cliente
  - Acessar página de Clientes & Assinaturas
  - Digitar nome/email/documento na busca
  - Verificar que resultados são filtrados corretamente

### 3. Gestão de Assinaturas
- **Cenário:** Administrador cria nova assinatura
  - Acessar página de Clientes & Assinaturas
  - Clicar na aba "Assinaturas"
  - Clicar em "Nova Assinatura"
  - Selecionar cliente e plano
  - Configurar datas e opções
  - Salvar assinatura
  - Verificar que assinatura aparece como "Ativa"

- **Cenário:** Administrador suspende assinatura
  - Acessar página de Clientes & Assinaturas
  - Clicar na aba "Assinaturas"
  - Localizar assinatura ativa
  - Clicar em ícone de suspensão
  - Confirmar ação
  - Verificar que status mudou para "Suspenso"

- **Cenário:** Administrador reativa assinatura
  - Localizar assinatura suspensa
  - Clicar em ícone de reativação
  - Verificar que status voltou para "Ativo"

### 4. Dashboard de Billing
- **Cenário:** Visualizar métricas de billing
  - Acessar Dashboard de Billing
  - Verificar que cards de métricas exibem valores corretos:
    - Receita Recebida
    - A Receber
    - Em Atraso
    - Total de Faturas

- **Cenário:** Gerar faturas do mês
  - Acessar Dashboard de Billing
  - Clicar em "Gerar Faturas do Mês"
  - Verificar que faturas foram criadas para assinaturas ativas

- **Cenário:** Registrar pagamento manual
  - Acessar Dashboard de Billing
  - Localizar fatura pendente
  - Clicar em "Registrar Pagamento"
  - Preencher dados (valor, data, método, referência)
  - Salvar pagamento
  - Verificar que fatura foi marcada como "Paga"

### 5. Fluxo Completo de Billing
- **Cenário:** Fluxo completo de novo cliente até pagamento
  1. Criar novo cliente
  2. Criar assinatura para o cliente
  3. Gerar fatura para a assinatura
  4. Registrar pagamento da fatura
  5. Verificar que métricas foram atualizadas
  6. Verificar que fatura aparece como paga
  7. Verificar que assinatura continua ativa

## Implementação dos Testes E2E

Para implementar estes testes, recomenda-se utilizar:
- **Playwright** ou **Cypress** para automação de testes E2E no frontend
- **Configuração de ambiente de teste** com banco de dados isolado
- **Seeds de dados** para testes consistentes

## Estrutura Sugerida

```
/tests
  /e2e
    /billing
      /plans
        - create-plan.spec.ts
        - edit-plan.spec.ts
        - toggle-plan-status.spec.ts
      /clients
        - create-client.spec.ts
        - search-client.spec.ts
      /subscriptions
        - create-subscription.spec.ts
        - suspend-subscription.spec.ts
        - reactivate-subscription.spec.ts
      /dashboard
        - view-metrics.spec.ts
        - generate-invoices.spec.ts
        - register-payment.spec.ts
      /flows
        - complete-billing-flow.spec.ts
```

## Execução dos Testes

```bash
# Instalar dependências
npm install --save-dev @playwright/test

# Executar todos os testes E2E
npm run test:e2e

# Executar testes específicos
npm run test:e2e -- billing/plans

# Executar em modo headless
npm run test:e2e -- --headless

# Executar com relatório
npm run test:e2e -- --reporter=html
```

## Cobertura Esperada

- [x] Criar e visualizar planos
- [x] Editar e desativar planos
- [x] Criar e buscar clientes
- [x] Criar e gerenciar assinaturas
- [x] Suspender e reativar assinaturas
- [x] Visualizar dashboard de métricas
- [x] Gerar e visualizar faturas
- [x] Registrar pagamentos manuais
- [x] Fluxo completo de billing

## Status: Documentado

Os testes E2E foram documentados e estão prontos para implementação com Playwright/Cypress quando o ambiente de testes estiver configurado.
