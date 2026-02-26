-- Migration 027: Campos de configuração societária no sócio
-- Adiciona flags de cláusulas/direitos societários na tabela shareholders

ALTER TABLE shareholders
    ADD COLUMN earn_out                   TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Earn-out: pagamento vinculado a resultados futuros',
    ADD COLUMN tag_along                  TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Tag Along: direito de vender junto se o controlador vender',
    ADD COLUMN drag_along                 TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Drag Along: obrigação de vender junto (proteção do majoritário)',
    ADD COLUMN shareholders_agreement     TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Acordo de sócios que define regras de governança',
    ADD COLUMN right_of_first_refusal     TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'ROFR: prioridade de compra antes de terceiros',
    ADD COLUMN liquidation_preference_right TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Prioridade de recebimento em caso de venda/liquidação',
    ADD COLUMN anti_dilution              TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Proteção contra diluição em rodadas futuras a preço menor';
