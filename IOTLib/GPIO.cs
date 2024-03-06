using Iot.Device.Ft232H;

using System.Device.Gpio;
using System.Diagnostics;

namespace IOTLib;

/// <summary>
/// Represents a GPIO (General Purpose Input/Output) controller.
/// </summary>
public class GPIO
{
	private static GpioController? _gpioController = null;
	private readonly Dictionary<int, EventHandler<GPIOEventArgs>> _pinHandlers;
	private readonly Dictionary<int, bool> _pinStates;

	/// <summary>
	/// Initializes a new instance of the GPIO class.
	/// </summary>
	public GPIO()
	{
		_pinHandlers = new Dictionary<int, EventHandler<GPIOEventArgs>>();
		_pinStates = new Dictionary<int, bool>();
	}

	/// <summary>
	/// Creates a GPIO controller.
	/// </summary>
	/// <returns>The created GPIO controller.</returns>
	public GpioController CreateController()
	{
		if (_gpioController != null)
		{
			return _gpioController;
		}

		var devices = Ft232HDevice.GetFt232H();
		Debug.Assert(devices != null, "No devices found.");
		Debug.Assert(devices.Any(), "Device list is empty.");
		var device = new Ft232HDevice(devices[0]);
		Debug.Assert(device != null, "Device initialization failed.");

		_gpioController = device.CreateGpioController();
		Debug.Assert(_gpioController != null, "Failed to create GPIO controller.");
		Debug.Assert(_gpioController.PinCount > 0, "Controller pin count is Zero.");
		return _gpioController;
	}

	/// <summary>
	/// Registers a pin with the specified handler.
	/// </summary>
	/// <param name="pin">The pin number.</param>
	/// <param name="handler">The event handler for the pin.</param>
	public void RegisterPin(int pin, EventHandler<GPIOEventArgs> handler)
	{
		_pinStates.Add(pin, false);
		_pinHandlers.Add(pin, handler);
	}

	/// <summary>
	/// Scans the registered pins asynchronously.
	/// </summary>
	/// <param name="delay">The delay between scans in milliseconds.</param>
	public async Task ScanPinsAsync(int delay = 0)
	{
		var gpio = new GPIO();
		var gpioController = gpio.CreateController();
		var pins = _pinStates.Keys;

		while (true)
		{
			foreach (var pin in pins)
			{
				var inputPin = gpioController.OpenPin(pin);
				Debug.Assert(inputPin != null, $"Failed to open pin {pin}.");
				inputPin.SetPinMode(PinMode.Input);
				var pinValue = inputPin.Read();

				if ((pinValue == PinValue.High) && !_pinStates[pin])
				{
					_pinStates[pin] = true;
					_pinHandlers[pin].Invoke(this, new GPIOEventArgs(pin, _pinStates[pin]));
				}
				else if (pinValue == PinValue.Low && _pinStates[pin])
				{
					_pinStates[pin] = false;
					_pinHandlers[pin].Invoke(this, new GPIOEventArgs(pin, _pinStates[pin]));
				}
			}

			await Task.Delay(delay);
		}
	}
}
