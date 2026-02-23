# Templates de Contratos - Guia de Uso

Este documento descreve os templates de contratos exemplo criados para o sistema Partnership Manager.

## 📋 Templates Disponíveis

### 1. Acordo de Confidencialidade (NDA)

**Código**: `NDA-BILATERAL-001`  
**Tipo**: `Confidentiality`  
**Descrição**: Template bilateral de Acordo de Confidencialidade para proteger informações sensíveis compartilhadas entre empresas.

#### Características:
- ✅ NDA bilateral (ambas as partes protegem informações)
- ✅ Cláusulas de definição de informações confidenciais com exceções
- ✅ Obrigações de confidencialidade detalhadas
- ✅ Proteção de propriedade intelectual
- ✅ Regras de devolução e destruição de documentos
- ✅ Vigência configurável com extensão pós-término
- ✅ Penalidades por violação
- ✅ Foro de eleição

#### Variáveis Disponíveis:

| Variável | Descrição | Exemplo |
|----------|-----------|---------|
| `party_discloser_name` | Nome da empresa divulgadora | Ophir Tecnologia Ltda |
| `party_discloser_cnpj` | CNPJ da empresa divulgadora | 12.345.678/0001-90 |
| `party_discloser_address` | Endereço completo da divulgadora | Av. Paulista, 1000, São Paulo/SP |
| `party_discloser_representative_name` | Nome do representante legal | João Silva |
| `party_discloser_representative_role` | Cargo do representante | Diretor Executivo |
| `party_recipient_name` | Nome da empresa receptora | TechPartner Inovações SA |
| `party_recipient_cnpj` | CNPJ da receptora | 98.765.432/0001-10 |
| `party_recipient_address` | Endereço completo da receptora | Rua dos Investidores, 500, Rio/RJ |
| `party_recipient_representative_name` | Nome do representante receptor | Maria Santos |
| `party_recipient_representative_role` | Cargo do representante receptor | CEO |
| `business_purpose` | Finalidade do compartilhamento | negociação de investimento em rodada Série A |
| `contract_duration_months` | Duração do acordo em meses | 24 |
| `contract_start_date` | Data de início | 17/02/2026 |
| `confidentiality_duration_years` | Anos de confidencialidade pós-término | 5 |
| `penalty_amount` | Valor da multa por violação | 500.000,00 |
| `jurisdiction_city` | Cidade do foro | São Paulo |
| `jurisdiction_state` | Estado do foro | SP |
| `contract_city` | Cidade da assinatura | São Paulo |
| `contract_date` | Data da assinatura | 17 de fevereiro de 2026 |

---

### 2. Plano de Opções de Ações (Stock Option Plan)

**Código**: `STOCK-OPTION-001`  
**Tipo**: `StockOption`  
**Descrição**: Template completo de Contrato de Outorga de Opção de Compra de Ações para incentivar e reter colaboradores chave.

#### Características:
- ✅ Definição clara de quantidade de opções e percentual de participação
- ✅ Preço de exercício (strike price) configurável
- ✅ Período de vesting (carência) personalizável
- ✅ Cronograma de vesting com cliff e liberação gradual
- ✅ Regras para desligamento (justa causa, sem justa causa, demissão)
- ✅ Cláusulas de aceleração em eventos (IPO, M&A)
- ✅ Lock-up e restrições de transferência
- ✅ Aspectos tributários
- ✅ Confidencialidade
- ✅ Anexos (cronograma detalhado e formulário de exercício)

#### Variáveis Disponíveis:

| Variável | Descrição | Exemplo |
|----------|-----------|---------|
| `company_name` | Nome da empresa | StartupTech Inovações Ltda |
| `company_cnpj` | CNPJ da empresa | 11.222.333/0001-44 |
| `company_address` | Endereço da empresa | Av. Brigadeiro Faria Lima, 1500, São Paulo/SP |
| `company_representative_name` | Nome do representante da empresa | Carlos Fundador |
| `company_representative_role` | Cargo do representante | CEO e Sócio Fundador |
| `beneficiary_name` | Nome completo do beneficiário | Ana Paula Desenvolvedora |
| `beneficiary_nationality` | Nacionalidade | brasileira |
| `beneficiary_marital_status` | Estado civil | solteira |
| `beneficiary_profession` | Profissão | engenheira de software |
| `beneficiary_cpf` | CPF do beneficiário | 123.456.789-00 |
| `beneficiary_address` | Endereço do beneficiário | Rua das Flores, 100, São Paulo/SP |
| `total_options` | Número total de opções | 10.000 |
| `ownership_percentage` | Percentual do capital social | 2,5 |
| `strike_price` | Preço de exercício por ação | 5,00 |
| `valuation_date` | Data da avaliação/valuation | 01/01/2026 |
| `vesting_period_years` | Período total de vesting em anos | 4 |
| `grant_date` | Data da outorga | 17/02/2026 |
| `vesting_schedule` | Descrição do cronograma de vesting | Ver exemplo abaixo |
| `cliff_percentage` | Percentual após cliff | 25 |
| `monthly_vesting_percentage` | Percentual mensal após cliff | 2,08 |
| `exercise_start_date` | Data inicial para exercício | 17/02/2027 |
| `exercise_end_date` | Data final para exercício | 17/02/2036 |
| `exercise_notice_days` | Dias de antecedência para exercício | 30 |
| `termination_exercise_days` | Prazo para exercício após demissão | 90 |
| `resignation_exercise_days` | Prazo após pedido de demissão | 30 |
| `lockup_period_months` | Período de lock-up em meses | 12 |
| `ipo_exercise_window_days` | Janela de exercício em IPO | 180 |
| `jurisdiction_city` | Cidade do foro | São Paulo |
| `jurisdiction_state` | Estado do foro | SP |
| `contract_city` | Cidade da assinatura | São Paulo |
| `contract_signature_date` | Data da assinatura | 17 de fevereiro de 2026 |
| `vesting_schedule_detailed` | Cronograma detalhado (anexo) | Ver tabela exemplo |

#### Exemplo de Cronograma de Vesting:

```
Cronograma padrão de 4 anos com cliff de 1 ano:

- Ano 1 (cliff): 25% das opções (2.500 opções)
- Ano 2: 25% das opções (2.500 opções) - liberação mensal de 208 opções
- Ano 3: 25% das opções (2.500 opções) - liberação mensal de 208 opções
- Ano 4: 25% das opções (2.500 opções) - liberação mensal de 208 opções
```

---

## 🚀 Como Usar os Templates

### 1. Via API

```bash
# Buscar templates disponíveis
GET /api/contract-templates?templateType=Confidentiality

# Criar contrato a partir de template
POST /api/contracts
{
  "templateId": "uuid-do-template",
  "title": "NDA - Parceria com TechPartner",
  "variables": {
    "party_discloser_name": "Ophir Tecnologia Ltda",
    "party_recipient_name": "TechPartner SA",
    "business_purpose": "negociação de investimento",
    "contract_duration_months": "24",
    ...
  }
}
```

### 2. Via Interface Web

1. Acesse **Contratos** → **Novo Contrato**
2. Selecione **"Usar Template"**
3. Escolha o template desejado (NDA ou Stock Option)
4. Preencha as variáveis no formulário
5. Revise o contrato gerado
6. Envie para assinatura

### 3. Aplicar Migration

Para adicionar os templates ao banco de dados:

```bash
# Executar migration diretamente
mysql -h <host> -u <user> -p <database> < docker/mysql/migrations/016_insert_contract_templates_examples.sql

# Ou via Docker Compose (se configurado)
docker exec -i mysql-container mysql -u root -p <database> < /docker-entrypoint-initdb.d/migrations/016_insert_contract_templates_examples.sql
```

---

## 📝 Boas Práticas

### Personalização de Templates

1. **Clone antes de modificar**: Use a funcionalidade de clone para criar versões personalizadas
2. **Versionamento**: Mantenha versões do template para rastreabilidade
3. **Revisão jurídica**: Sempre valide templates customizados com advogado
4. **Teste as variáveis**: Verifique se todas as variáveis são substituídas corretamente

### Variáveis

1. **Nomenclatura**: Use snake_case para variáveis (ex: `party_name`)
2. **Formato**: Variáveis são delimitadas por `{{variavel}}`
3. **Validação**: Configure validações no frontend para campos obrigatórios
4. **Valores padrão**: Considere definir valores padrão quando aplicável

### Segurança

1. **Confidencialidade**: Templates podem conter informações sensíveis
2. **Controle de acesso**: Limite quem pode criar/editar templates
3. **Auditoria**: Mantenha log de quem usa e modifica templates
4. **Backup**: Faça backup regular dos templates

---

---

### 3. Acordo de Acionistas (Shareholders Agreement)

**Código**: `SHAREHOLDERS-AGREEMENT-001`  
**Tipo**: `ShareholdersAgreement`  
**Descrição**: Template completo de Acordo de Acionistas com cláusulas de tag-along, drag-along, direito de preferência e governança corporativa.

#### Características:
- ✅ Estrutura de governança corporativa (Conselho e Diretoria)
- ✅ Matérias sujeitas a aprovação especial
- ✅ Política de dividendos
- ✅ Direito de preferência em novas emissões
- ✅ First Refusal (preferência na venda)
- ✅ Tag-along (direito de venda conjunta)
- ✅ Drag-along (venda forçada)
- ✅ Lock-up e vesting de fundadores
- ✅ Proteção anti-diluição
- ✅ Cláusulas de não-concorrência e não-solicitação
- ✅ Resolução de deadlocks
- ✅ Eventos de liquidez (IPO, M&A)

#### Principais Variáveis:

| Variável | Descrição |
|----------|-----------|
| `company_name` | Nome da sociedade |
| `shareholder_X_name` | Nome de cada acionista |
| `shareholder_X_ownership` | Percentual de participação |
| `board_members_total` | Total de conselheiros |
| `tag_along_threshold` | Percentual mínimo para tag-along |
| `drag_along_threshold` | Percentual para drag-along |
| `lockup_period_years` | Período de lock-up |
| `founder_vesting_years` | Período de vesting dos fundadores |
| `anti_dilution_type` | Tipo de proteção anti-diluição |
| `ipo_approval_quorum` | Quorum para aprovação de IPO |

---

### 4. Contrato de Investimento (Investment Agreement)

**Código**: `INVESTMENT-ROUND-001`  
**Tipo**: `Investment`  
**Descrição**: Template de Contrato de Investimento para rounds de captação (Seed, Série A, B, C) com termos, direitos dos investidores e condições precedentes.

#### Características:
- ✅ Estrutura completa para rounds de investimento
- ✅ Valuation pré e pós-money
- ✅ Condições precedentes detalhadas
- ✅ Declarações e garantias (representations & warranties)
- ✅ Direitos políticos (assentos no conselho, vetos)
- ✅ Direitos econômicos (preferência de liquidação, dividendos)
- ✅ Direitos de informação e reporting
- ✅ Preferência pro-rata em rodadas futuras
- ✅ Proteção anti-diluição
- ✅ Milestones e tranches condicionadas
- ✅ Cláusulas de uso de recursos
- ✅ Direitos de registro (IPO)

#### Principais Variáveis:

| Variável | Descrição |
|----------|-----------|
| `round_name` | Nome da rodada (Seed/Série A/B/C) |
| `pre_money_valuation` | Valuation pré-investimento |
| `post_money_valuation` | Valuation pós-investimento |
| `total_round_amount` | Valor total da rodada |
| `price_per_share` | Preço por ação |
| `liquidation_preference` | Tipo de preferência de liquidação |
| `investor_board_seats` | Assentos de investidores no conselho |
| `anti_dilution_protection` | Tipo de proteção anti-diluição |
| `use_of_proceeds` | Destinação dos recursos |
| `milestones` | Milestones para tranches |

---

### 5. Contrato de Trabalho (Employment Contract)

**Código**: `EMPLOYMENT-CLT-PJ-001`  
**Tipo**: `Employment`  
**Descrição**: Template versátil de Contrato de Trabalho incluindo modalidades CLT e PJ/Prestação de Serviços, com cláusulas de não concorrência e confidencialidade.

#### Características:
- ✅ **Modalidade CLT completa**:
  - Tipo de contrato (prazo determinado/indeterminado/experiência)
  - Jornada de trabalho e banco de horas
  - Remuneração e benefícios completos
  - Férias, 13º, FGTS, INSS
  - Aviso prévio e verbas rescisórias
  
- ✅ **Modalidade PJ/Prestação de Serviços**:
  - Autonomia e ausência de subordinação
  - Formas de remuneração (fixo, hora, projeto)
  - Emissão de notas fiscais
  - Rescisão e multas
  
- ✅ **Cláusulas Comuns**:
  - Confidencialidade detalhada
  - Não-concorrência com abrangência geográfica
  - Não-solicitação (clientes e colaboradores)
  - Propriedade intelectual
  - Compliance e LGPD
  - Equipamentos e recursos

#### Principais Variáveis:

| Variável | Descrição |
|----------|-----------|
| `contract_type` | CLT ou PJ |
| `employee_name` | Nome do(a) contratado(a) |
| `job_title` | Cargo/função |
| `base_salary` | Salário base (CLT) |
| `pj_monthly_fee` | Valor mensal (PJ) |
| `work_mode` | Presencial/Híbrido/Remoto |
| `weekly_hours` | Carga horária semanal |
| `non_compete_period_years` | Anos de não-concorrência |
| `confidentiality_period_years` | Anos de confidencialidade |
| `benefits` | Pacote de benefícios |

---

## 🔄 Próximos Templates Sugeridos

### Templates Adicionais:

4. **Acordo de Prestação de Serviços** (ServiceAgreement)
   - Escopo de trabalho detalhado
   - Prazos e entregas
   - Forma de pagamento
   - SLA (Service Level Agreement)

5. **Termo de Transferência de Quotas/Ações**
   - Venda de participação societária
   - Formas de pagamento
   - Garantias e responsabilidades

6. **Contrato de Parceria Comercial** (Partnership)
   - Objetivos e responsabilidades
   - Comissionamento
   - Exclusividade territorial

---

## 📞 Suporte

Para dúvidas sobre os templates ou para solicitar novos templates, entre em contato com a equipe de desenvolvimento.

**Última atualização**: 17/02/2026
