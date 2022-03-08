using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DiabetesOnWatch_v2.Model;
using DiabetesOnWatch_v2.Utils;
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
        List<GlucoseData> historyList = new List<GlucoseData>();
        List<GlucoseData> trendList = new List<GlucoseData>();

        
        private BluetoothModel _bleModel;
        private GlucoseData _glucoseData;


        private double slope = 1.05;
        private double intercept = -30;

        public BluetoothModel bleModel
        {
            set{SetProperty(ref _bleModel, value); }
            get { return _bleModel; }
        }

        public GlucoseData glucoseDataModel
        {
            set { SetProperty(ref _glucoseData, value); }
            get { return _glucoseData; }
        }

        public BluetoothViewModel()
        {
            _bleAdapter = CrossBluetoothLE.Current.Adapter;
            glucoseDataModel = new GlucoseData();
            bleModel = new BluetoothModel() { bleStatus = "", devicesCount=0, devicesList = new ObservableCollection<IDevice>()};

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
                    if (bytes[0] != 0x28 && bytes[bytes.Length - 1] != 0x29)
                    {
                        dataMiaoMiao.AddRange(bytes);
                    }
                    else
                    {
                        dataMiaoMiao.AddRange(bytes);
                        _PackageMounted();
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private void _PackageMounted()
        {
            //var packetLenght = StructConverter.Unpack(">h", dataMiaoMiao.GetRange(1, 2).ToArray())[0];
            //string sensorId = BitConverter.ToString(dataMiaoMiao.GetRange(3, 9).ToArray());
            bleModel.batteryDevice = dataMiaoMiao[13];
            //var Firmeware = StructConverter.Unpack(">h", dataMiaoMiao.GetRange(14, 2).ToArray())[0];
            //var Hardware = StructConverter.Unpack(">h", dataMiaoMiao.GetRange(16, 2).ToArray())[0];

            historyList.Clear();
            trendList.Clear();
            List<byte> payload = dataMiaoMiao.GetRange(18, 343);
            


            int trendIndex = payload[26] & 0xFF;
            int historyIndex = payload[27] & 0xFF;
            int sensorTime = 256 * (payload[317] & 0xFF) + (payload[316] & 0xFF);
            long sensorStartTime = TimeUtils.CurrentTimeMillis() - sensorTime * 60000;

            
            // loads history values (ring buffer, starting at index_trent. byte 124-315)
            for (int index = 0; index < 32; index++)
            {
                GlucoseData glucoseData = new GlucoseData();
                int i = historyIndex - index - 1;
                if (i < 0) i += 32;

                glucoseData.glucoseLevelRaw = getGlucoseRaw(new byte[] { payload[(i * 6 + 125)], payload[(i * 6 + 124)] }, true);
                glucoseData.flags = readBits(payload.ToArray(), i * 6 + 124, 0xe, 0xc);
                glucoseData.temp = readBits(payload.ToArray(), i * 6 + 124, 0x1a, 0xc);

                int time = Math.Max(0, Math.Abs((sensorTime - 3) / 15) * 15 - index * 15);
                glucoseData.realDateMilliseconds = sensorStartTime + time * 60000;
                glucoseData.sensorTime = time;

                DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                glucoseData.realDate = start.AddMilliseconds(glucoseData.realDateMilliseconds).ToLocalTime();

                glucoseData.glucoseRaw = glucoseData.glucoseLevelRaw * (117.64705 / 1000);
                glucoseData.glucose = glucoseDataModel.slope * glucoseData.glucoseRaw + glucoseDataModel.intercept; 
                historyList.Add(glucoseData);
            }

            // loads trend values (ring buffer, starting at index_trent. byte 28-123)
            for (int index = 0; index < 16; index++)
            {
                GlucoseData glucoseData = new GlucoseData();
                int i = trendIndex - index - 1;
                if (i < 0) i += 16;

                glucoseData.glucoseLevelRaw = getGlucoseRaw(new byte[] { payload[(i * 6 + 29)], payload[(i * 6 + 28)] }, true);
                int time = Math.Max(0, sensorTime - index);

                glucoseData.realDateMilliseconds = sensorStartTime + time * 60000;
                glucoseData.sensorTime = time;

                DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                glucoseData.realDate = start.AddMilliseconds(glucoseData.realDateMilliseconds).ToLocalTime();

                glucoseData.glucoseRaw = glucoseData.glucoseLevelRaw * (117.64 / 1000);
                glucoseData.glucose = glucoseDataModel.slope * glucoseData.glucoseRaw + glucoseDataModel.intercept;
                trendList.Add(glucoseData);

                //Console.WriteLine($"TREND idx: { index }  | Glucose: {glucoseData.glucoseTest}  --- Time: {glucoseData.realDateTest.ToString("dd / MM / yyyy HH: mm")}");
            }

            glucoseDataModel.realDate = trendList[0].realDate;
            glucoseDataModel.glucoseRaw = trendList[0].glucoseRaw;
            glucoseDataModel.glucose = trendList[0].glucose;
        }




        public static String bytesToHex(byte[] bytes)
        {
            char[] hexArray = "0123456789ABCDEF".ToCharArray();
            if (bytes == null) return "<empty>";
            char[] hexChars = new char[bytes.Count() * 2];
            for (int j = 0; j < bytes.Count(); j++)
            {
                int v = bytes[j] & 0xFF;
                hexChars[j * 2] = hexArray[v >> 4];
                hexChars[j * 2 + 1] = hexArray[v & 0x0F];
            }
            return new String(hexChars);
        }
        public static int readBits(byte[] buffer, int byteOffset, int bitOffset, int bitCount)
        {
            if (bitCount == 0)
            {
                return 0;
            }
            int res = 0;
            for (int i = 0; i < bitCount; i++)
            {
                int totalBitOffset = byteOffset * 8 + bitOffset + i;
                int byte1 = (int)(totalBitOffset / 8);
                int bit = totalBitOffset % 8;
                if (totalBitOffset >= 0 && ((buffer[byte1] >> bit) & 0x1) == 1)
                {
                    res = res | (1 << i);
                }
            }
            return res;
        }
        private static int getGlucoseRaw(byte[] bytes, bool thirteen)
        {
            if (thirteen)
            {
                return ((256 * (bytes[0] & 0xFF) + (bytes[1] & 0xFF)) & 0x1FFF);
            }
            else
            {
                return ((256 * (bytes[0] & 0xFF) + (bytes[1] & 0xFF)) & 0x0FFF);
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
