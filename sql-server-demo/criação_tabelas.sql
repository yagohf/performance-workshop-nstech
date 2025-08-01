-- ===================================================================
-- SCRIPT DE CRIAÇÃO DAS TABELAS PARA A DEMO DE PERFORMANCE .NET
-- ===================================================================

-- Garante que estamos no banco de dados correto. 
-- Troque 'SeuBancoDeDados' pelo nome que você criou no Docker.
-- Ex: USE [performance-demo-db]
USE [master];
GO

-- Se o banco já existir, você pode querer limpá-lo para um novo começo.
-- CUIDADO: USE APENAS EM AMBIENTE DE DEMONSTRAÇÃO.
/*
IF DB_ID('performance-demo-db') IS NOT NULL
BEGIN
    ALTER DATABASE [performance-demo-db] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [performance-demo-db];
END
GO
*/

-- Cria o banco de dados se ele não existir
IF DB_ID('performance-demo-db') IS NULL
BEGIN
    CREATE DATABASE [performance-demo-db];
    PRINT 'Banco criado com sucesso!';
END
GO

-- Usa o banco de dados recém-criado
USE [performance-demo-db];
GO

-- Limpa as tabelas se elas já existirem, para que o script possa ser executado várias vezes.
IF OBJECT_ID('dbo.Transactions', 'U') IS NOT NULL DROP TABLE dbo.Transactions;
IF OBJECT_ID('dbo.Categories', 'U') IS NOT NULL DROP TABLE dbo.Categories;
GO

-- ===================================================================
-- Tabela 1: Categories
-- Tabela de lookup simples para as categorias das transações.
-- ===================================================================
CREATE TABLE dbo.Categories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL,
    Code CHAR(4) NOT NULL -- Um código qualquer para a categoria
);
GO

-- ===================================================================
-- Tabela 2: Transactions
-- Tabela principal, com milhões de registros.
--
-- *** PROBLEMAS INTENCIONAIS ***
-- 1. Ausência de um índice em 'AccountId'. Filtrar por conta será lento.
-- 2. Ausência de um índice em 'TransactionDate'. Filtrar por período será lento.
-- A combinação de filtros em AccountId e TransactionDate será especialmente custosa.
-- 3. Ausência de um índice em 'CategoryId', tornando o JOIN do problema N+1 mais custoso.
-- ===================================================================
CREATE TABLE dbo.Transactions (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,  -- PK Clustered (padrão)
    AccountId INT NOT NULL,                -- Chave da conta (SEM ÍNDICE)
    CategoryId INT NOT NULL,               -- Chave da categoria (SEM ÍNDICE)
    TransactionDate DATETIME2 NOT NULL,    -- Data da transação (SEM ÍNDICE)
    Amount DECIMAL(18, 2) NOT NULL,        -- Valor da transação
    Description NVARCHAR(200) NOT NULL,    -- Descrição gerada aleatoriamente
    Protocol VARBINARY(128) NOT NULL       -- Campo para simular alocação de memória (ex: conversão para Base64)
);
GO

-- Adiciona a Foreign Key para garantir a integridade, mas sem criar um índice automaticamente.
-- Em muitas configurações de SGBD, a criação de FK gera um índice, mas aqui garantimos que não.
ALTER TABLE dbo.Transactions 
ADD CONSTRAINT FK_Transactions_Categories 
FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(Id);
GO

PRINT 'Tabelas criadas com sucesso!';