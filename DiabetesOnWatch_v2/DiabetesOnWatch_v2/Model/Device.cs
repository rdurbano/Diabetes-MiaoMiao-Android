using System;
using System.Collections.Generic;
using System.Text;

namespace DiabetesOnWatch_v2.Model
{
    public class Device
    {
        public Device(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}
