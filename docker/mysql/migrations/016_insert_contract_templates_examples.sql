-- Migration: 016_insert_contract_templates_examples.sql
-- Author: GitHub Copilot
-- Date: 17/02/2026
-- Description: Insert example contract templates (NDA and Stock Option Plan)

USE ppro;

-- ============================================
-- TEMPLATE 1: NDA (Non-Disclosure Agreement)
-- ============================================
INSERT INTO contract_templates (
    id,
    client_id,
    name,
    code,
    template_type,
    content,
    default_status,
    version,
    is_active,
    tags,
    description,
    created_at,
    updated_at,
    is_deleted
) VALUES (
    UUID(),
    '00000000-0000-0000-0000-000000000001',
    'Acordo de Confidencialidade - NDA Bilateral',
    'NDA-BILATERAL-001',
    'confidentiality',
    '# ACORDO DE CONFIDENCIALIDADE (NDA)

## PARTES

**PARTE DIVULGADORA**: {{party_discloser_name}}, pessoa jurídica de direito privado, inscrita no CNPJ sob o nº {{party_discloser_cnpj}}, com sede na {{party_discloser_address}}, doravante denominada "PARTE DIVULGADORA".

**PARTE RECEPTORA**: {{party_recipient_name}}, pessoa jurídica de direito privado, inscrita no CNPJ sob o nº {{party_recipient_cnpj}}, com sede na {{party_recipient_address}}, doravante denominada "PARTE RECEPTORA".

## CONSIDERAÇÕES

CONSIDERANDO QUE as partes pretendem estabelecer uma relação comercial envolvendo o compartilhamento de informações confidenciais relacionadas a {{business_purpose}};

CONSIDERANDO QUE ambas as partes reconhecem a natureza sensível e estratégica das informações a serem compartilhadas;

CONSIDERANDO QUE as partes desejam proteger tais informações contra divulgação não autorizada;

As partes celebram este Acordo de Confidencialidade, mediante as seguintes cláusulas e condições:

## CLÁUSULA 1 - DEFINIÇÕES

1.1. **Informações Confidenciais**: Todas as informações, dados, documentos, materiais, conhecimentos técnicos ou comerciais, orais ou escritos, que sejam divulgados por uma parte à outra durante a vigência deste acordo, incluindo, mas não se limitando a:
- Planos de negócios e estratégias comerciais
- Informações financeiras e contábeis
- Dados de clientes e fornecedores
- Tecnologias, processos e know-how
- Propriedade intelectual
- Quaisquer informações marcadas como "confidencial" ou que, pela sua natureza, devam ser consideradas confidenciais

1.2. **Exceções**: Não serão consideradas Informações Confidenciais aquelas que:
- Sejam de domínio público antes da divulgação ou se tornem de domínio público sem violação deste acordo
- Já eram conhecidas pela Parte Receptora antes da divulgação
- Sejam desenvolvidas independentemente pela Parte Receptora
- Sejam legitimamente recebidas de terceiros sem violação de confidencialidade

## CLÁUSULA 2 - OBRIGAÇÕES DE CONFIDENCIALIDADE

2.1. A Parte Receptora se compromete a:
- Manter sigilo absoluto sobre as Informações Confidenciais recebidas
- Não divulgar, reproduzir, utilizar ou dar conhecimento das Informações Confidenciais a terceiros
- Utilizar as Informações Confidenciais exclusivamente para {{business_purpose}}
- Proteger as Informações Confidenciais com o mesmo grau de cuidado que dispensa às suas próprias informações confidenciais

2.2. A divulgação das Informações Confidenciais dentro da organização da Parte Receptora será limitada aos funcionários, consultores e representantes que necessitem ter conhecimento das mesmas para os fins deste acordo.

2.3. A Parte Receptora deverá informar os destinatários internos sobre a natureza confidencial das informações e assegurar seu cumprimento às obrigações deste acordo.

## CLÁUSULA 3 - PROPRIEDADE INTELECTUAL

3.1. Todas as Informações Confidenciais divulgadas permanecem como propriedade exclusiva da Parte Divulgadora.

3.2. Este acordo não confere à Parte Receptora qualquer licença, direito ou participação sobre patentes, marcas, direitos autorais ou outros direitos de propriedade intelectual da Parte Divulgadora.

## CLÁUSULA 4 - DEVOLUÇÃO E DESTRUIÇÃO

4.1. Mediante solicitação da Parte Divulgadora ou ao término deste acordo, a Parte Receptora deverá:
- Devolver todos os documentos e materiais contendo Informações Confidenciais
- Destruir todas as cópias, reproduções ou extratos de tais informações
- Certificar por escrito o cumprimento desta cláusula

## CLÁUSULA 5 - VIGÊNCIA

5.1. Este acordo terá vigência de {{contract_duration_months}} meses, iniciando em {{contract_start_date}}, podendo ser renovado mediante acordo entre as partes.

5.2. As obrigações de confidencialidade permanecerão em vigor por {{confidentiality_duration_years}} anos após o término deste acordo.

## CLÁUSULA 6 - PENALIDADES

6.1. A violação de qualquer cláusula deste acordo sujeitará a parte infratora ao pagamento de multa no valor de R$ {{penalty_amount}}, sem prejuízo de indenização por perdas e danos adicionais.

6.2. A Parte Divulgadora poderá buscar medidas judiciais cabíveis, incluindo ações de obrigação de fazer/não fazer e indenização por danos.

## CLÁUSULA 7 - DISPOSIÇÕES GERAIS

7.1. Este acordo representa o entendimento completo entre as partes sobre o assunto tratado.

7.2. Qualquer alteração a este acordo deverá ser feita por escrito e assinada por ambas as partes.

7.3. A tolerância ou não exercício de qualquer direito previsto neste acordo não constituirá renúncia.

7.4. Este acordo será regido pelas leis brasileiras.

## CLÁUSULA 8 - FORO

8.1. As partes elegem o foro de {{jurisdiction_city}}, {{jurisdiction_state}}, para dirimir quaisquer controvérsias oriundas deste acordo.

---

**Local e Data**: {{contract_city}}, {{contract_date}}

___________________________________
**{{party_discloser_name}}**
{{party_discloser_representative_name}}
{{party_discloser_representative_role}}

___________________________________
**{{party_recipient_name}}**
{{party_recipient_representative_name}}
{{party_recipient_representative_role}}',
    'draft',
    1,
    TRUE,
    '["confidencialidade","nda","bilateral","proteção de dados"]',
    'Template bilateral de Acordo de Confidencialidade (NDA) para proteger informações sensíveis compartilhadas entre empresas durante negociações ou parcerias comerciais.',
    NOW(6),
    NOW(6),
    FALSE
);

-- ============================================
-- TEMPLATE 2: Stock Option Plan
-- ============================================
INSERT INTO contract_templates (
    id,
    client_id,
    name,
    code,
    template_type,
    content,
    default_status,
    version,
    is_active,
    tags,
    description,
    created_at,
    updated_at,
    is_deleted
) VALUES (
    UUID(),
    '00000000-0000-0000-0000-000000000001',
    'Plano de Opções de Ações - Stock Option Plan',
    'STOCK-OPTION-001',
    'stock_option',
    '# CONTRATO DE OUTORGA DE OPÇÃO DE COMPRA DE AÇÕES
## (STOCK OPTION PLAN)

## PARTES

**EMPRESA OUTORGANTE**: {{company_name}}, sociedade empresária limitada, inscrita no CNPJ sob o nº {{company_cnpj}}, com sede na {{company_address}}, doravante denominada "EMPRESA".

**BENEFICIÁRIO**: {{beneficiary_name}}, {{beneficiary_nationality}}, {{beneficiary_marital_status}}, {{beneficiary_profession}}, portador do CPF nº {{beneficiary_cpf}}, residente e domiciliado na {{beneficiary_address}}, doravante denominado "BENEFICIÁRIO".

## PREÂMBULO

CONSIDERANDO QUE a Empresa estabeleceu um Plano de Opções de Ações ("Plano") com o objetivo de alinhar interesses dos colaboradores chave com os acionistas da Empresa;

CONSIDERANDO QUE o Beneficiário é um colaborador valioso e estratégico para o crescimento da Empresa;

CONSIDERANDO QUE a Empresa deseja incentivar a permanência e dedicação do Beneficiário através da outorga de opções de compra de ações;

As partes celebram este Contrato de Outorga de Opção de Compra de Ações, mediante as seguintes cláusulas e condições:

## CLÁUSULA 1 - OBJETO

1.1. Por meio deste instrumento, a Empresa outorga ao Beneficiário o direito de adquirir até **{{total_options}}** opções de compra de ações ordinárias da Empresa, representando {{ownership_percentage}}% do capital social total da Empresa.

1.2. Cada opção dá direito ao Beneficiário de adquirir uma (1) ação ordinária da Empresa, observadas as condições deste contrato.

## CLÁUSULA 2 - PREÇO DE EXERCÍCIO

2.1. O preço de exercício de cada opção será de **R$ {{strike_price}}** por ação (o "Preço de Exercício").

2.2. O Preço de Exercício foi determinado com base no valor justo de mercado das ações da Empresa na data de {{valuation_date}}.

2.3. O Preço de Exercício poderá ser ajustado proporcionalmente em caso de:
- Desdobramento ou grupamento de ações
- Bonificações em ações
- Subscrição de novas ações com direito de preferência
- Outros eventos corporativos que afetem o valor das ações

## CLÁUSULA 3 - PERÍODO DE VESTING (CARÊNCIA)

3.1. As opções outorgadas estarão sujeitas a um período de vesting (carência) de **{{vesting_period_years}} anos**, com início em {{grant_date}}.

3.2. O vesting seguirá o seguinte cronograma:
{{vesting_schedule}}

Exemplo padrão (caso não especificado):
- Cliff de 12 meses: nenhuma opção disponível no primeiro ano
- Após 12 meses: {{cliff_percentage}}% das opções
- Meses subsequentes: {{monthly_vesting_percentage}}% mensalmente até completar 100%

3.3. As opções só poderão ser exercidas após o vesting da respectiva parcela.

## CLÁUSULA 4 - PERÍODO DE EXERCÍCIO

4.1. O Beneficiário poderá exercer as opções que já tenham sofrido vesting a qualquer momento durante o período de exercício, que se estende de {{exercise_start_date}} até {{exercise_end_date}}.

4.2. O exercício das opções deverá ser comunicado à Empresa por escrito, com antecedência mínima de {{exercise_notice_days}} dias.

4.3. O pagamento do Preço de Exercício deverá ser realizado em moeda corrente nacional, mediante transferência bancária para conta indicada pela Empresa.

## CLÁUSULA 5 - CONDIÇÕES PARA EXERCÍCIO

5.1. O exercício das opções está condicionado a:
- Cumprimento do período de vesting
- Manutenção do vínculo trabalhista ou de prestação de serviços com a Empresa
- Inexistência de violação de deveres contratuais ou legais
- Aprovação dos demais acionistas, quando aplicável

5.2. Em caso de desligamento do Beneficiário:
- **Por justa causa**: perda imediata de todas as opções (exercidas ou não)
- **Sem justa causa pela Empresa**: manutenção das opções já em vesting, com prazo de {{termination_exercise_days}} dias para exercício
- **Pedido de demissão**: manutenção das opções já em vesting, com prazo de {{resignation_exercise_days}} dias para exercício

## CLÁUSULA 6 - LIMITAÇÕES E RESTRIÇÕES

6.1. As opções outorgadas são:
- Pessoais e intransferíveis, exceto por sucessão hereditária
- Não podem ser vendidas, cedidas, transferidas, ou dadas em garantia
- Não conferem ao Beneficiário direitos de acionista até o efetivo exercício

6.2. As ações adquiridas mediante exercício das opções estarão sujeitas a:
- Lock-up de {{lockup_period_months}} meses após o exercício
- Direito de preferência dos demais acionistas em caso de venda
- Tag-along e drag-along conforme Acordo de Acionistas
- Demais restrições previstas no Estatuto Social e Acordo de Acionistas

## CLÁUSULA 7 - EVENTOS EXTRAORDINÁRIOS

7.1. **Mudança de Controle (Change of Control)**:
Em caso de venda da Empresa, fusão, incorporação ou aquisição, todas as opções não exercidas sofrerão vesting acelerado (acceleration), permitindo exercício imediato.

7.2. **IPO (Oferta Pública Inicial)**:
Em caso de IPO, o Beneficiário terá um período adicional de {{ipo_exercise_window_days}} dias para exercer as opções em vesting, sujeito às regras do período de lock-up da oferta.

7.3. **Liquidação ou Falência**:
Em caso de liquidação ou falência da Empresa, todas as opções não exercidas serão canceladas sem qualquer compensação.

## CLÁUSULA 8 - DILUIÇÃO

8.1. O Beneficiário reconhece e concorda que sua participação acionária resultante do exercício das opções poderá ser diluída por:
- Novas rodadas de investimento
- Emissão de novas ações ou valores mobiliários conversíveis
- Exercício de opções por outros beneficiários do Plano

8.2. A Empresa não tem obrigação de manter o percentual de participação do Beneficiário.

## CLÁUSULA 9 - ASPECTOS TRIBUTÁRIOS

9.1. O Beneficiário é responsável por todos os tributos, impostos e contribuições incidentes sobre:
- A outorga das opções
- O exercício das opções
- A venda das ações adquiridas

9.2. A Empresa poderá reter na fonte os tributos aplicáveis conforme legislação vigente.

9.3. O Beneficiário deverá buscar orientação fiscal independente sobre as implicações tributárias deste contrato.

## CLÁUSULA 10 - CONFIDENCIALIDADE

10.1. Os termos e condições deste contrato são confidenciais e não devem ser divulgados pelo Beneficiário, exceto:
- Para fins de declaração fiscal
- Mediante ordem judicial
- Para assessores legais e fiscais sob dever de confidencialidade

## CLÁUSULA 11 - ALTERAÇÕES DO PLANO

11.1. A Empresa se reserva o direito de modificar, suspender ou encerrar o Plano a qualquer momento, mediante notificação ao Beneficiário.

11.2. Opções já em vesting não serão afetadas negativamente por alterações do Plano.

## CLÁUSULA 12 - DISPOSIÇÕES GERAIS

12.1. Este contrato não constitui garantia de emprego ou continuidade da prestação de serviços.

12.2. Este contrato representa o entendimento completo entre as partes sobre o assunto.

12.3. Qualquer alteração deverá ser feita por escrito e assinada por ambas as partes.

12.4. A invalidade de qualquer cláusula não afetará a validade das demais.

12.5. Este contrato será regido pelas leis brasileiras.

## CLÁUSULA 13 - FORO

13.1. As partes elegem o foro de {{jurisdiction_city}}, {{jurisdiction_state}}, para dirimir quaisquer controvérsias oriundas deste contrato.

---

**Local e Data**: {{contract_city}}, {{contract_signature_date}}

___________________________________
**{{company_name}}**
{{company_representative_name}}
{{company_representative_role}}

___________________________________
**{{beneficiary_name}}**
Beneficiário

---

## ANEXO I - CRONOGRAMA DE VESTING

{{vesting_schedule_detailed}}

## ANEXO II - FORMULÁRIO DE EXERCÍCIO

(A ser fornecido pela Empresa quando do exercício das opções)',
    'draft',
    1,
    TRUE,
    '["stock option","opções de ações","vesting","incentivo","equity"]',
    'Template completo de Contrato de Outorga de Opção de Compra de Ações (Stock Option Plan) para incentivar e reter colaboradores chave através de participação acionária.',
    NOW(6),
    NOW(6),
    FALSE
);

-- ============================================
-- Confirmation
-- ============================================
SELECT 
    'Templates de exemplo criados com sucesso!' AS message,
    COUNT(*) AS total_templates
FROM contract_templates
WHERE code IN ('NDA-BILATERAL-001', 'STOCK-OPTION-001');
