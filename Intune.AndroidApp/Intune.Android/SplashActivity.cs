using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Support.V7.App;
using Xamarin.Auth;
using System.Linq;

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
            var store = AccountStore.Create();
            var storeAccounts = store.FindAccountsForService("IntuneTechnologiesApp");
            if (storeAccounts.Count() == 0)
            {
                IntuneService.SignIn("SystemWakeUpEmail", "SystemWakeUpPassword");
                StartActivity(new Intent(Application.Context, typeof(SignInActivity)));
                return;
            }

            var signInAccount = storeAccounts.ToArray()[0];
            var signInId = signInAccount.Username;
            var password = signInAccount.Properties["Password"];
            var user = IntuneService.SignIn(signInId, password);
            if (user == null)
                StartActivity(new Intent(Application.Context, typeof(SignInActivity)));
            else
                showAccountsActivity(user);
        }

        private void showAccountsActivity(User user)
        {
            var accountsActivity = new Intent(this, typeof(AccountsActivity));
            accountsActivity.PutExtra("LoginUserId", user.Id);
            accountsActivity.PutExtra("LoginUserName", user.Name);
            StartActivity(accountsActivity);
        }

        public override void OnBackPressed() { }
    }
}