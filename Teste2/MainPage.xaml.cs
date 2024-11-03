using System;
using Microsoft.Maui.Controls;

namespace FingerprintComparisonApp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            this.SizeChanged += OnPageSizeChanged;
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

        private void OnPageSizeChanged(object sender, EventArgs e)
        {
            // Definindo o WidthRequest do botão para 50% da largura da tela
            cadastroButton.WidthRequest = this.Width * 0.8;
            loginButton.WidthRequest = this.Width * 0.8;
            linha.WidthRequest = this.Width * 0.8;
        }

    }
}