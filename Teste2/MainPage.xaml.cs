using System;
using Microsoft.Maui.Controls;

namespace FingerprintComparisonApp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        // Evento de clique do botão de cadastro
        private async void OnCadastroButtonClicked(object sender, EventArgs e)
        {
            // Navega para a página de Cadastro
            await Navigation.PushAsync(new Cadastro());
        }

        // Evento de clique do botão de login
        private async void OnLoginButtonClicked(object sender, EventArgs e)
        {
            // Navega para a página de Login
            await Navigation.PushAsync(new Login());
        }
    }
}