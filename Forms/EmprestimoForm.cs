using System.Text.RegularExpressions;
using CoisasEmprestadas.Infra;
using CoisasEmprestadas.Models;
using CoisasEmprestadas.Services;

namespace CoisasEmprestadas.Forms;

/// <summary>Cadastro/edição de empréstimo. Valida formato na tela; regras de negócio ficam no serviço.</summary>
public class EmprestimoForm : Form
{
    private static readonly Regex RegexEmail = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    private static readonly Regex RegexTelefone = new(@"^[\d\s\(\)\-\+]+$", RegexOptions.Compiled);

    private readonly EmprestimoService _servico;
    private readonly Emprestimo _emprestimo;

    private readonly TextBox txtItem = new();
    private readonly TextBox txtAmigo = new();
    private readonly TextBox txtContato = new();
    private readonly DateTimePicker dtpEmprestimo = new();
    private readonly CheckBox chkDevolucaoCombinada = new();
    private readonly DateTimePicker dtpDevolucaoCombinada = new();
    private readonly Button btnSalvar = new();
    private readonly Button btnCancelar = new();
    private readonly ErrorProvider errorProvider = new();

    public Emprestimo? EmprestimoSalvo { 
        get; private set; 
    }

    public EmprestimoForm(EmprestimoService servico, Emprestimo? existente)
    {
        _servico = servico;
        // Trabalha em uma cópia: se a validação falhar, a lista da tela não é alterada.
        _emprestimo = existente == null
            ? new Emprestimo()
            : new Emprestimo
            {
                Id = existente.Id,
                Item = existente.Item,
                NomeAmigo = existente.NomeAmigo,
                ContatoAmigo = existente.ContatoAmigo,
                DataEmprestimo = existente.DataEmprestimo,
                DataDevolucaoCombinada = existente.DataDevolucaoCombinada,
                DataDevolucaoReal = existente.DataDevolucaoReal
            };

        MontarInterface();
        PreencherCampos();
    }

    private void MontarInterface()
    {
        Text = _emprestimo.Id == 0 ? "Novo empréstimo" : "Editar empréstimo";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(440, 250);
        AcceptButton = btnSalvar;   // Enter salva
        CancelButton = btnCancelar; // Esc cancela
        errorProvider.ContainerControl = this;
        errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            Padding = new Padding(12),
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        txtItem.MaxLength = 100;
        txtAmigo.MaxLength = 100;
        txtContato.MaxLength = 100;
        dtpEmprestimo.Format = DateTimePickerFormat.Short;
        dtpEmprestimo.MaxDate = DateTime.Today;
        dtpDevolucaoCombinada.Format = DateTimePickerFormat.Short;
        chkDevolucaoCombinada.Text = "Devolução com&binada";
        chkDevolucaoCombinada.AutoSize = true;

        txtItem.TabIndex = 0;
        txtAmigo.TabIndex = 1;
        txtContato.TabIndex = 2;
        dtpEmprestimo.TabIndex = 3;
        chkDevolucaoCombinada.TabIndex = 4;
        dtpDevolucaoCombinada.TabIndex = 5;

        AdicionarLinha(layout, "&Item:", txtItem);
        AdicionarLinha(layout, "&Amigo:", txtAmigo);
        AdicionarLinha(layout, "&Contato (tel./e-mail):", txtContato);
        AdicionarLinha(layout, "&Data do empréstimo:", dtpEmprestimo);
        layout.Controls.Add(chkDevolucaoCombinada);
        layout.Controls.Add(dtpDevolucaoCombinada);

        btnSalvar.Text = "&Salvar";
        btnSalvar.TabIndex = 6;
        btnSalvar.AutoSize = true;
        btnSalvar.Click += BtnSalvar_Click;
        btnCancelar.Text = "Cancelar";
        btnCancelar.TabIndex = 7;
        btnCancelar.AutoSize = true;
        btnCancelar.CausesValidation = false;
        btnCancelar.DialogResult = DialogResult.Cancel;

        var botoes = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Dock = DockStyle.Fill,
            AutoSize = true
        };
        botoes.Controls.Add(btnCancelar);
        botoes.Controls.Add(btnSalvar);
        layout.Controls.Add(new Label()); // célula vazia
        layout.Controls.Add(botoes);

        Controls.Add(layout);

        // Validação de formato na interface
        txtItem.Validating += (_, _) => ValidarObrigatorio(txtItem, "Informe o item emprestado.");
        txtAmigo.Validating += (_, _) => ValidarObrigatorio(txtAmigo, "Informe o nome do amigo.");
        txtContato.Validating += (_, _) => ValidarContato();
        txtItem.TextChanged += (_, _) => errorProvider.SetError(txtItem, "");
        txtAmigo.TextChanged += (_, _) => errorProvider.SetError(txtAmigo, "");
        txtContato.TextChanged += (_, _) => errorProvider.SetError(txtContato, "");
        chkDevolucaoCombinada.CheckedChanged += (_, _) =>
        {
            dtpDevolucaoCombinada.Enabled = chkDevolucaoCombinada.Checked;
            errorProvider.SetError(dtpDevolucaoCombinada, "");
        };
        dtpEmprestimo.ValueChanged += (_, _) => ValidarDatas();
        dtpDevolucaoCombinada.ValueChanged += (_, _) => ValidarDatas();

        ActiveControl = txtItem; // foco inicial
    }

    private static void AdicionarLinha(TableLayoutPanel layout, string rotulo, Control campo)
    {
        layout.Controls.Add(new Label { Text = rotulo, AutoSize = true, Anchor = AnchorStyles.Left, UseMnemonic = true });
        campo.Dock = DockStyle.Fill;
        layout.Controls.Add(campo);
    }

    private void PreencherCampos()
    {
        txtItem.Text = _emprestimo.Item;
        txtAmigo.Text = _emprestimo.NomeAmigo;
        txtContato.Text = _emprestimo.ContatoAmigo;
        dtpEmprestimo.Value = _emprestimo.DataEmprestimo.Date > DateTime.Today ? DateTime.Today : _emprestimo.DataEmprestimo.Date;
        chkDevolucaoCombinada.Checked = _emprestimo.DataDevolucaoCombinada.HasValue;
        dtpDevolucaoCombinada.Enabled = chkDevolucaoCombinada.Checked;
        dtpDevolucaoCombinada.Value = _emprestimo.DataDevolucaoCombinada ?? DateTime.Today.AddDays(7);
        errorProvider.Clear();
    }

    // ---------- Validação de formato ----------

    private bool ValidarObrigatorio(TextBox caixa, string mensagem)
    {
        bool ok = !string.IsNullOrWhiteSpace(caixa.Text);
        errorProvider.SetError(caixa, ok ? "" : mensagem);
        return ok;
    }

    private bool ValidarContato()
    {
        string texto = txtContato.Text.Trim();
        string? erro = null;

        if (texto.Length == 0)
        {
            erro = "Informe o contato do amigo.";
        }
        else if (texto.Contains('@'))
        {
            if (!RegexEmail.IsMatch(texto)) erro = "E-mail inválido.";
        }
        else
        {
            int digitos = texto.Count(char.IsDigit);
            if (!RegexTelefone.IsMatch(texto) || digitos < 8 || digitos > 13)
                erro = "Informe um telefone (8 a 13 dígitos) ou um e-mail válido.";
        }

        errorProvider.SetError(txtContato, erro ?? "");
        return erro == null;
    }

    private bool ValidarDatas()
    {
        bool ok = true;
        if (chkDevolucaoCombinada.Checked && dtpDevolucaoCombinada.Value.Date < dtpEmprestimo.Value.Date)
        {
            errorProvider.SetError(dtpDevolucaoCombinada, "A devolução combinada não pode ser anterior ao empréstimo.");
            ok = false;
        }
        else
        {
            errorProvider.SetError(dtpDevolucaoCombinada, "");
        }
        return ok;
    }

    private bool ValidarCampos()
    {
        bool itemOk = ValidarObrigatorio(txtItem, "Informe o item emprestado.");
        bool amigoOk = ValidarObrigatorio(txtAmigo, "Informe o nome do amigo.");
        bool contatoOk = ValidarContato();
        bool datasOk = ValidarDatas();

        if (!itemOk) txtItem.Focus();
        else if (!amigoOk) txtAmigo.Focus();
        else if (!contatoOk) txtContato.Focus();
        else if (!datasOk) dtpDevolucaoCombinada.Focus();

        return itemOk && amigoOk && contatoOk && datasOk;
    }

    // ---------- Salvar ----------

    private void BtnSalvar_Click(object? sender, EventArgs e)
    {
        if (!ValidarCampos()) return;

        _emprestimo.Item = txtItem.Text;
        _emprestimo.NomeAmigo = txtAmigo.Text;
        _emprestimo.ContatoAmigo = txtContato.Text;
        _emprestimo.DataEmprestimo = dtpEmprestimo.Value.Date;
        _emprestimo.DataDevolucaoCombinada = chkDevolucaoCombinada.Checked
            ? dtpDevolucaoCombinada.Value.Date
            : null;

        try
        {
            _servico.Salvar(_emprestimo);
            EmprestimoSalvo = _emprestimo;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (RegraNegocioException ex)
        {
            MessageBox.Show(ex.Message, "Dados inválidos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (AcessoDadosException ex)
        {
            MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
