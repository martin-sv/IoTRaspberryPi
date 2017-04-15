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

        Laser laser;
        Button button;
        RGBLed rgbLed;
        FCP8591Lightning ADConverter;
        LCDDisplayI2C lcd;
        Buzzer buzzer;
        WebServer webServer;

        private ThreadPoolTimer timer2;
        private Timer LedTimer;

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

        private void InitGPIO()
        {
            // creates a PCF8591 instance
            laser = new Laser();
            button = new Button();
            rgbLed = new RGBLed();
            //ADConverter = new FCP8591Lightning();
            lcd = new LCDDisplayI2C(0x27, "I2C1", 0, 1, 2, 4, 5, 6, 7, 3);
            lcd.init();
            lcd.Print2Lines("01234567890123456789012345678901234567890");
            buzzer = new Buzzer();

            webServer = new WebServer();

            //OnButtonClick
            button.ValueChanged += Button_ValueChanged;

            //OnNewMessageFromWebToLCD
            webServer.NewMessage += WebServer_NewMessage;

            //Read ADConverter (Stick + Potenciomenter)
            //timer = ThreadPoolTimer.CreatePeriodicTimer(ADConverter_Timer_Tick, TimeSpan.FromMilliseconds(1000));

            //Buzzer Timer
            //timer2 = ThreadPoolTimer.CreatePeriodicTimer(Timer_Tick3, TimeSpan.FromMilliseconds(500));

            txt = new string[4,2];
            txt[0,0] = "Hola Romi";
            txt[1,0] = "Te AMO!";
            txt[2,0] = "Queres ir al";
            txt[2,1] = "Cine Conmigo?";
            txt[3,0] = "El Jueves";

            webServer.StartServer();
        }

        private void WebServer_NewMessage(WebServer sender, string message)
        {
            lcd.clrscr();
            lcd.Print2Lines(message);
            buzzer.Start();
        }

        string[,] txt;
        int time = 0;
        /// <summary>
        /// OnButtonClick
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="pushStatus"></param>
        /// <param name="e"></param>
        private void Button_ValueChanged(Button sender, PushStatus pushStatus, GpioPinValueChangedEventArgs e)
        {
            // toggle the state of the LED every time the button is pressed
            if (pushStatus == PushStatus.DOWN)
            {
                //RandomGpioPinValue(pins[i]);
                Debug.WriteLine("Button Pressed");
                laser.TurnLaserOn();
                LedTimer = new Timer(rgbLed.ConstantChange, null, 0, 5);
                if (time < txt.Length / txt.Rank)
                {
                    lcd.clrscr();
                    lcd.Print2Lines(txt[time,0],txt[time,1]);
                    time++;
                } else
                {
                    lcd.clrscr();
                }
                buzzer.Stop();
            }
            else
            {
                Debug.WriteLine("Button Released");
                laser.TurnLaserOff();
                rgbLed.TurnOff();
                //Stop timer and LED
                try
                {
                    LedTimer.Dispose();
                }
                catch
                {

                }
            }
        }

        private void ADConverter_Timer_Tick(ThreadPoolTimer timer)
        {
            try
            {
                double y = ADConverter.ReadI2CAnalog_AsDouble(FCP8591Lightning.PCF8591_AnalogPin.A0);
                double x = ADConverter.ReadI2CAnalog_AsDouble(FCP8591Lightning.PCF8591_AnalogPin.A1);
                double btn = ADConverter.ReadI2CAnalog_AsDouble(FCP8591Lightning.PCF8591_AnalogPin.A2);
                double pot = ADConverter.ReadI2CAnalog_AsDouble(FCP8591Lightning.PCF8591_AnalogPin.A3);
                Debug.WriteLine("Y: {0} |X: {1} |Btn: {2} |Pot: {3} ", y, x, btn, pot);

                double y1 = ADConverter.ReadI2CAnalog_AsDouble_2Decimal(FCP8591Lightning.PCF8591_AnalogPin.A0);
                double x1 = ADConverter.ReadI2CAnalog_AsDouble_2Decimal(FCP8591Lightning.PCF8591_AnalogPin.A1);
                double btn1 = ADConverter.ReadI2CAnalog_AsDouble_2Decimal(FCP8591Lightning.PCF8591_AnalogPin.A2);
                double pot1 = ADConverter.ReadI2CAnalog_AsDouble_2Decimal(FCP8591Lightning.PCF8591_AnalogPin.A3);
                Debug.WriteLine("Y: {0} |X: {1} |Btn: {2} |Pot: {3} ", y1, x1, btn1, pot1);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                // dispose the PCF8591.
                ADConverter.Dispose();
                // Terminates the application.
                deferral.Complete();
            }
        }

        private void Timer_Tick3(ThreadPoolTimer timer)
        {
            /*
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
            */
            //double desiredPercentage = currentPulseLength / (1000.0 / pwmController.ActualFrequency);
            //motorPin.SetActiveDutyCyclePercentage(desiredPercentage);
        }
    }
}
