namespace Xamarin.Forms.Xaml.LiveReload.Test
{
    public class App : Application
    {
        public App()
        {
            // The root page of your application
            MainPage = new MainPage();


        }

        protected override void OnStart()
        {
            // Handle when your app starts
            #if DEBUG
            Xamarin.Forms.Xaml.LiveReload.LiveReload.Enable(this, exception =>
            {
                System.Diagnostics.Debug.WriteLine(exception);
            });
#endif
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
