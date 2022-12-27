using Android.App;
using Android.OS;
using Android.Util;
using static Android.Content.Res.Resources;
using System.Threading.Tasks;
using AndroidX.AppCompat.App;
using Android.Content;

namespace Kauppa_Appi.Droid
{
    internal class Splash
    {
        [Activity(Theme = "@style/MyTheme.Splash", Label = "@string/ApplicationName", MainLauncher = true, NoHistory = true)]
        public class SplashActivity : AppCompatActivity
        {
            static readonly string TAG = "X:" + typeof(SplashActivity).Name;

            public override void OnCreate(Bundle savedInstanceState, PersistableBundle persistentState)
            {
                base.OnCreate(savedInstanceState, persistentState);
                Log.Debug(TAG, "SplashActivity.OnCreate");
            }

            // Aloitus
            protected override void OnResume()
            {
                base.OnResume();
                Task startupWork = new Task(() => { SimulateStartup(); });
                startupWork.Start();
            }

            public override void OnBackPressed() { }

            //Simuloi aloitusnäytön takana tapahtuvaa työtä (latausta)
            async void SimulateStartup()
            {
                Log.Debug(TAG, "Performing some startup work that takes a bit of time.");
                await Task.Delay(5000); //Näkyy 5sekuntia
                Log.Debug(TAG, "Startup work is finished - starting MainActivity.");
                StartActivity(new Intent(Application.Context, typeof(MainActivity)));
            }
        }
    }
}