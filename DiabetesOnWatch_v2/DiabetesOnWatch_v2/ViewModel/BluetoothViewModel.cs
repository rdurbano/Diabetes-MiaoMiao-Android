using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
        private static Guid GLUCOSE_CHARACTERISTIC_RECV = Guid.Parse("6E400002-B5A3-F393-E0A9-E50E24DCCA9E");
        private static Guid GLUCOSE_CHARACTERISTIC_XMIT = Guid.Parse("6e400003-b5a3-f393-e0a9-e50e24dcca9e");
        private static Guid GLUCOSE_CHARACTERISTIC_DESCRIPTOR_XMIT = Guid.Parse("00002902-0000-1000-8000-00805f9b34fb");

        private IAdapter _bleAdapter;
        private IService _serviceGlucose;
        private ICharacteristic _characteristicGlucoseRecv;
        private ICharacteristic _characteristicGlucoseXmit;
        private IDescriptor _characteristicDescriptorXmit;

        public List<byte> dataMiaoMiao;
        
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
                            _characteristicGlucoseRecv = (ICharacteristic)await _serviceGlucose.GetCharacteristicAsync(GLUCOSE_CHARACTERISTIC_RECV);
                            _characteristicGlucoseXmit = (ICharacteristic) await _serviceGlucose.GetCharacteristicAsync(GLUCOSE_CHARACTERISTIC_XMIT);
                            _characteristicDescriptorXmit = (IDescriptor)await _characteristicGlucoseXmit.GetDescriptorAsync(GLUCOSE_CHARACTERISTIC_DESCRIPTOR_XMIT);

                            //_characteristicGlucoseRecv.ValueUpdated += _CharacteristicGlucose_ValueUpdated;
                            _characteristicGlucoseXmit.ValueUpdated += _CharacteristicGlucose_ValueUpdated;
                            _characteristicGlucoseXmit.StartUpdatesAsync();


                            byte[] dataRecv = { 0xf0 };
                            Thread.Sleep(2000);
                            await _characteristicGlucoseRecv.WriteAsync(dataRecv);

                            bleModel.bleStatus = "Device Connected";

                            await ((NavigationPage)App.Current.MainPage).PushAsync(new View.DeviceInfo(App.vmBle));
                        }
                        catch (Exception ex)
                        {
                            _Disconnect();
                            //throw ex;

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
                
                if (bytes[0] == 0x28)
                {
                    dataMiaoMiao = new List<byte>();
                    dataMiaoMiao.AddRange(bytes);
                }
                else
                {
                    if (bytes[0] != 0x28 && bytes[bytes.Length -1] != 0x29)
                    {
                        dataMiaoMiao.AddRange(bytes);
                    }
                    else
                    {
                        dataMiaoMiao.AddRange(bytes);
                        Int32 teste = BitConverter.ToInt32(dataMiaoMiao.GetRange(27+32,1).ToArray(), 0);
                    }
                }
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
