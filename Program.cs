using CoisasEmprestadas.Data;
using CoisasEmprestadas.Forms;
using CoisasEmprestadas.Infra;
using CoisasEmprestadas.Services;
using Microsoft.Data.SqlClient;

namespace CoisasEmprestadas;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        // Rede de segurança: erros inesperados são logados e o usuário vê só uma mensagem amigável.
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, args) =>
        {
            Logger.Erro(args.Exception);
            MessageBox.Show("Ocorreu um erro inesperado. A operação foi cancelada.",
                "Coisas Emprestadas", MessageBoxButtons.OK, MessageBoxIcon.Error);
        };

        string connectionString;
        try
        {
            connectionString = Configuracao.ObterConnectionString();
            BancoInicializador.Inicializar(connectionString);
        }
        catch (SqlException ex)
        {
            Logger.Erro(ex);
            MessageBox.Show(
                "Não foi possível acessar o banco de dados.\n\n" +
                "Verifique se o SQL Server LocalDB está instalado e se a connection string no appsettings.json está correta.",
                "Coisas Emprestadas", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        catch (Exception ex) when (ex is FileNotFoundException || ex is InvalidOperationException)
        {
            Logger.Erro(ex);
            MessageBox.Show("Arquivo de configuração (appsettings.json) ausente ou inválido.",
                "Coisas Emprestadas", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var servico = new EmprestimoService(new EmprestimoRepository(connectionString));
        Application.Run(new MainForm(servico));
    }
}
