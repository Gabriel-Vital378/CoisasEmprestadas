# Coisas Emprestadas (C# WinForms + SQL Server LocalDB)

## Como executar
1. Requisitos: Visual Studio 2022+ com workload "Desenvolvimento para desktop com .NET" e o componente
   "SQL Server Express LocalDB" (já vem com o workload de dados do Visual Studio).
2. Abra `CoisasEmprestadas.csproj`, restaure os pacotes NuGet e pressione F5.
3. Na primeira execução o banco `CoisasEmprestadas` e as tabelas são criados automaticamente
   (alternativa: executar `script.sql` manualmente).
4. Para usar outro SQL Server, altere a connection string em `appsettings.json`.
   Se seu Visual Studio usa outro target, ajuste `TargetFramework` no `.csproj` (ex.: `net8.0-windows`).

## Arquitetura (SoC)
- `Forms/`    → somente interface (eventos, ErrorProvider, MessageBox). Sem SQL.
- `Services/` → regras de negócio + tradução de `SqlException` para mensagens amigáveis.
- `Data/`     → acesso a dados (ADO.NET, `using`, parâmetros, transações) + criação do banco.
- `Models/`   → entidade `Emprestimo` (status Atrasado/Devolvido calculado).
- `Infra/`    → leitura do appsettings.json, log de erros em arquivo, exceções próprias.

## Atalhos
Ctrl+N novo | F2 / Enter editar | F5 marcar como devolvido | Del excluir | Alt+letra sublinhada nos botões.
