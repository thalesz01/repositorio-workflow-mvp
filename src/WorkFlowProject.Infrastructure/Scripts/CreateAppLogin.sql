/*
    Cria um login SQL dedicado à aplicação e o autoriza no banco WorkFlow_Project.

    Por que não usar o 'sa': o sa é sysadmin da instância inteira. A API e o Worker
    só fazem leitura e escrita nas tabelas de dbo, então recebem apenas db_datareader
    e db_datawriter — se a credencial vazar, o estrago fica contido a este banco.

    A senha NÃO está neste arquivo: ela chega por variável do sqlcmd, de modo que
    este script pode ser versionado sem carregar segredo.

    Uso (a partir da pasta do script):

        sqlcmd -S localhost,1433 -U sa -i CreateAppLogin.sql ^
               -v AppLogin="workflow_app" -v AppPassword="SuaSenhaForte#2026"

    O script é idempotente: rodar de novo não quebra nada.
*/

:on error exit

------------------------------------------------------------------
-- 1. Login no nível da instância
------------------------------------------------------------------
USE [master];
GO

IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'$(AppLogin)')
BEGIN
    CREATE LOGIN [$(AppLogin)]
        WITH PASSWORD    = N'$(AppPassword)',
             CHECK_POLICY = ON;
    PRINT 'Login [$(AppLogin)] criado.';
END
ELSE
    PRINT 'Login [$(AppLogin)] ja existia - mantido.';
GO

------------------------------------------------------------------
-- 2. Usuário dentro do banco da aplicação
------------------------------------------------------------------
USE [WorkFlow_Project];
GO

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'$(AppLogin)')
BEGIN
    CREATE USER [$(AppLogin)] FOR LOGIN [$(AppLogin)];
    PRINT 'Usuario [$(AppLogin)] criado em WorkFlow_Project.';
END
ELSE
    PRINT 'Usuario [$(AppLogin)] ja existia - mantido.';
GO

------------------------------------------------------------------
-- 3. Permissões: apenas DML nas tabelas
------------------------------------------------------------------
ALTER ROLE db_datareader ADD MEMBER [$(AppLogin)];
ALTER ROLE db_datawriter ADD MEMBER [$(AppLogin)];
GO

PRINT 'Pronto. [$(AppLogin)] tem leitura e escrita em WorkFlow_Project.';
GO
