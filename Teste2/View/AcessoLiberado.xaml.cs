using static OpenCvSharp.ML.DTrees;

namespace FingerprintComparisonApp;

public partial class AcessoLiberado : ContentPage
{
    public AcessoLiberado(string nome, string cargo)
    {
        InitializeComponent();
        NomeLabel.Text = $"Nome: {nome}";
        CargoLabel.Text = $"Cargo: {cargo}";
        NavigationPage.SetHasBackButton(this, false);
        this.SizeChanged += OnPageSizeChanged;
    }
    protected override bool OnBackButtonPressed()
    {
        return true;
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }

    private void OnPageSizeChanged(object sender, EventArgs e)
    {
        // Definindo o WidthRequest do botão para 50% da largura da tela
        labelAcesso.WidthRequest = this.Width * 0.8;
        NomeLabel.WidthRequest = this.Width * 0.8;
        CargoLabel.WidthRequest = this.Width * 0.8;
        buttonBack.WidthRequest = this.Width * 0.8;
    }
}