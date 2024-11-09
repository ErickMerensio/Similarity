namespace FingerprintComparisonApp;

public partial class AcessoLiberado : ContentPage
{
    public AcessoLiberado(string nome, string cargo)
    {
        InitializeComponent();
        NomeLabel.Text = $"Nome: {nome}";
        CargoLabel.Text = $"Cargo: {(cargo == "Nível 1" ? (cargo == "Nível 2" ? "Civil" : "Diretor de divisão") 
            : "Ministro do meio ambiente")}";
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
        labelAcesso.WidthRequest = this.Width * 0.8;
        NomeLabel.WidthRequest = this.Width * 0.8;
        CargoLabel.WidthRequest = this.Width * 0.8;
        buttonBack.WidthRequest = this.Width * 0.8;
    }
}