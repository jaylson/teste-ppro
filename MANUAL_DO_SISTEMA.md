# Partnership Manager — Manual do Sistema

> Plataforma de Gestão Societária para gerenciar Cap Table, Vesting, Contratos, Valuations e comunicação com investidores.

---

## Sumário

1. [Login](#1-login)
2. [Dashboards](#2-dashboards)
   - 2.1 [Dashboard Principal](#21-dashboard-principal)
   - 2.2 [Valuation Dashboard](#22-valuation-dashboard)
   - 2.3 [Financeiro Dashboard](#23-financeiro-dashboard)
   - 2.4 [Meu Vesting](#24-meu-vesting)
   - 2.5 [Portal do Investidor](#25-portal-do-investidor)
3. [Cap Table](#3-cap-table)
   - 3.1 [Empresas](#31-empresas)
   - 3.2 [Sócios](#32-sócios)
   - 3.3 [Cap Table](#33-cap-table)
   - 3.4 [Transações](#34-transações)
   - 3.5 [Vesting — Planos](#35-vesting--planos)
   - 3.6 [Vesting — Grants](#36-vesting--grants)
4. [Valuation](#4-valuation)
   - 4.1 [Valuation](#41-valuation)
   - 4.2 [Financeiro](#42-financeiro)
   - 4.3 [Documentos](#43-documentos)
   - 4.4 [Data Room](#44-data-room)
5. [Contratos](#5-contratos)
   - 5.1 [Contratos](#51-contratos)
   - 5.2 [Templates de Contratos](#52-templates-de-contratos)
6. [Comunicações](#6-comunicações)
   - 6.1 [Comunicações](#61-comunicações)
   - 6.2 [Notificações](#62-notificações)
7. [Aprovações](#7-aprovações)
   - 7.1 [Fluxos de Aprovação](#71-fluxos-de-aprovação)
   - 7.2 [Aprovadores](#72-aprovadores)
   - 7.3 [Aprovações](#73-aprovações)
8. [Administração](#8-administração)
   - 8.1 [Usuários](#81-usuários)
   - 8.2 [Perfis de Acesso](#82-perfis-de-acesso)
9. [Acessórios](#9-acessórios)
   - 9.1 [Fórmulas Customizadas](#91-fórmulas-customizadas)
   - 9.2 [Cláusulas](#92-cláusulas)
   - 9.3 [Templates de Milestone](#93-templates-de-milestone)

---

## 1. Login

**Rota:** `/login`

Tela de acesso ao sistema. O usuário informa e-mail e senha cadastrados. O sistema valida as credenciais via JWT e, em caso de sucesso, redireciona para o Dashboard Principal. Há opção de "Lembrar-me" para manter a sessão ativa.

![Login](prints/01_login.png)

---

## 2. Dashboards

### 2.1 Dashboard Principal

**Rota:** `/dashboard`

Visão geral consolidada de toda a empresa: gráfico de distribuição do Cap Table por sócio/classe de ações, eventos de vesting próximos, últimas comunicações publicadas e alertas de ação pendente (contratos a assinar, aprovações aguardando). Ponto de entrada diário do usuário.

![Dashboard](prints/02_dashboard.png)

---

### 2.2 Valuation Dashboard

**Rota:** `/valuations/dashboard`

Painel analítico de valuations. Exibe a evolução histórica do valor da empresa ao longo dos rounds, comparação entre metodologias (DCF, Múltiplos, Berkus, Scorecard e fórmulas customizadas), e métricas-chave como Pré-Money, Pós-Money e diluição por rodada.

![Valuation Dashboard](prints/03_valuation_dashboard.png)

---

### 2.3 Financeiro Dashboard

**Rota:** `/financial/dashboard`

Resumo financeiro da empresa: receita recorrente mensal (MRR), despesas, balanço por período e indicadores de saúde financeira. Integrado com os registros de períodos financeiros cadastrados no módulo Financeiro.

![Financeiro Dashboard](prints/04_financial_dashboard.png)

---

### 2.4 Meu Vesting

**Rota:** `/my-vesting`

Visão personalizada do usuário logado sobre seus próprios grants de vesting: percentual já adquirido (vested), cliff restante, calendário de aquisição futura e valor estimado das cotas com base no último valuation registrado.

![Meu Vesting](prints/05_meu_vesting.png)

---

### 2.5 Portal do Investidor

**Rota:** `/investor`

Interface simplificada voltada para investidores: mostra o portfólio de participações, últimas comunicações da empresa, eventos de vesting relevantes, valuations compartilhados e documentos disponíveis no data room. Acesso controlado por perfil de investidor.

![Portal do Investidor](prints/06_portal_investidor.png)

---

## 3. Cap Table

### 3.1 Empresas

**Rota:** `/companies`

Cadastro e gerenciamento das empresas do grupo. Cada empresa é um tenant independente com seu próprio Cap Table, usuários e configurações. Permite criar, editar e inativar empresas, além de configurar dados como CNPJ, endereço e responsável.

![Empresas](prints/07_empresas.png)

---

### 3.2 Sócios

**Rota:** `/shareholders`

Lista completa dos sócios/acionistas da empresa. Para cada sócio é possível visualizar participação atual (%), classe de ações detidas, histórico de investimentos e documentos vinculados. Suporta pessoas físicas e jurídicas e permite acesso ao perfil detalhado de cada um.

![Sócios](prints/08_socios.png)

---

### 3.3 Cap Table

**Rota:** `/cap-table`

Tabela de capitalização da empresa em tempo real. Exibe todas as classes de ações (Ordinárias, Preferenciais, SAFEs, Opções), quantidade emitida, percentual por titular e diluição. Gráfico de pizza interativo ilustra a distribuição de participação.

![Cap Table](prints/09_cap_table.png)

---

### 3.4 Transações

**Rota:** `/cap-table/transactions`

Histórico completo de todas as movimentações societárias: emissões de novas ações, transferências, conversões de instrumentos (SAFE, notas conversíveis), exercício de opções e amortizações. Cada transação registra data, partes envolvidas, quantidade e valor.

![Transações](prints/10_cap_table_transacoes.png)

---

### 3.5 Vesting — Planos

**Rota:** `/vesting`

Gerenciamento dos planos de vesting da empresa. Permite configurar planos com diferentes regras de carência (cliff), período total de vesting, frequência de aquisição (mensal, trimestral, anual) e condições especiais (milestones, aceleração por saída). Visão consolidada de todos os planos ativos.

![Vesting Planos](prints/11_vesting_planos.png)

---

### 3.6 Vesting — Grants

**Rota:** `/vesting/grants`

Lista de concessões individuais de opções/ações vinculadas a planos de vesting. Cada grant exibe o beneficiário, plano associado, data de início, quantidade total concedida, quantidade já adquirida e saldo restante. Permite criar novos grants e acompanhar o status de cada um.

![Vesting Grants](prints/12_vesting_grants.png)

---

## 4. Valuation

### 4.1 Valuation

**Rota:** `/valuations`

Lista de todas as avaliações de valuation registradas para a empresa, organizadas por data. Suporta múltiplos métodos de avaliação (DCF, Múltiplos de Mercado, Berkus, Scorecard, First Chicago e fórmulas customizadas). Permite criar novos valuations e comparar rodadas distintas lado a lado.

![Valuation](prints/13_valuation_lista.png)

---

### 4.2 Financeiro

**Rota:** `/financial`

Módulo de gestão financeira por períodos (meses/anos). Registra receitas, despesas, indicadores de MRR/ARR e métricas operacionais. Os dados financeiros alimentam os modelos de valuation (especialmente DCF) e o Dashboard Financeiro.

![Financeiro](prints/14_financeiro.png)

---

### 4.3 Documentos

**Rota:** `/documents`

Repositório de documentos vinculados à empresa e seus módulos (contratos, valuations, sócios, etc.). Permite upload, categorização, controle de versão e download. Documentos podem ser marcados como confidenciais, com acesso restrito por perfil.

![Documentos](prints/15_documentos.png)

---

### 4.4 Data Room

**Rota:** `/dataroom`

Espaço virtual seguro para compartilhamento de documentos sensíveis com investidores e partes externas durante processos de due diligence ou rodadas de investimento. Organizado em pastas com controle granular de visibilidade (interno, investidores, público).

![Data Room](prints/16_data_room.png)

---

## 5. Contratos

### 5.1 Contratos

**Rota:** `/contracts`

Gerenciamento completo do ciclo de vida dos contratos: criação a partir de templates ou do zero, envio para assinatura (física ou eletrônica), controle de versões e armazenamento. Permite filtrar contratos por status (rascunho, em revisão, assinado, vencido) e vinculá-los a sócios, grants ou rodadas.

![Contratos](prints/17_contratos.png)

---

### 5.2 Templates de Contratos

**Rota:** `/contracts/templates` *(requer role Admin ou Legal)*

Biblioteca de modelos de contratos reutilizáveis. Cada template pode conter cláusulas dinâmicas (variáveis que são preenchidas automaticamente com dados do sistema, como nome do sócio, percentual e data). Facilita a padronização e agiliza a criação de novos contratos.

![Templates de Contratos](prints/18_contratos_templates.png)

---

## 6. Comunicações

### 6.1 Comunicações

**Rota:** `/communications`

Central de comunicados internos e para investidores. Permite criar, rascunhar, agendar e publicar anúncios, atualizações, relatórios e alertas. Cada comunicado pode ter visibilidade configurável (todos, investidores, fundadores, colaboradores) e suporta fixar posts importantes. Ao publicar, o sistema gera notificações automáticas para os destinatários.

![Comunicações](prints/19_comunicacoes.png)

---

### 6.2 Notificações

**Rota:** `/notifications`

Central de notificações do usuário logado. Lista todas as notificações recebidas (comunicados publicados, fluxos de aprovação atribuídos, contratos prontos para assinar, eventos de vesting, etc.) com indicador de lidas/não lidas. Permite marcar individualmente ou em lote como lida.

![Notificações](prints/20_notificacoes.png)

---

## 7. Aprovações

### 7.1 Fluxos de Aprovação

**Rota:** `/approvals/flows`

Criação e gerenciamento de fluxos de aprovação multi-etapas. Cada fluxo define uma sequência ordenada de etapas (aprovação, revisão, notificação ou automático) com responsável por papel (Admin, Legal, Finance, etc.) e prazo individual por etapa. O prazo total do fluxo é calculado automaticamente pela soma cumulativa das etapas. Permite filtrar fluxos por tipo, status e prioridade.

![Fluxos de Aprovação](prints/21_aprovacoes_fluxos.png)

---

### 7.2 Aprovadores

**Rota:** `/approvals/approvers`

Gerenciamento de quais usuários têm perfil de aprovador. Exibe um resumo dos papéis com responsabilidade de aprovação (Admin, Legal, Finance, BoardMember) e lista os usuários em cada grupo. Permite adicionar ou remover papéis de aprovação por usuário diretamente nesta tela.

![Aprovadores](prints/22_aprovacoes_aprovadores.png)

---

### 7.3 Aprovações

**Rota:** `/approvals`

Fila de aprovações pendentes para o usuário logado e visão geral de todos os fluxos em andamento. Organizado em abas: *Pendentes para mim*, *Em andamento*, *Aprovados*, *Rejeitados* e *Todos*. Permite aprovar, rejeitar ou solicitar alterações com comentários.

![Aprovações](prints/23_aprovacoes.png)

---

## 8. Administração

### 8.1 Usuários

**Rota:** `/settings/users`

Gestão completa de usuários do sistema: criação, edição, ativação/inativação e atribuição de papéis (roles). Permite buscar por nome ou e-mail, filtrar por status e perfil. Cada usuário pode ter múltiplos papéis que determinam o que pode visualizar e editar.

![Usuários](prints/24_admin_usuarios.png)

---

### 8.2 Perfis de Acesso

**Rota:** `/settings/roles`

Visão consolidada de todos os papéis disponíveis no sistema (SuperAdmin, Admin, Founder, BoardMember, Legal, Finance, HR, Employee, Investor) com a quantidade de usuários em cada um. Permite gerenciar as atribuições de perfil por usuário de forma centralizada.

![Perfis de Acesso](prints/25_admin_perfis.png)

---

## 9. Acessórios

### 9.1 Fórmulas Customizadas

**Rota:** `/valuations/custom-formulas`

Criação e gerenciamento de fórmulas de valuation personalizadas para a empresa. Permite definir variáveis, operações matemáticas e pesos específicos do negócio. As fórmulas customizadas ficam disponíveis como um método adicional de avaliação ao criar novos valuations.

![Fórmulas Customizadas](prints/26_acessorios_formulas.png)

---

### 9.2 Cláusulas

**Rota:** `/contracts/clauses` *(requer role Admin ou Legal)*

Biblioteca de cláusulas contratuais reutilizáveis. Cada cláusula tem título, texto, categoria e pode ser marcada como obrigatória. As cláusulas são referenciadas nos templates de contratos, permitindo atualizar o texto em um único lugar e propagar para todos os templates que a utilizam.

![Cláusulas](prints/27_acessorios_clausulas.png)

---

### 9.3 Templates de Milestone

**Rota:** `/vesting/milestone-templates` *(requer role Admin)*

Modelos pré-configurados de marcos (milestones) para uso em planos de vesting baseados em eventos. Por exemplo: "Atingir R$ 1M de receita", "Lançamento do produto", "Aprovação de patente". Ao criar um plano de vesting por milestone, o gestor pode selecionar templates desta biblioteca ao invés de criar do zero.

![Templates de Milestone](prints/28_acessorios_milestones.png)

---

## Informações Técnicas

| Item | Detalhe |
|------|---------|
| Plataforma | Web — acesso via navegador moderno |
| Autenticação | JWT com refresh token |
| Permissões | Controle por Roles (perfis de acesso) |
| Dados | Isolamento por empresa (multi-tenant) |
| Idioma | Português (BR) |

---

*Documento gerado automaticamente em março de 2026.*
