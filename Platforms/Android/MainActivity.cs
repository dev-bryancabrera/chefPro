using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Auth.Api;
using Android.Gms.Auth.Api.SignIn;
using Android.OS;
using Android.Runtime;

namespace chefPro
{
    [Activity(Theme = "@style/Maui.SplashTheme",
               MainLauncher = true,
               ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode)]
    public class MainActivity : MauiAppCompatActivity
    {
        public static Action<GoogleSignInResult> GoogleSignInCallback;
        private const int RC_SIGN_IN = 9001;

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == RC_SIGN_IN)
            {
                var task = Auth.GoogleSignInApi.GetSignInResultFromIntent(data);
                GoogleSignInCallback?.Invoke(task);
            }
        }
    }
}