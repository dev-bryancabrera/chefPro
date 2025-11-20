using chefPro.Views;

namespace chefPro
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var loginPage = new vLogin();

            return new Window(new NavigationPage(loginPage));
        }
    }
}