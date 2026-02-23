-- Migration 017: Inserir Templates de Contratos Adicionais
-- Templates: Acordo de Acionistas, Contrato de Investimento, Contrato de Trabalho
-- Date: 17/02/2026

USE ppro;

-- =====================================================
-- Template 1: Acordo de Acionistas (ShareholdersAgreement)
-- =====================================================

INSERT INTO contract_templates (
    id,
    client_id,
    company_id,
    name,
    description,
    code,
    template_type,
    content,
    default_status,
    tags,
    version,
    is_active,
    created_at,
    updated_at,
    is_deleted
) VALUES (
    UUID(),
    '00000000-0000-0000-0000-000000000001',
    NULL,
    'Acordo de Acionistas',
    'Template completo de Acordo de Acionistas (Shareholders Agreement) com cláusulas de tag-along, drag-along, direito de preferência e governança corporativa.',
    'SHAREHOLDERS-AGREEMENT-001',
    'shareholders_agreement',
    '# ACORDO DE ACIONISTAS
## (SHAREHOLDERS AGREEMENT)

## PARTES

As partes qualificadas a seguir:

**SOCIEDADE**: {{company_name}}, sociedade empresária limitada/anônima, inscrita no CNPJ sob o nº {{company_cnpj}}, com sede na {{company_address}}, doravante denominada "SOCIEDADE" ou "COMPANHIA".

**ACIONISTAS**:

1. {{shareholder_1_name}}, {{shareholder_1_nationality}}, {{shareholder_1_marital_status}}, {{shareholder_1_profession}}, portador do CPF/CNPJ nº {{shareholder_1_document}}, residente/sediado na {{shareholder_1_address}}, titular de {{shareholder_1_ownership}}% do capital social.

2. {{shareholder_2_name}}, {{shareholder_2_nationality}}, {{shareholder_2_marital_status}}, {{shareholder_2_profession}}, portador do CPF/CNPJ nº {{shareholder_2_document}}, residente/sediado na {{shareholder_2_address}}, titular de {{shareholder_2_ownership}}% do capital social.

{{additional_shareholders}}

Doravante denominados individualmente "ACIONISTA" ou coletivamente "ACIONISTAS".

## PREÂMBULO

CONSIDERANDO QUE os Acionistas são os únicos titulares das ações representativas do capital social da Sociedade;

CONSIDERANDO QUE os Acionistas desejam estabelecer regras claras de governança, transferência de participações e proteção de direitos;

CONSIDERANDO QUE é do interesse comum dos Acionistas regular suas relações e a administração da Sociedade;

Os Acionistas celebram o presente Acordo de Acionistas, regido pelas seguintes cláusulas e condições:

## CLÁUSULA 1 - OBJETO E FINALIDADE

1.1. O presente Acordo tem por objeto regular os direitos e obrigações dos Acionistas em relação à Sociedade, estabelecendo regras sobre:
- Governança corporativa e administração
- Transferência de participações societárias
- Direitos de preferência e acompanhamento
- Política de dividendos e reinvestimento
- Resolução de conflitos

1.2. Este Acordo vincula todos os Acionistas e seus sucessores a qualquer título.

## CLÁUSULA 2 - COMPOSIÇÃO DO CAPITAL SOCIAL

2.1. O capital social da Sociedade está assim distribuído:

{{capital_distribution_table}}

Exemplo:
| Acionista | Ações/Quotas | Percentual | Classe |
|-----------|--------------|------------|--------|
| {{shareholder_1_name}} | {{shareholder_1_shares}} | {{shareholder_1_ownership}}% | {{shareholder_1_class}} |
| {{shareholder_2_name}} | {{shareholder_2_shares}} | {{shareholder_2_ownership}}% | {{shareholder_2_class}} |

2.2. Total de ações/quotas emitidas: {{total_shares}}
2.3. Valor patrimonial por ação/quota: R$ {{share_value}}

## CLÁUSULA 3 - GOVERNANÇA CORPORATIVA

### 3.1. CONSELHO DE ADMINISTRAÇÃO

3.1.1. A Sociedade será administrada por um Conselho de Administração composto por {{board_members_total}} membros.

3.1.2. Distribuição de assentos:
{{board_seats_distribution}}

Exemplo:
- {{shareholder_1_name}}: {{shareholder_1_board_seats}} assento(s)
- {{shareholder_2_name}}: {{shareholder_2_board_seats}} assento(s)
- Membro independente: {{independent_board_seats}} assento(s)

3.1.3. Mandato dos conselheiros: {{board_term_years}} anos, permitida recondução.

3.1.4. Presidente do Conselho: {{board_chairman_appointment}}

### 3.2. DIRETORIA EXECUTIVA

3.2.1. A Sociedade terá uma Diretoria Executiva composta por:
- CEO (Chief Executive Officer): {{ceo_name}}
- CFO (Chief Financial Officer): {{cfo_name}}
- {{additional_officers}}

3.2.2. A nomeação e destituição de diretores requer aprovação de {{officer_approval_quorum}}% dos Acionistas.

### 3.3. MATÉRIAS SUJEITAS A APROVAÇÃO ESPECIAL

3.3.1. As seguintes matérias requerem aprovação de {{special_matters_quorum}}% dos Acionistas:

a) Alteração do objeto social ou estatuto/contrato social
b) Fusão, cisão, incorporação ou transformação
c) Aumento ou redução do capital social
d) Emissão de valores mobiliários (ações, debêntures, bônus)
e) Contratação de endividamento acima de {{debt_threshold}}
f) Aquisição ou alienação de ativos acima de {{asset_threshold}}
g) Distribuição de dividendos fora da política estabelecida
h) Aprovação do plano de negócios anual e orçamento
i) Contratação ou dispensa de auditores independentes
j) Criação de stock option plan ou plano de remuneração variável
k) Abertura de capital (IPO)
l) {{additional_special_matters}}

## CLÁUSULA 4 - POLÍTICA DE DIVIDENDOS

4.1. A Sociedade distribuirá, no mínimo, {{minimum_dividend_percentage}}% do lucro líquido ajustado como dividendos, observada a necessidade de reinvestimento.

4.2. A distribuição de dividendos acima do mínimo legal dependerá de aprovação de {{dividend_approval_quorum}}% dos Acionistas.

4.3. Prioridade de distribuição:
{{dividend_priority}}

## CLÁUSULA 5 - DIREITO DE PREFERÊNCIA

5.1. **Preferência em Novas Emissões**: Em caso de aumento de capital, os Acionistas terão direito de preferência para subscrever novas ações na proporção de sua participação, pelo prazo de {{preemptive_right_days}} dias.

5.2. **Rateio de Sobras**: Ações não subscritas serão oferecidas aos demais Acionistas que manifestaram interesse, pro rata à sua participação.

5.3. **Renúncia à Preferência**: A renúncia ao direito de preferência deve ser expressa e por escrito.

## CLÁUSULA 6 - DIREITO DE PREFERÊNCIA NA VENDA (FIRST REFUSAL)

6.1. Nenhum Acionista poderá vender, total ou parcialmente, suas ações a terceiros sem antes oferecer aos demais Acionistas pelo mesmo preço e condições.

6.2. **Procedimento**:
a) Acionista vendedor notifica os demais com oferta detalhada
b) Prazo para exercício: {{rofr_exercise_days}} dias
c) Rateio pro rata se houver interesse de múltiplos Acionistas
d) Se não exercido, vendedor pode vender ao terceiro nas mesmas condições

6.3. Validade da autorização: {{rofr_validity_days}} dias após recusa dos Acionistas.

## CLÁUSULA 7 - TAG-ALONG (DIREITO DE VENDA CONJUNTA)

7.1. Na venda de participação representando {{tag_along_threshold}}% ou mais do capital social a terceiro, os demais Acionistas terão o direito de vender suas ações nas mesmas condições.

7.2. **Percentual de Tag-Along**:
- Acionistas fundadores: {{founder_tag_along_percentage}}%
- Investidores: {{investor_tag_along_percentage}}%
- Demais Acionistas: {{other_tag_along_percentage}}%

7.3. Prazo para exercício: {{tag_along_exercise_days}} dias após notificação.

7.4. O comprador deve ser notificado e aceitar a adesão dos demais Acionistas.

## CLÁUSULA 8 - DRAG-ALONG (DIREITO DE VENDA FORÇADA)

8.1. Acionistas representando {{drag_along_threshold}}% ou mais do capital social poderão obrigar os minoritários a vender suas ações em conjunto.

8.2. **Condições para Drag-Along**:
a) Oferta de terceiro bona fide
b) Preço e condições justas e uniformes para todos
c) Notificação prévia de {{drag_along_notice_days}} dias
d) Garantias e indenizações pro rata

8.3. **Proteções aos Minoritários**:
- Mesmo preço por ação/quota
- Mesmas condições de pagamento
- Limitação de responsabilidade proporcional à participação
- Garantias limitadas a {{drag_along_warranty_cap}}

## CLÁUSULA 9 - DIREITO DE ACOMPANHAMENTO (CO-SALE)

9.1. Minoritários têm direito de acompanhar vendas parciais (abaixo do threshold de tag-along) mediante notificação.

9.2. Aplicável a vendas superiores a {{co_sale_threshold}}% da participação do vendedor.

## CLÁUSULA 10 - LOCK-UP (RESTRIÇÃO DE TRANSFERÊNCIA)

10.1. Nenhum Acionista poderá transferir suas ações durante o período de {{lockup_period_years}} anos contados de {{lockup_start_date}}.

10.2. **Exceções ao Lock-Up**:
a) Transferências para cônjuge, descendentes ou ascendentes
b) Transferências para holding familiar
c) Transferências entre Acionistas signatários deste Acordo
d) Aprovação unânime dos demais Acionistas
e) {{lockup_exceptions}}

## CLÁUSULA 11 - VESTING DE AÇÕES DOS FUNDADORES

11.1. As ações dos fundadores estarão sujeitas a vesting pelo período de {{founder_vesting_years}} anos:

{{founder_vesting_schedule}}

Exemplo padrão:
- Cliff de 1 ano: 25%
- Meses seguintes: liberação mensal de 2,08% até 100%

11.2. Em caso de saída antes do vesting completo, ações não vestidas retornam à Sociedade ou são redistribuídas.

## CLÁUSULA 12 - EVENTOS DE LIQUIDEZ

### 12.1. IPO (OFERTA PÚBLICA INICIAL)

12.1.1. A decisão de realizar IPO requer aprovação de {{ipo_approval_quorum}}%.

12.1.2. Lock-up pós-IPO: {{ipo_lockup_months}} meses.

### 12.2. VENDA DA EMPRESA (M&A)

12.2.1. Venda total da Sociedade requer aprovação de {{ma_approval_quorum}}%.

12.2.2. Aplicam-se as regras de drag-along.

### 12.3. DIREITO DE VENDA FORÇADA (DEMAND REGISTRATION)

12.3.1. Acionistas representando {{demand_registration_threshold}}% podem exigir processo de venda após {{demand_registration_years}} anos.

## CLÁUSULA 13 - ANTI-DILUIÇÃO

13.1. Em rodadas de investimento futuras com valuation inferior ("down round"), investidores terão proteção anti-diluição {{anti_dilution_type}}.

Tipos:
- **Full Ratchet**: ajuste total ao preço mais baixo
- **Weighted Average**: média ponderada (broad-based ou narrow-based)

13.2. Cálculo de ajuste conforme fórmula:
{{anti_dilution_formula}}

## CLÁUSULA 14 - NÃO CONCORRÊNCIA E NÃO SOLICITAÇÃO

14.1. Durante a vigência deste Acordo e por {{non_compete_years}} anos após saída, os Acionistas não poderão:
a) Atuar em negócio concorrente
b) Solicitar clientes da Sociedade
c) Aliciar colaboradores da Sociedade

14.2. Abrangência geográfica: {{non_compete_geography}}

14.3. Pagamento de compensação: {{non_compete_compensation}}

## CLÁUSULA 15 - RESOLUÇÃO DE CONFLITOS

15.1. **Deadlock**: Em caso de impasse em matérias essenciais:
a) Mediação por {{deadlock_mediator}} em {{deadlock_mediation_days}} dias
b) Se não resolvido: {{deadlock_resolution_mechanism}}

Opções:
- Buy-sell (Russian Roulette)
- Shotgun clause
- Venda a terceiros
- Dissolução da Sociedade

15.2. **Arbitragem**: Disputas não resolvidas serão submetidas a arbitragem conforme regras {{arbitration_rules}}.

15.3. Árbitros: {{arbitrators_number}}, sede em {{arbitration_location}}.

## CLÁUSULA 16 - CONFIDENCIALIDADE

16.1. Todos os termos deste Acordo são confidenciais.

16.2. Exceções: divulgação legal obrigatória, auditores, assessores jurídicos.

## CLÁUSULA 17 - AVERBAÇÃO E ARQUIVAMENTO

17.1. Este Acordo será averbado nos livros sociais da Sociedade conforme Lei das S.A. (Lei 6.404/76, art. 118).

17.2. Cópia autenticada será arquivada na sede social.

## CLÁUSULA 18 - VIGÊNCIA

18.1. Este Acordo vigorará pelo prazo de {{agreement_duration_years}} anos, renovável automaticamente.

18.2. Pode ser denunciado com antecedência de {{termination_notice_months}} meses.

## CLÁUSULA 19 - SUCESSORES E CESSIONÁRIOS

19.1. Este Acordo vincula herdeiros, sucessores e cessionários autorizados dos Acionistas.

19.2. Qualquer transferência de ações deve ser condicionada à adesão do adquirente a este Acordo.

## CLÁUSULA 20 - DISPOSIÇÕES GERAIS

20.1. Alterações requerem aprovação unânime dos Acionistas.

20.2. Renúncia a qualquer direito deve ser expressa e por escrito.

20.3. Invalidade parcial não afeta as demais cláusulas.

20.4. Este Acordo prevalece sobre o estatuto/contrato social em caso de conflito, nos limites legais.

## CLÁUSULA 21 - LEI APLICÁVEL E FORO

21.1. Este Acordo é regido pelas leis brasileiras.

21.2. Foro eleito: {{jurisdiction_city}}, {{jurisdiction_state}}.

---

**Local e Data**: {{contract_city}}, {{contract_date}}

___________________________________
**SOCIEDADE: {{company_name}}**
Por: {{company_representative_name}}
Cargo: {{company_representative_role}}

___________________________________
**ACIONISTA: {{shareholder_1_name}}**

___________________________________
**ACIONISTA: {{shareholder_2_name}}**

{{additional_signature_blocks}}

---

## ANEXO I - TABELA DE CAPITALIZAÇÃO

{{cap_table}}

## ANEXO II - FÓRMULA DE ANTI-DILUIÇÃO

{{anti_dilution_detailed_formula}}

## ANEXO III - POLÍTICA DE GOVERNANÇA

{{governance_policy}}',
    'draft',
    '["acordo de acionistas","shareholders agreement","tag-along","drag-along","governança","direito de preferência"]',
    1,
    TRUE,
    NOW(),
    NOW(),
    FALSE
);

-- =====================================================
-- Template 2: Contrato de Investimento (Investment)
-- =====================================================

INSERT INTO contract_templates (
    id,
    client_id,
    company_id,
    name,
    description,
    code,
    template_type,
    content,
    default_status,
    tags,
    version,
    is_active,
    created_at,
    updated_at,
    is_deleted
) VALUES (
    UUID(),
    '00000000-0000-0000-0000-000000000001',
    NULL,
    'Contrato de Investimento - Rodada de Captação',
    'Template de Contrato de Investimento para rounds de captação (Seed, Série A, B, C) com termos, direitos dos investidores e condições precedentes.',
    'INVESTMENT-ROUND-001',
    'investment',
    '# CONTRATO DE INVESTIMENTO
## (INVESTMENT AGREEMENT)

## PARTES

**SOCIEDADE INVESTIDA**: {{company_name}}, sociedade empresária inscrita no CNPJ sob o nº {{company_cnpj}}, com sede na {{company_address}}, doravante denominada "SOCIEDADE" ou "COMPANHIA".

**INVESTIDOR(ES)**:

{{investor_1_name}}, {{investor_1_type}}, {{investor_1_qualification}}, inscrito no CPF/CNPJ sob nº {{investor_1_document}}, com endereço na {{investor_1_address}}, doravante denominado "INVESTIDOR 1".

{{additional_investors}}

Doravante denominados individualmente "INVESTIDOR" ou coletivamente "INVESTIDORES".

**ACIONISTAS ATUAIS**:

{{existing_shareholders}}

Doravante denominados "ACIONISTAS FUNDADORES" ou "FUNDADORES".

## PREÂMBULO

CONSIDERANDO QUE a Sociedade desenvolve {{business_description}} e busca captação de recursos para {{funding_purpose}};

CONSIDERANDO QUE os Investidores desejam aportar recursos na Sociedade mediante subscrição de novas ações/quotas;

CONSIDERANDO QUE as partes desejam estabelecer os termos e condições do investimento;

As partes celebram o presente Contrato de Investimento, conforme segue:

## CLÁUSULA 1 - DEFINIÇÕES

1.1. **Rodada**: {{round_name}} (Seed / Série A / Série B / Série C)

1.2. **Valuation**:
- **Pré-Money**: R$ {{pre_money_valuation}}
- **Pós-Money**: R$ {{post_money_valuation}}

1.3. **Instrumento**: {{investment_instrument}}
- Ações Ordinárias
- Ações Preferenciais
- Debêntures Conversíveis
- SAFE / Convertible Note

## CLÁUSULA 2 - INVESTIMENTO

2.1. **Valor Total da Rodada**: R$ {{total_round_amount}}

2.2. **Valor por Investidor**:
{{investor_commitments}}

Exemplo:
- {{investor_1_name}}: R$ {{investor_1_amount}}
- {{investor_2_name}}: R$ {{investor_2_amount}}

2.3. **Preço por Ação**: R$ {{price_per_share}}

2.4. **Número de Ações Emitidas**: {{shares_issued}}

2.5. **Participação Pós-Investimento**:
{{post_investment_ownership}}

Exemplo:
- Fundadores: {{founders_post_ownership}}%
- Investidores desta rodada: {{new_investors_ownership}}%
- Investidores anteriores: {{previous_investors_ownership}}%

## CLÁUSULA 3 - FORMA E PRAZO DO INVESTIMENTO

3.1. **Modalidade**: {{investment_mode}}
- Pagamento único (lump sum)
- Tranches/parcelas condicionadas a milestones

3.2. **Primeira Tranche**: R$ {{first_tranche_amount}} em até {{first_tranche_days}} dias após assinatura e cumprimento das Condições Precedentes.

3.3. **Tranches Subsequentes** (se aplicável):
{{subsequent_tranches}}

3.4. **Conta de Depósito**:
Banco: {{bank_name}}
Agência: {{bank_branch}}
Conta: {{bank_account}}
Titular: {{company_name}}

## CLÁUSULA 4 - CONDIÇÕES PRECEDENTES

4.1. O investimento está condicionado ao cumprimento das seguintes condições até {{conditions_deadline}}:

**4.1.1. Documentação Societária**:
- [ ] Alteração do estatuto/contrato social aprovada
- [ ] Ata de assembleia/reunião aprovando o investimento
- [ ] Registro na Junta Comercial

**4.1.2. Due Diligence**:
- [ ] Conclusão satisfatória de due diligence legal, financeira e operacional
- [ ] Inexistência de contingências materiais não divulgadas
- [ ] {{additional_dd_items}}

**4.1.3. Documentos Complementares**:
- [ ] Acordo de Acionistas assinado
- [ ] Declarações e garantias dos Fundadores
- [ ] Termo de não concorrência
- [ ] {{additional_documents}}

**4.1.4. Aprovações Regulatórias**:
- [ ] Aprovação do CADE (se aplicável)
- [ ] Autorizações setoriais
- [ ] {{regulatory_approvals}}

**4.1.5. Outras Condições**:
{{other_conditions}}

4.2. Se as Condições Precedentes não forem cumpridas, este contrato será rescindido sem ônus.

## CLÁUSULA 5 - DECLARAÇÕES E GARANTIAS DA SOCIEDADE

5.1. A Sociedade declara e garante que:

**5.1.1. Constituição e Existência**:
- Está devidamente constituída e registrada
- Possui capacidade legal para celebrar este contrato
- Todas as autorizações societárias foram obtidas

**5.1.2. Capital Social**:
- Estrutura de capital conforme declarado
- Ações/quotas livres de ônus e gravames
- Inexistência de opções, warrants ou direitos pendentes, exceto: {{disclosed_options}}

**5.1.3. Demonstrações Financeiras**:
- Demonstrações apresentadas são verdadeiras e completas
- Preparadas conforme práticas contábeis adequadas
- Refletem patrimônio e resultado de forma fidedigna

**5.1.4. Passivos**:
- Inexistência de passivos não divulgados
- Processos judiciais foram totalmente revelados
- Dívidas conforme declarado: {{disclosed_debts}}

**5.1.5. Propriedade Intelectual**:
- Sociedade é titular ou licenciada de toda PI necessária
- Inexistência de violação de direitos de terceiros
- PI devidamente protegida e registrada

**5.1.6. Contratos Relevantes**:
- Todos os contratos materiais foram divulgados
- Sociedade não está em default em nenhum contrato
- {{key_contracts}}

**5.1.7. Conformidade Legal**:
- Operações em conformidade com legislação aplicável
- Licenças e autorizações em vigor
- Cumprimento de obrigações trabalhistas e tributárias

**5.1.8. Eventos Adversos**:
- Inexistência de Material Adverse Change (MAC) desde {{mac_reference_date}}
- Nenhum evento que possa afetar substancialmente o negócio

5.2. **Prazo de Garantia**: {{warranty_period}} meses a partir do closing.

5.3. **Limitação de Responsabilidade**: 
- Individual: {{individual_claim_threshold}}
- Agregado: {{aggregate_claim_cap}} (geralmente 100% do investimento)

## CLÁUSULA 6 - DECLARAÇÕES E GARANTIAS DOS INVESTIDORES

6.1. Os Investidores declaram que:
- Possuem capacidade financeira e jurídica para realizar o investimento
- Compreendem os riscos do investimento em startup
- São investidores qualificados conforme CVM (se aplicável)
- Investimento com recursos próprios lícitos

## CLÁUSULA 7 - DIREITOS DOS INVESTIDORES

### 7.1. DIREITOS POLÍTICOS

7.1.1. **Indicação de Conselheiros**:
- Investidores têm direito a indicar {{investor_board_seats}} membro(s) do Conselho
- Titulares: {{investor_board_nominees}}

7.1.2. **Veto Rights (Matérias Reservadas)**:
Matérias que requerem aprovação dos Investidores:
- Aumento ou redução de capital
- Emissão de novos valores mobiliários
- Alteração do objeto social
- Fusão, cisão, incorporação
- Endividamento acima de {{debt_approval_threshold}}
- Venda de ativos acima de {{asset_approval_threshold}}
- Transações com partes relacionadas
- Alteração do plano de negócios ou orçamento anual
- {{additional_veto_matters}}

7.1.3. **Aprovação de Orçamento**:
- Orçamento anual sujeito a aprovação dos Investidores
- Desvios acima de {{budget_deviation_threshold}}% requerem nova aprovação

### 7.2. DIREITOS ECONÔMICOS

7.2.1. **Preferência de Liquidação**:
{{liquidation_preference}}

Opções:
- **1x não-participante**: recebe 1x investimento OU pro rata
- **1x participante**: recebe 1x investimento E pro rata do restante
- **2x participante**: recebe 2x investimento E pro rata do restante

Cálculo:
```
Exemplo: Investimento de R$ 5M por 25% com 1x participante
Venda por R$ 30M:
- Investidor recebe: R$ 5M + (25% × R$ 25M) = R$ 11,25M
- Fundadores recebem: 75% × R$ 25M = R$ 18,75M
```

7.2.2. **Dividendos Preferenciais**: {{dividend_preference}}

7.2.3. **Anti-Diluição**: {{anti_dilution_protection}}

### 7.3. DIREITOS DE INFORMAÇÃO

7.3.1. A Sociedade fornecerá aos Investidores:
- Demonstrações financeiras mensais em até {{monthly_reporting_days}} dias
- Demonstrações financeiras anuais auditadas em até {{annual_reporting_days}} dias
- Orçamento anual com {{budget_advance_days}} dias de antecedência
- Relatório de gestão trimestral (KPIs, métricas operacionais)
- Acesso a registros contábeis e documentos societários

7.3.2. **Formato de Relatórios**: {{reporting_format}}

### 7.4. DIREITOS DE TRANSFERÊNCIA

7.4.1. **Tag-Along**: {{investor_tag_along_percentage}}%

7.4.2. **Drag-Along**: Majoritários podem forçar venda se representarem {{drag_threshold}}%

7.4.3. **Direito de Preferência**: Mesmos termos dos Fundadores

### 7.5. DIREITO DE PARTICIPAÇÃO PRO-RATA

7.5.1. Investidores têm direito de participar de rodadas futuras para manter sua participação percentual.

7.5.2. Prazo para exercício: {{pro_rata_exercise_days}} dias

7.5.3. **Super Pro-Rata**: {{super_pro_rata_terms}}

### 7.6. DIREITO DE REGISTRO (IPO)

7.6.1. **Demand Rights**: Após {{demand_registration_years}} anos, Investidores representando {{demand_registration_threshold}}% podem exigir IPO.

7.6.2. **Piggyback Rights**: Direito de incluir ações em qualquer registro iniciado pela Sociedade.

7.6.3. **Custos**: Sociedade arca com despesas de registro.

## CLÁUSULA 8 - USO DOS RECURSOS

8.1. Os recursos do investimento serão utilizados conforme plano aprovado:

{{use_of_proceeds}}

Exemplo:
- Desenvolvimento de produto: {{product_development_percentage}}%
- Marketing e vendas: {{marketing_percentage}}%
- Contratações: {{hiring_percentage}}%
- Capital de giro: {{working_capital_percentage}}%
- Outros: {{other_percentage}}%

8.2. Desvios superiores a {{use_deviation_threshold}}% do plano requerem aprovação prévia dos Investidores.

## CLÁUSULA 9 - MILESTONES E TRANCHES

9.1. Liberação de tranches subsequentes condicionada ao atingimento de:

{{milestones}}

Exemplos:
- [ ] {{milestone_1}}: liberação de R$ {{tranche_2_amount}}
- [ ] {{milestone_2}}: liberação de R$ {{tranche_3_amount}}
- [ ] {{milestone_3}}: liberação de R$ {{tranche_4_amount}}

9.2. Prazo para verificação: {{milestone_verification_days}} dias após alegado cumprimento.

9.3. Se milestones não atingidos: {{milestone_failure_consequence}}

## CLÁUSULA 10 - OBRIGAÇÕES DA SOCIEDADE

10.1. **Plano de Negócios**: Desenvolver e seguir plano de negócios aprovado.

10.2. **Key Persons**: Manter {{key_persons}} no time com dedicação exclusiva.

10.3. **Exclusividade**: Durante {{exclusivity_period}} meses, não negociar com outros investidores sem aprovação.

10.4. **Governance**: Implementar políticas de governança corporativa.

10.5. **Auditoria**: Contratar auditoria independente anualmente.

10.6. **Budget**: Operar dentro do orçamento aprovado.

10.7. **Stock Option Pool**: Reservar {{option_pool_percentage}}% para ESOP.

## CLÁUSULA 11 - OBRIGAÇÕES DOS FUNDADORES

11.1. **Dedicação Exclusiva**: Tempo integral à Sociedade.

11.2. **Vesting**: Ações dos fundadores sujeitas a vesting de {{founder_vesting_years}} anos.

11.3. **Não-Concorrência**: Pelo prazo de {{founder_non_compete_years}} anos após saída.

11.4. **PI Assignment**: Toda propriedade intelectual desenvolvida é da Sociedade.

11.5. **Confidencialidade**: Manutenção de sigilo de informações.

## CLÁUSULA 12 - EVENTOS DE SAÍDA (EXIT)

### 12.1. IPO

12.1.1. Decisão requer aprovação de {{ipo_approval_threshold}}% dos acionistas.

12.1.2. Lock-up pós-IPO: {{ipo_lockup_months}} meses.

### 12.2. VENDA ESTRATÉGICA (M&A)

12.2.1. Investidores têm preferência na distribuição conforme liquidation preference.

### 12.3. DRAG-ALONG

12.3.1. Se oferta acima de {{drag_minimum_valuation}} for recebida, maioria pode forçar venda.

## CLÁUSULA 13 - PROTEÇÃO CONTRA DILUIÇÃO

13.1. **Tipo**: {{anti_dilution_formula_type}}
- Full Ratchet
- Weighted Average - Broad Based
- Weighted Average - Narrow Based

13.2. **Exceções** (não acionam anti-diluição):
- Stock option pool
- Split/reverse split
- Emissões aprovadas
- {{anti_dilution_exceptions}}

## CLÁUSULA 14 - DISPOSIÇÕES SOBRE SAÍDA ANTECIPADA

14.1. **Good Leaver**:
- Saída por morte, invalidez, aposentadoria
- Mantém ações vested
- Call option pela Sociedade pelo valor justo

14.2. **Bad Leaver**:
- Saída por justa causa
- Perda de todas as ações não-vested
- Call option pela Sociedade sobre vested por {{bad_leaver_price}}

## CLÁUSULA 15 - INDENIZAÇÃO

15.1. Fundadores indenizarão Investidores por:
- Violação de declarações e garantias
- Quebra de obrigações contratuais
- Contingências não declaradas

15.2. **Limitações**:
- Basket: {{indemnification_basket}}
- Cap individual: {{indemnification_individual_cap}}
- Cap agregado: {{indemnification_aggregate_cap}}
- Prazo: {{indemnification_period}}

15.3. **Escrow**: {{escrow_percentage}}% do valor pode ficar em escrow por {{escrow_period}} meses.

## CLÁUSULA 16 - CONFIDENCIALIDADE

16.1. Termos deste contrato são confidenciais.

16.2. Exceções: divulgação legal, regulatória, a LPs (para fundos).

## CLÁUSULA 17 - RESOLUÇÃO DE CONFLITOS

17.1. **Mediação**: Tentativa de mediação em {{mediation_days}} dias.

17.2. **Arbitragem**: Disputas serão resolvidas por arbitragem conforme {{arbitration_rules}}.

17.3. Local: {{arbitration_location}}

17.4. Idioma: Português

17.5. Árbitros: {{number_of_arbitrators}}

## CLÁUSULA 18 - DISPOSIÇÕES GERAIS

18.1. **Lei Aplicável**: Leis brasileiras

18.2. **Foro**: {{jurisdiction_city}}, {{jurisdiction_state}} (subsidiário à arbitragem)

18.3. **Notificações**: Conforme endereços no preâmbulo

18.4. **Cessão**: Não pode ser cedido sem aprovação

18.5. **Integralidade**: Este contrato substitui acordos anteriores

18.6. **Aditivos**: Alterações por escrito com aprovação de todas as partes

---

**Local e Data**: {{contract_city}}, {{contract_date}}

___________________________________
**SOCIEDADE: {{company_name}}**
{{company_representative_name}}
{{company_representative_role}}

___________________________________
**INVESTIDOR: {{investor_1_name}}**
{{investor_1_representative}}

{{additional_investor_signatures}}

___________________________________
**ACIONISTA FUNDADOR: {{founder_1_name}}**

{{additional_founder_signatures}}

---

## ANEXO I - ESTRUTURA DE CAPITAL PRÉ E PÓS-INVESTIMENTO

{{cap_table_pre_post}}

## ANEXO II - CRONOGRAMA DE DESEMBOLSO

{{disbursement_schedule}}

## ANEXO III - MILESTONES DETALHADOS

{{milestones_detailed}}

## ANEXO IV - LISTA DE DECLARAÇÕES E GARANTIAS

{{warranties_list}}

## ANEXO V - MODELO DE RELATÓRIO MENSAL

{{monthly_report_template}}',
    'draft',
    '["investimento","investment","seed","série a","venture capital","startup","equity"]',
    1,
    TRUE,
    NOW(),
    NOW(),
    FALSE
);

-- =====================================================
-- Template 3: Contrato de Trabalho (Employment)
-- =====================================================

INSERT INTO contract_templates (
    id,
    client_id,
    company_id,
    name,
    description,
    code,
    template_type,
    content,
    default_status,
    tags,
    version,
    is_active,
    created_at,
    updated_at,
    is_deleted
) VALUES (
    UUID(),
    '00000000-0000-0000-0000-000000000001',
    NULL,
    'Contrato de Trabalho - CLT e PJ',
    'Template versátil de Contrato de Trabalho incluindo modalidades CLT e PJ/Prestação de Serviços, com cláusulas de não concorrência e confidencialidade.',
    'EMPLOYMENT-CLT-PJ-001',
    'employment',
    '# CONTRATO DE TRABALHO
## {{contract_type_title}}

## PARTES

**CONTRATANTE / EMPREGADOR**: {{company_name}}, pessoa jurídica de direito privado, inscrita no CNPJ sob o nº {{company_cnpj}}, com sede na {{company_address}}, doravante denominada "EMPRESA" ou "CONTRATANTE".

**CONTRATADO(A) / EMPREGADO(A)**: {{employee_name}}, {{employee_nationality}}, {{employee_marital_status}}, {{employee_profession}}, portador(a) do CPF nº {{employee_cpf}}, RG nº {{employee_rg}}, CTPS nº {{employee_ctps}} série {{employee_ctps_series}}, residente e domiciliado(a) na {{employee_address}}, doravante denominado(a) "CONTRATADO(A)" ou "EMPREGADO(A)".

## PREÂMBULO

CONSIDERANDO QUE a Empresa necessita contratar profissional qualificado para {{job_purpose}};

CONSIDERANDO QUE o(a) Contratado(a) possui as qualificações técnicas e profissionais necessárias;

As partes celebram o presente {{contract_type}}, mediante as seguintes cláusulas e condições:

---

# PARTE A - MODALIDADE CLT (CONSOLIDAÇÃO DAS LEIS DO TRABALHO)
*[Aplicável se {{contract_type}} = "CLT"]*

## CLÁUSULA 1 - TIPO DE CONTRATO E ADMISSÃO

1.1. **Tipo de Contrato**: {{contract_duration_type}}
- [ ] Prazo Indeterminado
- [ ] Prazo Determinado por {{fixed_term_duration}} meses
- [ ] Experiência por {{trial_period_days}} dias

1.2. **Data de Admissão**: {{admission_date}}

1.3. **Registro**: O(a) Empregado(a) será registrado conforme CLT (Decreto-Lei 5.452/43).

## CLÁUSULA 2 - FUNÇÃO E ATRIBUIÇÕES

2.1. **Cargo**: {{job_title}}

2.2. **Atribuições**:
{{job_responsibilities}}

Exemplo:
- {{responsibility_1}}
- {{responsibility_2}}
- {{responsibility_3}}

2.3. **Requisitos**:
- Formação: {{education_requirement}}
- Experiência: {{experience_requirement}}
- Habilidades: {{skills_requirement}}

2.4. **Subordinação**: O(a) Empregado(a) reporta-se a {{reports_to}}.

2.5. **Alterações**: Eventuais alterações de função serão formalizadas por escrito, respeitando a legislação trabalhista.

## CLÁUSULA 3 - LOCAL DE TRABALHO

3.1. **Sede Principal**: {{work_location}}

3.2. **Modalidade**: {{work_mode}}
- [ ] Presencial 100%
- [ ] Híbrido ({{hybrid_office_days}} dias presenciais/semana)
- [ ] Remoto 100%

3.3. **Transferência**: A Empresa poderá transferir o(a) Empregado(a) para outra localidade, mediante:
- Notificação prévia de {{transfer_notice_days}} dias
- Pagamento de adicional de transferência de {{transfer_allowance}}% (se aplicável)
- Auxílio mudança conforme política interna

## CLÁUSULA 4 - JORNADA DE TRABALHO

4.1. **Jornada Regular**: {{weekly_hours}} horas semanais, distribuídas em:
{{work_schedule}}

Exemplo padrão:
- Segunda a Sexta: {{daily_start_time}} às {{daily_end_time}}
- Intervalo para refeição: {{lunch_break_duration}} hora(s)
- Sábados, Domingos e Feriados: Folga

4.2. **Banco de Horas**: {{has_time_bank}}
- Compensação de horas extras em até {{time_bank_period}} meses
- Acordo individual/coletivo anexo

4.3. **Horas Extras**:
- Remuneração: {{overtime_rate}}% sobre o valor da hora normal
- Limite: {{overtime_limit}} horas mensais
- Necessidade de autorização prévia

4.4. **Trabalho Noturno**: {{night_shift_policy}}
- Adicional noturno: 20% conforme CLT

4.5. **Controle de Ponto**: {{time_tracking_method}}
- Eletrônico / Manual / REP (Registro Eletrônico de Ponto)

## CLÁUSULA 5 - REMUNERAÇÃO E BENEFÍCIOS

### 5.1. SALÁRIO

5.1.1. **Salário Base**: R$ {{base_salary}} ({{salary_period}})

5.1.2. **Forma de Pagamento**:
- Periodicidade: {{payment_frequency}}
- Meio: {{payment_method}}
- Data: Até o {{payment_day}}º dia útil do mês

5.1.3. **Descontos Legais**:
- INSS conforme tabela vigente
- Imposto de Renda conforme tabela progressiva
- Outros descontos autorizados

### 5.2. BENEFÍCIOS

5.2.1. **Vale Transporte**: {{transportation_allowance}}
- Valor: {{vt_value}} ou conforme uso
- Desconto: 6% do salário base

5.2.2. **Vale Refeição/Alimentação**: {{meal_allowance}}
- Valor: R$ {{meal_value}} por dia útil trabalhado
- Desconto: {{meal_discount_percentage}}%

5.2.3. **Plano de Saúde**: {{health_insurance}}
- Operadora: {{health_provider}}
- Tipo: {{health_plan_type}}
- Cobertura: Titular e {{dependents_covered}}
- Coparticipação: {{health_copay}}

5.2.4. **Plano Odontológico**: {{dental_insurance}}
- Operadora: {{dental_provider}}

5.2.5. **Seguro de Vida**: {{life_insurance}}
- Capital segurado: R$ {{life_insurance_amount}}

5.2.6. **Outros Benefícios**:
{{additional_benefits}}

Exemplos:
- Auxílio home office: R$ {{home_office_allowance}}
- Auxílio educação: R$ {{education_allowance}} ou {{education_percentage}}%
- Gympass / wellhub
- Day off de aniversário
- {{other_benefits}}

### 5.3. BÔNUS E REMUNERAÇÃO VARIÁVEL

5.3.1. **Programa de Bônus**: {{bonus_program}}
- Periodicidade: {{bonus_frequency}}
- Métricas: {{bonus_metrics}}
- Potencial: Até {{bonus_percentage}}% do salário anual

5.3.2. **Participação nos Lucros (PLR)**: Conforme acordo coletivo/programa específico

5.3.3. **Stock Options**: {{stock_option_eligibility}}
- Sujeito a plano de opções separado

## CLÁUSULA 6 - FÉRIAS

6.1. O(a) Empregado(a) fará jus a {{vacation_days}} dias de férias anuais após cada período aquisitivo de 12 meses.

6.2. **Conversão em Abono**: Permitida conversão de até 1/3 das férias em abono pecuniário.

6.3. **Fracionamento**: Permitido em até {{vacation_split}} períodos, sendo um com mínimo de {{vacation_min_days}} dias.

6.4. **Período Concessivo**: Férias devem ser concedidas nos 12 meses subsequentes ao período aquisitivo.

## CLÁUSULA 7 - PERÍODO DE EXPERIÊNCIA

7.1. Os primeiros {{trial_period_days}} dias constituem período de experiência.

7.2. Durante este período, qualquer das partes pode rescindir sem aviso prévio ou indenização substitutiva.

7.3. **Avaliação**: O(a) Empregado(a) será avaliado(a) em {{trial_evaluation_date}}.

## CLÁUSULA 8 - RESCISÃO

### 8.1. AVISO PRÉVIO

8.1.1. Qualquer das partes pode rescindir mediante aviso prévio de {{notice_period_days}} dias.

8.1.2. **Acréscimo por Tempo de Serviço**: +3 dias por ano trabalhado, até máximo de 90 dias.

8.1.3. **Dispensa do Cumprimento**: Por acordo entre as partes ou mediante pagamento indenizatório.

### 8.2. JUSTA CAUSA

8.2.1. Constituem motivos de rescisão por justa causa (art. 482 CLT):
- Ato de improbidade
- Incontinência de conduta ou mau procedimento
- Negociação habitual
- Condenação criminal
- Desídia
- Embriaguez habitual ou em serviço
- Violação de segredo da empresa
- Ato de indisciplina ou insubordinação
- Abandono de emprego
- Ofensas físicas
- Prática de jogos de azar
- {{additional_just_cause_items}}

### 8.3. VERBAS RESCISÓRIAS

8.3.1. Por iniciativa da Empresa sem justa causa:
- Saldo de salário
- Aviso prévio proporcional
- Férias vencidas e proporcionais + 1/3
- 13º salário proporcional
- FGTS + multa de 40%
- Seguro-desemprego (se elegível)

8.3.2. Por iniciativa do(a) Empregado(a):
- Saldo de salário
- Férias vencidas + 1/3
- 13º salário proporcional
- Férias proporcionais + 1/3 (se mais de 1 ano)

8.3.3. Por justa causa:
- Saldo de salário
- Férias vencidas + 1/3 (se houver)

## CLÁUSULA 9 - FGTS E INSS

9.1. A Empresa depositará mensalmente 8% do salário na conta FGTS.

9.2. Contribuição ao INSS conforme legislação vigente.

---

# PARTE B - MODALIDADE PJ (PESSOA JURÍDICA / PRESTAÇÃO DE SERVIÇOS)
*[Aplicável se {{contract_type}} = "PJ"]*

## CLÁUSULA 1 - OBJETO

1.1. **Natureza**: Contrato de prestação de serviços profissionais sem vínculo empregatício.

1.2. **Serviços**: O(a) Contratado(a) prestará os seguintes serviços:
{{services_description}}

1.3. **Autonomia**: O(a) Contratado(a) possui autonomia técnica e administrativa, não caracterizando subordinação.

## CLÁUSULA 2 - QUALIFICAÇÃO DO CONTRATADO PJ

2.1. **Empresa do Contratado**:
- Razão Social: {{contractor_company_name}}
- CNPJ: {{contractor_cnpj}}
- Inscrição Municipal: {{contractor_municipal_registration}}
- Atividade: {{contractor_activity_code}}

2.2. **Representante Legal**: {{contractor_representative}}

## CLÁUSULA 3 - PRAZO

3.1. **Vigência**: {{pj_contract_duration}}
- [ ] Indeterminado
- [ ] Determinado: De {{pj_start_date}} até {{pj_end_date}}

3.2. **Renovação**: {{pj_renewal_terms}}

## CLÁUSULA 4 - REMUNERAÇÃO E PAGAMENTO

4.1. **Forma de Remuneração**: {{pj_payment_type}}
- [ ] Valor fixo mensal: R$ {{pj_monthly_fee}}
- [ ] Hora trabalhada: R$ {{pj_hourly_rate}}
- [ ] Por projeto: R$ {{pj_project_fee}}
- [ ] Outra: {{pj_other_payment}}

4.2. **Nota Fiscal**: O(a) Contratado(a) emitirá nota fiscal {{pj_invoice_frequency}}.

4.3. **Prazo de Pagamento**: Até {{pj_payment_days}} dias após recebimento e aprovação da nota fiscal.

4.4. **Descontos**:
- ISS: Conforme legislação municipal
- IRRF: Se aplicável conforme legislação
- PIS, COFINS, CSLL: Se aplicável

## CLÁUSULA 5 - LOCAL E HORÁRIO DE PRESTAÇÃO

5.1. **Local**: {{pj_work_location}}
- Instalações da Contratante
- Estabelecimento do(a) Contratado(a)
- Remoto

5.2. **Horário**: {{pj_work_hours}}
- Flexível, desde que cumpra entregas
- Ou outro conforme acordado

5.3. **Disponibilidade**: {{pj_availability_requirement}}

## CLÁUSULA 6 - OBRIGAÇÕES DO CONTRATADO PJ

6.1. Executar os serviços com qualidade e dentro dos prazos acordados.

6.2. Manter-se em dia com obrigações fiscais, tributárias e previdenciárias.

6.3. Fornecer notas fiscais válidas.

6.4. Contratar seguro de responsabilidade civil profissional (se aplicável).

6.5. Cumprir políticas internas da Contratante (código de conduta, segurança da informação).

## CLÁUSULA 7 - OBRIGAÇÕES DA CONTRATANTE

7.1. Efetuar pagamentos conforme acordado.

7.2. Fornecer informações e recursos necessários para execução dos serviços.

7.3. Dar feedback sobre a qualidade dos serviços.

## CLÁUSULA 8 - RESCISÃO PJ

8.1. **Rescisão Imotivada**: Qualquer parte pode rescindir mediante aviso prévio de {{pj_notice_period}} dias.

8.2. **Rescisão Motivada**: Rescisão imediata em caso de:
- Inadimplemento contratual
- Violação de confidencialidade ou não-concorrência
- Perda de capacidade legal/regularidade fiscal

8.3. **Multa Rescisória**: {{pj_termination_penalty}}

---

# PARTE C - CLÁUSULAS COMUNS (CLT E PJ)

## CLÁUSULA 10 - CONFIDENCIALIDADE

10.1. O(a) Contratado(a)/Empregado(a) compromete-se a manter sigilo absoluto sobre:
- Informações técnicas, comerciais, financeiras
- Estratégias de negócio
- Dados de clientes e parceiros
- Propriedade intelectual
- Processos internos
- {{additional_confidential_items}}

10.2. **Definição de Informação Confidencial**: Qualquer informação não pública recebida da Empresa ou desenvolvida no curso da relação.

10.3. **Exceções**:
- Informações de domínio público
- Informações obtidas legitimamente de terceiros
- Divulgação legalmente obrigatória

10.4. **Prazo**: A obrigação de confidencialidade permanece por {{confidentiality_period_years}} anos após término do contrato.

10.5. **Devolução**: Ao término, o(a) Contratado(a) devolverá todos os documentos e arquivos confidenciais.

## CLÁUSULA 11 - NÃO-CONCORRÊNCIA

11.1. Durante a vigência do contrato e por {{non_compete_period_years}} anos após o término, o(a) Contratado(a) não poderá:

a) **Concorrência Direta**:
- Ingressar, como empregado, sócio, consultor ou qualquer forma, em empresa concorrente
- Desenvolver negócio concorrente
- Prestar serviços a concorrentes

b) **Definição de Concorrente**:
{{competitor_definition}}

c) **Abrangência Geográfica**: {{non_compete_geography}}
- Brasil inteiro
- Ou: {{specific_regions}}

11.2. **Exceções**: {{non_compete_exceptions}}

11.3. **Compensação**: {{non_compete_compensation}}
- Não aplicável
- Ou: Pagamento de R$ {{non_compete_payment}} pelo período

11.4. **Salvaguarda (Blue Pencil)**: Se cláusula for considerada excessiva, será reduzida ao limite legal aceitável.

## CLÁUSULA 12 - NÃO-SOLICITAÇÃO (NON-SOLICITATION)

12.1. Durante a vigência e por {{non_solicitation_period_years}} anos após término, o(a) Contratado(a) não poderá:

a) **Clientes**:
- Solicitar ou aceitar negócios de clientes da Empresa
- Interferir em relacionamentos comerciais da Empresa
- Desviar clientes

b) **Empregados/Colaboradores**:
- Aliciar, recrutar ou contratar empregados da Empresa
- Induzir colaboradores a deixarem a Empresa

12.2. **Definição de Cliente**: Qualquer pessoa/empresa que foi cliente nos últimos {{client_definition_period}} meses.

12.3. **Exceções**: Processos seletivos abertos ao público ou headhunters.

## CLÁUSULA 13 - PROPRIEDADE INTELECTUAL

13.1. **Titularidade**: Toda propriedade intelectual desenvolvida durante a relação, relacionada às atividades da Empresa, pertence exclusivamente à Empresa.

13.2. **Tipos de PI**:
- Invenções, patentes
- Software, código-fonte
- Designs, marcas
- Trade secrets, know-how
- Obras autorais

13.3. **Cessão**: O(a) Contratado(a) cede, de forma definitiva, irrevogável e irretratável, todos os direitos sobre criações.

13.4. **Cooperação**: O(a) Contratado(a) cooperará com registro de patentes e marcas sem custo adicional.

13.5. **PI Pré-Existente**: {{pre_existing_ip_terms}}

13.6. **Remuneração**: Incluída na remuneração regular, sem pagamento adicional.

## CLÁUSULA 14 - EQUIPAMENTOS E RECURSOS

14.1. A Empresa fornecerá (se aplicável):
{{company_provided_equipment}}

Exemplos:
- [ ] Notebook: {{laptop_model}}
- [ ] Celular: {{phone_model}}
- [ ] Acesso a softwares: {{software_licenses}}
- [ ] Outros: {{other_equipment}}

14.2. **Responsabilidade**: O(a) Contratado(a) responde por:
- Uso adequado e cuidadoso
- Danos por negligência ou mau uso
- Devolução ao término em bom estado

14.3. **BYOD (Bring Your Own Device)**: {{byod_policy}}

## CLÁUSULA 15 - POLÍTICAS INTERNAS

15.1. O(a) Contratado(a) compromete-se a cumprir:
- Código de Conduta e Ética
- Política de Segurança da Informação
- Política de Uso de TI
- Política de Prevenção ao Assédio
- {{additional_policies}}

15.2. Violação de políticas pode resultar em advertência, suspensão (CLT) ou rescisão.

## CLÁUSULA 16 - PROTEÇÃO DE DADOS (LGPD)

16.1. O(a) Contratado(a) compromete-se a cumprir a Lei Geral de Proteção de Dados (Lei 13.709/18).

16.2. **Tratamento de Dados**: Seguir instruções da Empresa sobre coleta, uso, armazenamento e descarte de dados pessoais.

16.3. **Incidentes**: Reportar imediatamente qualquer vazamento ou incidente de segurança.

16.4. **Treinamento**: Participar de treinamentos sobre LGPD.

## CLÁUSULA 17 - COMPLIANCE E ANTICORRUPÇÃO

17.1. O(a) Contratado(a) declara conhecer e comprometer-se a cumprir:
- Lei Anticorrupção (Lei 12.846/13)
- Lei de Lavagem de Dinheiro (Lei 9.613/98)

17.2. **Proibições**:
- Pagamento de propinas ou vantagens indevidas
- Facilitação de atos de corrupção
- Conflito de interesses

17.3. **Canal de Denúncias**: {{whistleblower_channel}}

## CLÁUSULA 18 - AUSÊNCIAS E LICENÇAS (CLT)

18.1. **Licença Médica**: Mediante atestado médico conforme legislação.

18.2. **Licença Maternidade/Paternidade**: Conforme CLT.

18.3. **Faltas Justificadas**: Art. 473 CLT (casamento, falecimento familiar, etc.).

18.4. **Outras Licenças**: {{other_leaves}}

## CLÁUSULA 19 - CONDUTA E DISCIPLINA

19.1. O(a) Contratado(a) deve manter conduta profissional, ética e respeitosa.

19.2. **Proibições**:
- Assédio moral ou sexual
- Discriminação
- Violência
- Uso de substâncias ilícitas
- {{additional_prohibitions}}

19.3. **Medidas Disciplinares (CLT)**:
- Advertência verbal
- Advertência escrita
- Suspensão
- Justa causa

## CLÁUSULA 20 - ALTERAÇÕES

20.1. Alterações contratuais devem ser formalizadas por escrito mediante aditivo.

20.2. **CLT**: Respeitado o princípio da imodificabilidade lesiva.

## CLÁUSULA 21 - CESSÃO

21.1. O(a) Contratado(a) não pode ceder ou transferir direitos/obrigações sem anuência da Empresa.

21.2. A Empresa pode ceder este contrato em caso de reorganização societária.

## CLÁUSULA 22 - COMUNICAÇÕES

22.1. **Endereços**: Conforme preâmbulo.

22.2. **E-mails**: 
- Empresa: {{company_email}}
- Contratado(a): {{employee_email}}

22.3. Alterações de endereço devem ser comunicadas em {{notice_change_days}} dias.

## CLÁUSULA 23 - RESOLUÇÃO DE CONFLITOS

23.1. **CLT**: Justiça do Trabalho competente.

23.2. **PJ**: 
- Tentativa de mediação/arbitragem: {{pj_dispute_resolution}}
- Foro: {{jurisdiction_city}}, {{jurisdiction_state}}

## CLÁUSULA 24 - LEI APLICÁVEL

24.1. **CLT**: Legislação trabalhista brasileira e convenções coletivas aplicáveis.

24.2. **PJ**: Código Civil Brasileiro.

## CLÁUSULA 25 - DISPOSIÇÕES FINAIS

25.1. Este contrato substitui acordos anteriores verbais ou escritos.

25.2. Invalidade de cláusula não afeta as demais.

25.3. Tolerância não constitui renúncia de direitos.

25.4. Anexos integram este contrato:
- Anexo I: Descrição detalhada de cargo/serviços
- Anexo II: Políticas da Empresa
- Anexo III: Termo de responsabilidade - equipamentos
- Anexo IV: {{additional_annexes}}

---

**Local e Data**: {{contract_city}}, {{contract_date}}

___________________________________
**EMPRESA: {{company_name}}**
{{company_representative_name}}
{{company_representative_role}}

___________________________________
**CONTRATADO(A): {{employee_name}}**
CPF: {{employee_cpf}}

**Testemunhas**:

1. _________________________________
Nome: {{witness_1_name}}
CPF: {{witness_1_cpf}}

2. _________________________________
Nome: {{witness_2_name}}
CPF: {{witness_2_cpf}}

---

## ANEXO I - DESCRIÇÃO DETALHADA DO CARGO/SERVIÇOS

{{job_detailed_description}}

## ANEXO II - POLÍTICAS DA EMPRESA

{{company_policies_summary}}

## ANEXO III - TERMO DE RESPONSABILIDADE - EQUIPAMENTOS

{{equipment_responsibility_term}}

## ANEXO IV - CRONOGRAMA DE AVALIAÇÃO DE DESEMPENHO

{{performance_review_schedule}}',
    'draft',
    '["trabalho","emprego","CLT","PJ","prestação de serviços","não concorrência","confidencialidade"]',
    1,
    TRUE,
    NOW(),
    NOW(),
    FALSE
);

-- =====================================================
-- Verificação
-- =====================================================

SELECT 
    name,
    code,
    template_type,
    is_active,
    created_at
FROM contract_templates
WHERE code IN (
    'SHAREHOLDERS-AGREEMENT-001',
    'INVESTMENT-ROUND-001',
    'EMPLOYMENT-CLT-PJ-001'
)
ORDER BY created_at DESC;
