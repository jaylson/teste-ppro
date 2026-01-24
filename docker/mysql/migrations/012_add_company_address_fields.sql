-- Migration: Add address fields to companies table
-- Date: 2026-01-24

ALTER TABLE companies
    ADD COLUMN cep VARCHAR(9) NULL AFTER logo_url,
    ADD COLUMN street VARCHAR(255) NULL AFTER cep,
    ADD COLUMN number VARCHAR(20) NULL AFTER street,
    ADD COLUMN complement VARCHAR(100) NULL AFTER number,
    ADD COLUMN neighborhood VARCHAR(100) NULL AFTER complement,
    ADD COLUMN city VARCHAR(100) NULL AFTER neighborhood,
    ADD COLUMN state VARCHAR(2) NULL AFTER city;

-- Add index for city/state queries
CREATE INDEX idx_companies_city_state ON companies(city, state);
