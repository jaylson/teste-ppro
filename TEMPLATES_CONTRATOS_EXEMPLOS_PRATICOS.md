# Exemplos Práticos de Uso dos Templates de Contratos

Este documento apresenta exemplos práticos de como preencher os templates de contratos com valores reais.

---

## 📄 Exemplo 1: NDA para Negociação de Investimento

### Cenário:
A startup "Ophir Tecnologia" está negociando investimento Série A com o fundo "VentureCapital Partners". É necessário compartilhar informações financeiras sensíveis e por isso as partes assinam um NDA bilateral.

### Preenchimento das Variáveis:

```json
{
  "templateCode": "NDA-BILATERAL-001",
  "contractTitle": "NDA - Negociação Série A com VentureCapital Partners",
  "variables": {
    "party_discloser_name": "Ophir Tecnologia Ltda",
    "party_discloser_cnpj": "12.345.678/0001-90",
    "party_discloser_address": "Avenida Paulista, 1000, 10º andar, Bela Vista, São Paulo/SP, CEP 01310-100",
    "party_discloser_representative_name": "Carlos Eduardo Silva",
    "party_discloser_representative_role": "Diretor Executivo e Sócio Fundador",
    
    "party_recipient_name": "VentureCapital Partners Investimentos S.A.",
    "party_recipient_cnpj": "98.765.432/0001-10",
    "party_recipient_address": "Rua dos Investidores, 500, Jardins, São Paulo/SP, CEP 01452-000",
    "party_recipient_representative_name": "Ana Paula Investidora",
    "party_recipient_representative_role": "Managing Partner",
    
    "business_purpose": "negociação de potencial investimento em rodada Série A, incluindo compartilhamento de informações financeiras, projeções, estratégias comerciais e dados operacionais",
    
    "contract_duration_months": "24",
    "contract_start_date": "17 de fevereiro de 2026",
    "confidentiality_duration_years": "5",
    
    "penalty_amount": "500.000,00 (quinhentos mil reais)",
    
    "jurisdiction_city": "São Paulo",
    "jurisdiction_state": "SP",
    "contract_city": "São Paulo",
    "contract_date": "17 de fevereiro de 2026"
  }
}
```

### Chamada API:

```bash
curl -X POST https://api.partnership-manager.com/api/contracts \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {token}" \
  -d '{
    "templateId": "uuid-do-template-nda",
    "title": "NDA - Negociação Série A com VentureCapital Partners",
    "description": "Acordo de confidencialidade bilateral para proteção de informações durante due diligence",
    "variables": {
      "party_discloser_name": "Ophir Tecnologia Ltda",
      "party_recipient_name": "VentureCapital Partners Investimentos S.A.",
      ...
    }
  }'
```

---

## 📄 Exemplo 2: Stock Option para CTO

### Cenário:
A startup "TechInnovate" está contratando uma CTO (Chief Technology Officer) e, como parte do pacote de remuneração, oferece opções de ações com vesting de 4 anos.

### Preenchimento das Variáveis:

```json
{
  "templateCode": "STOCK-OPTION-001",
  "contractTitle": "Stock Option Plan - Ana Paula Desenvolvedora (CTO)",
  "variables": {
    "company_name": "TechInnovate Soluções Ltda",
    "company_cnpj": "11.222.333/0001-44",
    "company_address": "Avenida Brigadeiro Faria Lima, 1500, conjunto 201, Pinheiros, São Paulo/SP, CEP 01451-000",
    "company_representative_name": "Roberto Fundador",
    "company_representative_role": "CEO e Sócio Fundador",
    
    "beneficiary_name": "Ana Paula Desenvolvedora da Silva",
    "beneficiary_nationality": "brasileira",
    "beneficiary_marital_status": "casada",
    "beneficiary_profession": "engenheira de software",
    "beneficiary_cpf": "123.456.789-00",
    "beneficiary_address": "Rua das Flores, 100, apartamento 502, Vila Madalena, São Paulo/SP, CEP 05435-020",
    
    "total_options": "50.000",
    "ownership_percentage": "5,0",
    "strike_price": "2,50",
    "valuation_date": "01 de janeiro de 2026",
    
    "vesting_period_years": "4",
    "grant_date": "17 de fevereiro de 2026",
    
    "vesting_schedule": "- Cliff de 12 meses: 25% das opções (12.500 opções) disponíveis em 17/02/2027\n- Meses 13 a 48: 2,08% ao mês (1.042 opções mensais) totalizando 75% das opções restantes",
    
    "cliff_percentage": "25",
    "monthly_vesting_percentage": "2,08",
    
    "exercise_start_date": "17 de fevereiro de 2027",
    "exercise_end_date": "17 de fevereiro de 2036",
    "exercise_notice_days": "30",
    
    "termination_exercise_days": "90",
    "resignation_exercise_days": "30",
    
    "lockup_period_months": "12",
    "ipo_exercise_window_days": "180",
    
    "jurisdiction_city": "São Paulo",
    "jurisdiction_state": "SP",
    "contract_city": "São Paulo",
    "contract_signature_date": "17 de fevereiro de 2026",
    
    "vesting_schedule_detailed": "| Período | Opções Liberadas | Opções Acumuladas | % Total |\n|---------|------------------|-------------------|----------|\n| Mês 0-11 | 0 | 0 | 0% |\n| Mês 12 (Cliff) | 12.500 | 12.500 | 25% |\n| Mês 13-24 | 1.042/mês | 25.000 | 50% |\n| Mês 25-36 | 1.042/mês | 37.500 | 75% |\n| Mês 37-48 | 1.042/mês | 50.000 | 100% |"
  }
}
```

### Chamada API:

```bash
curl -X POST https://api.partnership-manager.com/api/contracts \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {token}" \
  -d '{
    "templateId": "uuid-do-template-stock-option",
    "title": "Stock Option Plan - Ana Paula Desenvolvedora (CTO)",
    "description": "Plano de opções de ações para retenção e incentivo da CTO",
    "variables": {
      "company_name": "TechInnovate Soluções Ltda",
      "beneficiary_name": "Ana Paula Desenvolvedora da Silva",
      "total_options": "50.000",
      "ownership_percentage": "5,0",
      ...
    }
  }'
```

---

## 📄 Exemplo 3: NDA Simplificado para Parceria Comercial

### Cenário:
Duas empresas de tecnologia vão explorar uma possível parceria comercial e precisam trocar informações sobre produtos e base de clientes.

### Preenchimento das Variáveis:

```json
{
  "templateCode": "NDA-BILATERAL-001",
  "contractTitle": "NDA - Parceria Comercial CloudSoft e DataAnalytics",
  "variables": {
    "party_discloser_name": "CloudSoft Computação em Nuvem Ltda",
    "party_discloser_cnpj": "22.333.444/0001-55",
    "party_discloser_address": "Rua da Tecnologia, 250, Brooklin, São Paulo/SP, CEP 04571-010",
    "party_discloser_representative_name": "João Pedro Santos",
    "party_discloser_representative_role": "Diretor Comercial",
    
    "party_recipient_name": "DataAnalytics Inteligência de Negócios S.A.",
    "party_recipient_cnpj": "33.444.555/0001-66",
    "party_recipient_address": "Avenida das Nações Unidas, 8000, Pinheiros, São Paulo/SP, CEP 05425-070",
    "party_recipient_representative_name": "Maria Fernanda Costa",
    "party_recipient_representative_role": "VP de Parcerias",
    
    "business_purpose": "avaliação de possível parceria comercial envolvendo integração de produtos e compartilhamento de base de clientes",
    
    "contract_duration_months": "12",
    "contract_start_date": "17 de fevereiro de 2026",
    "confidentiality_duration_years": "3",
    
    "penalty_amount": "100.000,00 (cem mil reais)",
    
    "jurisdiction_city": "São Paulo",
    "jurisdiction_state": "SP",
    "contract_city": "São Paulo",
    "contract_date": "17 de fevereiro de 2026"
  }
}
```

---

## 📄 Exemplo 4: Stock Option para Desenvolvedor Sênior

### Cenário:
Startup early-stage oferece equity menor para um desenvolvedor sênior chave, com vesting acelerado em caso de IPO.

### Preenchimento das Variáveis:

```json
{
  "templateCode": "STOCK-OPTION-001",
  "contractTitle": "Stock Option Plan - Pedro Henrique (Senior Developer)",
  "variables": {
    "company_name": "StartupAI Inteligência Artificial Ltda",
    "company_cnpj": "44.555.666/0001-77",
    "company_address": "Rua Startups, 42, Vila Olímpia, São Paulo/SP, CEP 04551-060",
    "company_representative_name": "Lucas Empreendedor",
    "company_representative_role": "CEO",
    
    "beneficiary_name": "Pedro Henrique Programador Junior",
    "beneficiary_nationality": "brasileiro",
    "beneficiary_marital_status": "solteiro",
    "beneficiary_profession": "desenvolvedor de software",
    "beneficiary_cpf": "987.654.321-00",
    "beneficiary_address": "Avenida Ibirapuera, 3000, apto 1205, Moema, São Paulo/SP, CEP 04029-200",
    
    "total_options": "15.000",
    "ownership_percentage": "1,5",
    "strike_price": "1,00",
    "valuation_date": "01 de fevereiro de 2026",
    
    "vesting_period_years": "4",
    "grant_date": "17 de fevereiro de 2026",
    
    "vesting_schedule": "- Cliff de 12 meses: 25% (3.750 opções) em 17/02/2027\n- Meses 13-48: Vesting mensal linear de 312 opções por mês\n- Aceleração total em caso de IPO ou aquisição",
    
    "cliff_percentage": "25",
    "monthly_vesting_percentage": "2,08",
    
    "exercise_start_date": "17 de fevereiro de 2027",
    "exercise_end_date": "17 de fevereiro de 2035",
    "exercise_notice_days": "15",
    
    "termination_exercise_days": "60",
    "resignation_exercise_days": "30",
    
    "lockup_period_months": "6",
    "ipo_exercise_window_days": "90",
    
    "jurisdiction_city": "São Paulo",
    "jurisdiction_state": "SP",
    "contract_city": "São Paulo",
    "contract_signature_date": "17 de fevereiro de 2026",
    
    "vesting_schedule_detailed": "| Data | Evento | Opções | Acumulado |\n|------|--------|--------|------------|\n| 17/02/2026 | Outorga | 0 | 0 |\n| 17/02/2027 | Cliff | 3.750 | 3.750 |\n| 17/03/2027 | Vesting mensal | 312 | 4.062 |\n| ... | ... | 312/mês | ... |\n| 17/02/2030 | Completo | 312 | 15.000 |"
  }
}
```

---

## 🔧 Dicas para Preenchimento

### 1. Informações Cadastrais
- **CNPJ**: Sempre com pontuação (00.000.000/0001-00)
- **CPF**: Com pontuação (000.000.000-00)
- **Endereços**: Completos com CEP

### 2. Valores Monetários
- **Formato brasileiro**: "500.000,00"
- **Por extenso**: Incluir entre parênteses quando relevante
- **Moeda**: Sempre especificar "R$" ou "reais"

### 3. Datas
- **Formato por extenso**: "17 de fevereiro de 2026"
- **Formato numérico**: "17/02/2026"
- **Consistência**: Use o mesmo formato em todo o contrato

### 4. Percentuais
- **Formato**: "2,5%" ou "2,5" (dependendo do contexto)
- **Precisão**: Use até 2 casas decimais

### 5. Stock Options
- **Total de opções**: Número inteiro sem pontuação ou "50.000"
- **Percentual**: Com uma casa decimal "5,0%"
- **Preço de exercício**: Com duas casas decimais "2,50"

### 6. Cronogramas
- **Tabelas**: Use formato Markdown para melhor visualização
- **Clareza**: Especifique datas exatas quando aplicável
- **Exemplos**: Inclua cálculos de exemplo

---

## 🚨 Validações Importantes

Antes de gerar um contrato, valide:

✅ **Todas as variáveis obrigatórias** foram preenchidas  
✅ **Datas são consistentes** (início < fim, vesting < exercício)  
✅ **Valores numéricos** são razoáveis  
✅ **Percentuais** somam 100% quando aplicável  
✅ **Informações cadastrais** estão corretas (CNPJ, CPF, endereços)  
✅ **Nomes e cargos** dos representantes estão corretos  
✅ **Foro de eleição** está adequado  

---

## 📊 Casos de Uso Comuns

### NDAs
1. **Due Diligence de Investimento**: 24 meses, multa alta
2. **Parcerias Comerciais**: 12 meses, multa moderada
3. **M&A**: 36 meses, multa muito alta
4. **Consultoria**: 12-24 meses, multa baixa-moderada

### Stock Options
1. **C-Level (CEO, CTO, CFO)**: 3-7% equity, 4 anos vesting
2. **Executivos Sênior**: 1-3% equity, 4 anos vesting
3. **Gerentes/Líderes**: 0,5-1,5% equity, 3-4 anos vesting
4. **Desenvolvedores/Especialistas**: 0,1-0,5% equity, 3-4 anos vesting

---

**Última atualização**: 17/02/2026
