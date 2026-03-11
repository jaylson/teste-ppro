-- Migration 052: Create data room tables
-- Date: 2026-03-02
-- Description: Virtual data room per company with hierarchical folders and
--              document links. References the existing `documents` table
--              (created in migration 045) — does NOT recreate it.

CREATE TABLE data_rooms (
  id          CHAR(36)     NOT NULL DEFAULT (UUID()),
  company_id  CHAR(36)     NOT NULL,
  name        VARCHAR(200) NOT NULL,
  description TEXT         NULL,
  is_active   TINYINT(1)   NOT NULL DEFAULT 1,
  created_at  TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at  TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uk_dataroom_company (company_id),
  CONSTRAINT fk_dr_company FOREIGN KEY (company_id) REFERENCES companies(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE data_room_folders (
  id              CHAR(36)     NOT NULL DEFAULT (UUID()),
  data_room_id    CHAR(36)     NOT NULL,
  parent_id       CHAR(36)     NULL,
  name            VARCHAR(200) NOT NULL,
  description     TEXT         NULL,
  display_order   INT          NOT NULL DEFAULT 0,
  visibility      ENUM('internal','investors','public') NOT NULL DEFAULT 'internal',
  created_by      CHAR(36)     NOT NULL,
  created_at      TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
  deleted_at      TIMESTAMP    NULL,
  PRIMARY KEY (id),
  CONSTRAINT fk_drfolder_room   FOREIGN KEY (data_room_id) REFERENCES data_rooms(id) ON DELETE CASCADE,
  CONSTRAINT fk_drfolder_parent FOREIGN KEY (parent_id)    REFERENCES data_room_folders(id),
  INDEX idx_drfolder_room   (data_room_id),
  INDEX idx_drfolder_parent (parent_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Links folders to existing documents (documents table from migration 045)
CREATE TABLE data_room_documents (
  id            CHAR(36)   NOT NULL DEFAULT (UUID()),
  folder_id     CHAR(36)   NOT NULL,
  document_id   CHAR(36)   NOT NULL,
  display_order INT        NOT NULL DEFAULT 0,
  added_by      CHAR(36)   NOT NULL,
  added_at      TIMESTAMP  NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uk_drdoc_folder_doc (folder_id, document_id),
  CONSTRAINT fk_drdoc_folder FOREIGN KEY (folder_id)   REFERENCES data_room_folders(id) ON DELETE CASCADE,
  CONSTRAINT fk_drdoc_doc    FOREIGN KEY (document_id) REFERENCES documents(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;


-- DOWN (rollback)
-- DROP TABLE IF EXISTS data_room_documents;
-- DROP TABLE IF EXISTS data_room_folders;
-- DROP TABLE IF EXISTS data_rooms;
