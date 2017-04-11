using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;
using Windows.System.Threading;
using System.Threading;
using System.Diagnostics;
using Windows.Devices.Pwm;
using Microsoft.IoT.Lightning.Providers;
using Windows.Devices;
using Windows.Devices.I2c;
//using PwmSoftware;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace IoTRaspberryPi
{
    public sealed class StartupTask : IBackgroundTask
    {
        BackgroundTaskDeferral deferral;

        PCF8591 ADConverter { get; set; }
        Laser laser;
        Button button;
        RGBLed rgbLed;

        private const int BUZZER_PIN = 24;


        private Timer constantTimer;

        double ClockwisePulseLength = 1;
        double CounterClockwisePulseLegnth = 2;
        double RestingPulseLegnth = 0;
        double currentPulseLength = 0;
        double secondPulseLength = 0;
        int iteration = 0;
        PwmPin motorPin;
        PwmController pwmController;



        private const int LED_PIN = 25;
        private GpioPin pin = null;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            // set deferral to keep the Application running at the end of the method.
            deferral = taskInstance.GetDeferral();

            // Set the Lightning Provider as the default if Lightning driver is enabled on the target device
            // Otherwise, the inbox provider will continue to be the default
            if (LightningProvider.IsLightningEnabled)
            {
                // Set Lightning as the default provider
                LowLevelDevicesController.DefaultProvider = LightningProvider.GetAggregateProvider();
                Debug.WriteLine("GPIO Using Lightning Provider");
            }
            else
            {
                Debug.WriteLine("GPIO Using Default Provider");
            }

            var gpioController = GpioController.GetDefault(); /* Get the default GPIO controller on the system */
            if (gpioController == null)
            {
                Debug.WriteLine("No GPIO Controller found!");
            }

            InitGPIO();
        }


        private ThreadPoolTimer timer;
        I2cDevice sensor;


        private async void InitGPIO()
        {
            // creates a PCF8591 instance
            laser = new Laser();
            button = new Button();
            rgbLed = new RGBLed();
            //ADConverter = await PCF8591.Create();

            // set timer to be executed every 500ms.
            //var timer = new System.Threading.Timer(Timer_Tick_ADConverter, null, 500, 3000);
            //Timer_Tick_ADConverter(new object());

            button.ValueChanged += Button_ValueChanged;



            // The code below should work the same with any provider, including Lightning and the default one.
            //I2cController controller = await I2cController.GetDefaultAsync();
            I2cController controller = (await I2cController.GetControllersAsync(LightningI2cProvider.GetI2cProvider()))[0];
            // Ox40 was determined by looking at the datasheet for the Weather shield

            sensor = controller.GetDevice(new I2cConnectionSettings(0x90 >> 1));

            timer = ThreadPoolTimer.CreatePeriodicTimer(Timer_Tick, TimeSpan.FromMilliseconds(1000));
            //Timer_Tick();


            /*
            timer2 = ThreadPoolTimer.CreatePeriodicTimer(Timer_Tick2, TimeSpan.FromSeconds(2));
            pwmController = (await PwmController.GetControllersAsync(PwmProviderSoftware.GetPwmProvider()))[0];
            pwmController.SetDesiredFrequency(50);
            motorPin = pwmController.OpenPin(24);
            motorPin.SetActiveDutyCyclePercentage(RestingPulseLegnth);
            motorPin.Start();
            */
        }


        private void Timer_Tick(ThreadPoolTimer timer)
        {
            int y = ReadI2CAnalog(PCF8591_AnalogPin.A0);
            int x = ReadI2CAnalog(PCF8591_AnalogPin.A1);
            int btn = ReadI2CAnalog(PCF8591_AnalogPin.A2);
            int pot = ReadI2CAnalog(PCF8591_AnalogPin.A3);
            Debug.WriteLine("Y: {0} |X: {1} |Btn: {2} |Pot: {3} ", y, x, btn, pot);
        }

        /// <summary>
        /// Returns an int value from 0 to 255 (included).
        /// </summary>
        /// <param name="InputPin">The Input pin on the PCF8591 to read analog value from</param>
        /// <returns>int</returns>
        private int ReadI2CAnalog(PCF8591_AnalogPin InputPin)
        {
            try
            {
                byte[] b = new byte[2];
                sensor.WriteRead(new byte[] { (byte)InputPin }, b);
                return b[1];
            } catch (Exception e)
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

        private void Timer_Tick_ADConverter(object sender)
        {
            try
            {
                // reads the analog value from pin A0.
                double y = ADConverter.ReadI2CAnalog_AsDouble(PCF8591_AnalogPin.A0);
                double x = ADConverter.ReadI2CAnalog_AsDouble(PCF8591_AnalogPin.A1);
                double bt = ADConverter.ReadI2CAnalog_AsDouble(PCF8591_AnalogPin.A2);
                double potenciomenter = ADConverter.ReadI2CAnalog_AsDouble(PCF8591_AnalogPin.A3);

                // shows value in console
                System.Diagnostics.Debug.WriteLine("Y: {0} - X: {1} - Bt: {2} - P: {3}", y, x, bt, potenciomenter);
            }
            catch (Exception ex)
            {
                // dispose the PCF8591.
                ADConverter.Dispose();
                // Terminates the application.
                deferral.Complete();
            }
        }


        private void Timer_Tick2(ThreadPoolTimer timer)
        {
            iteration++;
            if (iteration % 3 == 0)
            {
                currentPulseLength = ClockwisePulseLength;
                secondPulseLength = CounterClockwisePulseLegnth;
            }
            else if (iteration % 3 == 1)
            {
                currentPulseLength = CounterClockwisePulseLegnth;
                secondPulseLength = ClockwisePulseLength;
            }
            else
            {
                currentPulseLength = 0;
                secondPulseLength = 0;
            }

            //double desiredPercentage = currentPulseLength / (1000.0 / pwmController.ActualFrequency);
            //motorPin.SetActiveDutyCyclePercentage(desiredPercentage);
            //double secondDesiredPercentage = secondPulseLength / (1000.0 / pwmController.ActualFrequency);
            //secondMotorPin.SetActiveDutyCyclePercentage(secondDesiredPercentage);
            rgbLed.SetRGBColor(0, 0, 0);
        }

        private void StartPWM()
        {
            //soft PWM loop, runs all the time
            while (true)
            {
                //VER!!!!
                /*
                buzzer.Write(GpioPinValue.High);
                NOP(5000);
                buzzer.Write(GpioPinValue.Low);
                NOP(5000);
                */
            }
        }

        private void NOP(int ticks)
        {
            while (ticks >= 0)
            {
                ticks--;
            }
        }

        //private void Button_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
        private void Button_ValueChanged(Button sender, PushStatus pushStatus, GpioPinValueChangedEventArgs e)
        {
            // toggle the state of the LED every time the button is pressed
            if (pushStatus == PushStatus.DOWN)
            {
                //RandomGpioPinValue(pins[i]);
                Debug.WriteLine("Button Pressed");
                laser.TurnLaserOn();
                constantTimer = new Timer(rgbLed.ConstantChange, null, 0, 5);
            }
            else
            {
                Debug.WriteLine("Button Released");
                laser.TurnLaserOff();
                rgbLed.TurnOff();
                //Stop timer and LED
                try
                {
                    constantTimer.Dispose();
                }
                catch
                {

                }
            }
        }
    }
}
