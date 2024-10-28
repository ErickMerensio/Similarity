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
            await DisplayAlert("Sucesso", "Impressão Digital adicionada com Sucesso!.", "OK");
            ValidateForm(); 
        }
    }

    private void OnEntryTextChanged(object sender, TextChangedEventArgs e)
    {
        ValidateForm(); 
    }

    private void OnPickerSelectedIndexChanged(object sender, EventArgs e)
    {
        ValidateForm(); 
    }

    private void ValidateForm()
    {
        ButtonCadastrar.IsEnabled = !string.IsNullOrEmpty(EntryNome.Text) &&
                                    CargoPicker.SelectedItem != null &&
                                    !string.IsNullOrEmpty(imagePath1);
    }

    private async void OnAddFingerprintClicked(object sender, EventArgs e)
    {
        string nome = EntryNome.Text;
        string cargo = CargoPicker.SelectedItem?.ToString();

        if (string.IsNullOrEmpty(imagePath1))
        {
            await DisplayAlert("Erro", "Por favor, selecione uma imagem.", "OK");
            return;
        }

        dbService.AddFingerprint(imagePath1, nome, cargo);
        await DisplayAlert("Sucesso", "Cadastrado com Sucesso!", "OK");

        // Volta para a MainPage
        await Navigation.PopToRootAsync();
    }
}
