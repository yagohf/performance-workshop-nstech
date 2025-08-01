-- ===================================================================
-- SCRIPT DE CARGA DE DADOS PARA A DEMO DE PERFORMANCE .NET
-- ===================================================================

USE [performance-demo-db];
GO

-- Limpa os dados antigos para começar do zero
TRUNCATE TABLE dbo.Transactions;
DELETE FROM dbo.Categories;
-- CORREÇÃO: Reseta o IDENTITY para começar em 1 na próxima inserção
DBCC CHECKIDENT ('dbo.Categories', RESEED, 1);
GO

PRINT 'Tabelas limpas. Iniciando a inserção de dados...';

-- Insere as Categorias (sem alteração aqui)
INSERT INTO dbo.Categories (Name, Code) VALUES
('Alimentação', 'ALIM'), ('Transporte', 'TRSP'), ('Moradia', 'MRDA'),
('Lazer', 'LAZR'), ('Saúde', 'SAUD'), ('Educação', 'EDUC'),
('Compras', 'COMP'), ('Serviços', 'SERV'), ('Investimentos', 'INVS'),
('Outros', 'OUTR');
GO

PRINT 'Categorias inseridas.';

-- Insere as Transações em Lotes (lógica de geração de ID já está correta para 1-10)
SET NOCOUNT ON;
GO

DECLARE @i INT = 0;
DECLARE @totalBatches INT = 400;

PRINT 'Iniciando inserção de transações. Isso pode levar alguns minutos...';

WHILE @i < @totalBatches
BEGIN
    INSERT INTO dbo.Transactions (
        AccountId, CategoryId, TransactionDate, Amount, Description, Protocol
    )
    SELECT
        ABS(CHECKSUM(NEWID())) % 100 + 1,
        ABS(CHECKSUM(NEWID())) % 10 + 1, -- Gera IDs de 1 a 10
        DATEADD(SECOND, -(ABS(CHECKSUM(NEWID())) % 157680000), GETDATE()),
        CAST(RAND(CHECKSUM(NEWID())) * 999.0 + 1.0 AS DECIMAL(18, 2)),
        'Pagamento efetuado via app para ' + CAST(NEWID() AS VARCHAR(36)),
        CAST(NEWID() AS BINARY(16))
    FROM 
        (VALUES (0),(1),(2),(3),(4),(5),(6),(7),(8),(9)) AS a(N)
    CROSS JOIN (VALUES (0),(1),(2),(3),(4),(5),(6),(7),(8),(9)) AS b(N)
    CROSS JOIN (VALUES (0),(1),(2),(3),(4),(5),(6),(7),(8),(9)) AS c(N)
    CROSS JOIN (VALUES (0),(1),(2),(3),(4),(5),(6),(7),(8),(9)) AS d(N);

    SET @i = @i + 1;
    
    IF (@i % 10 = 0)
    BEGIN
        DECLARE @percent INT = (@i * 100) / @totalBatches;
        DECLARE @msg NVARCHAR(100) = FORMATMESSAGE('%d%% concluído (%d de %d lotes)...', @percent, @i, @totalBatches);
        RAISERROR(@msg, 0, 1) WITH NOWAIT;
    END
END

PRINT 'Carga de dados concluída com sucesso!';
PRINT 'Total de registros em [Transactions]:';
SELECT COUNT(*) FROM dbo.Transactions;
GO