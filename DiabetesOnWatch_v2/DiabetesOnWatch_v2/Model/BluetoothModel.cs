﻿using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace DiabetesOnWatch_v2.Model
{
    public class BluetoothModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _bleStatus;
        private int  _devicesCount;
        private double _glucoseLevel;
        private int _batteryDevice;
        private ObservableCollection<IDevice> _devicesList;
        private IDevice _deviceConnected;

 
        public ObservableCollection<IDevice> devicesList
        {
            set { SetProperty(ref _devicesList, value); }
            get { return _devicesList; }
        }

        public IDevice deviceConnected
        {
            set { SetProperty(ref _deviceConnected, value); }
            get { return _deviceConnected; }
        }

        public int devicesCount 
        {
            set { SetProperty(ref _devicesCount, value); }
            get { return _devicesCount; }
        }

        public string bleStatus
        {
            set { SetProperty(ref _bleStatus, value); }
            get { return _bleStatus; }
        }

        public int batteryDevice
        {
            set { SetProperty(ref _batteryDevice, value); }
            get { return _batteryDevice; }
        }

        bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Object.Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
