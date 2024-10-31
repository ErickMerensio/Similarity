using FingerprintComparisonApp;

namespace Teste2
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new MainPage());
            SQLitePCL.Batteries_V2.Init();

        }
    }
}
