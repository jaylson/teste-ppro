-- =====================================================
-- Migration 010: Fix UTF-8 mojibake in text fields
-- Date: 2026-01-24
-- Description: Corrigir textos salvos com encoding incorreto (Ã§, Ã£, etc.)
-- =====================================================

USE partnership_manager;

-- Companies
UPDATE companies
SET
  name = CONVERT(CAST(CONVERT(name USING latin1) AS BINARY) USING utf8mb4),
  trading_name = CASE
    WHEN trading_name IS NULL THEN NULL
    ELSE CONVERT(CAST(CONVERT(trading_name USING latin1) AS BINARY) USING utf8mb4)
  END
WHERE HEX(name) LIKE '%C383C2%' OR HEX(name) LIKE '%C382C2%'
   OR HEX(trading_name) LIKE '%C383C2%' OR HEX(trading_name) LIKE '%C382C2%';

-- Clients (core)
UPDATE clients
SET
  name = CONVERT(CAST(CONVERT(name USING latin1) AS BINARY) USING utf8mb4),
  trading_name = CASE
    WHEN trading_name IS NULL THEN NULL
    ELSE CONVERT(CAST(CONVERT(trading_name USING latin1) AS BINARY) USING utf8mb4)
  END
WHERE HEX(name) LIKE '%C383C2%' OR HEX(name) LIKE '%C382C2%'
   OR HEX(trading_name) LIKE '%C383C2%' OR HEX(trading_name) LIKE '%C382C2%';

-- Users
UPDATE users
SET
  name = CONVERT(CAST(CONVERT(name USING latin1) AS BINARY) USING utf8mb4)
WHERE HEX(name) LIKE '%C383C2%' OR HEX(name) LIKE '%C382C2%';

-- Shareholders
UPDATE shareholders
SET
  name = CONVERT(CAST(CONVERT(name USING latin1) AS BINARY) USING utf8mb4),
  notes = CASE
    WHEN notes IS NULL THEN NULL
    ELSE CONVERT(CAST(CONVERT(notes USING latin1) AS BINARY) USING utf8mb4)
  END
WHERE HEX(name) LIKE '%C383C2%' OR HEX(name) LIKE '%C382C2%'
   OR HEX(notes) LIKE '%C383C2%' OR HEX(notes) LIKE '%C382C2%';

-- Share Classes
UPDATE share_classes
SET
  name = CONVERT(CAST(CONVERT(name USING latin1) AS BINARY) USING utf8mb4),
  description = CASE
    WHEN description IS NULL THEN NULL
    ELSE CONVERT(CAST(CONVERT(description USING latin1) AS BINARY) USING utf8mb4)
  END,
  rights = CASE
    WHEN rights IS NULL THEN NULL
    ELSE CONVERT(CAST(CONVERT(rights USING latin1) AS BINARY) USING utf8mb4)
  END
WHERE HEX(name) LIKE '%C383C2%' OR HEX(name) LIKE '%C382C2%'
   OR HEX(description) LIKE '%C383C2%' OR HEX(description) LIKE '%C382C2%'
   OR HEX(rights) LIKE '%C383C2%' OR HEX(rights) LIKE '%C382C2%';

-- Migration successfully applied
