using Iot.Device.Ft232H;

using System.Device.Gpio;
using System.Diagnostics;

namespace IOTLib;

/// <summary>
/// Represents a SPI (Serial Peripheral Interface) communication class.
/// </summary>
public class SPI
{
	private readonly GpioPin? _pClock, _pMoSi, _pMiSo, _pChipSelect;
	private readonly int _delay;

	/// <summary>
	/// Initializes a new instance of the <see cref="SPI"/> class.
	/// </summary>
	/// <param name="clk">The clock pin number.</param>
	/// <param name="mosi">The MOSI (Master Out Slave In) pin number.</param>
	/// <param name="miso">The MISO (Master In Slave Out) pin number.</param>
	/// <param name="cs">The chip select pin number.</param>
	/// <param name="delay">The delay in milliseconds between clock transitions.</param>
	public SPI(int clk, int mosi, int miso, int cs, int delay = 10)
	{
		var devices = Ft232HDevice.GetFt232H();
		Debug.Assert(devices != null, "No devices found.");
		Debug.Assert(devices.Any(), "Device list is empty.");
		var device = new Ft232HDevice(devices[0]);
		Debug.Assert(device != null, "Device initialization failed.");

		var gpioController = device.CreateGpioController();
		Debug.Assert(gpioController != null, "Failed to create GPIO controller.");
		Debug.Assert(gpioController.PinCount > 0, "Controller pin count is Zero.");

		GpioPin? setPinMode(int pin, PinMode pinMode = PinMode.Output)
		{
			if (pin < 0 || pin >= gpioController.PinCount)
			{
				return null;
			}
			Debug.Assert(gpioController != null, "Failed to create GPIO controller.");
			GpioPin gpioPin = gpioController.OpenPin(pin);
			Debug.Assert(gpioPin != null, $"Failed to open pin {pin}.");
			gpioPin.SetPinMode(pinMode);
			gpioPin.Write(0);
			return gpioPin;
		}

		_delay = delay;

		_pClock = setPinMode(clk);
		Debug.Assert(_pClock != null, "Clock pin is null.");
		_pChipSelect = setPinMode(cs);
		Debug.Assert(_pChipSelect != null, "CS pin is null.");

		_pMoSi = setPinMode(mosi, PinMode.Output);
		Debug.Assert(_pMoSi != null, "MOSI pin is null.");
		_pMiSo = setPinMode(miso, PinMode.Input);
		Debug.Assert(_pMiSo != null, "MISO pin is null.");

		Thread.Sleep(10 * _delay);
	}

	/// <summary>
	/// Sends a byte of data over the SPI bus.
	/// </summary>
	/// <param name="data">The data to send.</param>
	public void Send(byte data)
	{
		Debug.Assert(_pClock != null, "Clock pin is null.");
		Debug.Assert(_pMoSi != null, "MOSI pin is null.");
		Debug.Assert(_pChipSelect != null, "CS pin is null.");

		_pChipSelect.Write(0);

		for (var i = 7; i >= 0; i--)
		{
			byte mask = (byte)(0x01 << i);
			bool bit = (data & mask) == mask;
			//Console.WriteLine($"#{i} = {(bit ? "1" : "0")}");
			_pMoSi.Write(bit ? 1 : 0);

			_pClock.Write(1);
			Thread.Sleep(_delay);
			_pClock.Write(0);
			Thread.Sleep(_delay);
		}

		_pChipSelect.Write(1);

		//Thread.Sleep(_delay);
		//Console.WriteLine("-----");
	}
}
