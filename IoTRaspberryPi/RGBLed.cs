using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace IoTRaspberryPi
{
    class RGBLed
    {
        private int[] default_pins = new int[3] { 17, 18, 27 };
        private int[] pins = new int[3];
        private GpioPin[] leds = new GpioPin[3];

        Random r = new Random();

        public RGBLed(int[] pins)
        {
            Init(pins);
        }

        public RGBLed()
        {
            Init(default_pins);
        }

        private void Init(int[] pins)
        {
            this.pins = pins;
            //Turn off pins
            for (int i = 0; i < pins.Length; i++)
            {
                leds[i] = GpioController.GetDefault().OpenPin(pins[i]);
                leds[i].Write(GpioPinValue.High);
                leds[i].SetDriveMode(GpioPinDriveMode.Output);
            }
        }

        public void SetRGBColor(byte r, byte g, byte b)
        {

        }
        //TODO Remove obj
        public void ConstantChange(object obj)
        {
            for (int i = 0; i < pins.Length; i++)
            {
                RandomRGBColor(leds[i]);
            }
        }

        private void RandomRGBColor(GpioPin led)
        {
            /*
            if (r.Next(2) == 0)
                led.Write(GpioPinValue.High);
            else
                led.Write(GpioPinValue.Low);
            */
            led.Write(GpioPinValue.Low);
        }

        public void TurnOff()
        {
            for (int i = 0; i < pins.Length; i++)
            {
                leds[i].Write(GpioPinValue.High);
            }
        }
    }
}
