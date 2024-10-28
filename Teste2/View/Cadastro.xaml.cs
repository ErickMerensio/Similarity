namespace FingerprintComparisonApp;

public partial class Cadastro : ContentPage
{
    private readonly DatabaseService dbService;
    private string imagePath1;
    public Cadastro()
	{
		InitializeComponent();
        dbService = DatabaseService.Instance;
    }
    private async Task<string> PickImageAsync()
    {
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Escolha uma impressão digital",
                FileTypes = FilePickerFileType.Images
            });

            if (result != null)
            {
                return result.FullPath;
            }
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private async void OnSelectImage1Clicked(object sender, EventArgs e)
    {
        imagePath1 = await PickImageAsync();
        if (!string.IsNullOrEmpty(imagePath1))
        {
            imagePath1 = Path.GetFullPath(imagePath1);
            //LabelImage1.Text = $"Imagem 1: {Path.GetFileName(imagePath1)}";
        }
    }
    private async void OnAddFingerprintClicked(object sender, EventArgs e)
    {
        string imagePath = await PickImageAsync(); // Seleciona uma imagem
        string nome = EntryNome.Text; // Certifique-se de que EntryNome está definido no XAML
        string cargo = CargoPicker.SelectedItem?.ToString(); // Certifique-se de que CargoPicker está definido no XAML
        if (string.IsNullOrEmpty(nome) || string.IsNullOrEmpty(cargo))
        {
            await DisplayAlert("Erro", "Por favor, digite o nome e selecione o cargo.", "OK");
            return;
        }

        if (!string.IsNullOrEmpty(imagePath))
        {
            // Adiciona a impressão digital junto com o nome e o cargo ao banco de dados
            dbService.AddFingerprint(imagePath, nome, cargo);
            await DisplayAlert("Sucesso", "Impressão digital adicionada ao banco de dados.", "OK");
        }
        else
        {
            await DisplayAlert("Erro", "Por favor, selecione uma imagem.", "OK");
        }
    }
}
