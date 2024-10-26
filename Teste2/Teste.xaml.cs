using static OpenCvSharp.ML.DTrees;

namespace FingerprintComparisonApp;

public partial class Teste : ContentPage
{
	public Teste(string nome,string cargo)
	{
        InitializeComponent();
        NomeLabel.Text = $"Nome: {nome}";
        CargoLabel.Text = $"Cargo: {cargo}";
    }
}