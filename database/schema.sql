-- ========================================================
-- BotSaaS - Database Schema Definition (MySQL)
-- Extraído e sincronizado com o banco local botsaasapi
-- ========================================================

CREATE DATABASE IF NOT EXISTS botsaasapi
    DEFAULT CHARACTER SET utf8mb4
    DEFAULT COLLATE utf8mb4_general_ci;

USE botsaasapi;

-- 1. Empresas (Tenants)
CREATE TABLE IF NOT EXISTS companies (
    id CHAR(36) NOT NULL COMMENT 'Primary Key',
    name VARCHAR(150) NOT NULL,
    segment INT(11) NOT NULL,
    created_at DATETIME NOT NULL COMMENT 'Created Time',
    PRIMARY KEY (id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 2. Usuários (Donos de negócio que fazem login no painel)
CREATE TABLE IF NOT EXISTS users (
    id CHAR(36) NOT NULL COMMENT 'Primary Key',
    company_id CHAR(36) NOT NULL,
    name VARCHAR(150) NOT NULL,
    email VARCHAR(255) NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    created_at DATETIME NOT NULL COMMENT 'Created Time',
    PRIMARY KEY (id),
    UNIQUE KEY email (email),
    KEY fk_users_company (company_id),
    CONSTRAINT fk_users_company FOREIGN KEY (company_id) REFERENCES companies (id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 3. Horários de Funcionamento (Grade semanal)
CREATE TABLE IF NOT EXISTS business_hours (
    company_id CHAR(36) NOT NULL,
    day_of_week INT(11) NOT NULL,
    opens_at TIME NOT NULL,
    closes_at TIME NOT NULL,
    is_closed TINYINT(1) NOT NULL DEFAULT 0,
    PRIMARY KEY (company_id, day_of_week),
    CONSTRAINT fk_business_hours_company FOREIGN KEY (company_id) REFERENCES companies (id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 4. Profissionais (Equipe da empresa)
CREATE TABLE IF NOT EXISTS professionals (
    id CHAR(36) NOT NULL,
    company_id CHAR(36) NOT NULL,
    name VARCHAR(150) NOT NULL,
    status INT(11) NOT NULL DEFAULT 1,
    created_at DATETIME NOT NULL,
    PRIMARY KEY (id),
    KEY idx_professionals_company (company_id, status),
    CONSTRAINT fk_professionals_company FOREIGN KEY (company_id) REFERENCES companies (id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 5. Clientes (Identidade permanente dos clientes finais)
CREATE TABLE IF NOT EXISTS customers (
    id CHAR(36) NOT NULL,
    company_id CHAR(36) NOT NULL,
    name VARCHAR(150) NOT NULL,
    phone VARCHAR(50) NOT NULL,
    created_at DATETIME NOT NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uk_company_phone (company_id, phone),
    CONSTRAINT fk_customers_company FOREIGN KEY (company_id) REFERENCES companies (id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 6. Sessões de Conversa
CREATE TABLE IF NOT EXISTS conversations (
    id CHAR(36) NOT NULL COMMENT 'Primary Key',
    company_id CHAR(36) NOT NULL,
    channel INT(11) NOT NULL DEFAULT 0 COMMENT '0 = WhatsApp, 1 = Telegram, 2 = Instagram',
    channel_contact_id VARCHAR(100) NOT NULL DEFAULT '' COMMENT 'Phone, Telegram ChatId, or Instagram IGSID',
    customer_phone VARCHAR(50) DEFAULT NULL COMMENT 'Real customer phone when provided',
    is_active TINYINT(1) NOT NULL DEFAULT 1,
    created_at DATETIME NOT NULL COMMENT 'Created Time',
    closed_at DATETIME DEFAULT NULL,
    PRIMARY KEY (id),
    KEY idx_company_channel_contact_active (company_id, channel, channel_contact_id, is_active),
    CONSTRAINT fk_conversations_company_id FOREIGN KEY (company_id) REFERENCES companies (id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 7. Mensagens (Histórico de cada conversa)
CREATE TABLE IF NOT EXISTS messages (
    id CHAR(36) NOT NULL COMMENT 'Primary Key',
    conversation_id CHAR(36) NOT NULL,
    role INT(11) NOT NULL,
    content TEXT NOT NULL,
    created_at DATETIME(6) NOT NULL,
    PRIMARY KEY (id),
    KEY fk_messages_conversations_id (conversation_id),
    CONSTRAINT fk_messages_conversations_id FOREIGN KEY (conversation_id) REFERENCES conversations (id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 8. Agendamentos (Registros transacionais)
CREATE TABLE IF NOT EXISTS appointments (
    id CHAR(36) NOT NULL COMMENT 'Primary Key',
    company_id CHAR(36) NOT NULL,
    conversation_id CHAR(36) DEFAULT NULL,
    professional_id CHAR(36) DEFAULT NULL,
    service_name VARCHAR(150) NOT NULL,
    customer_name VARCHAR(150) NOT NULL,
    customer_phone VARCHAR(50) NOT NULL DEFAULT '',
    scheduled_at DATETIME NOT NULL,
    status INT(11) NOT NULL,
    origin INT(11) NOT NULL DEFAULT 0,
    created_at DATETIME NOT NULL,
    reminded_at DATETIME DEFAULT NULL,
    PRIMARY KEY (id),
    KEY fk_appointment_conversation (conversation_id),
    KEY idx_appointments_company_scheduled (company_id, scheduled_at),
    KEY idx_appointments_reminder (reminded_at, scheduled_at, status),
    KEY idx_appointments_professional_sched (company_id, professional_id, scheduled_at, status),
    CONSTRAINT fk_appointment_company FOREIGN KEY (company_id) REFERENCES companies (id),
    CONSTRAINT fk_appointment_conversation FOREIGN KEY (conversation_id) REFERENCES conversations (id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
