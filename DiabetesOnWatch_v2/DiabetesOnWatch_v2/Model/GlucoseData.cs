using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace DiabetesOnWatch_v2.Model
{
    public class GlucoseData : INotifyPropertyChanged
    {

        private double _glucose;
        private double _glucoseRaw;
        private double _slope;
        private double _intercept;
        private DateTime _realDate;

        public event PropertyChangedEventHandler PropertyChanged;

        public int glucoseLevelRaw { get; set; }

        public double glucoseRaw
        {
            set { SetProperty(ref _glucoseRaw, value); }
            get { return _glucoseRaw; }
        }

        public long sensorTime { get; set; }

        public string sensorId { get; set; }

        public long realDateMilliseconds { get; set; }


        public double slope
        {
            set { SetProperty(ref _slope, value); }
            get { return _slope; }
        }

        public double intercept
        {
            set { SetProperty(ref _intercept, value); }
            get { return _intercept; }
        }

        public DateTime realDate
        {
            set { SetProperty(ref _realDate, value,"realDateString"); }
            get { return _realDate; }
        }

        public string realDateString
        {
            get { return _realDate.ToString("dd / MM / yyyy HH: mm"); }
        }
        public int flags { get; set; }

        public int temp { get; set; }

        public double glucose
        {
            set { SetProperty(ref _glucose, value); }
            get { return _glucose; }
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
