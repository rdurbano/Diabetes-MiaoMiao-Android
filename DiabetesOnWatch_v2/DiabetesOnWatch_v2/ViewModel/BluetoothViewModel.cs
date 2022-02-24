using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DiabetesOnWatch_v2.Model;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Xamarin.Forms;

namespace DiabetesOnWatch_v2
{
    public class BluetoothViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        private static Guid GLUCOSE_SERVICE = Guid.Parse("6e400001-b5a3-f393-e0a9-e50e24dcca9e");
        private static Guid GLUCOSE_CHARACTERISTIC = Guid.Parse("6e400003-b5a3-f393-e0a9-e50e24dcca9e");

        private IAdapter _bleAdapter;
        private IService _serviceGlucose;
        private ICharacteristic _characteristicGlucose;
        private BluetoothModel _bleModel;
        public BluetoothModel bleModel
        {
            set{SetProperty(ref _bleModel, value); }
            get { return _bleModel; }
        }

        public BluetoothViewModel()
        {
            _bleAdapter = CrossBluetoothLE.Current.Adapter;
            bleModel = new BluetoothModel() { bleStatus = "", devicesCount=0,servicesList = new ObservableCollection<IService>(), devicesList = new ObservableCollection<IDevice>()};

            _bleAdapter.ScanMode = ScanMode.LowPower;
            _bleAdapter.ScanTimeout = 10000;


            _bleAdapter.ScanTimeoutElapsed += (sender, args) =>
            {
                bleModel.bleStatus = $"Scan Finaliazado";
            };
            _bleAdapter.DeviceDiscovered += (sender, args) =>
            {
                IDevice deviceDiscovered = args.Device;
                if (deviceDiscovered.Name != null)
                    bleModel.devicesList.Add(deviceDiscovered);

                bleModel.devicesCount = bleModel.devicesList.Count();

            };
        }


        private Command _StartScan;
        public Command StartScan
        {
            get 
            {
                if (_StartScan == null)
                {
                    _StartScan = new Command(async () =>
                    {
                        try
                        {
                            if  (!_bleAdapter.IsScanning)
                            {
                                bleModel.devicesList.Clear();
                                bleModel.bleStatus = "Executing Scan ...";
                                await _bleAdapter.StartScanningForDevicesAsync();
                            }
                        }
                        catch (Exception ex)
                        {

                            throw ex;
                        }
                    });
                }
                return _StartScan;
            }
        }


        private Command _ConnectDevice;
        public Command ConnectDevice
        {
            get 
            {
                if (_ConnectDevice == null)
                {
                    _ConnectDevice = new Command(async () =>
                    {
                        if (_bleAdapter.IsScanning)
                        {
                            await _bleAdapter.StopScanningForDevicesAsync();
                        }


                        try
                        {
                            bleModel.bleStatus = "Connecting device ...";
                            await _bleAdapter.ConnectToDeviceAsync(bleModel.deviceConnected);

                            
                            _serviceGlucose = (IService)await bleModel.deviceConnected.GetServiceAsync(GLUCOSE_SERVICE);
                            _characteristicGlucose = (ICharacteristic) await _serviceGlucose.GetCharacteristicAsync(GLUCOSE_CHARACTERISTIC);


                            _characteristicGlucose.ValueUpdated += _CharacteristicGlucose_ValueUpdated;
                            _characteristicGlucose.StartUpdatesAsync();

                            bleModel.bleStatus = "Device Connected";

                            await ((NavigationPage)App.Current.MainPage).PushAsync(new View.DeviceInfo(App.vmBle));
                        }
                        catch (Exception ex)
                        {
                            _Disconnect();
                            throw ex;
                        }
                    });
                }
                return _ConnectDevice;
            }
        }


        private Command _DisconnectDevice;
        public Command DisconnectDevice
        {
            get 
            {
                if (_DisconnectDevice == null)
                {
                    _DisconnectDevice = new Command(async () =>
                    {
                        _Disconnect();
                    });
                }
                return _DisconnectDevice;
            }
        }

        private async Task _Disconnect()
        {
            bleModel.bleStatus = "Device disconnected";
            await _bleAdapter.DisconnectDeviceAsync(bleModel.deviceConnected);
        }

        private void _CharacteristicGlucose_ValueUpdated(object sender, CharacteristicUpdatedEventArgs args)
        {
            try
            {
                var bytes = args.Characteristic.Value;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Object.Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
