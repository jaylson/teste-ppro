# Partnership Manager
## Fase 5 — ADDENDUM: Motor de Valuation por Fórmula Customizada

**Versão:** 1.0  
**Data:** 26 de Fevereiro de 2026  
**Tipo:** Extensão do Plano `FASE5_VALUATION_FINANCEIRO_PLANO_EXECUCAO.md`  
**Impacto:** +9 tarefas atômicas | +~55h | Migração 043–044 adicionada  
**Posição no Cronograma:** Intercalado em S1 (DB), S2 (BE) e S3/S4 (FE)

---

## 📋 Sumário

1. [Motivação e Casos de Uso](#1-motivação-e-casos-de-uso)
2. [Decisões de Arquitetura](#2-decisões-de-arquitetura)
3. [Modelo de Dados — Novas Tabelas](#3-modelo-de-dados--novas-tabelas)
4. [Integração com o Engine Existente](#4-integração-com-o-engine-existente)
5. [UX e Interfaces](#5-ux-e-interfaces)
6. [Tarefas Atômicas — DB](#6-tarefas-atômicas--db)
7. [Tarefas Atômicas — Backend](#7-tarefas-atômicas--backend)
8. [Tarefas Atômicas — Frontend](#8-tarefas-atômicas--frontend)
9. [Tarefas Atômicas — Testes](#9-tarefas-atômicas--testes)
10. [Regras de Negócio Novas](#10-regras-de-negócio-novas)
11. [Resumo de Impacto no Plano Original](#11-resumo-de-impacto-no-plano-original)

---

## 1. Motivação e Casos de Uso

### 1.1 Por que metodologias padrão não são suficientes?

As 7 metodologias do engine original (ARR Multiple, DCF, Comparáveis, etc.) cobrem o universo de **empresas SaaS B2B e startups de tecnologia**. Porém, o Partnership Manager serve a um espectro muito mais amplo:

| Setor | Variáveis não-padrão que determinam valor |
|-------|------------------------------------------|
| **Agronegócio** | hectares × produtividade × preço de commodity × NDVI |
| **Saúde / Clínicas** | número de leitos × taxa de ocupação × ticket médio por procedimento |
| **Impacto Social** | SROI (retorno social), CO₂ evitado × crédito de carbono, beneficiários atendidos |
| **Marketplace B2B** | GMV × take_rate × NPS × índice de retenção de compradores |
| **Energia Renovável** | capacidade instalada (MW) × fator de capacidade × PPA médio |
| **Imóveis / PropTech** | VGV × velocidade de vendas × custo de terreno × índice regional |
| **EdTech** | alunos ativos × LTV por curso × taxa de conclusão |
| **Franquias** | unidades × royalty médio × potencial de expansão regional |

### 1.2 O que a feature deve permitir

- Empresa define **suas próprias variáveis** (nome, tipo, unidade, descrição)
- Empresa escreve uma **fórmula matemática** usando essas variáveis
- A fórmula é **versionada** — changes não quebram valuations históricos
- A fórmula pode ser usada como **mais uma metodologia** dentro de um valuation normal (ao lado de ARR Multiple, DCF, etc.)
- A fórmula pode ser **testada com valores de exemplo** antes de ser salva
- O sistema **registra os inputs usados** em cada execução para auditoria
- A empresa pode ter **múltiplas fórmulas** (ex: "Fórmula Safra", "Fórmula Expansão")

---

## 2. Decisões de Arquitetura

### 2.1 Motor de Avaliação de Expressões: NCalc2

**Decisão:** usar a biblioteca **NCalc2** para avaliação segura de expressões matemáticas.

```bash
# Instalar no projeto backend
dotnet add package NCalc2 --version 3.x
```

**Por que NCalc2 e não alternativas?**

| Alternativa | Problema |
|-------------|----------|
| Roslyn / C# eval | Executa código C# real — risco de injeção crítico |
| Python/script externo | Overhead de processo, latência, surface de ataque |
| Fórmula hardcoded | Não resolve o problema (precisa de deploy para cada empresa) |
| **NCalc2** ✅ | Whitelist de operações, sem reflexão, sem I/O, bem testado, decimal nativo |

**Funções permitidas (whitelist explícita no engine):**

```csharp
// Funções matemáticas seguras habilitadas
ROUND(x, decimais)   → arredondamento
MIN(a, b)            → mínimo
MAX(a, b)            → máximo
ABS(x)               → valor absoluto
IF(cond, vtrue, vfalse) → condicional
POW(base, exp)       → potência
SQRT(x)              → raiz quadrada
LOG(x)               → logaritmo natural

// Operadores habilitados
+, -, *, /           → aritméticos
>, <, >=, <=, ==, != → comparação
AND, OR, NOT         → lógicos
```

**Funções BLOQUEADAS (segurança):**

```csharp
// Nenhuma função de I/O, reflexão, acesso a sistema
// NCalc2 por padrão não tem — garantir via EvaluateOptions.IgnoreCase
// + validação do AST antes da execução
```

### 2.2 Sintaxe da Fórmula

**Formato:** variáveis entre colchetes `[nome_da_variavel]`

```
Exemplos de fórmulas válidas:

// Agronegócio
[hectares] * [preco_saca] * [sacas_por_hectare] * [fator_sustentabilidade]

// Marketplace
[gmv] * [take_rate] * MIN([retencao_compradores], 0.95) * 12

// Com condicional
IF([stage] == 1, [arr] * 8, IF([stage] == 2, [arr] * 12, [arr] * 6))

// Com arredondamento
ROUND([receita_liquida] * [multiplo_setor] - [divida_liquida], 2)

// Impacto social (valor híbrido)
([receita] * [mrr_multiple]) + ([beneficiarios] * [valor_por_beneficiario])
```

### 2.3 Versionamento de Fórmulas

**Problema:** se uma empresa usa a Fórmula v1 num valuation de Jan/26 e depois modifica a fórmula para v2, o valuation histórico deve mostrar v1 (com os inputs originais).

**Solução:**
- Cada `ValuationCustomFormula` tem uma `current_version`
- Cada `ValuationFormulaVersion` é **imutável** após criação
- `ValuationMethod` referencia `formula_version_id` (não `formula_id`)
- Editar a fórmula → cria nova versão → `current_version` aponta para ela
- Valuation histórico ainda referencia a versão que usou

```
ValuationCustomFormula (formula_id: abc)
  ├── ValuationFormulaVersion v1 (version_id: x1) [imutável]
  │     expression: "[gmv] * [take_rate]"
  │     variables: [gmv, take_rate]
  ├── ValuationFormulaVersion v2 (version_id: x2) [imutável]
  │     expression: "[gmv] * [take_rate] * [retention]"
  │     variables: [gmv, take_rate, retention]
  └── current_version_id: x2  ← aponta para a mais recente

ValuationMethod (no valuation de Jan/26)
  ├── method_type: 'custom'
  └── formula_version_id: x1  ← referencia v1 imutavelmente
```

### 2.4 Tipos de Variáveis

| Tipo | Storage | Validação | Exemplo de uso |
|------|---------|-----------|----------------|
| `currency` | DECIMAL(18,2) | ≥ 0 | GMV, Receita, Dívida |
| `percentage` | DECIMAL(8,4) | 0–100 | Take Rate, Retenção |
| `number` | DECIMAL(18,4) | qualquer | Hectares, Alunos, Leitos |
| `integer` | INT | ≥ 0 | Unidades, Funcionários |
| `multiplier` | DECIMAL(8,2) | > 0 | Múltiplo setorial |
| `boolean` | TINYINT(1) | 0 ou 1 | Flag de certificação |

### 2.5 Segurança — Validação do AST antes da execução

```csharp
public class FormulaSecurityValidator
{
    private static readonly HashSet<string> AllowedFunctions = new()
    {
        "round", "min", "max", "abs", "if", "pow", "sqrt", "log"
    };

    // Parsear a expressão e verificar o AST ANTES de executar
    // Rejeitar qualquer nó que não seja: número, variável, operador aritmético, função da whitelist
    public ValidationResult Validate(string expression, IEnumerable<string> declaredVariables)
    {
        // 1. Verificar que todas as variáveis na expressão foram declaradas
        // 2. Verificar que todas as funções estão na whitelist
        // 3. Verificar profundidade máxima do AST (evitar recursão infinita)
        // 4. Verificar tamanho máximo da expressão (4096 chars)
        // 5. Rejeitar divisão literal por zero
    }
}
```

---

## 3. Modelo de Dados — Novas Tabelas

### 3.1 Diagrama de Relacionamento

```
valuation_custom_formulas
    │ 1
    │
    │ N
valuation_formula_versions (imutáveis)
    │ 1
    │
    │ N
valuation_formula_variables (por versão)
    │
    │ (formula_version_id referenciado em)
    │
valuation_methods.formula_version_id  ←  já existe, apenas adicionar coluna
    │
    │ 1
    │
    │ N
valuation_formula_executions (log de inputs usados)
```

### 3.2 Tabela: `valuation_custom_formulas`

Definição da fórmula customizada de uma empresa (container — as versões ficam em outra tabela).

| Campo | Tipo | Obrigatório | Descrição |
|-------|------|-------------|-----------|
| `id` | CHAR(36) | Sim | UUID |
| `company_id` | CHAR(36) | Sim | FK para companies |
| `name` | VARCHAR(200) | Sim | Nome da fórmula: "Fórmula Safra Agro" |
| `description` | TEXT | Não | Explicação da lógica de negócio |
| `sector_tag` | VARCHAR(100) | Não | Tag livre: "agro", "saude", "impacto" |
| `current_version_id` | CHAR(36) | Não | FK para valuation_formula_versions (nullable — null até 1ª versão ser salva) |
| `is_active` | TINYINT(1) | Sim | Ativa/desativada (soft) |
| `created_by` | CHAR(36) | Sim | FK para users |
| `created_at` | TIMESTAMP | Sim | |
| `updated_at` | TIMESTAMP | Sim | |
| `deleted_at` | TIMESTAMP | Não | Soft delete |

### 3.3 Tabela: `valuation_formula_versions`

**Imutável após criação.** Contém a expressão e as variáveis de uma versão específica.

| Campo | Tipo | Obrigatório | Descrição |
|-------|------|-------------|-----------|
| `id` | CHAR(36) | Sim | UUID |
| `formula_id` | CHAR(36) | Sim | FK para valuation_custom_formulas |
| `version_number` | INT UNSIGNED | Sim | 1, 2, 3... (AUTO_INCREMENT por fórmula) |
| `expression` | TEXT | Sim | Expressão matemática: `[gmv] * [take_rate] * 12` |
| `variables` | JSON | Sim | Array de VariableDefinition (nome, tipo, unidade, descrição, valor_padrão) |
| `result_unit` | VARCHAR(50) | Sim | Unidade do resultado: "BRL", "USD", "pontos" |
| `result_label` | VARCHAR(200) | Não | Label de exibição: "Valor da Empresa (Modelo Agro)" |
| `test_inputs` | JSON | Não | Inputs de exemplo para testar a fórmula (readonly) |
| `test_result` | DECIMAL(18,2) | Não | Resultado calculado com test_inputs (readonly) |
| `validation_status` | ENUM | Sim | `draft`, `validated`, `invalid` |
| `validation_errors` | JSON | Não | Lista de erros encontrados na validação |
| `created_by` | CHAR(36) | Sim | FK para users |
| `created_at` | TIMESTAMP | Sim | (imutável após criação) |

**Estrutura do campo `variables` (JSON Array):**
```json
[
  {
    "name": "gmv",
    "label": "GMV (Gross Merchandise Volume)",
    "type": "currency",
    "unit": "BRL",
    "description": "Volume total transacionado na plataforma nos últimos 12 meses",
    "required": true,
    "default_value": null,
    "min_value": 0,
    "max_value": null,
    "order": 1
  },
  {
    "name": "take_rate",
    "label": "Take Rate",
    "type": "percentage",
    "unit": "%",
    "description": "Percentual de comissão cobrado sobre cada transação",
    "required": true,
    "default_value": 15,
    "min_value": 0,
    "max_value": 100,
    "order": 2
  },
  {
    "name": "retention_bonus",
    "label": "Bônus de Retenção",
    "type": "multiplier",
    "unit": "x",
    "description": "Multiplicador baseado na taxa de retenção de compradores ativos",
    "required": false,
    "default_value": 1.0,
    "min_value": 0.5,
    "max_value": 2.0,
    "order": 3
  }
]
```

### 3.4 Tabela: `valuation_formula_executions`

Log imutável de cada execução da fórmula dentro de um valuation. Garante auditoria total.

| Campo | Tipo | Obrigatório | Descrição |
|-------|------|-------------|-----------|
| `id` | CHAR(36) | Sim | UUID |
| `valuation_method_id` | CHAR(36) | Sim | FK para valuation_methods |
| `formula_version_id` | CHAR(36) | Sim | FK para valuation_formula_versions |
| `inputs_used` | JSON | Sim | Snapshot dos valores informados (exatamente o que foi calculado) |
| `calculated_value` | DECIMAL(18,2) | Sim | Resultado do cálculo |
| `expression_snapshot` | TEXT | Sim | Cópia da expressão no momento da execução (extra segurança) |
| `executed_by` | CHAR(36) | Sim | FK para users |
| `executed_at` | TIMESTAMP | Sim | |

**Estrutura do campo `inputs_used`:**
```json
{
  "gmv": 12000000.00,
  "take_rate": 15.50,
  "retention_bonus": 1.20,
  "_formula_result": 2232000.00,
  "_expression_evaluated": "[gmv] * ([take_rate] / 100) * [retention_bonus]"
}
```

### 3.5 Alteração na Tabela Existente: `valuation_methods`

Adicionar coluna `formula_version_id` (nullable — nulo para metodologias padrão):

```sql
-- Migration 043 altera valuation_methods
ALTER TABLE valuation_methods
  ADD COLUMN formula_version_id CHAR(36) NULL AFTER method_type,
  ADD CONSTRAINT fk_method_formula_version
    FOREIGN KEY (formula_version_id)
    REFERENCES valuation_formula_versions(id)
    ON DELETE RESTRICT;

-- Alterar ENUM method_type para incluir 'custom'
ALTER TABLE valuation_methods
  MODIFY COLUMN method_type
    ENUM('arr_multiple','mrr_multiple','ebitda_multiple','dcf','comparable',
         'book_value','replacement','custom','other') NOT NULL;
```

> **Regra:** `formula_version_id` é **obrigatório** quando `method_type = 'custom'` e **NULL** para todos os outros tipos. Validado na camada de Service.

---

## 4. Integração com o Engine Existente

### 4.1 Ponto de extensão no `ValuationCalculationEngine`

O engine existente (F5-ENG-BE-001) precisa de uma extensão para delegar ao `CustomFormulaEngine` quando o tipo for `custom`:

```csharp
// ValuationCalculationEngine.cs — MODIFICAR (não recriar)
// Adicionar injeção de dependência do CustomFormulaEngine

public class ValuationCalculationEngine
{
    private readonly ICustomFormulaEngine _customFormulaEngine;

    // Métodos existentes: CalculateArrMultiple, CalculateDcf, etc.
    // ...

    // NOVO método — delega para o engine de fórmulas customizadas
    public async Task<ValuationCalculationResult> CalculateCustomAsync(
        ValuationFormulaVersion formulaVersion,
        Dictionary<string, decimal> inputs,
        Guid executedBy)
    {
        return await _customFormulaEngine.EvaluateAsync(formulaVersion, inputs, executedBy);
    }
}
```

### 4.2 Fluxo completo — adicionar metodologia custom a um valuation

```
1. Usuário acessa Wizard de Valuation → Step 2: Metodologias
2. Clica em "Usar Fórmula Customizada"
3. Sistema lista as fórmulas ativas da empresa (GET /custom-formulas)
4. Usuário seleciona "Fórmula Safra Agro v3"
5. Sistema exibe formulário dinâmico com as variáveis da fórmula (version_id = current)
6. Usuário preenche: gmv=12M, take_rate=15.5, retention_bonus=1.2
7. Botão "Calcular" → POST /valuations/{id}/methods/calculate-custom
8. Backend:
   a. Valida todos os inputs (tipo, min, max, required)
   b. FormulaSecurityValidator.Validate(expression, variables)
   c. NCalc2 evalua: 12.000.000 * (15.5/100) * 1.2 = 2.232.000,00
   d. Persiste ValuationMethod com method_type='custom', formula_version_id=x3
   e. Persiste ValuationFormulaExecution com snapshot dos inputs e resultado
9. Frontend exibe: "Fórmula Safra Agro v3 → R$ 2.232.000,00" com breakdown de variáveis
10. Usuário pode selecionar como metodologia principal (ou manter ao lado das padrão)
```

---

## 5. UX e Interfaces

### 5.1 Nova Seção no Menu

```
SIDEBAR NAVIGATION (adição)

💰 Valuation
   ├── 📋 Lista de Valuations
   ├── ➕ Novo Valuation
   ├── 📊 Dashboard de Valuation
   ├── 📜 Histórico
   └── 🔧 Fórmulas Customizadas     ← NOVO (apenas admin/founder)
```

### 5.2 Telas Novas — Rotas

```
/valuations/custom-formulas                    → Lista de Fórmulas da Empresa
/valuations/custom-formulas/new                → Criar nova fórmula
/valuations/custom-formulas/:id                → Detalhe e histórico de versões
/valuations/custom-formulas/:id/edit           → Editar (cria nova versão)
/valuations/custom-formulas/:id/test           → Testar fórmula com valores de exemplo
```

### 5.3 Wireframe — Lista de Fórmulas Customizadas

```
FÓRMULAS DE VALUATION CUSTOMIZADAS
─────────────────────────────────────────────────────────────────────
[🔍 Buscar...]   [Setor ▼]   [Status ▼]                [+ Nova Fórmula]

Nome                      Versão   Setor      Última alteração   Usos   Status
──────────────────────────────────────────────────────────────────────────────
Fórmula Safra Agro        v3       agro       12/01/2026 por JS   4      ✅ Ativa
Modelo Marketplace B2B    v1       tech       05/01/2026 por MS   1      ✅ Ativa
Fórmula Impacto Social    v2       social     28/12/2025 por JS   0      ⏸ Inativa

[1-3 de 3]
```

### 5.4 Wireframe — Criar/Editar Fórmula (Formula Builder)

```
NOVA FÓRMULA DE VALUATION
─────────────────────────────────────────────────────────────────────
Passo 1: Identificação
──────────────────────
Nome *                [Fórmula Safra Agro                        ]
Descrição             [Valoração baseada em produção agrícola e  ]
                      [preço de commodity no mercado futuro...   ]
Tag de Setor          [agro            ]  (livre, para organizar)
Unidade do Resultado  [BRL ▼]


Passo 2: Variáveis da Fórmula
─────────────────────────────
Define as variáveis que serão informadas a cada uso da fórmula.

[+ Adicionar Variável]

┌── Variável 1 ─────────────────────────────────────────────────────┐
│ Nome interno *    [hectares            ]  (sem espaços, sem acento)│
│ Label para exib.  [Área cultivada (ha) ]                          │
│ Tipo *            [number        ▼]                               │
│ Unidade           [hectares      ]                                │
│ Descrição         [Total de hectares sob cultivo na safra atual   │
│ Obrigatória?      [✅ Sim]                                        │
│ Valor padrão      [____________]                                  │
│ Min / Max         [0      ] / [____________]                      │
│ Ordem de exibição [1  ]                                           │
│                                         [🗑️ Remover variável]   │
└───────────────────────────────────────────────────────────────────┘

┌── Variável 2 ─────────────────────────────────────────────────────┐
│ Nome interno *    [preco_saca          ]                           │
│ Label para exib.  [Preço da saca (R$)  ]                          │
│ Tipo *            [currency      ▼]                               │
│ Unidade           [BRL           ]                                │
│ Obrigatória?      [✅ Sim]                                        │
│ Min / Max         [0      ] / [____________]                      │
│                                         [🗑️ Remover variável]   │
└───────────────────────────────────────────────────────────────────┘

┌── Variável 3 ─────────────────────────────────────────────────────┐
│ Nome interno *    [sacas_por_hectare   ]                           │
│ Label para exib.  [Sacas por hectare   ]                          │
│ Tipo *            [number        ▼]                               │
│ Obrigatória?      [✅ Sim]                                        │
└───────────────────────────────────────────────────────────────────┘

┌── Variável 4 ─────────────────────────────────────────────────────┐
│ Nome interno *    [multiplo_setor      ]                           │
│ Label para exib.  [Múltiplo do setor   ]                          │
│ Tipo *            [multiplier    ▼]                               │
│ Obrigatória?      [✅ Sim]                                        │
│ Min / Max         [0.5    ] / [20    ]                            │
└───────────────────────────────────────────────────────────────────┘


Passo 3: Expressão da Fórmula
─────────────────────────────
Escreva a expressão usando [nome_da_variavel] como referência.

┌── Editor de Expressão ────────────────────────────────────────────┐
│  [hectares] * [preco_saca] * [sacas_por_hectare] * [multiplo_setor]│
│                                                                   │
│  Variáveis disponíveis (clique para inserir):                     │
│  [hectares]  [preco_saca]  [sacas_por_hectare]  [multiplo_setor]  │
│                                                                   │
│  Funções: ROUND()  MIN()  MAX()  ABS()  IF(,, )  POW()           │
└───────────────────────────────────────────────────────────────────┘

⚠️ Validação:  [🟢 Expressão válida — todas as variáveis declaradas]
               (ou: 🔴 Erro: variável [preco_acao] não declarada)


Passo 4: Testar com Valores de Exemplo
───────────────────────────────────────
Preencha valores para validar o resultado antes de salvar.

hectares             [  500        ]  hectares
preco_saca (R$)      [  180,00     ]  BRL
sacas_por_hectare    [  60         ]
multiplo_setor       [  8,0        ]  x

[▶ Calcular]

┌── Resultado do Teste ─────────────────────────────────────────────┐
│  500 × R$180,00 × 60 × 8,0  =  R$ 43.200.000,00                  │
│                                                                   │
│  Breakdown passo a passo:                                         │
│  hectares × preco_saca = 500 × 180 = 90.000                      │
│  × sacas_por_hectare = 90.000 × 60 = 5.400.000                   │
│  × multiplo_setor = 5.400.000 × 8 = R$ 43.200.000,00             │
└───────────────────────────────────────────────────────────────────┘

[💾 Salvar Fórmula]      (salva como versão 1 ou versão N+1 se edição)
```

### 5.5 Wireframe — Usar Fórmula no Wizard de Valuation (Extensão do Step 2)

```
NOVO VALUATION — Passo 2: Metodologias
─────────────────────────────────────────────────────────────────────
Metodologias disponíveis para adicionar:
[ARR Multiple] [DCF] [Comparáveis] [EBITDA] [MRR] [Patrimonial]
[🔧 Fórmula Customizada ▼]   ← dropdown com as fórmulas ativas da empresa
   ├── Fórmula Safra Agro (v3)
   ├── Modelo Marketplace B2B (v1)
   └── [+ Criar nova fórmula...]

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Após selecionar "Fórmula Safra Agro (v3)":

┌── 🔧 Fórmula Safra Agro — v3 ────────────────────────────────────┐
│                                                                   │
│  Fórmula: [hectares] * [preco_saca] * [sacas_por_hectare]        │
│           * [multiplo_setor]                                      │
│                                                                   │
│  ─ Preencha os valores para este valuation ─                     │
│                                                                   │
│  Área cultivada (ha)        [  1.200         ]  hectares          │
│  Preço da saca (R$)         [  192,00        ]  BRL               │
│  Sacas por hectare          [  65             ]                   │
│  Múltiplo do setor          [  7,5            ]  x               │
│                                                                   │
│  [▶ Calcular em tempo real]                                       │
│                                                                   │
│  ┌─────────────────────────────────────────────────────────┐     │
│  │  Resultado: R$ 112.320.000,00                           │     │
│  │  1.200 × R$192 × 65 × 7,5                              │     │
│  └─────────────────────────────────────────────────────────┘     │
│                                                                   │
│  [⭐ Selecionar como metodologia principal]  [🗑️ Remover]        │
└───────────────────────────────────────────────────────────────────┘
```

### 5.6 Wireframe — Detalhe do Valuation com Metodologia Custom

```
VALUATION — Safra 2026 | Janeiro 2026
Status: ✅ APROVADO
─────────────────────────────────────────────────────────────────────

METODOLOGIAS CALCULADAS
────────────────────────────────────────────────────────────────────
⭐ Fórmula Safra Agro v3    R$ 112.320.000,00   [ver detalhes ▼]
   ARR Multiple              R$  85.000.000,00
   DCF                       R$  78.500.000,00

   [ver detalhes ▼] expande para:
   ┌─────────────────────────────────────────────────────────────┐
   │  Expressão: [hectares] * [preco_saca] * [sacas_por_hectare] │
   │             * [multiplo_setor]                              │
   │                                                             │
   │  Variáveis utilizadas:                                      │
   │  ├── hectares:          1.200 ha                            │
   │  ├── preco_saca:        R$ 192,00                           │
   │  ├── sacas_por_hectare: 65                                  │
   │  └── multiplo_setor:    7,5x                                │
   │                                                             │
   │  Calculado por: Maria Souza em 12/01/2026 14:32             │
   │  Fórmula versão: v3 (criada em 05/01/2026)                  │
   └─────────────────────────────────────────────────────────────┘
```

---

## 6. Tarefas Atômicas — DB

### F5-CFV-DB-001: Migrations — Tabelas de Fórmulas Customizadas

- **Status:** [ ] Pendente
- **Tipo:** Database Migration
- **Estimativa:** 5h
- **Responsável:** DBA / BE1
- **Dependências:** F5-DB-001 (tabela `valuations` deve existir)
- **Posição no cronograma:** Final da Semana 1

**Arquivos a criar:**
```
/database/migrations/043_create_custom_formula_tables.sql
/database/migrations/044_alter_valuation_methods_add_formula_version.sql
```

**SQL — 043_create_custom_formula_tables.sql:**
```sql
-- UP

-- 1. Fórmulas customizadas da empresa
CREATE TABLE valuation_custom_formulas (
    id                  CHAR(36)        NOT NULL PRIMARY KEY DEFAULT (UUID()),
    company_id          CHAR(36)        NOT NULL,
    name                VARCHAR(200)    NOT NULL,
    description         TEXT            NULL,
    sector_tag          VARCHAR(100)    NULL,
    current_version_id  CHAR(36)        NULL,  -- preenchido após criar 1ª versão
    is_active           TINYINT(1)      NOT NULL DEFAULT 1,
    created_by          CHAR(36)        NOT NULL,
    created_at          TIMESTAMP       NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at          TIMESTAMP       NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    deleted_at          TIMESTAMP       NULL,
    CONSTRAINT fk_formula_company   FOREIGN KEY (company_id)  REFERENCES companies(id),
    CONSTRAINT fk_formula_created   FOREIGN KEY (created_by)  REFERENCES users(id),
    INDEX idx_formula_company  (company_id),
    INDEX idx_formula_active   (company_id, is_active)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 2. Versões imutáveis da fórmula
CREATE TABLE valuation_formula_versions (
    id                  CHAR(36)        NOT NULL PRIMARY KEY DEFAULT (UUID()),
    formula_id          CHAR(36)        NOT NULL,
    version_number      INT UNSIGNED    NOT NULL,
    expression          TEXT            NOT NULL,
    variables           JSON            NOT NULL,
    result_unit         VARCHAR(50)     NOT NULL DEFAULT 'BRL',
    result_label        VARCHAR(200)    NULL,
    test_inputs         JSON            NULL,
    test_result         DECIMAL(18,2)   NULL,
    validation_status   ENUM('draft','validated','invalid') NOT NULL DEFAULT 'draft',
    validation_errors   JSON            NULL,
    created_by          CHAR(36)        NOT NULL,
    created_at          TIMESTAMP       NOT NULL DEFAULT CURRENT_TIMESTAMP,
    -- SEM updated_at — versões são IMUTÁVEIS após criação
    CONSTRAINT fk_version_formula  FOREIGN KEY (formula_id)  REFERENCES valuation_custom_formulas(id),
    CONSTRAINT fk_version_created  FOREIGN KEY (created_by)  REFERENCES users(id),
    UNIQUE INDEX idx_version_formula_number (formula_id, version_number),
    INDEX idx_version_formula (formula_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 3. Self-reference: current_version_id na fórmula aponta para versão
ALTER TABLE valuation_custom_formulas
    ADD CONSTRAINT fk_formula_current_version
        FOREIGN KEY (current_version_id)
        REFERENCES valuation_formula_versions(id)
        ON DELETE SET NULL;

-- 4. Log de execuções (imutável)
CREATE TABLE valuation_formula_executions (
    id                      CHAR(36)        NOT NULL PRIMARY KEY DEFAULT (UUID()),
    valuation_method_id     CHAR(36)        NOT NULL,
    formula_version_id      CHAR(36)        NOT NULL,
    inputs_used             JSON            NOT NULL,
    calculated_value        DECIMAL(18,2)   NOT NULL,
    expression_snapshot     TEXT            NOT NULL,
    executed_by             CHAR(36)        NOT NULL,
    executed_at             TIMESTAMP       NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_exec_method   FOREIGN KEY (valuation_method_id) REFERENCES valuation_methods(id),
    CONSTRAINT fk_exec_version  FOREIGN KEY (formula_version_id)  REFERENCES valuation_formula_versions(id),
    CONSTRAINT fk_exec_user     FOREIGN KEY (executed_by)         REFERENCES users(id),
    INDEX idx_exec_method   (valuation_method_id),
    INDEX idx_exec_version  (formula_version_id),
    INDEX idx_exec_date     (executed_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- DOWN (ordem inversa)
DROP TABLE IF EXISTS valuation_formula_executions;
ALTER TABLE valuation_custom_formulas DROP FOREIGN KEY fk_formula_current_version;
DROP TABLE IF EXISTS valuation_formula_versions;
DROP TABLE IF EXISTS valuation_custom_formulas;
```

**SQL — 044_alter_valuation_methods_add_formula_version.sql:**
```sql
-- UP
ALTER TABLE valuation_methods
    ADD COLUMN formula_version_id CHAR(36) NULL AFTER method_type,
    ADD CONSTRAINT fk_method_formula_version
        FOREIGN KEY (formula_version_id)
        REFERENCES valuation_formula_versions(id)
        ON DELETE RESTRICT;

ALTER TABLE valuation_methods
    MODIFY COLUMN method_type
        ENUM('arr_multiple','mrr_multiple','ebitda_multiple','dcf','comparable',
             'book_value','replacement','custom','other') NOT NULL;

-- DOWN
ALTER TABLE valuation_methods DROP FOREIGN KEY fk_method_formula_version;
ALTER TABLE valuation_methods DROP COLUMN formula_version_id;
ALTER TABLE valuation_methods
    MODIFY COLUMN method_type
        ENUM('arr_multiple','mrr_multiple','ebitda_multiple','dcf','comparable',
             'book_value','replacement','other') NOT NULL;
```

**Critérios de Aceite:**
- [ ] 3 novas tabelas criadas + 1 tabela existente alterada
- [ ] Self-reference de `current_version_id` funcional (circular FK resolvida com SET NULL)
- [ ] UNIQUE INDEX `(formula_id, version_number)` impede duplicatas
- [ ] Tabela `valuation_formula_versions` **sem** `updated_at` (imutável por design)
- [ ] Enum de `method_type` em `valuation_methods` inclui `'custom'`
- [ ] Scripts DOWN testados e funcionais na ordem correta
- [ ] `DESCRIBE valuation_formula_versions;` retorna todas as colunas

---

## 7. Tarefas Atômicas — Backend

### F5-CFV-BE-001: Entities + Enums — Custom Formula

- **Status:** [ ] Pendente
- **Tipo:** Backend Entity
- **Estimativa:** 5h
- **Responsável:** BE1
- **Dependências:** F5-CFV-DB-001
- **Posição:** Semana 2 (início)

**Arquivos a criar:**
```
/src/backend/PartnershipManager.Domain/Entities/ValuationCustomFormula.cs
/src/backend/PartnershipManager.Domain/Entities/ValuationFormulaVersion.cs
/src/backend/PartnershipManager.Domain/Entities/ValuationFormulaExecution.cs
/src/backend/PartnershipManager.Domain/Models/FormulaVariable.cs
/src/backend/PartnershipManager.Domain/Enums/FormulaValidationStatus.cs
/src/backend/PartnershipManager.Domain/Enums/FormulaVariableType.cs
```

**Notas de implementação:**
```csharp
// FormulaVariable.cs — Model (não é Entity de banco — é deserializado do JSON)
public record FormulaVariable(
    string Name,              // nome interno (slug)
    string Label,             // nome para exibição
    FormulaVariableType Type, // currency, percentage, number, integer, multiplier, boolean
    string? Unit,
    string? Description,
    bool Required,
    decimal? DefaultValue,
    decimal? MinValue,
    decimal? MaxValue,
    int Order
);

// ValuationFormulaVersion.cs — Herdar BaseEntity MAS sem updated_at
// Deserializar campo JSON 'variables' → List<FormulaVariable>
// Não usar ICollection<> para variáveis — elas vivem no JSON

// ValuationCustomFormula.cs — Navigation property para CurrentVersion
public ValuationFormulaVersion? CurrentVersion { get; set; }
```

**Modificar Entity existente:**
```csharp
// ValuationMethod.cs — adicionar propriedade
public Guid? FormulaVersionId { get; set; }
public ValuationFormulaVersion? FormulaVersion { get; set; }
```

**Critérios de Aceite:**
- [ ] `ValuationFormulaVersion` não tem `UpdatedAt` (imutabilidade por design)
- [ ] `FormulaVariable` é um record (value object), não uma Entity
- [ ] JSON desserialização de `variables` funciona via `System.Text.Json`
- [ ] `FormulaValidationStatus`: `Draft`, `Validated`, `Invalid`
- [ ] `FormulaVariableType`: `Currency`, `Percentage`, `Number`, `Integer`, `Multiplier`, `Boolean`
- [ ] `dotnet build` sem erros após criação

---

### F5-CFV-BE-002: Repository ICustomFormulaRepository + CustomFormulaRepository

- **Status:** [ ] Pendente
- **Tipo:** Backend Repository
- **Estimativa:** 7h
- **Responsável:** BE1
- **Dependências:** F5-CFV-BE-001

**Arquivos a criar:**
```
/src/backend/PartnershipManager.Domain/Interfaces/ICustomFormulaRepository.cs
/src/backend/PartnershipManager.Infrastructure/Repositories/CustomFormulaRepository.cs
```

**Métodos obrigatórios:**
```csharp
// Fórmulas
Task<ValuationCustomFormula?> GetByIdAsync(Guid id, Guid companyId);
Task<IEnumerable<ValuationCustomFormula>> GetActiveByCompanyAsync(Guid companyId);
Task<PagedResult<ValuationCustomFormula>> GetByCompanyAsync(Guid companyId, PaginationRequest request);
Task<Guid> CreateAsync(ValuationCustomFormula formula);
Task<bool> UpdateActiveStatusAsync(Guid id, bool isActive);
Task<bool> UpdateCurrentVersionAsync(Guid formulaId, Guid versionId);

// Versões
Task<ValuationFormulaVersion?> GetVersionByIdAsync(Guid versionId);
Task<ValuationFormulaVersion?> GetCurrentVersionAsync(Guid formulaId);
Task<IEnumerable<ValuationFormulaVersion>> GetVersionHistoryAsync(Guid formulaId);
Task<int> GetNextVersionNumberAsync(Guid formulaId);  // MAX(version_number) + 1
Task<Guid> CreateVersionAsync(ValuationFormulaVersion version);
Task<bool> UpdateValidationStatusAsync(Guid versionId, FormulaValidationStatus status, List<string>? errors = null);

// Execuções
Task<Guid> LogExecutionAsync(ValuationFormulaExecution execution);
Task<IEnumerable<ValuationFormulaExecution>> GetExecutionsByMethodAsync(Guid methodId);
```

**Critérios de Aceite:**
- [ ] `GetActiveByCompanyAsync` filtra `is_active = 1 AND deleted_at IS NULL`
- [ ] `CreateVersionAsync` usa transação para: criar versão + atualizar `current_version_id` na fórmula
- [ ] `LogExecutionAsync` é chamado APÓS o cálculo ser persistido em `valuation_methods`
- [ ] Todas as queries filtram `company_id` (multi-tenancy)

---

### F5-CFV-BE-003: FormulaSecurityValidator + CustomFormulaEngine

- **Status:** [ ] Pendente
- **Tipo:** Backend Engine (CRÍTICO — segurança)
- **Estimativa:** 12h
- **Responsável:** BE1 (sênior)
- **Dependências:** F5-CFV-BE-001

**Arquivos a criar:**
```
/src/backend/PartnershipManager.Application/Services/Formula/FormulaSecurityValidator.cs
/src/backend/PartnershipManager.Application/Services/Formula/CustomFormulaEngine.cs
/src/backend/PartnershipManager.Application/Services/Formula/FormulaParser.cs
/src/backend/PartnershipManager.Application/Models/FormulaValidationResult.cs
/src/backend/PartnershipManager.Application/Models/FormulaEvaluationResult.cs
```

**Instalar dependência:**
```bash
cd src/backend/PartnershipManager.Application
dotnet add package NCalc2 --version 3.7.0
```

**FormulaSecurityValidator — implementação completa:**
```csharp
public class FormulaSecurityValidator
{
    private const int MaxExpressionLength = 4096;
    private const int MaxAstDepth = 20;
    
    private static readonly HashSet<string> AllowedFunctions = new(StringComparer.OrdinalIgnoreCase)
    {
        "round", "min", "max", "abs", "if", "pow", "sqrt", "log"
    };

    public FormulaValidationResult Validate(string expression, IEnumerable<FormulaVariable> declaredVariables)
    {
        var errors = new List<string>();
        
        // 1. Tamanho
        if (expression.Length > MaxExpressionLength)
            errors.Add($"Expressão excede {MaxExpressionLength} caracteres.");
        
        // 2. Parse com NCalc2 (detecta erros de sintaxe)
        Expression ncalcExpr;
        try { ncalcExpr = new Expression(expression, EvaluateOptions.IgnoreCase); }
        catch (Exception ex) { return FormulaValidationResult.Invalid($"Erro de sintaxe: {ex.Message}"); }
        
        // 3. Extrair variáveis usadas na expressão
        var usedVariables = ExtractVariableNames(expression);
        var declaredNames = declaredVariables.Select(v => v.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        
        // 4. Variáveis usadas mas não declaradas
        var undeclaredVars = usedVariables.Where(v => !declaredNames.Contains(v)).ToList();
        if (undeclaredVars.Any())
            errors.Add($"Variáveis não declaradas: [{string.Join("], [", undeclaredVars)}]");
        
        // 5. Variáveis declaradas obrigatórias mas não usadas (warning, não erro)
        var unusedRequired = declaredVariables
            .Where(v => v.Required && !usedVariables.Contains(v.Name, StringComparer.OrdinalIgnoreCase))
            .Select(v => v.Name).ToList();
        
        // 6. Funções não permitidas (verificar no texto — NCalc não expõe o AST facilmente)
        var functionMatches = Regex.Matches(expression, @"\b([A-Za-z_][A-Za-z0-9_]*)\s*\(");
        foreach (Match match in functionMatches)
        {
            var funcName = match.Groups[1].Value;
            if (!AllowedFunctions.Contains(funcName))
                errors.Add($"Função não permitida: '{funcName}()'");
        }
        
        return errors.Any()
            ? FormulaValidationResult.Invalid(errors)
            : FormulaValidationResult.Valid(unusedRequired);
    }
    
    // Extrai nomes de variáveis no formato [nome_variavel]
    private static HashSet<string> ExtractVariableNames(string expression)
    {
        var matches = Regex.Matches(expression, @"\[([A-Za-z_][A-Za-z0-9_]*)\]");
        return matches.Select(m => m.Groups[1].Value).ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
}
```

**CustomFormulaEngine — avaliação segura:**
```csharp
public class CustomFormulaEngine
{
    private readonly FormulaSecurityValidator _validator;

    public async Task<FormulaEvaluationResult> EvaluateAsync(
        ValuationFormulaVersion version,
        Dictionary<string, decimal> inputs,
        CancellationToken ct = default)
    {
        // 1. Validar inputs contra variáveis declaradas
        var variables = version.GetVariables(); // desserializa JSON
        ValidateInputs(inputs, variables);
        
        // 2. Converter expressão: [nome] → NCalc usa [nome] natively
        var expression = new Expression(version.Expression, EvaluateOptions.IgnoreCase);
        
        // 3. Injetar parâmetros como decimal (nunca double)
        foreach (var input in inputs)
            expression.Parameters[input.Key] = input.Value;
        
        // 4. Registrar funções personalizadas (apenas whitelist)
        expression.EvaluateFunction += (name, args) =>
        {
            // Implementar ROUND, MIN, MAX, ABS, IF, POW, SQRT, LOG
            // com retorno em decimal
        };
        
        // 5. Avaliar (em background thread para não bloquear)
        object rawResult;
        try
        {
            rawResult = await Task.Run(() => expression.Evaluate(), ct);
        }
        catch (Exception ex)
        {
            throw new FormulaEvaluationException($"Erro ao avaliar fórmula: {ex.Message}");
        }
        
        // 6. Converter resultado para decimal (nunca retornar double)
        var result = Convert.ToDecimal(rawResult);
        result = Math.Round(result, 2, MidpointRounding.AwayFromZero);
        
        return new FormulaEvaluationResult
        {
            Value = result,
            InputsUsed = inputs,
            ExpressionSnapshot = version.Expression,
            FormulaVersionId = version.Id
        };
    }

    private void ValidateInputs(Dictionary<string, decimal> inputs, List<FormulaVariable> variables)
    {
        foreach (var variable in variables.Where(v => v.Required))
        {
            if (!inputs.ContainsKey(variable.Name))
                throw new ValidationException($"Variável obrigatória não informada: {variable.Label}");
        }
        foreach (var (key, value) in inputs)
        {
            var variable = variables.FirstOrDefault(v =>
                v.Name.Equals(key, StringComparison.OrdinalIgnoreCase));
            if (variable == null) continue; // ignora variáveis extras silenciosamente
            
            if (variable.MinValue.HasValue && value < variable.MinValue)
                throw new ValidationException($"{variable.Label}: valor {value} abaixo do mínimo {variable.MinValue}");
            if (variable.MaxValue.HasValue && value > variable.MaxValue)
                throw new ValidationException($"{variable.Label}: valor {value} acima do máximo {variable.MaxValue}");
        }
    }
}
```

**Critérios de Aceite:**
- [ ] `FormulaSecurityValidator.Validate` rejeita funções fora da whitelist
- [ ] `FormulaSecurityValidator.Validate` detecta variáveis não declaradas
- [ ] `CustomFormulaEngine.EvaluateAsync` nunca usa `double` — apenas `decimal`
- [ ] Divisão por zero lança `FormulaEvaluationException` com mensagem amigável
- [ ] Expressões com mais de 4096 chars são rejeitadas na validação
- [ ] Teste unitário: fórmula com `exec(` → `FormulaSecurityValidator` rejeita
- [ ] Resultado sempre arredondado para 2 casas decimais com `AwayFromZero`

---

### F5-CFV-BE-004: CustomFormulaService + Controller

- **Status:** [ ] Pendente
- **Tipo:** Backend Service + Controller
- **Estimativa:** 10h
- **Responsável:** BE1
- **Dependências:** F5-CFV-BE-002, F5-CFV-BE-003

**Arquivos a criar:**
```
/src/backend/PartnershipManager.Application/Services/CustomFormulaService.cs
/src/backend/PartnershipManager.Application/Interfaces/ICustomFormulaService.cs
/src/backend/PartnershipManager.Application/DTOs/CustomFormula/CreateFormulaRequest.cs
/src/backend/PartnershipManager.Application/DTOs/CustomFormula/FormulaVersionRequest.cs
/src/backend/PartnershipManager.Application/DTOs/CustomFormula/FormulaResponse.cs
/src/backend/PartnershipManager.Application/DTOs/CustomFormula/FormulaVersionResponse.cs
/src/backend/PartnershipManager.Application/DTOs/CustomFormula/TestFormulaRequest.cs
/src/backend/PartnershipManager.Application/DTOs/CustomFormula/TestFormulaResponse.cs
/src/backend/PartnershipManager.Application/DTOs/CustomFormula/EvaluateFormulaRequest.cs
/src/backend/PartnershipManager.Application/Validators/CustomFormula/CreateFormulaValidator.cs
/src/backend/PartnershipManager.Application/Validators/CustomFormula/FormulaVersionValidator.cs
/src/backend/PartnershipManager.API/Controllers/CustomFormulasController.cs
```

**Endpoints obrigatórios:**
```
GET    /api/v1/companies/{companyId}/valuation-formulas                     → listar fórmulas
POST   /api/v1/companies/{companyId}/valuation-formulas                     → criar fórmula (sem expressão ainda)
GET    /api/v1/companies/{companyId}/valuation-formulas/{id}                → detalhe + versão atual
PUT    /api/v1/companies/{companyId}/valuation-formulas/{id}/activate       → ativar
PUT    /api/v1/companies/{companyId}/valuation-formulas/{id}/deactivate     → desativar
DELETE /api/v1/companies/{companyId}/valuation-formulas/{id}                → soft delete

POST   /api/v1/companies/{companyId}/valuation-formulas/{id}/versions       → criar nova versão (com expressão + variáveis)
GET    /api/v1/companies/{companyId}/valuation-formulas/{id}/versions       → histórico de versões
GET    /api/v1/companies/{companyId}/valuation-formulas/{id}/versions/{vid} → detalhe de versão

POST   /api/v1/companies/{companyId}/valuation-formulas/{id}/test           → testar fórmula com inputs de exemplo
POST   /api/v1/companies/{companyId}/valuations/{valId}/methods/custom      → usar fórmula em um valuation
```

**Lógica crítica — `CreateVersionAsync`:**
```csharp
public async Task<FormulaVersionResponse> CreateVersionAsync(
    Guid formulaId, Guid companyId, FormulaVersionRequest request)
{
    // 1. Validar que fórmula pertence à empresa
    // 2. Validar sintaxe via FormulaSecurityValidator
    //    → Se inválida: salvar versão com status=Invalid + errors, mas não jogar exceção
    // 3. Testar com test_inputs (se fornecidos) para obter test_result
    // 4. Calcular próximo version_number = MAX + 1
    // 5. Criar ValuationFormulaVersion (imutável)
    // 6. Atualizar current_version_id na fórmula pai (apenas se Validated)
    // 7. Audit log
}
```

**Lógica crítica — `EvaluateAndAddToValuationAsync`:**
```csharp
// Chamado quando usuário adiciona fórmula custom a um valuation
public async Task<ValuationMethodResponse> EvaluateAndAddToValuationAsync(
    Guid valuationId, Guid companyId, EvaluateFormulaRequest request)
{
    // request: { formulaId, inputs: { "hectares": 1200, "preco_saca": 192.00, ... } }
    
    // 1. Buscar current_version da fórmula
    // 2. Validar inputs
    // 3. Executar CustomFormulaEngine.EvaluateAsync
    // 4. Persistir ValuationMethod com method_type='custom', formula_version_id, calculated_value, inputs
    // 5. Persistir ValuationFormulaExecution (log de auditoria)
    // 6. Retornar ValuationMethodResponse com breakdown
}
```

**Critérios de Aceite:**
- [ ] `POST /versions`: versão com sintaxe inválida salva com `validation_status='invalid'` (não retorna 400)
- [ ] `POST /test`: nunca persiste nada no banco — apenas avalia e retorna resultado
- [ ] `POST /methods/custom`: persiste método + execução em transação única
- [ ] Controller com `[Authorize(Roles = "Admin,Founder,Finance")]` para leitura
- [ ] Controller com `[Authorize(Roles = "Admin,Founder")]` para criação/edição de fórmulas
- [ ] Swagger documentado para todos os endpoints
- [ ] DI: registrar `CustomFormulaService`, `CustomFormulaEngine`, `FormulaSecurityValidator`

---

## 8. Tarefas Atômicas — Frontend

### F5-CFV-FE-001: Types TypeScript + Service + Hook — Custom Formula

- **Status:** [ ] Pendente
- **Tipo:** Frontend Types + Service + Hook
- **Estimativa:** 4h
- **Responsável:** FE1
- **Dependências:** F5-CFV-BE-004
- **Posição:** Semana 3

**Arquivos a criar:**
```
/src/frontend/src/types/customFormula.ts
/src/frontend/src/services/customFormulaService.ts
/src/frontend/src/hooks/useCustomFormulas.ts
/src/frontend/src/hooks/useFormulaVersions.ts
```

**Types obrigatórios:**
```typescript
export type FormulaValidationStatus = 'draft' | 'validated' | 'invalid';
export type FormulaVariableType = 'currency' | 'percentage' | 'number' | 'integer' | 'multiplier' | 'boolean';

export interface FormulaVariable {
  name: string;
  label: string;
  type: FormulaVariableType;
  unit?: string;
  description?: string;
  required: boolean;
  defaultValue?: number;
  minValue?: number;
  maxValue?: number;
  order: number;
}

export interface FormulaVersion {
  id: string;
  formulaId: string;
  versionNumber: number;
  expression: string;
  variables: FormulaVariable[];
  resultUnit: string;
  resultLabel?: string;
  testInputs?: Record<string, number>;
  testResult?: number;
  validationStatus: FormulaValidationStatus;
  validationErrors?: string[];
  createdAt: string;
  createdBy: string;
}

export interface CustomFormula {
  id: string;
  companyId: string;
  name: string;
  description?: string;
  sectorTag?: string;
  currentVersion?: FormulaVersion;
  isActive: boolean;
  createdAt: string;
}

export interface TestFormulaRequest {
  expression: string;
  variables: FormulaVariable[];
  testInputs: Record<string, number>;
}

export interface TestFormulaResponse {
  result: number;
  resultFormatted: string;
  breakdown: Array<{ step: string; value: number }>;
  isValid: boolean;
  errors?: string[];
}

export interface EvaluateFormulaRequest {
  formulaId: string;
  inputs: Record<string, number>;
}
```

**Critérios de Aceite:**
- [ ] Sem `any` — usar `Record<string, number>` para inputs dinâmicos
- [ ] Hook `useCustomFormulas(companyId)` com paginação e filtro isActive
- [ ] Hook `useFormulaVersions(formulaId)` com histórico de versões
- [ ] Cache invalidado após criação de versão

---

### F5-CFV-FE-002: Formula Builder — Criação e Edição de Fórmulas

- **Status:** [ ] Pendente
- **Tipo:** Frontend Page + Components (mais complexo do frontend desta extensão)
- **Estimativa:** 12h ⚠️
- **Responsável:** FE1
- **Dependências:** F5-CFV-FE-001
- **Posição:** Semana 3/4

**Arquivos a criar:**
```
/src/frontend/src/pages/valuations/custom-formulas/index.tsx         (lista)
/src/frontend/src/pages/valuations/custom-formulas/new.tsx           (criar)
/src/frontend/src/pages/valuations/custom-formulas/[id]/index.tsx    (detalhe)
/src/frontend/src/pages/valuations/custom-formulas/[id]/edit.tsx     (editar → nova versão)
/src/frontend/src/components/formula/FormulaBuilder.tsx              (componente principal)
/src/frontend/src/components/formula/VariableEditor.tsx              (editor de variável)
/src/frontend/src/components/formula/ExpressionEditor.tsx            (editor de expressão com autocomplete)
/src/frontend/src/components/formula/FormulaTester.tsx               (painel de teste)
/src/frontend/src/components/formula/FormulaVersionHistory.tsx       (histórico de versões)
```

**ExpressionEditor — funcionalidade de autocomplete:**
```
Ao digitar '[' no campo de expressão:
→ Exibe dropdown com as variáveis declaradas
→ Selecionar variável insere [nome_variavel]
→ Highlight de variáveis declaradas (verde) vs não declaradas (vermelho)
→ Highlight de funções da whitelist (azul)
→ Erro inline para funções não permitidas

Implementar com CodeMirror 6 (leve, extensível):
npm install @codemirror/view @codemirror/state @codemirror/autocomplete
```

**FormulaTester — validação em tempo real:**
```typescript
// Ao clicar em "Calcular":
// 1. POST /test com expression + variables + testInputs
// 2. Exibir resultado formatado
// 3. Exibir breakdown passo-a-passo
// 4. Exibir erros de validação inline

// Ao digitar na expressão (debounce 800ms):
// POST /test (mode: validate-only) → apenas exibir ✅/❌ na expressão
```

**Critérios de Aceite:**
- [ ] `ExpressionEditor`: highlight de variáveis declaradas vs não declaradas
- [ ] `ExpressionEditor`: autocomplete ao digitar `[`
- [ ] `VariableEditor`: drag-and-drop para reordenar variáveis
- [ ] `FormulaTester`: cálculo em tempo real com debounce
- [ ] `FormulaTester`: breakdown passo-a-passo visível
- [ ] Editar fórmula existente → exibe modal "Isso criará a versão N+1" antes de salvar
- [ ] Versão com `validation_status = 'invalid'` exibe erros com destaque vermelho
- [ ] `FormulaVersionHistory`: linha do tempo de versões com diff da expressão

---

### F5-CFV-FE-003: Integração no Wizard de Valuation (Extensão do Step 2)

- **Status:** [ ] Pendente
- **Tipo:** Frontend Component (extensão do componente existente)
- **Estimativa:** 6h
- **Responsável:** FE1
- **Dependências:** F5-CFV-FE-001, F5-VLT-FE-004 (Wizard existente)
- **Posição:** Semana 4

**Arquivos a modificar/criar:**
```
/src/frontend/src/components/valuation/WizardStep2Methods.tsx
  → Adicionar opção "Fórmula Customizada" no dropdown de metodologias

/src/frontend/src/components/formula/CustomFormulaMethodCard.tsx   (novo)
  → Card que exibe os inputs dinâmicos e o resultado calculado
  
/src/frontend/src/components/formula/DynamicVariableInputs.tsx     (novo)
  → Renderiza inputs baseado nas variáveis da fórmula (sem hardcode)
```

**DynamicVariableInputs — renderização por tipo:**
```typescript
// Renderiza o input correto para cada tipo de variável
const renderInput = (variable: FormulaVariable, value: number, onChange: (v: number) => void) => {
  switch (variable.type) {
    case 'currency':
      return <CurrencyInput label={variable.label} value={value} onChange={onChange} />;
    case 'percentage':
      return <PercentageInput label={variable.label} value={value} min={0} max={100} onChange={onChange} />;
    case 'boolean':
      return <ToggleInput label={variable.label} value={value === 1} onChange={(v) => onChange(v ? 1 : 0)} />;
    default:
      return <NumberInput label={variable.label} value={value} onChange={onChange} />;
  }
};
```

**Critérios de Aceite:**
- [ ] Dropdown de fórmulas carrega apenas fórmulas `is_active = true` da empresa
- [ ] Ao selecionar fórmula, carrega `current_version` e renderiza variáveis dinamicamente
- [ ] Cálculo em tempo real (debounce 500ms) ao alterar qualquer variável
- [ ] Exibe breakdown do cálculo (nome da variável + valor utilizado)
- [ ] "Selecionar como principal" funciona igual às metodologias padrão
- [ ] Link "Criar nova fórmula →" abre em nova aba (não abandona o wizard)

---

## 9. Tarefas Atômicas — Testes

### F5-CFV-TST-001: Testes Unitários — Engine de Segurança e Avaliação

- **Status:** [ ] Pendente
- **Tipo:** Tests — Backend Unit (CRÍTICO — segurança)
- **Estimativa:** 8h
- **Responsável:** BE1
- **Dependências:** F5-CFV-BE-003
- **Posição:** Semana 4

**Arquivo a criar:**
```
/src/backend/PartnershipManager.Tests/Services/Formula/FormulaSecurityValidatorTests.cs
/src/backend/PartnershipManager.Tests/Services/Formula/CustomFormulaEngineTests.cs
```

**Casos de teste de segurança (FormulaSecurityValidator):**

| Teste | Input | Esperado |
|-------|-------|---------|
| Expressão válida simples | `[arr] * [multiple]` | Valid |
| Variável não declarada | `[arr] * [undefined_var]` | Invalid: "Variáveis não declaradas: [undefined_var]" |
| Função não permitida | `[arr] * exec("rm -rf /")` | Invalid: "Função não permitida: 'exec()'" |
| Função não permitida 2 | `Environment.Exit(0)` | Invalid |
| Função permitida | `ROUND([arr] * [m], 2)` | Valid |
| Expressão muito longa | string de 5000 chars | Invalid: "Excede 4096 caracteres" |
| Variável sem colchetes | `arr * multiple` (sem []) | Identifica como sem variáveis declaradas |
| Fórmula com IF válido | `IF([stage] > 0, [arr]*8, [arr]*5)` | Valid |
| Sintaxe inválida | `[arr] * (` | Invalid: erro de sintaxe |

**Casos de teste de avaliação (CustomFormulaEngine):**

| Teste | Expressão | Inputs | Expected |
|-------|-----------|--------|---------|
| Multiplicação simples | `[a] * [b]` | a=500, b=8 | 4.000,00 |
| Percentual | `[gmv] * ([rate] / 100)` | gmv=1M, rate=15 | 150.000,00 |
| MIN com limite | `[gmv] * MIN([rate]/100, 0.20)` | gmv=1M, rate=25 | 200.000,00 |
| IF condicional | `IF([stage]==1, [arr]*8, [arr]*5)` | stage=1, arr=1M | 8.000.000,00 |
| ROUND resultado | `ROUND([a]/[b], 2)` | a=10, b=3 | 3,33 |
| Divisão por zero | `[a] / [b]` | a=100, b=0 | FormulaEvaluationException |
| Input abaixo do min | variable.MinValue=0, input=-1 | ValidationException |
| Variável obrigatória ausente | required=true, não enviada | ValidationException |
| Resultado negativo | `[a] - [b]` | a=100, b=500 | -400,00 (permitido) |
| Precisão decimal | `[a] * [b]` | a=1.234.567,89, b=3 | 3.703.703,67 |

**Critérios de Aceite:**
- [ ] Cobertura ≥ 90% para `FormulaSecurityValidator`
- [ ] Cobertura ≥ 90% para `CustomFormulaEngine`
- [ ] Todos os testes de injeção passam (rejeição confirmada)
- [ ] Nenhum resultado usa `double` — verificado via reflection nos testes
- [ ] Teste de performance: 1.000 avaliações em < 500ms (via `Stopwatch`)

---

## 10. Regras de Negócio Novas

| Código | Regra | Implementação |
|--------|-------|---------------|
| **CF-01** | Uma fórmula só pode ser usada em um valuation se `validation_status = 'validated'` | Validação no `CustomFormulaService.EvaluateAndAddToValuationAsync` |
| **CF-02** | Uma versão de fórmula é imutável após criação | Sem `UpdateAsync` para `ValuationFormulaVersion` |
| **CF-03** | Editar fórmula sempre cria uma nova versão — nunca altera a atual | `CreateVersionAsync` — nunca `UpdateVersionAsync` |
| **CF-04** | Valuation histórico sempre referencia a versão usada no momento | `formula_version_id` no `valuation_methods` é imutável |
| **CF-05** | Apenas funções da whitelist são permitidas na expressão | `FormulaSecurityValidator` — rejeitado antes de qualquer execução |
| **CF-06** | Variáveis declaradas como obrigatórias devem ser informadas na avaliação | `CustomFormulaEngine.ValidateInputs` — lança ValidationException |
| **CF-07** | Fórmula inativa não pode ser adicionada a novos valuations | Validação: `is_active = true` antes de `EvaluateAndAddToValuationAsync` |
| **CF-08** | Teste de fórmula (`/test`) não persiste nada no banco | Garantido: `CustomFormulaService.TestAsync` não chama nenhum repositório |
| **CF-09** | Expressions com > 4096 caracteres são rejeitadas | `FormulaSecurityValidator` — primeiro check |
| **CF-10** | `formula_version_id` é obrigatório quando `method_type = 'custom'` | `AddMethodValidator` + check no `ValuationService` |

---

## 11. Resumo de Impacto no Plano Original

### 11.1 Novas Tarefas

| ID | Título | Semana | Estimativa |
|----|--------|--------|-----------|
| F5-CFV-DB-001 | Migrations Fórmulas Customizadas | S1 | 5h |
| F5-CFV-BE-001 | Entities + Enums Custom Formula | S2 | 5h |
| F5-CFV-BE-002 | Repository Custom Formula | S2 | 7h |
| F5-CFV-BE-003 | FormulaSecurityValidator + CustomFormulaEngine | S2 | 12h |
| F5-CFV-BE-004 | CustomFormulaService + Controller | S2 | 10h |
| F5-CFV-FE-001 | Types + Service + Hook Custom Formula | S3 | 4h |
| F5-CFV-FE-002 | Formula Builder (Página + Componentes) | S3–4 | 12h |
| F5-CFV-FE-003 | Integração no Wizard de Valuation | S4 | 6h |
| F5-CFV-TST-001 | Testes Unitários de Segurança e Engine | S4 | 8h |
| **TOTAL** | **9 tarefas atômicas** | | **~69h** |

### 11.2 Modificações em Tarefas Existentes

| Tarefa Existente | Modificação Necessária |
|-----------------|----------------------|
| F5-DB-001 (Migration Valuation Methods) | Enum `method_type` já inclui `'custom'` desde o início — evitar dupla migration. Ajustar SQL original para incluir o tipo. |
| F5-ENG-BE-001 (ValuationCalculationEngine) | Adicionar injeção de `ICustomFormulaEngine` e método `CalculateCustomAsync` |
| F5-VLT-BE-005 (ValuationsController) | Documentar no Swagger que `method_type=custom` requer `formula_version_id` |
| F5-VLT-FE-004 (Wizard Step 2) | Adicionar opção de fórmula customizada no dropdown de metodologias |
| F5-CFG-001 (DI Registration) | Registrar `CustomFormulaService`, `CustomFormulaEngine`, `FormulaSecurityValidator`, `CustomFormulaRepository` |
| F5-TST-BE-001 (Engine Tests) | Adicionar caso de teste: `CalculateCustomAsync` com mock do `CustomFormulaEngine` |

### 11.3 Novas Rotas a Adicionar no Frontend

```
/valuations/custom-formulas
/valuations/custom-formulas/new
/valuations/custom-formulas/:id
/valuations/custom-formulas/:id/edit
```

### 11.4 Adição ao Menu (Sidebar)

```diff
💰 Valuation
   ├── 📋 Lista de Valuations
   ├── ➕ Novo Valuation
   ├── 📊 Dashboard de Valuation
   ├── 📜 Histórico
+  └── 🔧 Fórmulas Customizadas     (roles: Admin, Founder)
```

### 11.5 Dependência de Biblioteca External

```bash
# Backend — instalar NCalc2
cd src/backend/PartnershipManager.Application
dotnet add package NCalc2 --version 3.7.0

# Frontend — instalar CodeMirror (Formula Builder)
cd src/frontend
npm install @codemirror/view @codemirror/state @codemirror/autocomplete @codemirror/lang-javascript
```

> **⚠️ Nota para o agente de IA:** Antes de iniciar F5-CFV-BE-003, verificar se `NCalc2` está disponível no ambiente com `dotnet list package`. Se não estiver, executar o `dotnet add package` acima. A tarefa F5-CFV-BE-003 não pode começar sem esta dependência instalada e validada.

---

## Apêndice — Exemplos de Fórmulas por Setor

Para onboarding e inspiração de novos clientes:

```
// Agronegócio
[hectares] * [preco_saca] * [sacas_por_hectare] * [multiplo_setor]

// Marketplace
[gmv] * ([take_rate] / 100) * MIN([retencao_compradores] / 100, 0.95) * 12

// Impacto Social (valor híbrido financeiro + social)
([receita] * [mrr_multiple]) + ([beneficiarios] * [valor_social_por_beneficiario])

// Energia Renovável
[capacidade_mw] * [fator_capacidade] * 8760 * [preco_mwh] * [multiplo_setor]

// Clínica / Saúde
[leitos] * [taxa_ocupacao_percent] / 100 * [ticket_medio_diario] * 365 * [multiplo_ebitda]

// EdTech
[alunos_ativos] * [ltv_por_aluno] * MIN([taxa_conclusao_percent] / 100, 1.0)

// PropTech / Imóveis
[vgv] * (1 - [custo_construcao_percent] / 100) * [velocidade_vendas_index]
```

---

*Partnership Manager — Fase 5 ADDENDUM: Fórmulas Customizadas*  
*Versão: 1.0 | Data: 26/02/2026*  
*Integra com: FASE5_VALUATION_FINANCEIRO_PLANO_EXECUCAO.md*
