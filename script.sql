IF DB_ID('CoisasEmprestadas') IS NULL CREATE DATABASE CoisasEmprestadas;
GO
USE CoisasEmprestadas;
GO
IF OBJECT_ID('dbo.Emprestimos') IS NULL
CREATE TABLE dbo.Emprestimos (
    Id                     INT IDENTITY(1,1) PRIMARY KEY,
    Item                   NVARCHAR(100) NOT NULL,
    NomeAmigo              NVARCHAR(100) NOT NULL,
    ContatoAmigo           NVARCHAR(100) NOT NULL,
    DataEmprestimo         DATE NOT NULL,
    DataDevolucaoCombinada DATE NULL,
    DataDevolucaoReal      DATE NULL
);
GO
IF OBJECT_ID('dbo.Historico') IS NULL
CREATE TABLE dbo.Historico (
    Id           INT IDENTITY(1,1) PRIMARY KEY,
    EmprestimoId INT NOT NULL REFERENCES dbo.Emprestimos(Id),
    Evento       NVARCHAR(30) NOT NULL,
    DataEvento   DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);
GO
