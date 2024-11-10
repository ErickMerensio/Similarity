namespace Similarity
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            this.SizeChanged += OnPageSizeChanged;
        }

        private async void OnCadastroButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Cadastro());
        }

        private async void OnLoginButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Login());
        }

        private void OnPageSizeChanged(object sender, EventArgs e)
        {
            cadastroButton.WidthRequest = this.Width * 0.8;
            loginButton.WidthRequest = this.Width * 0.8;
            linha.WidthRequest = this.Width * 0.8;
        }

    }
}