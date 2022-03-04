using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace DiabetesOnWatch_v2.Model
{
    public class GlucoseData : INotifyPropertyChanged
    {

        private double _glucoseTest;
        private DateTime _realDateTest;

        public event PropertyChangedEventHandler PropertyChanged;

        public int glucoseLevelRaw { get; set; }

        public int glucoseLevelRawSmoothed { get; set; }

        public long sensorTime { get; set; }

        public string sensorId { get; set; }

        public long realDate { get; set; }

        public DateTime realDateTest
        {
            set { SetProperty(ref _realDateTest, value); }
            get { return _realDateTest ; }
        }

        public string realDateTestString
        {
            get { return _realDateTest.ToString("dd / MM / yyyy HH: mm"); }
        }
        public int flags { get; set; }

        public int temp { get; set; }

        public double glucoseTest
        {
            set { SetProperty(ref _glucoseTest, value); }
            get { return _glucoseTest; }
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
