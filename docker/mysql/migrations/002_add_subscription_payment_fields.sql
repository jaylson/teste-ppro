-- Adicionar campos DueDay e PaymentMethod na tabela BillingSubscriptions
ALTER TABLE BillingSubscriptions 
ADD COLUMN DueDay INT NOT NULL DEFAULT 10 COMMENT 'Dia do vencimento da fatura (1-31)',
ADD COLUMN PaymentMethod INT NOT NULL DEFAULT 3 COMMENT 'Método de pagamento preferencial (1=BankTransfer, 2=CreditCard, 3=Pix, 4=Boleto, 5=Cash, 99=Other)';

-- Adicionar constraint para validar o dia de vencimento
ALTER TABLE BillingSubscriptions
ADD CONSTRAINT CHK_Subscription_DueDay CHECK (DueDay >= 1 AND DueDay <= 31);

-- Adicionar índice para melhorar performance em consultas por método de pagamento
CREATE INDEX IX_BillingSubscriptions_PaymentMethod ON BillingSubscriptions(PaymentMethod);
