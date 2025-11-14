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

            // Quitar la barra de navegación para esa página
            NavigationPage.SetHasNavigationBar(loginPage, false);

            // Devolver la ventana con un NavigationPage que contiene tu login
            return new Window(new NavigationPage(loginPage));
        }
    }
}