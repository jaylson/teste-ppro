-- Migration 054: Create email_logs table
-- Date: 2026-03-02
-- Description: Audit trail for all outbound emails sent by the platform,
--              including delivery status and optional Resend message ID tracking.

CREATE TABLE email_logs (
  id                CHAR(36)     NOT NULL DEFAULT (UUID()),
  company_id        CHAR(36)     NULL,
  recipient_email   VARCHAR(255) NOT NULL,
  recipient_name    VARCHAR(200) NULL,
  subject           VARCHAR(300) NOT NULL,
  template_name     VARCHAR(100) NOT NULL,
  reference_type    VARCHAR(50)  NULL,
  reference_id      CHAR(36)     NULL,
  resend_message_id VARCHAR(100) NULL,
  status            ENUM('queued','sent','failed','bounced') NOT NULL DEFAULT 'queued',
  error_message     TEXT         NULL,
  sent_at           TIMESTAMP    NULL,
  created_at        TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  INDEX idx_emaillog_company  (company_id),
  INDEX idx_emaillog_status   (status),
  INDEX idx_emaillog_ref      (reference_type, reference_id),
  INDEX idx_emaillog_date     (created_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;


-- DOWN (rollback)
-- DROP TABLE IF EXISTS email_logs;
