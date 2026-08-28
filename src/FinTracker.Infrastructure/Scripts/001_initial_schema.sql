-- FinTracker PostgreSQL Initial Schema (EF Core 10)
-- Version: 1.0.0
-- Date: 2026-08-28

CREATE TABLE IF NOT EXISTS accounts (
    "Id" UUID PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "Institution" VARCHAR(100) NOT NULL,
    "Ownership" VARCHAR(20) NOT NULL,
    "InitialBalance" NUMERIC(12, 2) NOT NULL DEFAULT 0.00,
    "CurrentBalance" NUMERIC(12, 2) NOT NULL DEFAULT 0.00,
    "Currency" VARCHAR(3) NOT NULL DEFAULT 'BRL',
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_accounts_ownership ON accounts ("Ownership");

CREATE TABLE IF NOT EXISTS tags (
    "Id" UUID PRIMARY KEY,
    "Name" VARCHAR(50) NOT NULL UNIQUE,
    "Color" VARCHAR(7) NOT NULL DEFAULT '#10B981',
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS import_batches (
    "Id" UUID PRIMARY KEY,
    "AccountId" UUID NOT NULL REFERENCES accounts ("Id") ON DELETE CASCADE,
    "Filename" VARCHAR(255) NOT NULL,
    "FileType" VARCHAR(10) NOT NULL,
    "TotalImported" INTEGER NOT NULL DEFAULT 0,
    "ImportedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_import_batches_account ON import_batches ("AccountId", "ImportedAt");

CREATE TABLE IF NOT EXISTS budgets (
    "Id" UUID PRIMARY KEY,
    "TagId" UUID NOT NULL REFERENCES tags ("Id") ON DELETE CASCADE,
    "Month" VARCHAR(7) NOT NULL, -- YYYY-MM
    "Amount" NUMERIC(12, 2) NOT NULL DEFAULT 0.00,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uk_budgets_tag_month UNIQUE ("TagId", "Month")
);

CREATE TABLE IF NOT EXISTS transactions (
    "Id" UUID PRIMARY KEY,
    "AccountId" UUID NOT NULL REFERENCES accounts ("Id") ON DELETE RESTRICT,
    "ImportBatchId" UUID REFERENCES import_batches ("Id") ON DELETE SET NULL,
    "Date" DATE NOT NULL,
    "Amount" NUMERIC(12, 2) NOT NULL,
    "Type" VARCHAR(10) NOT NULL, -- income | expense | transfer
    "Status" VARCHAR(10) NOT NULL DEFAULT 'cleared', -- pending | cleared
    "IsRecurring" BOOLEAN NOT NULL DEFAULT FALSE,
    "RecurrencePeriod" VARCHAR(10), -- monthly | weekly | yearly
    "Description" VARCHAR(255) NOT NULL,
    "InvoiceNumber" VARCHAR(50),
    "Fingerprint" VARCHAR(64) NOT NULL,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_transactions_acc_date ON transactions ("AccountId", "Date");
CREATE INDEX IF NOT EXISTS idx_transactions_date ON transactions ("Date");
CREATE INDEX IF NOT EXISTS idx_transactions_fingerprint ON transactions ("Fingerprint");

CREATE TABLE IF NOT EXISTS transaction_tags (
    "TransactionId" UUID NOT NULL REFERENCES transactions ("Id") ON DELETE CASCADE,
    "TagId" UUID NOT NULL REFERENCES tags ("Id") ON DELETE CASCADE,
    PRIMARY KEY ("TransactionId", "TagId")
);
