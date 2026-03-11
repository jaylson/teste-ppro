-- Migration 055: Create workflow tables
-- Date: 2026-03-02
-- Description: Multi-step approval workflow engine. Supports contract approvals,
--              shareholder changes, communication approvals, document verification
--              and vesting approvals with per-step assignment and decision recording.

CREATE TABLE workflows (
  id                    CHAR(36)     NOT NULL DEFAULT (UUID()),
  company_id            CHAR(36)     NOT NULL,
  workflow_type         ENUM('contract_approval','shareholder_change',
                             'communication_approval','document_verification',
                             'vesting_approval') NOT NULL,
  reference_type        VARCHAR(50)  NOT NULL,
  reference_id          CHAR(36)     NOT NULL,
  title                 VARCHAR(200) NOT NULL,
  description           TEXT         NULL,
  status                ENUM('pending','in_progress','approved','rejected','cancelled')
                                     NOT NULL DEFAULT 'pending',
  priority              ENUM('low','medium','high','urgent') NOT NULL DEFAULT 'medium',
  current_step          INT          NOT NULL DEFAULT 1,
  total_steps           INT          NOT NULL DEFAULT 1,
  requested_by          CHAR(36)     NOT NULL,
  requested_at          TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
  due_date              TIMESTAMP    NULL,
  completed_at          TIMESTAMP    NULL,
  cancelled_at          TIMESTAMP    NULL,
  cancelled_by          CHAR(36)     NULL,
  cancellation_reason   TEXT         NULL,
  metadata              JSON         NULL,
  created_at            TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  CONSTRAINT fk_wf_company   FOREIGN KEY (company_id)   REFERENCES companies(id),
  CONSTRAINT fk_wf_requester FOREIGN KEY (requested_by) REFERENCES users(id),
  INDEX idx_wf_company    (company_id),
  INDEX idx_wf_status     (status),
  INDEX idx_wf_type       (workflow_type),
  INDEX idx_wf_ref        (reference_type, reference_id),
  INDEX idx_wf_due        (due_date)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE workflow_steps (
  id               CHAR(36)     NOT NULL DEFAULT (UUID()),
  workflow_id      CHAR(36)     NOT NULL,
  step_order       INT          NOT NULL,
  name             VARCHAR(100) NOT NULL,
  description      TEXT         NULL,
  step_type        ENUM('approval','review','notification','automated') NOT NULL,
  assigned_role    ENUM('admin','founder','employee','investor') NULL,
  assigned_user_id CHAR(36)     NULL,
  status           ENUM('pending','in_progress','completed','skipped')
                               NOT NULL DEFAULT 'pending',
  is_current       TINYINT(1)   NOT NULL DEFAULT 0,
  started_at       TIMESTAMP    NULL,
  due_date         TIMESTAMP    NULL,
  completed_at     TIMESTAMP    NULL,
  completed_by     CHAR(36)     NULL,
  notes            TEXT         NULL,
  PRIMARY KEY (id),
  CONSTRAINT fk_wfstep_workflow FOREIGN KEY (workflow_id)      REFERENCES workflows(id) ON DELETE CASCADE,
  CONSTRAINT fk_wfstep_user     FOREIGN KEY (assigned_user_id) REFERENCES users(id),
  INDEX idx_wfstep_workflow (workflow_id),
  INDEX idx_wfstep_current  (workflow_id, is_current)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE workflow_approvals (
  id               CHAR(36)    NOT NULL DEFAULT (UUID()),
  workflow_step_id CHAR(36)    NOT NULL,
  user_id          CHAR(36)    NOT NULL,
  decision         ENUM('approved','rejected','requested_changes') NOT NULL,
  comments         TEXT        NULL,
  decided_at       TIMESTAMP   NOT NULL DEFAULT CURRENT_TIMESTAMP,
  ip_address       VARCHAR(45) NULL,
  PRIMARY KEY (id),
  CONSTRAINT fk_wfapprv_step FOREIGN KEY (workflow_step_id) REFERENCES workflow_steps(id) ON DELETE CASCADE,
  CONSTRAINT fk_wfapprv_user FOREIGN KEY (user_id)          REFERENCES users(id),
  INDEX idx_wfapprv_step (workflow_step_id),
  INDEX idx_wfapprv_user (user_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;


-- DOWN (rollback)
-- DROP TABLE IF EXISTS workflow_approvals;
-- DROP TABLE IF EXISTS workflow_steps;
-- DROP TABLE IF EXISTS workflows;
