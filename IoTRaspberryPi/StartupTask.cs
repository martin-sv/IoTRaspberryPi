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
//using PwmSoftware;
//using Microsoft.IoT.Lightning.Providers;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace IoTRaspberryPi
{
    public sealed class StartupTask : IBackgroundTask
    {
        BackgroundTaskDeferral deferral;

        PCF8591 ADConverter { get; set; }
        Laser laser = new Laser();
        Button button = new Button();
        RGBLed rgbLed = new RGBLed();

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

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            // set deferral to keep the Application running at the end of the method.
            deferral = taskInstance.GetDeferral();
            InitGPIO();
        }





        private async void InitGPIO()
        {
            // creates a PCF8591 instance
            ADConverter = await PCF8591.Create();
            // set timer to be executed every 500ms.
            var timer = new System.Threading.Timer(Timer_Tick_ADConverter, null, 500, 300);

            button.ValueChanged += Button_ValueChanged;


            /*
            timer2 = ThreadPoolTimer.CreatePeriodicTimer(Timer_Tick2, TimeSpan.FromSeconds(2));
            pwmController = (await PwmController.GetControllersAsync(PwmProviderSoftware.GetPwmProvider()))[0];
            pwmController.SetDesiredFrequency(50);
            motorPin = pwmController.OpenPin(24);
            motorPin.SetActiveDutyCyclePercentage(RestingPulseLegnth);
            motorPin.Start();
            */
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
