namespace CoisasEmprestadas.Models;

public class Emprestimo
{
    public int Id { get; set; }
    public string Item { get; set; } = string.Empty;
    public string NomeAmigo { get; set; } = string.Empty;
    public string ContatoAmigo { get; set; } = string.Empty;
    public DateTime DataEmprestimo { get; set; } = DateTime.Today;
    public DateTime? DataDevolucaoCombinada { get; set; }
    public DateTime? DataDevolucaoReal { get; set; }

    public bool Devolvido => DataDevolucaoReal.HasValue;

    public bool Atrasado =>
        !Devolvido && DataDevolucaoCombinada.HasValue && DataDevolucaoCombinada.Value.Date < DateTime.Today;

    public string StatusTexto
    {
        get
        {
            if (Devolvido)
                return $"Devolvido em {DataDevolucaoReal:dd/MM/yyyy}";
            if (Atrasado)
            {
                int dias = (DateTime.Today - DataDevolucaoCombinada!.Value.Date).Days;
                return dias == 1 ? "Atrasado (1 dia)" : $"Atrasado ({dias} dias)";
            }
            return "Emprestado";
        }
    }
}
