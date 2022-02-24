using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace DiabetesOnWatch_v2.View
{
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public MainPage(BluetoothViewModel vmBle)
        {
            BindingContext = vmBle;
            InitializeComponent();
        }
    }
}
