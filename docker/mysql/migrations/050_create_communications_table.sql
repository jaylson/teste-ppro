-- Migration 050: Create communications table
-- Date: 2026-03-02
-- Description: Stores announcements, updates, reports, alerts and invitations
--              sent to stakeholders (investors, founders, employees, etc.) per company.

CREATE TABLE communications (
  id            CHAR(36)      NOT NULL DEFAULT (UUID()),
  company_id    CHAR(36)      NOT NULL,
  title         VARCHAR(200)  NOT NULL,
  content       LONGTEXT      NOT NULL,
  content_html  LONGTEXT      NULL,
  summary       VARCHAR(500)  NULL,
  comm_type     ENUM('announcement','update','report','alert','invitation') NOT NULL,
  visibility    ENUM('all','investors','founders','employees','specific') NOT NULL DEFAULT 'all',
  target_roles  JSON          NULL,
  attachments   JSON          NULL,
  is_pinned     TINYINT(1)    NOT NULL DEFAULT 0,
  published_at  TIMESTAMP     NULL,
  expires_at    TIMESTAMP     NULL,
  created_by    CHAR(36)      NOT NULL,
  created_at    TIMESTAMP     NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at    TIMESTAMP     NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  deleted_at    TIMESTAMP     NULL,
  PRIMARY KEY (id),
  CONSTRAINT fk_comm_company FOREIGN KEY (company_id) REFERENCES companies(id),
  CONSTRAINT fk_comm_user    FOREIGN KEY (created_by)  REFERENCES users(id),
  INDEX idx_comm_company   (company_id),
  INDEX idx_comm_type      (comm_type),
  INDEX idx_comm_published (published_at),
  INDEX idx_comm_pinned    (company_id, is_pinned),
  INDEX idx_comm_deleted   (deleted_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;


-- DOWN (rollback)
-- DROP TABLE IF EXISTS communications;
