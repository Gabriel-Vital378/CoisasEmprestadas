# Coisas Emprestadas

Sistema desktop para você não esquecer **para quem emprestou cada coisa**. Registra o item, o amigo, o contato e as datas, destaca o que está atrasado e guarda a data em que cada item foi devolvido.

Trabalho prático da disciplina, desenvolvido em C# (Windows Forms) com armazenamento em SQL Server LocalDB.

## Funcionalidades

- Tela inicial com a lista de coisas emprestadas e botão para incluir novo empréstimo.
- Cadastro com item, nome e contato do amigo, data do empréstimo e **data de devolução combinada (opcional)**.
- Itens com a data combinada vencida ficam **destacados em vermelho**, com a situação "Atrasado (n dias)".
- Marcação de devolução, que **grava a data em que o item voltou**.
- Edição e exclusão de empréstimos.
- Opção para mostrar ou ocultar os itens já devolvidos.
- Histórico de eventos (EMPRESTADO e DEVOLVIDO) gravado na tabela `Historico`.
- Os dados ficam salvos no banco, então permanecem ao fechar o programa.

## Tecnologias

- C# e .NET (Windows Forms)
- SQL Server LocalDB, acessado por ADO.NET (`Microsoft.Data.SqlClient`)
- `Microsoft.Extensions.Configuration.Json` para ler o `appsettings.json`

## Arquitetura

O projeto é dividido em camadas, e a interface não tem SQL nem regras de negócio.

| Pasta | Responsabilidade |
|---|---|
| `Forms/` | Telas: eventos, `ErrorProvider` e mensagens ao usuário. |
| `Services/` | Regras de negócio e conversão de `SqlException` em mensagens amigáveis. |
| `Data/` | Acesso ao banco: SQL parametrizado, conexões com `using` e transações. Também cria o banco e as tabelas. |
| `Models/` | Entidade `Emprestimo`, com as propriedades calculadas `Devolvido`, `Atrasado` e `StatusTexto`. |
| `Infra/` | Leitura da configuração, log de erros em arquivo e exceções próprias. |

## Boas práticas aplicadas

- **SQL Injection:** todas as consultas usam parâmetros.
- **Transações:** inserir, devolver e excluir gravam em duas tabelas, com `Commit` e `Rollback`.
- **Validação:** formato na tela (`ErrorProvider`) e regras de negócio no serviço.
- **Exceções:** captura de `SqlException` com mensagem amigável, e o detalhe técnico vai para `Logs/erros.log`.
- **Configuração:** a connection string fica no `appsettings.json`, com autenticação do Windows e sem senha no código.
- **Usabilidade:** ordem de tabulação lógica, foco inicial, Enter para salvar, Esc para cancelar e atalhos de teclado.

## Como executar

1. Instale o Visual Studio com a carga de trabalho **Desenvolvimento para desktop com .NET** e o componente **SQL Server Express LocalDB**.
2. Clone o repositório e abra o `CoisasEmprestadas.csproj`.
3. Restaure os pacotes NuGet e pressione **F5**.
4. Na primeira execução, o banco `CoisasEmprestadas` e as tabelas são criados automaticamente. O arquivo `script.sql` é opcional.

Para usar outro SQL Server, altere o `Server=` da connection string no `appsettings.json`. Se o seu Visual Studio não aceitar o `net10.0-windows`, troque o `TargetFramework` no `.csproj` pela versão instalada, por exemplo `net8.0-windows`.

## Atalhos

| Tecla | Ação |
|---|---|
| Ctrl+N | Novo empréstimo |
| F2 ou Enter | Editar |
| F5 | Marcar como devolvido |
| Del | Excluir |
| Alt + letra sublinhada | Acionar o botão correspondente |

## Autor

Gabriel
