using Similarity;

namespace Similarity
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
