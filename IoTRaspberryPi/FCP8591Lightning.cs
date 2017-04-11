using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.I2c;
using Microsoft.IoT.Lightning.Providers;
using System.Diagnostics;

namespace IoTRaspberryPi
{
    class FCP8591Lightning
    {
        /// <summary>
        /// The address of the device is 0x90, coded on 7 bits.
        /// To get the matching value om 8 bits, shift (>>) the bits by 1.
        /// </summary>
        private const byte addr_PCF8591 = (0x90 >> 1);
        private I2cDevice sensor;

        public FCP8591Lightning()
        {
            Init();
        }

        private async void Init()
        {
            // The code below should work the same with any provider, including Lightning and the default one.
            //I2cController controller = await I2cController.GetDefaultAsync();
            I2cController controller = (await I2cController.GetControllersAsync(LightningI2cProvider.GetI2cProvider()))[0];

            sensor = controller.GetDevice(new I2cConnectionSettings(0x90 >> 1));
        }




        /// <summary>
        /// Returns an int value from 0 to 255 (included).
        /// </summary>
        /// <param name="InputPin">The Input pin on the PCF8591 to read analog value from</param>
        /// <returns>int</returns>
        public int ReadI2CAnalog(PCF8591_AnalogPin InputPin)
        {
            try
            {
                byte[] b = new byte[2];
                sensor.WriteRead(new byte[] { (byte)InputPin }, b);
                return b[1];
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return -1;
            }
        }

        /// <summary>
        /// Returns an double value from 0 to 1.
        /// </summary>
        /// <param name="InputPin">The Input pin on the PCF8591 to read analog value from</param>
        /// <returns>double</returns>
        public double ReadI2CAnalog_AsDouble(PCF8591_AnalogPin InputPin)
        {
            return ReadI2CAnalog(InputPin) / 255d;
        }

        /// <summary>
        /// Returns an double value from 0 to 1.
        /// </summary>
        /// <param name="InputPin">The Input pin on the PCF8591 to read analog value from</param>
        /// <returns>double</returns>
        public double ReadI2CAnalog_AsDouble_2Decimal(PCF8591_AnalogPin InputPin)
        {
            return (Math.Round(ReadI2CAnalog_AsDouble(InputPin) * 100) / 100);
        }

        public void Dispose()
        {
            this.sensor.Dispose();
        }

        /// <summary>
        /// The 4 available analog input pins on the PCF8591.
        /// Value defined by their internal address.
        /// </summary>
        public enum PCF8591_AnalogPin
        {
            A0 = 0x40,
            A1 = 0x41,
            A2 = 0x42,
            A3 = 0x43
        }
    }
}
