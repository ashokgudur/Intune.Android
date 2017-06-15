using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;

namespace Intune.Android
{
    [Activity(Theme = "@style/IntuneTheme.Splash", MainLauncher = true, NoHistory = true)]
    public class SplashActivity : AppCompatActivity
    {
        protected override void OnResume()
        {
            base.OnResume();
            var startupWork = new Task(() => { SimulateStartup(); });
            startupWork.Start();
        }

        void SimulateStartup()
        {
            IntuneService.SignIn("SystemWakeUpEmail", "SystemWakeUpPassword");
            StartActivity(new Intent(Application.Context, typeof(SignInActivity)));
        }

        public override void OnBackPressed() { }
    }
}