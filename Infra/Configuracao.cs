using Microsoft.Extensions.Configuration;

namespace CoisasEmprestadas.Infra;

/// <summary>Lê a connection string do appsettings.json (nada hardcoded no código).</summary>
public static class Configuracao
{
    public static string ObterConnectionString()
    {
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        return config.GetConnectionString("CoisasEmprestadas")
            ?? throw new InvalidOperationException("Connection string 'CoisasEmprestadas' não encontrada no appsettings.json.");
    }
}
