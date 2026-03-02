-- Migration 047: Alter valuation_methods — add formula_version_id FK + extend method_type ENUM
-- Date: 2026-03-02
-- Description: Now that valuation_formula_versions table exists (migration 046), we can:
--   1. Add FK constraint fk_valmet_formula_version on valuation_methods.formula_version_id
--   The column formula_version_id was already created in migration 043 as a plain nullable column.
--   The method_type CHECK constraint already includes 'custom' from migration 043.

-- ============================================
-- Add the FK that was deferred until formula tables existed
-- ============================================
ALTER TABLE valuation_methods
    ADD CONSTRAINT fk_valmet_formula_version
        FOREIGN KEY (formula_version_id)
        REFERENCES valuation_formula_versions(id)
        ON DELETE RESTRICT;

-- ============================================
-- Add index for formula_version lookups
-- ============================================
ALTER TABLE valuation_methods
    ADD INDEX idx_vm_formula_version (formula_version_id);


-- ============================================
-- DOWN (rollback)
-- ============================================
-- ALTER TABLE valuation_methods DROP INDEX idx_vm_formula_version;
-- ALTER TABLE valuation_methods DROP FOREIGN KEY fk_valmet_formula_version;
