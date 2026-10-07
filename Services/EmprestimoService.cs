using CoisasEmprestadas.Data;
using CoisasEmprestadas.Infra;
using CoisasEmprestadas.Models;
using Microsoft.Data.SqlClient;

namespace CoisasEmprestadas.Services;

/// <summary>Regras de negócio e tradução de erros de banco. A interface só conversa com esta classe.</summary>
public class EmprestimoService
{
    private readonly IEmprestimoRepository _repositorio;

    public EmprestimoService(IEmprestimoRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public List<Emprestimo> Listar(bool incluirDevolvidos) =>
        Executar(() => _repositorio.Listar(incluirDevolvidos));

    public void Salvar(Emprestimo e)
    {
        Validar(e);
        Executar(() =>
        {
            if (e.Id == 0) _repositorio.Inserir(e);
            else _repositorio.Atualizar(e);
        });
    }

    public void Devolver(Emprestimo e, DateTime dataDevolucao)
    {
        if (e.Devolvido)
            throw new RegraNegocioException("Este item já foi marcado como devolvido.");
        if (dataDevolucao.Date < e.DataEmprestimo.Date)
            throw new RegraNegocioException("A data de devolução não pode ser anterior à data do empréstimo.");
        if (dataDevolucao.Date > DateTime.Today)
            throw new RegraNegocioException("A data de devolução não pode estar no futuro.");

        bool ok = Executar(() => _repositorio.Devolver(e.Id, dataDevolucao));
        if (!ok)
            throw new RegraNegocioException("Não foi possível registrar a devolução: o item já foi devolvido ou não existe mais.");
        e.DataDevolucaoReal = dataDevolucao.Date;
    }

    public void Excluir(Emprestimo e) => Executar(() => _repositorio.Excluir(e.Id));

    private static void Validar(Emprestimo e)
    {
        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(e.Item))
            erros.Add("Informe o item emprestado.");
        else if (e.Item.Trim().Length > 100)
            erros.Add("O nome do item deve ter no máximo 100 caracteres.");

        if (string.IsNullOrWhiteSpace(e.NomeAmigo))
            erros.Add("Informe o nome do amigo.");
        else if (e.NomeAmigo.Trim().Length > 100)
            erros.Add("O nome do amigo deve ter no máximo 100 caracteres.");

        if (string.IsNullOrWhiteSpace(e.ContatoAmigo))
            erros.Add("Informe o contato do amigo.");
        else if (e.ContatoAmigo.Trim().Length > 100)
            erros.Add("O contato deve ter no máximo 100 caracteres.");

        if (e.DataEmprestimo.Date > DateTime.Today)
            erros.Add("A data do empréstimo não pode estar no futuro.");

        if (e.DataDevolucaoCombinada.HasValue && e.DataDevolucaoCombinada.Value.Date < e.DataEmprestimo.Date)
            erros.Add("A data de devolução combinada não pode ser anterior à data do empréstimo.");

        if (erros.Count > 0)
            throw new RegraNegocioException(erros);

        e.Item = e.Item.Trim();
        e.NomeAmigo = e.NomeAmigo.Trim();
        e.ContatoAmigo = e.ContatoAmigo.Trim();
    }

    private static T Executar<T>(Func<T> operacao)
    {
        try
        {
            return operacao();
        }
        catch (SqlException ex)
        {
            Logger.Erro(ex);
            throw new AcessoDadosException(Traduzir(ex), ex);
        }
    }

    private static void Executar(Action operacao) =>
        Executar<object?>(() => { operacao(); return null; });

    private static string Traduzir(SqlException ex) => ex.Number switch
    {
        -2 or -1 or 2 or 53 or 4060 or 18456 =>
            "Não foi possível conectar ao banco de dados. Verifique se o SQL Server LocalDB está instalado e em execução.",
        547 => "Não foi possível concluir a operação porque existem dados relacionados.",
        1205 => "O banco de dados está ocupado. Tente novamente em instantes.",
        _ => "Ocorreu um erro ao acessar o banco de dados. Tente novamente; se persistir, consulte o arquivo de log."
    };
}
