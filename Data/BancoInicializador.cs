using Microsoft.Data.SqlClient;

namespace CoisasEmprestadas.Data;

/// <summary>Cria o banco e as tabelas na primeira execução (o usuário só "instala e usa").</summary>
public static class BancoInicializador
{
    public static void Inicializar(string connectionString)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        string nomeBanco = builder.InitialCatalog;
        builder.InitialCatalog = "master";

        using (var conn = new SqlConnection(builder.ConnectionString))
        {
            conn.Open();
            const string sqlCriar = @"
                IF DB_ID(@nome) IS NULL
                BEGIN
                    DECLARE @cmd NVARCHAR(300) = N'CREATE DATABASE ' + QUOTENAME(@nome);
                    EXEC sp_executesql @cmd;
                END";
            using var cmd = new SqlCommand(sqlCriar, conn);
            cmd.Parameters.AddWithValue("@nome", nomeBanco);
            cmd.ExecuteNonQuery();
        }

        const string sqlTabelas = @"
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
            IF OBJECT_ID('dbo.Historico') IS NULL
            CREATE TABLE dbo.Historico (
                Id           INT IDENTITY(1,1) PRIMARY KEY,
                EmprestimoId INT NOT NULL REFERENCES dbo.Emprestimos(Id),
                Evento       NVARCHAR(30) NOT NULL,
                DataEvento   DATETIME2 NOT NULL DEFAULT SYSDATETIME()
            );";

        using var conn2 = new SqlConnection(connectionString);
        conn2.Open();
        using var cmd2 = new SqlCommand(sqlTabelas, conn2);
        cmd2.ExecuteNonQuery();
    }
}
