namespace CoisasEmprestadas.Infra;

/// <summary>Violação de regra de negócio ou dado inválido (mensagens já amigáveis).</summary>
public class RegraNegocioException : Exception
{
    public IReadOnlyList<string> Erros { get; }

    public RegraNegocioException(IEnumerable<string> erros)
        : base(string.Join(Environment.NewLine, erros))
    {
        Erros = erros.ToList();
    }

    public RegraNegocioException(string erro) : this(new[] { erro }) { }
}

/// <summary>Falha de acesso a dados, com mensagem segura para exibir ao usuário.</summary>
public class AcessoDadosException : Exception
{
    public AcessoDadosException(string mensagemAmigavel, Exception interna)
        : base(mensagemAmigavel, interna) { }
}
