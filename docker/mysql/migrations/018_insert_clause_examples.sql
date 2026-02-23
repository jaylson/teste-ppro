-- Migration: 018_insert_clause_examples.sql
-- Author: GitHub Copilot
-- Date: 18/02/2026
-- Description: Insert example clauses for testing and demonstration
--              Creates clauses for all clause types with dynamic variables

USE partnership_manager;

-- NOTE: Using REPLACE INTO to make migration idempotent
-- This allows re-running without errors if clauses already exist

-- ============================================
-- GOVERNANCE CLAUSES
-- ============================================

REPLACE INTO clauses (
    id, client_id, name, description, code, content, clause_type,
    is_mandatory, tags, display_order, version, is_active,
    created_at, updated_at, is_deleted
) VALUES (
    UUID(),
    (SELECT id FROM clients LIMIT 1),
    'Composição do Conselho de Administração',
    'Define a estrutura e composição do conselho de administração da empresa',
    'GOV-001',
    '<h3>CLÁUSULA - CONSELHO DE ADMINISTRAÇÃO</h3>
<p>O Conselho de Administração da {{COMPANY_NAME}} será composto por {{BOARD_MEMBERS_COUNT}} membros, sendo:</p>
<ul>
<li>{{MAJORITY_SHAREHOLDERS_SEATS}} indicados pelos acionistas majoritários;</li>
<li>{{MINORITY_SHAREHOLDERS_SEATS}} indicados pelos acionistas minoritários;</li>
<li>{{INDEPENDENT_MEMBERS_SEATS}} conselheiros independentes.</li>
</ul>
<p>As reuniões ordinárias do Conselho ocorrerão {{BOARD_MEETING_FREQUENCY}}, e as deliberações serão tomadas por {{BOARD_QUORUM}}.</p>',
    'governance',
    FALSE,
    '["governança", "conselho", "estrutura"]',
    10,
    1,
    TRUE,
    NOW(6),
    NOW(6),
    FALSE
);

REPLACE INTO clauses (
    id, client_id, name, description, code, content, clause_type,
    is_mandatory, tags, display_order, version, is_active,
    created_at, updated_at, is_deleted
) VALUES (
    UUID(),
    (SELECT id FROM clients LIMIT 1),
    'Direito de Voto',
    'Estabelece os direitos de voto dos acionistas nas assembleias',
    'GOV-002',
    '<h3>CLÁUSULA - DIREITO DE VOTO</h3>
<p>Cada ação ordinária da {{COMPANY_NAME}} confere ao seu titular o direito a um voto nas Assembleias Gerais.</p>
<p>As deliberações serão tomadas por {{VOTING_THRESHOLD}} dos votos dos acionistas presentes, exceto nos seguintes casos que exigirão aprovação de {{QUALIFIED_MAJORITY}}:</p>
<ul>
<li>Alteração do objeto social;</li>
<li>Fusão, cisão ou incorporação da sociedade;</li>
<li>Dissolução da sociedade;</li>
<li>Aumento de capital acima de {{CAPITAL_INCREASE_LIMIT}}.</li>
</ul>',
    'governance',
    TRUE,
    '["governança", "voto", "assembleia", "obrigatória"]',
    11,
    1,
    TRUE,
    NOW(6),
    NOW(6),
    FALSE
);

-- ============================================
-- RIGHTS & OBLIGATIONS CLAUSES
-- ============================================

REPLACE INTO clauses (
    id, client_id, name, description, code, content, clause_type,
    is_mandatory, tags, display_order, version, is_active,
    created_at, updated_at, is_deleted
) VALUES (
    UUID(),
    (SELECT id FROM clients LIMIT 1),
    'Direito de Preferência',
    'Direito de preferência na aquisição de ações',
    'RO-001',
    '<h3>CLÁUSULA - DIREITO DE PREFERÊNCIA</h3>
<p>Os acionistas da {{COMPANY_NAME}} terão direito de preferência na subscrição de novas ações, bônus de subscrição e debêntures conversíveis, na proporção de suas respectivas participações.</p>
<p>O prazo para exercício do direito de preferência será de {{PREEMPTIVE_RIGHT_PERIOD}} dias contados da data de notificação por escrito.</p>
<p>As ações não subscritas dentro do prazo serão oferecidas aos demais acionistas, também na proporção de suas participações. Remanescendo ainda ações, estas poderão ser oferecidas a terceiros.</p>',
    'rights_obligations',
    TRUE,
    '["direitos", "preferência", "subscrição"]',
    20,
    1,
    TRUE,
    NOW(6),
    NOW(6),
    FALSE
);

REPLACE INTO clauses (
    id, client_id, name, description, code, content, clause_type,
    is_mandatory, tags, display_order, version, is_active,
    created_at, updated_at, is_deleted
) VALUES (
    UUID(),
    (SELECT id FROM clients LIMIT 1),
    'Tag Along',
    'Direito de venda conjunta de ações',
    'RO-002',
    '<h3>CLÁUSULA - TAG ALONG</h3>
<p>Na hipótese de alienação, por qualquer acionista, de ações representativas de mais de {{TAG_ALONG_THRESHOLD}}% do capital social da {{COMPANY_NAME}}, os demais acionistas terão o direito de alienar suas ações nas mesmas condições oferecidas ao acionista alienante.</p>
<p>O acionista alienante deverá notificar os demais acionistas com antecedência mínima de {{TAG_ALONG_NOTICE_PERIOD}} dias, informando:</p>
<ul>
<li>Quantidade de ações a serem alienadas;</li>
<li>Preço por ação;</li>
<li>Forma de pagamento;</li>
<li>Identificação do adquirente.</li>
</ul>
<p>Os acionistas minoritários farão jus a, no mínimo, {{TAG_ALONG_PERCENTAGE}}% do valor oferecido ao acionista controlador.</p>',
    'rights_obligations',
    FALSE,
    '["direitos", "tag-along", "venda"]',
    21,
    1,
    TRUE,
    NOW(6),
    NOW(6),
    FALSE
);

REPLACE INTO clauses (
    id, client_id, name, description, code, content, clause_type,
    is_mandatory, tags, display_order, version, is_active,
    created_at, updated_at, is_deleted
) VALUES (
    UUID(),
    (SELECT id FROM clients LIMIT 1),
    'Drag Along',
    'Direito de venda forçada (arrasto)',
    'RO-003',
    '<h3>CLÁUSULA - DRAG ALONG</h3>
<p>Os acionistas representantes de, no mínimo, {{DRAG_ALONG_THRESHOLD}}% do capital social da {{COMPANY_NAME}} terão o direito de exigir que os demais acionistas vendam suas ações juntamente em uma operação de alienação de controle.</p>
<p>Os acionistas minoritários serão obrigados a vender suas ações pelo mesmo preço por ação e nas mesmas condições oferecidas aos acionistas majoritários.</p>
<p>Os acionistas deverão ser notificados com antecedência mínima de {{DRAG_ALONG_NOTICE_PERIOD}} dias úteis sobre a operação.</p>',
    'rights_obligations',
    FALSE,
    '["direitos", "drag-along", "arrasto"]',
    22,
    1,
    TRUE,
    NOW(6),
    NOW(6),
    FALSE
);

-- ============================================
-- COMPLIANCE CLAUSES
-- ============================================

REPLACE INTO clauses (
    id, client_id, name, description, code, content, clause_type,
    is_mandatory, tags, display_order, version, is_active,
    created_at, updated_at, is_deleted
) VALUES (
    UUID(),
    (SELECT id FROM clients LIMIT 1),
    'Lei Geral de Proteção de Dados (LGPD)',
    'Conformidade com a Lei Geral de Proteção de Dados',
    'COMP-001',
    '<h3>CLÁUSULA - PROTEÇÃO DE DADOS PESSOAIS</h3>
<p>As partes declaram conhecer e se comprometem a cumprir integralmente a Lei Geral de Proteção de Dados Pessoais (LGPD - Lei nº 13.709/2018).</p>
<p>{{PARTY_NAME}} se compromete a:</p>
<ul>
<li>Tratar dados pessoais apenas para as finalidades autorizadas neste contrato;</li>
<li>Implementar medidas de segurança técnicas e administrativas adequadas;</li>
<li>Notificar incidentes de segurança em até {{BREACH_NOTIFICATION_PERIOD}} horas;</li>
<li>Permitir auditorias de conformidade mediante aviso prévio de {{AUDIT_NOTICE_PERIOD}} dias.</li>
</ul>
<p>O não cumprimento desta cláusula sujeitará a parte infratora às penalidades previstas na LGPD e neste contrato.</p>',
    'compliance',
    TRUE,
    '["compliance", "lgpd", "dados-pessoais", "obrigatória"]',
    30,
    1,
    TRUE,
    NOW(6),
    NOW(6),
    FALSE
);

REPLACE INTO clauses (
    id, client_id, name, description, code, content, clause_type,
    is_mandatory, tags, display_order, version, is_active,
    created_at, updated_at, is_deleted
) VALUES (
    UUID(),
    (SELECT id FROM clients LIMIT 1),
    'Anticorrupção e Antissuborno',
    'Conformidade com leis anticorrupção',
    'COMP-002',
    '<h3>CLÁUSULA - ANTICORRUPÇÃO</h3>
<p>As partes declaram conhecer e se comprometem a cumprir integralmente a Lei Anticorrupção (Lei nº 12.846/2013), o Foreign Corrupt Practices Act (FCPA) e demais legislações aplicáveis.</p>
<p>Nenhuma das partes poderá, direta ou indiretamente:</p>
<ul>
<li>Oferecer, prometer, dar ou autorizar pagamentos ou benefícios indevidos a agentes públicos ou privados;</li>
<li>Realizar pagamentos de facilitação;</li>
<li>Falsificar registros contábeis ou documentos;</li>
<li>Ocultar ou dissimular a natureza, origem ou destinação de recursos.</li>
</ul>
<p>A violação desta cláusula constitui causa de rescisão imediata, sem prejuízo das penalidades legais.</p>',
    'compliance',
    TRUE,
    '["compliance", "anticorrupção", "ética"]',
    31,
    1,
    TRUE,
    NOW(6),
    NOW(6),
    FALSE
);

-- ============================================
-- FINANCIAL CLAUSES
-- ============================================

REPLACE INTO clauses (
    id, client_id, name, description, code, content, clause_type,
    is_mandatory, tags, display_order, version, is_active,
    created_at, updated_at, is_deleted
) VALUES (
    UUID(),
    (SELECT id FROM clients LIMIT 1),
    'Investimento e Forma de Pagamento',
    'Termos do investimento e condições de pagamento',
    'FIN-001',
    '<h3>CLÁUSULA - INVESTIMENTO</h3>
<p>{{INVESTOR_NAME}} investirá na {{COMPANY_NAME}} o montante de {{INVESTMENT_AMOUNT}} ({{INVESTMENT_AMOUNT_WRITTEN}}), mediante {{PAYMENT_METHOD}}.</p>
<p>O pagamento será realizado da seguinte forma:</p>
<ul>
<li>{{FIRST_INSTALLMENT}}% na data de assinatura deste instrumento;</li>
<li>{{SECOND_INSTALLMENT}}% em até {{SECOND_INSTALLMENT_DAYS}} dias após o cumprimento das condições precedentes;</li>
<li>{{REMAINING_INSTALLMENT}}% conforme marcos estabelecidos no Anexo I.</li>
</ul>
<p>Em contrapartida, {{INVESTOR_NAME}} receberá {{EQUITY_PERCENTAGE}}% do capital social, correspondendo a {{SHARES_QUANTITY}} ações {{SHARE_CLASS}}.</p>',
    'financial',
    FALSE,
    '["financeiro", "investimento", "pagamento"]',
    40,
    1,
    TRUE,
    NOW(6),
    NOW(6),
    FALSE
);

REPLACE INTO clauses (
    id, client_id, name, description, code, content, clause_type,
    is_mandatory, tags, display_order, version, is_active,
    created_at, updated_at, is_deleted
) VALUES (
    UUID(),
    (SELECT id FROM clients LIMIT 1),
    'Distribuição de Dividendos',
    'Regras para distribuição de lucros e dividendos',
    'FIN-002',
    '<h3>CLÁUSULA - DIVIDENDOS</h3>
<p>A {{COMPANY_NAME}} poderá distribuir dividendos aos seus acionistas, observadas as disposições legais e este acordo.</p>
<p>A distribuição de dividendos dependerá de deliberação da Assembleia Geral, respeitando:</p>
<ul>
<li>Mínimo obrigatório por lei: {{MINIMUM_DIVIDEND_PERCENTAGE}}% do lucro líquido ajustado;</li>
<li>Preferência de {{PREFERRED_DIVIDEND_PERCENTAGE}}% para ações preferenciais;</li>
<li>Retenção obrigatória para reservas: {{RESERVE_PERCENTAGE}}%.</li>
</ul>
<p>Os dividendos serão pagos em até {{DIVIDEND_PAYMENT_DAYS}} dias após a deliberação.</p>',
    'financial',
    FALSE,
    '["financeiro", "dividendos", "lucros"]',
    41,
    1,
    TRUE,
    NOW(6),
    NOW(6),
    FALSE
);

REPLACE INTO clauses (
    id, client_id, name, description, code, content, clause_type,
    is_mandatory, tags, display_order, version, is_active,
    created_at, updated_at, is_deleted
) VALUES (
    UUID(),
    (SELECT id FROM clients LIMIT 1),
    'Carência e Vesting',
    'Condições de carência e vesting para stock options',
    'FIN-003',
    '<h3>CLÁUSULA - VESTING</h3>
<p>As opções de compra de ações concedidas a {{GRANTEE_NAME}} estarão sujeitas ao seguinte cronograma de vesting:</p>
<ul>
<li><strong>Cliff period:</strong> {{CLIFF_MONTHS}} meses (nenhuma opção pode ser exercida antes deste período);</li>
<li><strong>Vesting period:</strong> {{VESTING_MONTHS}} meses a partir da data de outorga;</li>
<li><strong>Vesting schedule:</strong> {{VESTING_PERCENTAGE}}% ao mês após o cliff period.</li>
</ul>
<p>Em caso de desligamento antes do término do vesting period:</p>
<ul>
<li>Por justa causa: perda de todas as opções não exercidas;</li>
<li>Sem justa causa: aceleração de {{ACCELERATION_PERCENTAGE}}% das opções não vestidas;</li>
<li>Mudança de controle: aceleração de {{CHANGE_CONTROL_ACCELERATION}}% das opções.</li>
</ul>',
    'financial',
    FALSE,
    '["financeiro", "vesting", "stock-options"]',
    42,
    1,
    TRUE,
    NOW(6),
    NOW(6),
    FALSE
);

-- ============================================
-- TERMINATION CLAUSES
-- ============================================

REPLACE INTO clauses (
    id, client_id, name, description, code, content, clause_type,
    is_mandatory, tags, display_order, version, is_active,
    created_at, updated_at, is_deleted
) VALUES (
    UUID(),
    (SELECT id FROM clients LIMIT 1),
    'Rescisão por Justa Causa',
    'Hipóteses de rescisão contratual por justa causa',
    'TERM-001',
    '<h3>CLÁUSULA - RESCISÃO POR JUSTA CAUSA</h3>
<p>Qualquer das partes poderá rescindir este contrato por justa causa, mediante notificação por escrito, nas seguintes hipóteses:</p>
<ul>
<li>Descumprimento de qualquer obrigação material prevista neste contrato;</li>
<li>Violação de cláusula de confidencialidade ou não-concorrência;</li>
<li>Prática de atos de corrupção, fraude ou má-fé;</li>
<li>Decretação de falência, recuperação judicial ou insolvência;</li>
<li>{{ADDITIONAL_TERMINATION_CAUSE}}.</li>
</ul>
<p>A parte infratora terá prazo de {{CURE_PERIOD}} dias para sanar a infração, exceto nos casos de violação grave.</p>
<p>A rescisão por justa causa não exime a parte infratora do pagamento de perdas e danos.</p>',
    'termination',
    TRUE,
    '["rescisão", "justa-causa", "obrigatória"]',
    50,
    1,
    TRUE,
    NOW(6),
    NOW(6),
    FALSE
);

REPLACE INTO clauses (
    id, client_id, name, description, code, content, clause_type,
    is_mandatory, tags, display_order, version, is_active,
    created_at, updated_at, is_deleted
) VALUES (
    UUID(),
    (SELECT id FROM clients LIMIT 1),
    'Rescisão sem Justa Causa',
    'Condições para rescisão amigável do contrato',
    'TERM-002',
    '<h3>CLÁUSULA - RESCISÃO SEM JUSTA CAUSA</h3>
<p>Qualquer das partes poderá rescindir este contrato sem justa causa, mediante:</p>
<ul>
<li>Notificação prévia por escrito com antecedência mínima de {{NOTICE_PERIOD}} dias;</li>
<li>Pagamento das obrigações vencidas até a data da rescisão;</li>
<li>Cumprimento das obrigações de transição pelo período de {{TRANSITION_PERIOD}} dias.</li>
</ul>
<p>Em caso de rescisão sem justa causa iniciada por {{PARTY_NAME}}, deverá ser paga multa rescisória de {{TERMINATION_FEE_PERCENTAGE}}% sobre o valor total do contrato.</p>
<p>As obrigações de confidencialidade e não-concorrência permanecerão vigentes mesmo após a rescisão.</p>',
    'termination',
    FALSE,
    '["rescisão", "sem-justa-causa", "multa"]',
    51,
    1,
    TRUE,
    NOW(6),
    NOW(6),
    FALSE
);

-- ============================================
-- CONFIDENTIALITY CLAUSES
-- ============================================

REPLACE INTO clauses (
    id, client_id, name, description, code, content, clause_type,
    is_mandatory, tags, display_order, version, is_active,
    created_at, updated_at, is_deleted
) VALUES (
    UUID(),
    (SELECT id FROM clients LIMIT 1),
    'Obrigações de Confidencialidade',
    'Proteção de informações confidenciais',
    'CONF-001',
    '<h3>CLÁUSULA - CONFIDENCIALIDADE</h3>
<p>As partes se comprometem a manter sigilo absoluto sobre todas as Informações Confidenciais recebidas da outra parte.</p>
<p><strong>Informações Confidenciais</strong> incluem, mas não se limitam a:</p>
<ul>
<li>Estratégias de negócio, planos comerciais e financeiros;</li>
<li>Dados de clientes, fornecedores e parceiros;</li>
<li>Tecnologias, processos, software e know-how;</li>
<li>Valores, termos e condições deste contrato;</li>
<li>{{ADDITIONAL_CONFIDENTIAL_INFO}}.</li>
</ul>
<p>As obrigações de confidencialidade:</p>
<ul>
<li>Permanecerão vigentes por {{CONFIDENTIALITY_PERIOD}} anos após o término deste contrato;</li>
<li>Não se aplicam a informações públicas ou já conhecidas pela parte receptora;</li>
<li>Permitem divulgação quando exigida por lei ou ordem judicial.</li>
</ul>
<p>A violação desta cláusula sujeitará a parte infratora ao pagamento de multa de {{CONFIDENTIALITY_PENALTY}} e indenização por perdas e danos.</p>',
    'confidentiality',
    TRUE,
    '["confidencialidade", "sigilo", "nda", "obrigatória"]',
    60,
    1,
    TRUE,
    NOW(6),
    NOW(6),
    FALSE
);

REPLACE INTO clauses (
    id, client_id, name, description, code, content, clause_type,
    is_mandatory, tags, display_order, version, is_active,
    created_at, updated_at, is_deleted
) VALUES (
    UUID(),
    (SELECT id FROM clients LIMIT 1),
    'Não-Concorrência',
    'Obrigação de não concorrência',
    'CONF-002',
    '<h3>CLÁUSULA - NÃO-CONCORRÊNCIA</h3>
<p>Durante a vigência deste contrato e pelo período de {{NON_COMPETE_PERIOD}} após seu término, {{RESTRICTED_PARTY}} não poderá, direta ou indiretamente:</p>
<ul>
<li>Exercer atividades concorrentes com {{COMPANY_NAME}} em {{GEOGRAPHIC_SCOPE}};</li>
<li>Aliciar clientes, fornecedores ou colaboradores da empresa;</li>
<li>Investir, participar ou prestar serviços a empresas concorrentes;</li>
<li>Utilizar conhecimentos adquiridos em benefício de terceiros concorrentes.</li>
</ul>
<p>Excetuam-se desta restrição:</p>
<ul>
<li>Investimentos inferiores a {{INVESTMENT_THRESHOLD}}% em empresas de capital aberto;</li>
<li>Atividades expressamente autorizadas por escrito.</li>
</ul>
<p>A violação desta cláusula sujeitará {{RESTRICTED_PARTY}} ao pagamento de multa de {{NON_COMPETE_PENALTY}} por ocorrência.</p>',
    'confidentiality',
    FALSE,
    '["não-concorrência", "restrições", "competição"]',
    61,
    1,
    TRUE,
    NOW(6),
    NOW(6),
    FALSE
);

-- ============================================
-- DISPUTE RESOLUTION CLAUSES
-- ============================================

REPLACE INTO clauses (
    id, client_id, name, description, code, content, clause_type,
    is_mandatory, tags, display_order, version, is_active,
    created_at, updated_at, is_deleted
) VALUES (
    UUID(),
    (SELECT id FROM clients LIMIT 1),
    'Arbitragem',
    'Resolução de conflitos por arbitragem',
    'DISP-001',
    '<h3>CLÁUSULA - ARBITRAGEM</h3>
<p>Qualquer controvérsia ou reclamação decorrente deste contrato, ou relacionada a ele, será resolvida por arbitragem, de acordo com as regras da {{ARBITRATION_CHAMBER}}.</p>
<p>Condições da arbitragem:</p>
<ul>
<li><strong>Número de árbitros:</strong> {{ARBITRATORS_COUNT}};</li>
<li><strong>Sede da arbitragem:</strong> {{ARBITRATION_LOCATION}};</li>
<li><strong>Idioma:</strong> Português;</li>
<li><strong>Lei aplicável:</strong> Leis do Brasil.</li>
</ul>
<p>As partes se comprometem a:</p>
<ul>
<li>Manter sigilo sobre o procedimento arbitral;</li>
<li>Cumprir integralmente a decisão dos árbitros;</li>
<li>Arcar igualmente com os custos da arbitragem, salvo decisão em contrário.</li>
</ul>
<p>A sentença arbitral será definitiva e vinculante para as partes.</p>',
    'dispute_resolution',
    FALSE,
    '["arbitragem", "resolução", "conflitos"]',
    70,
    1,
    TRUE,
    NOW(6),
    NOW(6),
    FALSE
);

REPLACE INTO clauses (
    id, client_id, name, description, code, content, clause_type,
    is_mandatory, tags, display_order, version, is_active,
    created_at, updated_at, is_deleted
) VALUES (
    UUID(),
    (SELECT id FROM clients LIMIT 1),
    'Foro e Jurisdição',
    'Eleição de foro para resolução de disputas',
    'DISP-002',
    '<h3>CLÁUSULA - FORO</h3>
<p>As partes elegem o foro da Comarca de {{JURISDICTION_CITY}}, {{JURISDICTION_STATE}}, como competente para dirimir quaisquer questões oriundas deste contrato, com renúncia expressa a qualquer outro, por mais privilegiado que seja.</p>
<p>Antes de recorrer ao Poder Judiciário, as partes se comprometem a tentar resolver as controvérsias por meio de:</p>
<ol>
<li><strong>Negociação direta:</strong> prazo de {{NEGOTIATION_PERIOD}} dias;</li>
<li><strong>Mediação:</strong> caso a negociação não seja bem-sucedida, prazo adicional de {{MEDIATION_PERIOD}} dias.</li>
</ol>
<p>Somente após esgotadas as tentativas de resolução amigável, as partes poderão acionar o Poder Judiciário.</p>',
    'dispute_resolution',
    TRUE,
    '["foro", "jurisdição", "mediação", "obrigatória"]',
    71,
    1,
    TRUE,
    NOW(6),
    NOW(6),
    FALSE
);

-- ============================================
-- AMENDMENTS CLAUSES
-- ============================================

REPLACE INTO clauses (
    id, client_id, name, description, code, content, clause_type,
    is_mandatory, tags, display_order, version, is_active,
    created_at, updated_at, is_deleted
) VALUES (
    UUID(),
    (SELECT id FROM clients LIMIT 1),
    'Alteração Contratual',
    'Procedimentos para alteração do contrato',
    'AMEND-001',
    '<h3>CLÁUSULA - ALTERAÇÕES</h3>
<p>Este contrato somente poderá ser alterado mediante:</p>
<ul>
<li>Acordo por escrito assinado por todas as partes;</li>
<li>Aprovação de {{AMENDMENT_APPROVAL_THRESHOLD}}% dos votos em Assembleia Geral, quando aplicável;</li>
<li>Aditivo contratual devidamente registrado.</li>
</ul>
<p>Alterações que não afetem direitos substanciais das partes poderão ser aprovadas por maioria simples.</p>
<p>Qualquer tolerância ou concessão não constituirá novação ou renúncia de direitos, podendo ser revogada a qualquer momento.</p>',
    'amendments',
    TRUE,
    '["alteração", "aditivo", "modificação", "obrigatória"]',
    80,
    1,
    TRUE,
    NOW(6),
    NOW(6),
    FALSE
);

-- ============================================
-- GENERAL CLAUSES
-- ============================================

REPLACE INTO clauses (
    id, client_id, name, description, code, content, clause_type,
    is_mandatory, tags, display_order, version, is_active,
    created_at, updated_at, is_deleted
) VALUES (
    UUID(),
    (SELECT id FROM clients LIMIT 1),
    'Disposições Gerais',
    'Cláusulas gerais e práticas contratuais comuns',
    'GEN-001',
    '<h3>CLÁUSULA - DISPOSIÇÕES GERAIS</h3>
<p><strong>Integralidade:</strong> Este contrato representa o acordo integral entre as partes, revogando todos os entendimentos anteriores.</p>
<p><strong>Independência das Cláusulas:</strong> A invalidade de qualquer cláusula não prejudicará a validade das demais.</p>
<p><strong>Notificações:</strong> Todas as comunicações deverão ser feitas por escrito para os endereços indicados, considerando-se entregues:</p>
<ul>
<li>Se em mãos, na data da entrega;</li>
<li>Se por correio registrado, {{MAIL_DELIVERY_DAYS}} dias úteis após postagem;</li>
<li>Se por e-mail, na data do envio com confirmação de leitura.</li>
</ul>
<p><strong>Cessão:</strong> Nenhuma das partes poderá ceder este contrato sem o consentimento prévio e por escrito da outra parte.</p>
<p><strong>Despesas:</strong> Cada parte arcará com suas próprias despesas relacionadas a este contrato, salvo disposição em contrário.</p>',
    'general',
    TRUE,
    '["geral", "disposições", "notificações", "obrigatória"]',
    90,
    1,
    TRUE,
    NOW(6),
    NOW(6),
    FALSE
);

REPLACE INTO clauses (
    id, client_id, name, description, code, content, clause_type,
    is_mandatory, tags, display_order, version, is_active,
    created_at, updated_at, is_deleted
) VALUES (
    UUID(),
    (SELECT id FROM clients LIMIT 1),
    'Vigência e Prazo',
    'Prazo de vigência do contrato',
    'GEN-002',
    '<h3>CLÁUSULA - VIGÊNCIA</h3>
<p>Este contrato terá vigência de {{CONTRACT_TERM}} a partir da data de sua assinatura, ou seja, de {{START_DATE}} até {{END_DATE}}.</p>
<p>O contrato poderá ser:</p>
<ul>
<li><strong>Renovado automaticamente:</strong> por períodos sucessivos de {{RENEWAL_PERIOD}}, salvo manifestação em contrário com antecedência de {{RENEWAL_NOTICE}} dias;</li>
<li><strong>Rescindido antecipadamente:</strong> nas condições previstas na Cláusula de Rescisão;</li>
<li><strong>Prorrogado:</strong> mediante acordo por escrito entre as partes.</li>
</ul>
<p>As obrigações de confidencialidade, não-concorrência e indenização sobreviverão ao término deste contrato.</p>',
    'general',
    TRUE,
    '["vigência", "prazo", "renovação", "obrigatória"]',
    91,
    1,
    TRUE,
    NOW(6),
    NOW(6),
    FALSE
);

-- ============================================
-- VERIFY INSERTION
-- ============================================

SELECT 
    clause_type,
    COUNT(*) as clause_count,
    SUM(CASE WHEN is_mandatory = TRUE THEN 1 ELSE 0 END) as mandatory_count
FROM clauses 
WHERE is_deleted = FALSE
GROUP BY clause_type
ORDER BY clause_type;

SELECT CONCAT(
    'Successfully inserted ', 
    COUNT(*), 
    ' example clauses across ',
    COUNT(DISTINCT clause_type),
    ' categories'
) AS status
FROM clauses 
WHERE is_deleted = FALSE;
