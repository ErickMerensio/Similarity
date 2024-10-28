using static OpenCvSharp.ML.DTrees;

namespace FingerprintComparisonApp;

public partial class AcessoLiberado : ContentPage
{
	public AcessoLiberado(string nome,string cargo)
	{
        InitializeComponent();
        NomeLabel.Text = $"Nome: {nome}";
        CargoLabel.Text = $"Cargo: {cargo}";
    }
}