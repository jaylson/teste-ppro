-- Migration 051: Create communication_views table
-- Date: 2026-03-02
-- Description: Tracks which users have read each communication and for how long,
--              enabling read-receipt and engagement analytics.

CREATE TABLE communication_views (
  id                 CHAR(36)  NOT NULL DEFAULT (UUID()),
  communication_id   CHAR(36)  NOT NULL,
  user_id            CHAR(36)  NOT NULL,
  viewed_at          TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  view_duration_secs INT       NULL,
  PRIMARY KEY (id),
  UNIQUE KEY uk_comm_view_user (communication_id, user_id),
  CONSTRAINT fk_commview_comm FOREIGN KEY (communication_id) REFERENCES communications(id) ON DELETE CASCADE,
  CONSTRAINT fk_commview_user FOREIGN KEY (user_id)          REFERENCES users(id),
  INDEX idx_commview_comm (communication_id),
  INDEX idx_commview_user (user_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;


-- DOWN (rollback)
-- DROP TABLE IF EXISTS communication_views;
