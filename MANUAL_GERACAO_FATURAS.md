# Manual de Operação - Sistema de Geração de Faturas

## 📋 Índice
1. [Visão Geral](#visão-geral)
2. [Configuração Inicial](#configuração-inicial)
3. [Geração Automática de Faturas](#geração-automática-de-faturas)
4. [Geração Manual de Faturas](#geração-manual-de-faturas)
5. [Gestão de Faturas](#gestão-de-faturas)
6. [Download de PDF](#download-de-pdf)
7. [API Externa](#api-externa)
8. [Troubleshooting](#troubleshooting)

---

## 🎯 Visão Geral

O sistema de geração de faturas do Partnership Manager é responsável por:
- Gerar faturas mensais automaticamente para todas as assinaturas ativas
- Criar PDFs profissionais em formato de invoice
- Gerenciar status de pagamento das faturas
- Disponibilizar faturas via API para integrações externas

### Ciclo de Vida de uma Fatura

```
Criação → Pendente → Paga/Vencida/Cancelada
```

**Status possíveis:**
- `Pending` - Fatura criada, aguardando pagamento
- `Paid` - Fatura paga
- `Overdue` - Fatura vencida (passou da data de vencimento)
- `Cancelled` - Fatura cancelada

---

## ⚙️ Configuração Inicial

### 1. Verificar Configuração do Job

O job de geração mensal está configurado em `appsettings.json`:

```json
{
  "Jobs": {
    "MonthlyInvoiceGeneration": {
      "Enabled": true,
      "CronExpression": "0 0 3 1 * ?"
    }
  }
}
```

**Configurações:**
- `Enabled`: `true` para ativar, `false` para desativar
- `CronExpression`: `"0 0 3 1 * ?"` = Todo dia 1º às 3h da manhã (UTC)

### 2. Expressões Cron Alternativas

Exemplos de configurações alternativas:

```cron
0 0 3 1 * ?     # Dia 1º de cada mês às 3h (Padrão)
0 0 2 1 * ?     # Dia 1º de cada mês às 2h
0 0 0 1 * ?     # Dia 1º de cada mês à meia-noite
0 0 3 L * ?     # Último dia de cada mês às 3h
0 0 3 15 * ?    # Dia 15 de cada mês às 3h
```

### 3. Ambiente de Desenvolvimento

Para testar em desenvolvimento, você pode usar:

```cron
0 */5 * * * ?   # A cada 5 minutos (apenas para testes)
0 0 * * * ?     # A cada hora (para testes)
```

---

## 🤖 Geração Automática de Faturas

### Como Funciona

1. **Agendamento**: O job `MonthlyInvoiceGenerationJob` executa automaticamente
2. **Seleção**: Busca todas as assinaturas com status `Active`
3. **Verificação**: Verifica se já existe fatura para o período atual
4. **Criação**: Cria nova fatura para cada assinatura ativa
5. **Cálculo**: Calcula valores baseado no plano e possíveis descontos
6. **PDF**: Gera PDF automaticamente da fatura
7. **Notificação**: Registra em log a quantidade de faturas geradas

### Detalhes da Geração

#### Período de Faturamento
- **Período**: Sempre do dia 1º ao último dia do mês corrente
- **Data de Vencimento**: 10 dias após a emissão (configurável)

Exemplo:
```
Execução: 01/02/2026 03:00
Período: 01/02/2026 - 28/02/2026
Vencimento: 11/02/2026
```

#### Cálculo de Valores

```csharp
// Valor base
decimal baseAmount = subscription.Plan.Price;

// Aplicar descontos (se houver)
decimal discountAmount = subscription.DiscountPercentage > 0 
    ? baseAmount * (subscription.DiscountPercentage / 100) 
    : 0;

// Valor final
decimal totalAmount = baseAmount - discountAmount;
```

#### Número da Fatura

Formato: `INV-YYYYMM-XXXXX`

Exemplo: `INV-202602-00001`
- `202602` = Ano e mês (fevereiro de 2026)
- `00001` = Sequencial do mês

### Logs da Execução

Verifique os logs em:
```
/var/log/partnershipmanager/jobs.log
```

Exemplo de log:
```
[2026-02-01 03:00:01] [INFO] MonthlyInvoiceGenerationJob: Starting invoice generation...
[2026-02-01 03:00:05] [INFO] MonthlyInvoiceGenerationJob: Found 45 active subscriptions
[2026-02-01 03:00:15] [INFO] MonthlyInvoiceGenerationJob: Generated 45 new invoices
[2026-02-01 03:00:15] [INFO] MonthlyInvoiceGenerationJob: Invoice generation completed successfully
```

---

## 🖱️ Geração Manual de Faturas

### Via API (Postman/cURL)

#### Criar Fatura Individual

```bash
POST /api/billing/invoices
Content-Type: application/json

{
  "subscriptionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "periodStart": "2026-02-01",
  "periodEnd": "2026-02-28"
}
```

**Resposta:**
```json
{
  "id": "7b9c42d3-8f1e-4a5d-b2c7-1e8a6f3d9c5b",
  "invoiceNumber": "INV-202602-00001",
  "subscriptionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "clientName": "Empresa XYZ Ltda",
  "planName": "Plano Premium",
  "periodStart": "2026-02-01T00:00:00Z",
  "periodEnd": "2026-02-28T23:59:59Z",
  "dueDate": "2026-02-11T00:00:00Z",
  "amount": 199.90,
  "discount": 0.00,
  "totalAmount": 199.90,
  "status": "Pending",
  "pdfUrl": "/api/billing/invoices/7b9c42d3-8f1e-4a5d-b2c7-1e8a6f3d9c5b/pdf"
}
```

### Via Backend (C#)

```csharp
// Injetar IMediator
private readonly IMediator _mediator;

// Criar fatura
var command = new CreateInvoiceCommand
{
    SubscriptionId = subscriptionId,
    PeriodStart = new DateTime(2026, 2, 1),
    PeriodEnd = new DateTime(2026, 2, 28)
};

var invoiceId = await _mediator.Send(command);
```

---

## 📊 Gestão de Faturas

### Acessar Interface Web

1. Faça login no sistema
2. Navegue para: **Billing > Faturas**
3. URL: `https://seudominio.com/billing/invoices`

### Funcionalidades da Interface

#### 1. Listagem de Faturas

Exibe todas as faturas com:
- Número da fatura
- Cliente
- Plano
- Período
- Valor
- Status
- Ações (Ver detalhes, Download PDF)

#### 2. Filtros Disponíveis

**Por Cliente:**
```
Digite o nome do cliente no campo de busca
```

**Por Período:**
```
Selecione o mês/ano desejado
```

**Por Plano:**
```
Selecione o plano no dropdown
```

**Por Status:**
- Todas
- Pendente
- Paga
- Vencida
- Cancelada

#### 3. Atualizar Status

**Via Interface:**
1. Clique no ícone de ações (⋮)
2. Selecione "Marcar como Paga" ou "Cancelar"

**Via API:**
```bash
PUT /api/billing/invoices/{id}/status
Content-Type: application/json

{
  "status": "Paid",  # Paid, Overdue, Cancelled
  "paymentDate": "2026-02-05"  # Opcional, apenas para Paid
}
```

---

## 📄 Download de PDF

### Estrutura do PDF

O PDF gerado contém:

```
┌─────────────────────────────────────────────┐
│ INVOICE / FATURA                            │
│                                             │
│ [Logo da Empresa]                           │
│                                             │
│ Invoice Number: INV-202602-00001            │
│ Issue Date: 01/02/2026                      │
│ Due Date: 11/02/2026                        │
│ Status: PENDING                             │
│                                             │
│ ──────────────────────────────────────────  │
│                                             │
│ BILL TO:                                    │
│ Empresa XYZ Ltda                            │
│ CNPJ: 12.345.678/0001-90                   │
│ Email: contato@empresa.com.br              │
│                                             │
│ ──────────────────────────────────────────  │
│                                             │
│ ITEMS:                                      │
│                                             │
│ Plano Premium                               │
│ Subscription Period: 01/02 - 28/02/2026    │
│                                  R$ 199.90  │
│                                             │
│ Discount (10%)                  -R$ 19.99   │
│                                             │
│ ──────────────────────────────────────────  │
│                                             │
│ TOTAL:                          R$ 179.91   │
│                                             │
│ Payment Instructions:                       │
│ [Informações de pagamento]                  │
└─────────────────────────────────────────────┘
```

### Download via Interface

1. Acesse a lista de faturas
2. Clique no botão "Download PDF" (ícone 📄)
3. O PDF será baixado automaticamente

### Download via API

```bash
GET /api/billing/invoices/{id}/pdf
```

**Resposta:**
- Content-Type: `application/pdf`
- Content-Disposition: `attachment; filename="Invoice-INV-202602-00001.pdf"`

### Exemplos de Uso

**cURL:**
```bash
curl -X GET "https://api.seudominio.com/api/billing/invoices/7b9c42d3-8f1e-4a5d-b2c7-1e8a6f3d9c5b/pdf" \
  -H "Authorization: Bearer {token}" \
  --output invoice.pdf
```

**Python:**
```python
import requests

url = "https://api.seudominio.com/api/billing/invoices/7b9c42d3-8f1e-4a5d-b2c7-1e8a6f3d9c5b/pdf"
headers = {"Authorization": "Bearer {token}"}

response = requests.get(url, headers=headers)
with open("invoice.pdf", "wb") as f:
    f.write(response.content)
```

**JavaScript/Node.js:**
```javascript
const axios = require('axios');
const fs = require('fs');

const url = 'https://api.seudominio.com/api/billing/invoices/7b9c42d3-8f1e-4a5d-b2c7-1e8a6f3d9c5b/pdf';
const token = 'your-token';

axios.get(url, {
  headers: { 'Authorization': `Bearer ${token}` },
  responseType: 'arraybuffer'
}).then(response => {
  fs.writeFileSync('invoice.pdf', response.data);
});
```

### Regenerar PDF

Se precisar regenerar o PDF de uma fatura existente:

```bash
POST /api/billing/invoices/{id}/regenerate-pdf
```

---

## 🔌 API Externa

### Autenticação

Todas as requisições requerem autenticação via Bearer Token:

```bash
Authorization: Bearer {seu-token-aqui}
```

### Endpoints Disponíveis

#### 1. Listar Faturas

```bash
GET /api/billing/invoices
```

**Query Parameters:**
- `clientId` (opcional) - Filtrar por cliente
- `subscriptionId` (opcional) - Filtrar por assinatura
- `status` (opcional) - Filtrar por status (Pending, Paid, Overdue, Cancelled)
- `startDate` (opcional) - Data inicial do período
- `endDate` (opcional) - Data final do período

**Exemplo:**
```bash
GET /api/billing/invoices?status=Pending&startDate=2026-02-01
```

#### 2. Buscar Fatura por ID

```bash
GET /api/billing/invoices/{id}
```

#### 3. Buscar Faturas por Cliente

```bash
GET /api/billing/invoices/client/{clientId}
```

#### 4. Download PDF

```bash
GET /api/billing/invoices/{id}/pdf
```

#### 5. Criar Fatura

```bash
POST /api/billing/invoices
Content-Type: application/json

{
  "subscriptionId": "guid",
  "periodStart": "2026-02-01",
  "periodEnd": "2026-02-28"
}
```

#### 6. Atualizar Status

```bash
PUT /api/billing/invoices/{id}/status
Content-Type: application/json

{
  "status": "Paid",
  "paymentDate": "2026-02-05"
}
```

### Exemplo Completo de Integração

```python
import requests
from datetime import datetime

class InvoiceClient:
    def __init__(self, base_url, token):
        self.base_url = base_url
        self.headers = {
            "Authorization": f"Bearer {token}",
            "Content-Type": "application/json"
        }
    
    def get_pending_invoices(self):
        """Buscar todas as faturas pendentes"""
        url = f"{self.base_url}/api/billing/invoices?status=Pending"
        response = requests.get(url, headers=self.headers)
        return response.json()
    
    def download_invoice_pdf(self, invoice_id, filename):
        """Baixar PDF de uma fatura"""
        url = f"{self.base_url}/api/billing/invoices/{invoice_id}/pdf"
        response = requests.get(url, headers=self.headers)
        
        with open(filename, 'wb') as f:
            f.write(response.content)
        
        return filename
    
    def mark_as_paid(self, invoice_id, payment_date=None):
        """Marcar fatura como paga"""
        url = f"{self.base_url}/api/billing/invoices/{invoice_id}/status"
        
        payload = {
            "status": "Paid",
            "paymentDate": payment_date or datetime.now().isoformat()
        }
        
        response = requests.put(url, json=payload, headers=self.headers)
        return response.json()

# Uso
client = InvoiceClient("https://api.seudominio.com", "seu-token")

# Listar pendentes
pending = client.get_pending_invoices()
print(f"Faturas pendentes: {len(pending)}")

# Baixar PDF
for invoice in pending:
    filename = f"invoice_{invoice['invoiceNumber']}.pdf"
    client.download_invoice_pdf(invoice['id'], filename)
    print(f"PDF baixado: {filename}")

# Marcar como paga
client.mark_as_paid(pending[0]['id'])
```

---

## 🔧 Troubleshooting

### Problema: Job não está executando

**Sintomas:**
- Faturas não são geradas automaticamente
- Não aparecem logs de execução

**Soluções:**

1. **Verificar se o job está habilitado:**
```json
"Jobs": {
  "MonthlyInvoiceGeneration": {
    "Enabled": true  // Deve estar true
  }
}
```

2. **Verificar logs de erro:**
```bash
docker logs partnershipmanager-api | grep MonthlyInvoiceGenerationJob
```

3. **Verificar Quartz.NET:**
```bash
# Reiniciar o container
docker-compose restart api

# Verificar logs de inicialização
docker logs partnershipmanager-api --tail 100
```

### Problema: Faturas duplicadas

**Sintomas:**
- Mesma assinatura tem múltiplas faturas para o mesmo período

**Soluções:**

1. **Verificar lógica de verificação:**
O sistema já tem proteção contra duplicatas, mas se ocorrer:

```sql
-- Verificar duplicatas
SELECT subscription_id, period_start, period_end, COUNT(*)
FROM invoices
GROUP BY subscription_id, period_start, period_end
HAVING COUNT(*) > 1;
```

2. **Cancelar faturas duplicadas:**
```bash
PUT /api/billing/invoices/{id}/status
{
  "status": "Cancelled"
}
```

### Problema: PDF não é gerado

**Sintomas:**
- Campo `PdfUrl` está vazio
- Erro ao tentar baixar PDF

**Soluções:**

1. **Verificar pacote QuestPDF:**
```bash
# No container
dotnet list package | grep QuestPDF
```

2. **Regenerar PDF manualmente:**
```bash
POST /api/billing/invoices/{id}/regenerate-pdf
```

3. **Verificar logs:**
```bash
docker logs partnershipmanager-api | grep "PDF generation"
```

### Problema: Status não atualiza

**Sintomas:**
- Faturas vencidas não mudam para "Overdue"

**Solução:**

O sistema não atualiza status automaticamente. Você pode criar um job adicional:

```csharp
// Exemplo de job para atualizar status de vencidas
public class UpdateOverdueInvoicesJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        // Buscar faturas pendentes com data de vencimento passada
        // Atualizar status para Overdue
    }
}
```

### Problema: Cálculo incorreto de valores

**Sintomas:**
- Valores diferentes do esperado

**Verificar:**

1. **Preço do plano está correto:**
```bash
GET /api/billing/plans/{id}
```

2. **Desconto da assinatura:**
```bash
GET /api/billing/subscriptions/{id}
```

3. **Logs de cálculo:**
```bash
docker logs partnershipmanager-api | grep "Invoice calculation"
```

---

## 📞 Suporte

Para problemas não cobertos neste manual:

1. **Verificar logs completos:**
```bash
docker-compose logs -f api
```

2. **Verificar banco de dados:**
```bash
docker exec -it partnershipmanager-mysql mysql -u root -p
USE partnershipmanager;
SELECT * FROM invoices ORDER BY created_at DESC LIMIT 10;
```

3. **Abrir issue no repositório:**
```
https://github.com/jaylson/teste-ppro/issues
```

---

## 📚 Referências

- [Quartz.NET Documentation](https://www.quartz-scheduler.net/)
- [QuestPDF Documentation](https://www.questpdf.com/)
- [Cron Expression Generator](https://crontab.guru/)

---

**Versão do Manual:** 1.0  
**Última Atualização:** 22/01/2026  
**Sistema:** Partnership Manager - Billing Module
