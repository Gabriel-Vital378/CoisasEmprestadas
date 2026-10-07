using CoisasEmprestadas.Models;

namespace CoisasEmprestadas.Data;

public interface IEmprestimoRepository
{
    List<Emprestimo> Listar(bool incluirDevolvidos);
    void Inserir(Emprestimo emprestimo);
    void Atualizar(Emprestimo emprestimo);
    bool Devolver(int id, DateTime dataDevolucao);
    void Excluir(int id);
}
