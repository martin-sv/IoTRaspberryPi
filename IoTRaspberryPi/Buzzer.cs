using Microsoft.IoT.Lightning.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Pwm;

namespace IoTRaspberryPi
{
    class Buzzer
    {
        private const int default_pin = 24;
        private const int default_frequency = 250;
        private const int default_activeDutyCyclePercentage = 0;
        private int pin = 24;
        private int currentFrequency;
        private int activeDutyCyclePercentage = 0;
        public bool ready = false;
        PwmPin motorPin;
        PwmController pwmController;

        public delegate void BuzzerReadyEventHandler(Buzzer sender);
        public event BuzzerReadyEventHandler BuzzerReady;

        public Buzzer(int pin = default_pin, int frequency = default_frequency, int activeDutyCyclePercentage = default_activeDutyCyclePercentage)
        {
            Init(pin, frequency, activeDutyCyclePercentage);
        }

        private async void Init(int pin, int frequency, int activeDutyCyclePercentage)
        {
            this.pin = pin;
            currentFrequency = frequency;
            this.activeDutyCyclePercentage = activeDutyCyclePercentage;

            var pwmControllers = await PwmController.GetControllersAsync(LightningPwmProvider.GetPwmProvider());
            pwmController = pwmControllers[1]; // use the on-device controller

            motorPin = pwmController.OpenPin(pin);
            pwmController.SetDesiredFrequency(currentFrequency); // try to match 50Hz
            motorPin.SetActiveDutyCyclePercentage(activeDutyCyclePercentage);
            motorPin.Start();
            ready = true;
            BuzzerReady?.Invoke(this);
        }

        public void Start()
        {
            if (ready)
                motorPin.SetActiveDutyCyclePercentage(1);
        }

        public void Stop()
        {
            if (ready)
                motorPin.SetActiveDutyCyclePercentage(0);
        }
    }
}
