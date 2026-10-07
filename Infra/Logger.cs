using System.Diagnostics;

namespace CoisasEmprestadas.Infra;

/// <summary>Grava detalhes técnicos em arquivo (o usuário só vê a mensagem amigável).</summary>
public static class Logger
{
    private static readonly string Caminho =
        Path.Combine(AppContext.BaseDirectory, "Logs", "erros.log");

    public static void Erro(Exception ex)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Caminho)!);
            File.AppendAllText(Caminho, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {ex}{Environment.NewLine}{Environment.NewLine}");
        }
        catch (IOException ioEx)
        {
            Debug.WriteLine($"Falha ao gravar log: {ioEx.Message}");
        }
        catch (UnauthorizedAccessException uaEx)
        {
            Debug.WriteLine($"Sem permissão para gravar log: {uaEx.Message}");
        }
    }
}
