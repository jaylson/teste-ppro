-- Migration 067: Add send_email flag to communications
-- Date: 2026-03-24
-- Description: Adds a boolean column to control whether a communication
--              should send email notifications to target users when published.

ALTER TABLE communications
    ADD COLUMN send_email TINYINT(1) NOT NULL DEFAULT 0
        COMMENT 'When 1, an e-mail is sent to all targeted users upon publish'
    AFTER target_roles;

-- DOWN (rollback)
-- ALTER TABLE communications DROP COLUMN send_email;
