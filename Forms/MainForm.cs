using CoisasEmprestadas.Infra;
using CoisasEmprestadas.Models;
using CoisasEmprestadas.Services;

namespace CoisasEmprestadas.Forms;

/// <summary>Tela inicial: lista de empréstimos. Sem SQL nem regras de negócio aqui.</summary>
public class MainForm : Form
{
    private readonly EmprestimoService _servico;
    private readonly Font _fonteNegrito;

    private readonly DataGridView dgvEmprestimos = new();
    private readonly Button btnNovo = new();
    private readonly Button btnEditar = new();
    private readonly Button btnDevolver = new();
    private readonly Button btnExcluir = new();
    private readonly CheckBox chkMostrarDevolvidos = new();
    private readonly ToolStripStatusLabel lblResumo = new();

    public MainForm(EmprestimoService servico)
    {
        _servico = servico;
        _fonteNegrito = new Font(Font, FontStyle.Bold);
        MontarInterface();
        Load += (_, _) => CarregarLista();
    }

    private void MontarInterface()
    {
        Text = "Coisas Emprestadas";
        MinimumSize = new Size(820, 420);
        Size = new Size(960, 520);
        StartPosition = FormStartPosition.CenterScreen;
        KeyPreview = true;
        KeyDown += MainForm_KeyDown;

        // Grade
        dgvEmprestimos.Dock = DockStyle.Fill;
        dgvEmprestimos.ReadOnly = true;
        dgvEmprestimos.AllowUserToAddRows = false;
        dgvEmprestimos.AllowUserToDeleteRows = false;
        dgvEmprestimos.AllowUserToResizeRows = false;
        dgvEmprestimos.RowHeadersVisible = false;
        dgvEmprestimos.MultiSelect = false;
        dgvEmprestimos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvEmprestimos.AutoGenerateColumns = false;
        dgvEmprestimos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvEmprestimos.BackgroundColor = SystemColors.Window;
        dgvEmprestimos.TabIndex = 0;
        dgvEmprestimos.Columns.Add(Coluna("Item", nameof(Emprestimo.Item), 25));
        dgvEmprestimos.Columns.Add(Coluna("Amigo", nameof(Emprestimo.NomeAmigo), 20));
        dgvEmprestimos.Columns.Add(Coluna("Contato", nameof(Emprestimo.ContatoAmigo), 18));
        dgvEmprestimos.Columns.Add(Coluna("Emprestado em", nameof(Emprestimo.DataEmprestimo), 13, "dd/MM/yyyy"));
        dgvEmprestimos.Columns.Add(Coluna("Devolução combinada", nameof(Emprestimo.DataDevolucaoCombinada), 14, "dd/MM/yyyy"));
        dgvEmprestimos.Columns.Add(Coluna("Situação", nameof(Emprestimo.StatusTexto), 18));
        dgvEmprestimos.CellFormatting += DgvEmprestimos_CellFormatting;
        dgvEmprestimos.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) EditarSelecionado(); };
        dgvEmprestimos.KeyDown += DgvEmprestimos_KeyDown;
        dgvEmprestimos.SelectionChanged += (_, _) => AtualizarBotoes();

        // Barra de ações
        var painelTopo = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(8, 8, 8, 4),
            WrapContents = false
        };
        ConfigurarBotao(btnNovo, "&Novo (Ctrl+N)", 1, (_, _) => NovoEmprestimo());
        ConfigurarBotao(btnEditar, "&Editar (F2)", 2, (_, _) => EditarSelecionado());
        ConfigurarBotao(btnDevolver, "&Devolvido (F5)", 3, (_, _) => DevolverSelecionado());
        ConfigurarBotao(btnExcluir, "E&xcluir (Del)", 4, (_, _) => ExcluirSelecionado());
        chkMostrarDevolvidos.Text = "Mostrar já de&volvidos";
        chkMostrarDevolvidos.AutoSize = true;
        chkMostrarDevolvidos.Margin = new Padding(16, 8, 3, 3);
        chkMostrarDevolvidos.TabIndex = 5;
        chkMostrarDevolvidos.CheckedChanged += (_, _) => CarregarLista(SelecionadoId());
        painelTopo.Controls.AddRange(new Control[] { btnNovo, btnEditar, btnDevolver, btnExcluir, chkMostrarDevolvidos });

        // Rodapé
        var status = new StatusStrip();
        status.Items.Add(lblResumo);

        // Ordem importa para o Dock: o Fill entra primeiro.
        Controls.Add(dgvEmprestimos);
        Controls.Add(painelTopo);
        Controls.Add(status);

        ActiveControl = dgvEmprestimos; // foco inicial na lista
    }

    private static DataGridViewTextBoxColumn Coluna(string titulo, string propriedade, float peso, string? formato = null)
    {
        var col = new DataGridViewTextBoxColumn
        {
            HeaderText = titulo,
            DataPropertyName = propriedade,
            FillWeight = peso,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        if (formato != null) col.DefaultCellStyle.Format = formato;
        return col;
    }

    private static void ConfigurarBotao(Button botao, string texto, int tabIndex, EventHandler acao)
    {
        botao.Text = texto;
        botao.AutoSize = true;
        botao.Padding = new Padding(6, 2, 6, 2);
        botao.TabIndex = tabIndex;
        botao.Click += acao;
    }

    // ---------- Dados ----------

    private void CarregarLista(int? selecionarId = null)
    {
        try
        {
            var lista = _servico.Listar(chkMostrarDevolvidos.Checked);
            dgvEmprestimos.DataSource = lista;

            if (selecionarId.HasValue)
            {
                foreach (DataGridViewRow linha in dgvEmprestimos.Rows)
                {
                    if (linha.DataBoundItem is Emprestimo emp && emp.Id == selecionarId.Value)
                    {
                        linha.Selected = true;
                        dgvEmprestimos.CurrentCell = linha.Cells[0];
                        break;
                    }
                }
            }

            int pendentes = lista.Count(e => !e.Devolvido);
            int atrasados = lista.Count(e => e.Atrasado);
            lblResumo.Text = $"{pendentes} item(ns) emprestado(s) | {atrasados} atrasado(s)";
        }
        catch (AcessoDadosException ex)
        {
            MostrarErro(ex.Message);
        }
        AtualizarBotoes();
    }

    private Emprestimo? Selecionado() =>
        dgvEmprestimos.CurrentRow?.DataBoundItem as Emprestimo;

    private int? SelecionadoId() => Selecionado()?.Id;

    private void AtualizarBotoes()
    {
        var emp = Selecionado();
        btnEditar.Enabled = emp != null;
        btnExcluir.Enabled = emp != null;
        btnDevolver.Enabled = emp != null && !emp.Devolvido;
    }

    // ---------- Ações ----------

    private void NovoEmprestimo()
    {
        using var form = new EmprestimoForm(_servico, null);
        if (form.ShowDialog(this) == DialogResult.OK)
            CarregarLista(form.EmprestimoSalvo?.Id);
    }

    private void EditarSelecionado()
    {
        var emp = Selecionado();
        if (emp == null) return;

        using var form = new EmprestimoForm(_servico, emp);
        if (form.ShowDialog(this) == DialogResult.OK)
            CarregarLista(emp.Id);
    }

    private void DevolverSelecionado()
    {
        var emp = Selecionado();
        if (emp == null || emp.Devolvido) return;

        var resposta = MessageBox.Show(
            $"Marcar \"{emp.Item}\" (emprestado para {emp.NomeAmigo}) como devolvido hoje?",
            "Confirmar devolução", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (resposta != DialogResult.Yes) return;

        try
        {
            _servico.Devolver(emp, DateTime.Today);
            CarregarLista(emp.Id);
        }
        catch (RegraNegocioException ex)
        {
            MessageBox.Show(ex.Message, "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (AcessoDadosException ex)
        {
            MostrarErro(ex.Message);
        }
    }

    private void ExcluirSelecionado()
    {
        var emp = Selecionado();
        if (emp == null) return;

        var resposta = MessageBox.Show(
            $"Excluir o registro de \"{emp.Item}\"? Esta ação não pode ser desfeita.",
            "Confirmar exclusão", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
        if (resposta != DialogResult.Yes) return;

        try
        {
            _servico.Excluir(emp);
            CarregarLista();
        }
        catch (AcessoDadosException ex)
        {
            MostrarErro(ex.Message);
        }
    }

    private static void MostrarErro(string mensagem) =>
        MessageBox.Show(mensagem, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);

    // ---------- Destaque visual ----------

    private void DgvEmprestimos_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || e.CellStyle == null) return;
        if (dgvEmprestimos.Rows[e.RowIndex].DataBoundItem is not Emprestimo emp) return;

        if (emp.Atrasado)
        {
            e.CellStyle.BackColor = Color.MistyRose;
            e.CellStyle.ForeColor = Color.DarkRed;
            e.CellStyle.Font = _fonteNegrito;
            e.CellStyle.SelectionBackColor = Color.IndianRed;
            e.CellStyle.SelectionForeColor = Color.White;
        }
        else if (emp.Devolvido)
        {
            e.CellStyle.ForeColor = Color.Gray;
        }
    }

    // ---------- Teclado ----------

    private void MainForm_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.N) { NovoEmprestimo(); e.SuppressKeyPress = true; }
        else if (e.KeyCode == Keys.F2) { EditarSelecionado(); e.SuppressKeyPress = true; }
        else if (e.KeyCode == Keys.F5) { DevolverSelecionado(); e.SuppressKeyPress = true; }
    }

    private void DgvEmprestimos_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter) { EditarSelecionado(); e.SuppressKeyPress = true; e.Handled = true; }
        else if (e.KeyCode == Keys.Delete) { ExcluirSelecionado(); e.SuppressKeyPress = true; e.Handled = true; }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _fonteNegrito.Dispose();
        base.Dispose(disposing);
    }
}
