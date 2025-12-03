using chefPro.Models;
using System.Net;

namespace chefPro.Views;

public partial class vLogin : ContentPage
{
    private readonly GoogleAuthService _googleAuth = new GoogleAuthService();
    private const string URL_BASE = AppConfig.URL_BASE;

    public vLogin()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
    }

    private async void btnLogin_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (!txtEmail.IsValid || !txtPassword.IsValid)
            {
                return;
            }

            // Validar campos vacios
            if (string.IsNullOrWhiteSpace(txtEmail.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                await DisplayAlert("Error", "Credenciales incorrectas", "OK");
                return;
            }

            WebClient cliente = new WebClient();
            var parametros = new System.Collections.Specialized.NameValueCollection();
            parametros.Add("email", txtEmail.Text);
            parametros.Add("password", txtPassword.Text);

            // Hacer la petición
            byte[] respuestaBytes = cliente.UploadValues(
                $"{URL_BASE}/auth/login",
                "POST",
                parametros
            );

            // Convertir la respuesta a string
            string respuesta = System.Text.Encoding.UTF8.GetString(respuestaBytes);

            // Deserializar la respuesta JSON
            var resultado = System.Text.Json.JsonSerializer.Deserialize<Auth>(respuesta);

            if (resultado != null && resultado.usuario != null)
            {
                /*await DisplayAlert("Éxito", $"Bienvenido {resultado.usuario.nombres}", "OK");*/

                await Navigation.PushAsync(new vInicio(resultado.usuario.nombres, resultado.usuario.id_usuario));
            }
            else
            {
                await DisplayAlert("Error", "Credenciales incorrectas", "OK");
            }
        }
        catch (WebException wex)
        {
            try
            {
                // Leer la respuesta de error del servidor
                using (var stream = wex.Response.GetResponseStream())
                using (var reader = new System.IO.StreamReader(stream))
                {
                    string errorResponse = reader.ReadToEnd();
                    var error = System.Text.Json.JsonSerializer.Deserialize<ErrorResponse>(errorResponse);
                    await DisplayAlert("Error", error.error, "OK");
                }
            }
            catch
            {
                await DisplayAlert("Error", "Error de conexión: " + wex.Message, "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Ocurrió un problema:\n" + ex.Message, "OK");
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Limpiar campos
        txtEmail.Text = string.Empty;
        txtPassword.Text = string.Empty;

        // Resetear validaciones
        txtEmail.ResetValidation();
        txtPassword.ResetValidation();
    }

    private void btnRegister_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new vRegistro());
    }

    private async void btnLoginGoogle_Clicked(object sender, EventArgs e)
    {
        try
        {
            btnLoginGoogle.IsEnabled = false;
            btnLoginGoogle.Text = "Conectando...";

            var userInfo = await _googleAuth.SignInAsync();

            if (userInfo != null)
            {
                // Enviar a tu backend
                WebClient cliente = new WebClient();
                var parametros = new System.Collections.Specialized.NameValueCollection();
                parametros.Add("nombres", userInfo.name);
                parametros.Add("email", userInfo.email);
                parametros.Add("google_id", userInfo.id);
                parametros.Add("tipo_login", "2");

                byte[] respuestaBytes = cliente.UploadValues(
                    $"{URL_BASE}/auth/google-login",
                    "POST",
                    parametros
                );

                string respuesta = System.Text.Encoding.UTF8.GetString(respuestaBytes);

                await DisplayAlert("Éxito", $"Bienvenido {userInfo.name}", "OK");
                await Navigation.PushAsync(new vInicio());
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            btnLoginGoogle.IsEnabled = true;
            btnLoginGoogle.Text = "Iniciar con Google";
        }
    }
}