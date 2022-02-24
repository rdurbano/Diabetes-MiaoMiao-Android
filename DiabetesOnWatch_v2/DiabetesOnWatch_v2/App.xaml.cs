using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DiabetesOnWatch_v2
{
    public partial class App : Application
    {
        public static BluetoothViewModel vmBle;

        public App()
        {
            vmBle = new BluetoothViewModel();

            MainPage = new NavigationPage(new View.MainPage(vmBle));
        }

        
        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
