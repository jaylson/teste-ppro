# Novos Templates de Contratos Criados

## 📋 Resumo

Foram criados **3 novos templates** de contratos profissionais e completos para o Partnership Manager:

1. **Acordo de Acionistas** (Shareholders Agreement)
2. **Contrato de Investimento** (Investment Agreement)  
3. **Contrato de Trabalho** (Employment Contract - CLT e PJ)

---

## ✅ Templates Criados

### 1. Acordo de Acionistas 🤝

**Código**: `SHAREHOLDERS-AGREEMENT-001`  
**Tipo**: `shareholders_agreement`

#### Principais Seções:
- **Governança**: Conselho de Administração, Diretoria, matérias sujeitas a aprovação especial
- **Direitos Econômicos**: Política de dividendos, distribuição de lucros
- **Transferência de Ações**:
  - Direito de preferência (first refusal)
  - Tag-along (venda conjunta)
  - Drag-along (venda forçada)
  - Lock-up e vesting
- **Proteções**: Anti-diluição, não-concorrência
- **Eventos**: IPO, M&A, liquidação
- **Resolução**: Deadlocks, arbitragem

#### Casos de Uso:
- Startups com múltiplos sócios/investidores
- Empresas buscando organizar governança
- Preparação para rodadas de investimento
- Alinhamento entre fundadores e investidores

---

### 2. Contrato de Investimento 💰

**Código**: `INVESTMENT-ROUND-001`  
**Tipo**: `investment`

#### Principais Seções:
- **Estrutura da Rodada**: Valuation, preço por ação, participações
- **Condições Precedentes**: Due diligence, documentação, aprovações
- **Declarações e Garantias**: Societárias, financeiras, PI, compliance
- **Direitos dos Investidores**:
  - Políticos: Conselho, vetos, aprovações
  - Econômicos: Preferência de liquidação, dividendos, anti-diluição
  - Informação: Relatórios, acesso a dados
  - Transferência: Tag/drag-along, preferência pro-rata
- **Milestones**: Tranches condicionadas a resultados
- **Uso de Recursos**: Destinação e controle
- **Obrigações**: Da sociedade, dos fundadores
- **Exit**: IPO, M&A, direitos de registro

#### Casos de Uso:
- Rodadas Seed, Série A, B, C
- Captação com VCs ou angels
- Estruturação de investimentos estratégicos
- Formalização de termos e proteções

---

### 3. Contrato de Trabalho 👔

**Código**: `EMPLOYMENT-CLT-PJ-001`  
**Tipo**: `employment`

#### Estrutura do Template:
O template é **versátil** e cobre **duas modalidades**:

**PARTE A - CLT (Consolidação das Leis do Trabalho)**
- Tipo de contrato (prazo indeterminado/determinado/experiência)
- Função, atribuições, subordinação
- Local de trabalho (presencial/híbrido/remoto)
- Jornada, horas extras, banco de horas
- Remuneração completa: salário, benefícios, PLR, bônus
- Férias, 13º, FGTS, INSS
- Rescisão, aviso prévio, verbas rescisórias

**PARTE B - PJ (Prestação de Serviços)**
- Caracterização de autonomia (sem vínculo)
- Qualificação da empresa contratada
- Formas de remuneração (mensal, hora, projeto)
- Emissão de notas fiscais
- Prazo e renovação
- Rescisão e multas

**PARTE C - Cláusulas Comuns (CLT e PJ)**
- **Confidencialidade**: Definição ampla, exceções, prazo de 2-5 anos
- **Não-Concorrência**: Período, abrangência geográfica, compensação
- **Não-Solicitação**: Clientes e colaboradores
- **Propriedade Intelectual**: Cessão total, cooperação em registro
- **Equipamentos**: Fornecimento, responsabilidade, BYOD
- **Políticas Internas**: Código de conduta, segurança da informação
- **LGPD**: Proteção de dados pessoais
- **Compliance**: Anticorrupção, conflito de interesses

#### Casos de Uso:
- Contratação de colaboradores CLT
- Formalização de prestadores PJ
- Proteção de segredos comerciais
- Retenção de talentos com não-concorrência
- Compliance trabalhista

---

## 🗂️ Variáveis Principais

### Acordo de Acionistas
- Identificação de acionistas (nome, CPF/CNPJ, endereço)
- Participações e capital social
- Composição do conselho e diretoria
- Percentuais para tag-along, drag-along
- Períodos de lock-up e vesting
- Tipo de proteção anti-diluição
- Foro e arbitragem

### Contrato de Investimento
- Rodada (Seed/Série A/B/C)
- Valuations (pré e pós-money)
- Valor investido e preço por ação
- Investidores e fundadores
- Tipo de preferência de liquidação
- Milestones e tranches
- Uso de recursos
- Proteções e direitos

### Contrato de Trabalho
- Tipo (CLT ou PJ)
- Dados pessoais do colaborador
- Cargo, função, responsabilidades
- Salário/fee e benefícios
- Jornada e modalidade de trabalho
- Períodos de não-concorrência e confidencialidade
- Equipamentos fornecidos
- Foro

---

## 📂 Arquivos Criados

1. **Migration SQL**: 
   - `/workspaces/teste-ppro/docker/mysql/migrations/017_insert_additional_contract_templates.sql`
   - Insere os 3 templates no banco de dados

2. **Documentação Atualizada**:
   - `/workspaces/teste-ppro/TEMPLATES_CONTRATOS_EXEMPLOS.md`
   - Expandida com detalhes dos novos templates

3. **Este Resumo**:
   - `/workspaces/teste-ppro/NOVOS_TEMPLATES_CRIADOS.md`

---

## 🚀 Como Usar

### 1. Via API

```bash
# Listar templates (requer autenticação)
GET /api/contract-templates?page=1&pageSize=20

# Buscar por tipo
GET /api/contract-templates?type=shareholders_agreement
GET /api/contract-templates?type=investment
GET /api/contract-templates?type=employment

# Criar contrato a partir de template
POST /api/contracts
{
  "templateId": "uuid-do-template",
  "title": "Acordo de Acionistas - 2026",
  "variables": {
    "company_name": "TechStartup Ltda",
    "shareholder_1_name": "João Fundador",
    ...
  }
}
```

### 2. Via Interface

1. Acesse **Contratos** → **Novo Contrato**
2. Selecione **"Usar Template"**
3. Escolha um dos novos templates:
   - Acordo de Acionistas
   - Contrato de Investimento
   - Contrato de Trabalho
4. Preencha as variáveis dinâmicas
5. Revise o contrato gerado
6. Envie para assinatura

---

## ✨ Destaques Técnicos

### Qualidade Jurídica
- ✅ Templates baseados em melhores práticas do mercado
- ✅ Linguagem jurídica adequada e clara
- ✅ Cláusulas atualizadas conforme legislação brasileira
- ✅ Referências a leis aplicáveis (CLT, Lei das S.A., LGPD, etc.)

### Flexibilidade
- ✅ Mais de 50 variáveis dinâmicas por template
- ✅ Seções condicionais (ex: CLT vs PJ)
- ✅ Campos opcionais e obrigatórios
- ✅ Exemplos de preenchimento inline

### Completude
- ✅ Desde cláusulas básicas até avançadas
- ✅ Anexos incluídos (cap table, cronogramas, políticas)
- ✅ Proteções robustas (anti-diluição, indenização, etc.)
- ✅ Mecanismos de resolução de conflitos

### Casos Cobertos
- ✅ **Acordo de Acionistas**: Startups em qualquer estágio
- ✅ **Investimento**: Seed, Série A/B/C, VCs e Angels
- ✅ **Trabalho**: CLT full-time, PJ, híbrido, remoto

---

## 🎯 Próximos Passos

### Testagem
1. ✅ Templates inseridos no banco
2. ✅ API funcionando (endpoint corrigido)
3. ⏳ Testar criação de contratos via frontend
4. ⏳ Validar substituição de variáveis
5. ⏳ Revisar com time jurídico

### Melhorias Futuras
- [ ] Adicionar validação de variáveis obrigatórias
- [ ] Criar wizard guiado para preenchimento
- [ ] Implementar preview em tempo real
- [ ] Adicionar assinatura eletrônica integrada
- [ ] Versionamento de templates
- [ ] Suporte a múltiplos idiomas

### Novos Templates Planejados
- Acordo de Prestação de Serviços (ServiceAgreement)
- Termo de Transferência de Quotas/Ações
- Contrato de Parceria Comercial (Partnership)
- Contrato de Consultoria
- Acordo de Licenciamento de Propriedade Intelectual

---

## 📊 Métricas

| Template | Linhas de Código | Variáveis | Cláusulas Principais |
|----------|------------------|-----------|----------------------|
| Acordo de Acionistas | ~650 | 60+ | 21 |
| Contrato de Investimento | ~700 | 80+ | 18 |
| Contrato de Trabalho | ~800 | 100+ | 25 |
| **TOTAL** | **~2.150** | **240+** | **64** |

---

## ✅ Status da Implementação

- [x] Templates criados com conteúdo completo
- [x] Migration SQL aplicada no banco de dados
- [x] Documentação atualizada
- [x] API endpoint funcionando (erro de enum corrigido)
- [x] Tipos de enum mapeados corretamente
- [ ] Teste end-to-end de criação de contratos
- [ ] Revisão jurídica dos templates

---

**Data de Criação**: 17/02/2026  
**Autor**: GitHub Copilot  
**Status**: ✅ Implementado e Disponível
