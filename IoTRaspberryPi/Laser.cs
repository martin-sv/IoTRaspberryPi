using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace IoTRaspberryPi
{
    class Laser
    {
        private const int default_pin = 25;
        private int pin = 25;
        private GpioPin laser;

        public Laser(int pin = default_pin)
        {
            this.pin = pin;

            //Set Laser Pin
            laser = GpioController.GetDefault().OpenPin(this.pin);
            //Turn Laser Off
            laser.Write(GpioPinValue.High);
            //Set as Output
            laser.SetDriveMode(GpioPinDriveMode.Output);
        }

        public void TurnLaserOn()
        {
            laser.Write(GpioPinValue.Low);
        }

        public void TurnLaserOff()
        {
            laser.Write(GpioPinValue.High);
        }

        public void SwitchLaserStatus()
        {
            if (laser.Read() == GpioPinValue.High)
                laser.Write(GpioPinValue.Low);
            else
                laser.Write(GpioPinValue.High);
        }

    }
}
