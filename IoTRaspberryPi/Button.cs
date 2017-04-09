using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace IoTRaspberryPi
{

    public enum PushStatus { UP, DOWN, INIT };

    class Button
    {

        public delegate void ButtonDownEventHandler(Button sender, GpioPinValueChangedEventArgs e);
        public event ButtonDownEventHandler ButtonDown;

        public delegate void ButtonUpEventHandler(Button sender, GpioPinValueChangedEventArgs e);
        public event ButtonUpEventHandler ButtonUp;


        // A delegate type for hooking up change notifications.
        public delegate void ChangedEventHandler(Button sender, PushStatus buttonStatus, GpioPinValueChangedEventArgs e);
        public event ChangedEventHandler ValueChanged;


        private const int default_pin = 23;
        private int pin;
        private GpioPin button;

        public PushStatus pushStatus = PushStatus.INIT;

        public Button(int pin = default_pin)
        {
            //this.pin = pin;
            this.pin = default_pin;

            //Set Button Pin
            button = GpioController.GetDefault().OpenPin(this.pin);

            // Check if input pull-up resistors are supported
            if (button.IsDriveModeSupported(GpioPinDriveMode.InputPullUp))
                button.SetDriveMode(GpioPinDriveMode.InputPullUp);
            else
                button.SetDriveMode(GpioPinDriveMode.Input);
            // Set a debounce timeout to filter out switch bounce noise from a button press
            button.DebounceTimeout = TimeSpan.FromMilliseconds(50);

            // Register for the ValueChanged event so our buttonPin_ValueChanged 
            // function is called when the button is pressed
            button.ValueChanged += Button_ValueChanged;
        }

        private void Button_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (args.Edge == GpioPinEdge.FallingEdge)
            {
                pushStatus = PushStatus.DOWN;
                ValueChanged?.Invoke(this, pushStatus, args);
                ButtonDown?.Invoke(this, args);
            }
            else
            {
                pushStatus = PushStatus.UP;
                ValueChanged?.Invoke(this, pushStatus, args);
                ButtonUp?.Invoke(this, args);
            }
        }
    }
}
