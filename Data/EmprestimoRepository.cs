using CoisasEmprestadas.Models;
using Microsoft.Data.SqlClient;

namespace CoisasEmprestadas.Data;

public class EmprestimoRepository : IEmprestimoRepository
{
    private readonly string _connectionString;

    public EmprestimoRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public List<Emprestimo> Listar(bool incluirDevolvidos)
    {
        const string sql = @"
            SELECT Id, Item, NomeAmigo, ContatoAmigo, DataEmprestimo, DataDevolucaoCombinada, DataDevolucaoReal
            FROM Emprestimos
            WHERE @incluir = 1 OR DataDevolucaoReal IS NULL
            ORDER BY CASE WHEN DataDevolucaoReal IS NULL THEN 0 ELSE 1 END,
                     CASE WHEN DataDevolucaoCombinada IS NULL THEN 1 ELSE 0 END,
                     DataDevolucaoCombinada, DataEmprestimo";

        var lista = new List<Emprestimo>();
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@incluir", incluirDevolvidos ? 1 : 0);
        conn.Open();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            lista.Add(new Emprestimo
            {
                Id = reader.GetInt32(0),
                Item = reader.GetString(1),
                NomeAmigo = reader.GetString(2),
                ContatoAmigo = reader.GetString(3),
                DataEmprestimo = reader.GetDateTime(4),
                DataDevolucaoCombinada = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                DataDevolucaoReal = reader.IsDBNull(6) ? null : reader.GetDateTime(6)
            });
        }
        return lista;
    }

    // Inserção + registro no histórico: operações interdependentes => transação.
    public void Inserir(Emprestimo e)
    {
        const string sqlInsert = @"
            INSERT INTO Emprestimos (Item, NomeAmigo, ContatoAmigo, DataEmprestimo, DataDevolucaoCombinada)
            VALUES (@item, @amigo, @contato, @dataEmp, @dataComb);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        using var conn = new SqlConnection(_connectionString);
        conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            int id;
            using (var cmd = new SqlCommand(sqlInsert, conn, tx))
            {
                cmd.Parameters.AddWithValue("@item", e.Item);
                cmd.Parameters.AddWithValue("@amigo", e.NomeAmigo);
                cmd.Parameters.AddWithValue("@contato", e.ContatoAmigo);
                cmd.Parameters.AddWithValue("@dataEmp", e.DataEmprestimo.Date);
                cmd.Parameters.AddWithValue("@dataComb", (object?)e.DataDevolucaoCombinada?.Date ?? DBNull.Value);
                id = (int)cmd.ExecuteScalar()!;
            }
            RegistrarHistorico(conn, tx, id, "EMPRESTADO");
            tx.Commit();
            e.Id = id;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public void Atualizar(Emprestimo e)
    {
        const string sql = @"
            UPDATE Emprestimos
            SET Item = @item, NomeAmigo = @amigo, ContatoAmigo = @contato,
                DataEmprestimo = @dataEmp, DataDevolucaoCombinada = @dataComb
            WHERE Id = @id";

        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", e.Id);
        cmd.Parameters.AddWithValue("@item", e.Item);
        cmd.Parameters.AddWithValue("@amigo", e.NomeAmigo);
        cmd.Parameters.AddWithValue("@contato", e.ContatoAmigo);
        cmd.Parameters.AddWithValue("@dataEmp", e.DataEmprestimo.Date);
        cmd.Parameters.AddWithValue("@dataComb", (object?)e.DataDevolucaoCombinada?.Date ?? DBNull.Value);
        conn.Open();
        cmd.ExecuteNonQuery();
    }

    // Marca devolução + registra histórico na mesma transação.
    public bool Devolver(int id, DateTime dataDevolucao)
    {
        const string sql = "UPDATE Emprestimos SET DataDevolucaoReal = @data WHERE Id = @id AND DataDevolucaoReal IS NULL";

        using var conn = new SqlConnection(_connectionString);
        conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            int afetadas;
            using (var cmd = new SqlCommand(sql, conn, tx))
            {
                cmd.Parameters.AddWithValue("@data", dataDevolucao.Date);
                cmd.Parameters.AddWithValue("@id", id);
                afetadas = cmd.ExecuteNonQuery();
            }
            if (afetadas == 0)
            {
                tx.Rollback();
                return false;
            }
            RegistrarHistorico(conn, tx, id, "DEVOLVIDO");
            tx.Commit();
            return true;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    // Remove histórico e empréstimo juntos (FK) => transação.
    public void Excluir(int id)
    {
        using var conn = new SqlConnection(_connectionString);
        conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            using (var cmd = new SqlCommand("DELETE FROM Historico WHERE EmprestimoId = @id", conn, tx))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
            using (var cmd = new SqlCommand("DELETE FROM Emprestimos WHERE Id = @id", conn, tx))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    private static void RegistrarHistorico(SqlConnection conn, SqlTransaction tx, int emprestimoId, string evento)
    {
        using var cmd = new SqlCommand(
            "INSERT INTO Historico (EmprestimoId, Evento) VALUES (@id, @evento)", conn, tx);
        cmd.Parameters.AddWithValue("@id", emprestimoId);
        cmd.Parameters.AddWithValue("@evento", evento);
        cmd.ExecuteNonQuery();
    }
}
