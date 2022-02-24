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
using Xamarin.Forms.Xaml;

namespace DiabetesOnWatch_v2.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DeviceInfo : ContentPage
    {
        public DeviceInfo()
        {
            InitializeComponent();
        }

        public DeviceInfo(BluetoothViewModel vmBle)
        {
            BindingContext = vmBle;
            InitializeComponent();
        }
    }
}