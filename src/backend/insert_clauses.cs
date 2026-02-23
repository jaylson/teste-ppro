using System;
using System.Data;
using Dapper;
using MySqlConnector;

// Script para inserir cláusulas de exemplo
// Executar: dotnet script insert_clauses.cs (ou compilar e executar)

var connectionString = "Server=newdataserver.mysql.database.azure.com;Port=3306;Database=ppro;Uid=dataserver_wp;Pwd=wpadm@753;SslMode=Required;CharSet=utf8mb4;";

using var connection = new MySqlConnection(connectionString);
connection.Open();

// Obter client_id
var clientId = connection.QueryFirst<string>("SELECT id FROM clients LIMIT 1");

var clauses = new[]
{
    new { Code = "GOV-001", Name = "Direito de Voto", Description = "Estabelece os direitos de voto dos acionistas", Content = "<h3>CLÁUSULA - DIREITO DE VOTO</h3><p>Cada ação ordinária da {{COMPANY_NAME}} confere ao seu titular o direito a um voto nas Assembleias Gerais.</p>", Type = "governance", Mandatory = true, Tags = "[\"governança\",\"voto\"]", Order = 10 },
    new { Code = "RO-001", Name = "Direito de Preferência", Description = "Direito de preferência na aquisição de ações", Content = "<h3>CLÁUSULA - DIREITO DE PREFERÊNCIA</h3><p>Os acionistas terão direito de preferência na subscrição de novas ações, na proporção de suas participações.</p>", Type = "rights_obligations", Mandatory = true, Tags = "[\"direitos\",\"preferência\"]", Order = 20 },
    new { Code = "RO-002", Name = "Tag Along", Description = "Direito de venda conjunta de ações", Content = "<h3>CLÁUSULA - TAG ALONG</h3><p>Na hipótese de alienação de ações, os demais acionistas terão direito de alienar suas ações nas mesmas condições.</p>", Type = "rights_obligations", Mandatory = false, Tags = "[\"direitos\",\"tag-along\"]", Order = 21 },
    new { Code = "RO-003", Name = "Drag Along", Description = "Direito de venda forçada (arrasto)", Content = "<h3>CLÁUSULA - DRAG ALONG</h3><p>Os acionistas majoritários terão direito de exigir que os demais vendam suas ações em uma operação de alienação de controle.</p>", Type = "rights_obligations", Mandatory = false, Tags = "[\"direitos\",\"drag-along\"]", Order = 22 },
    new { Code = "COMP-001", Name = "Lei Geral de Proteção de Dados", Description = "Conformidade com LGPD", Content = "<h3>CLÁUSULA - LGPD</h3><p>As partes se comprometem a cumprir integralmente a Lei Geral de Proteção de Dados (Lei nº 13.709/2018).</p>", Type = "compliance", Mandatory = true, Tags = "[\"compliance\",\"lgpd\"]", Order = 30 },
    new { Code = "COMP-002", Name = "Anticorrupção", Description = "Conformidade com leis anticorrupção", Content = "<h3>CLÁUSULA - ANTICORRUPÇÃO</h3><p>As partes se comprometem a cumprir a Lei Anticorrupção (Lei nº 12.846/2013).</p>", Type = "compliance", Mandatory = true, Tags = "[\"compliance\",\"anticorrupção\"]", Order = 31 },
    new { Code = "FIN-001", Name = "Investimento", Description = "Termos do investimento e pagamento", Content = "<h3>CLÁUSULA - INVESTIMENTO</h3><p>{{INVESTOR_NAME}} investirá {{INVESTMENT_AMOUNT}} na {{COMPANY_NAME}}.</p>", Type = "financial", Mandatory = false, Tags = "[\"financeiro\",\"investimento\"]", Order = 40 },
    new { Code = "FIN-002", Name = "Dividendos", Description = "Regras para distribuição de dividendos", Content = "<h3>CLÁUSULA - DIVIDENDOS</h3><p>A distribuição de dividendos respeitará o mínimo legal e as preferências estabelecidas.</p>", Type = "financial", Mandatory = false, Tags = "[\"financeiro\",\"dividendos\"]", Order = 41 },
    new { Code = "FIN-003", Name = "Vesting", Description = "Cronograma de vesting para stock options", Content = "<h3>CLÁUSULA - VESTING</h3><p>As opções estarão sujeitas a cliff de {{CLIFF_MONTHS}} meses e vesting de {{VESTING_MONTHS}} meses.</p>", Type = "financial", Mandatory = false, Tags = "[\"financeiro\",\"vesting\"]", Order = 42 },
    new { Code = "TERM-001", Name = "Rescisão por Justa Causa", Description = "Hipóteses de rescisão por justa causa", Content = "<h3>CLÁUSULA - RESCISÃO POR JUSTA CAUSA</h3><p>O contrato poderá ser rescindido por justa causa nas hipóteses de descumprimento material.</p>", Type = "termination", Mandatory = true, Tags = "[\"rescisão\",\"justa-causa\"]", Order = 50 },
    new { Code = "TERM-002", Name = "Rescisão sem Justa Causa", Description = "Condições para rescisão amigável", Content = "<h3>CLÁUSULA - RESCISÃO SEM JUSTA CAUSA</h3><p>Rescisão mediante notificação prévia de {{NOTICE_PERIOD}} dias.</p>", Type = "termination", Mandatory = false, Tags = "[\"rescisão\"]", Order = 51 },
    new { Code = "CONF-001", Name = "Confidencialidade", Description = "Obrigações de sigilo", Content = "<h3>CLÁUSULA - CONFIDENCIALIDADE</h3><p>As partes manterão sigilo absoluto sobre Informações Confidenciais por {{CONFIDENTIALITY_PERIOD}} anos.</p>", Type = "confidentiality", Mandatory = true, Tags = "[\"confidencialidade\",\"sigilo\"]", Order = 60 },
    new { Code = "CONF-002", Name = "Não-Concorrência", Description = "Restrições de concorrência", Content = "<h3>CLÁUSULA - NÃO-CONCORRÊNCIA</h3><p>Durante {{NON_COMPETE_PERIOD}}, a parte não poderá exercer atividades concorrentes.</p>", Type = "confidentiality", Mandatory = false, Tags = "[\"não-concorrência\"]", Order = 61 },
    new { Code = "DISP-001", Name = "Arbitragem", Description = "Resolução por arbitragem", Content = "<h3>CLÁUSULA - ARBITRAGEM</h3><p>Controvérsias serão resolvidas por arbitragem conforme regras da {{ARBITRATION_CHAMBER}}.</p>", Type = "dispute_resolution", Mandatory = false, Tags = "[\"arbitragem\"]", Order = 70 },
    new { Code = "DISP-002", Name = "Foro", Description = "Eleição de foro", Content = "<h3>CLÁUSULA - FORO</h3><p>Foro da Comarca de {{JURISDICTION_CITY}}, com tentativa prévia de mediação.</p>", Type = "dispute_resolution", Mandatory = true, Tags = "[\"foro\",\"jurisdição\"]", Order = 71 },
    new { Code = "AMEND-001", Name = "Alterações", Description = "Procedimentos para alteração", Content = "<h3>CLÁUSULA - ALTERAÇÕES</h3><p>Alterações requerem acordo por escrito de todas as partes.</p>", Type = "amendments", Mandatory = true, Tags = "[\"alteração\",\"aditivo\"]", Order = 80 },
    new { Code = "GEN-001", Name = "Disposições Gerais", Description = "Cláusulas gerais", Content = "<h3>CLÁUSULA - DISPOSIÇÕES GERAIS</h3><p>Integralidade, independência de cláusulas, notificações e cessão.</p>", Type = "general", Mandatory = true, Tags = "[\"geral\",\"disposições\"]", Order = 90 },
    new { Code = "GEN-002", Name = "Vigência", Description = "Prazo de vigência", Content = "<h3>CLÁUSULA - VIGÊNCIA</h3><p>Vigência de {{CONTRACT_TERM}} com renovação automática se não houver manifestação contrária.</p>", Type = "general", Mandatory = true, Tags = "[\"vigência\",\"prazo\"]", Order = 91 },
};

var sql = @"
INSERT INTO clauses (id, client_id, name, description, code, content, clause_type, is_mandatory, tags, display_order, version, is_active, created_at, updated_at, is_deleted)
VALUES (UUID(), @ClientId, @Name, @Description, @Code, @Content, @Type, @Mandatory, @Tags, @Order, 1, 1, NOW(6), NOW(6), 0)
ON DUPLICATE KEY UPDATE
    name = @Name,
    description = @Description,
    content = @Content,
    is_mandatory = @Mandatory,
    tags = @Tags,
    display_order = @Order,
    updated_at = NOW(6)
";

var inserted = 0;
foreach (var clause in clauses)
{
    try
    {
        var result = connection.Execute(sql, new
        {
            ClientId = clientId,
            Name = clause.Name,
            Description = clause.Description,
            Code = clause.Code,
            Content = clause.Content,
            Type = clause.Type,
            Mandatory = clause.Mandatory,
            Tags = clause.Tags,
            Order = clause.Order
        });
        inserted++;
        Console.WriteLine($"✓ {clause.Code} - {clause.Name}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ {clause.Code} - Erro: {ex.Message}");
    }
}

var total = connection.QueryFirst<int>("SELECT COUNT(*) FROM clauses WHERE is_deleted = 0");
Console.WriteLine($"\n✅ {inserted} cláusulas inseridas com sucesso!");
Console.WriteLine($"📊 Total de cláusulas no banco: {total}");
