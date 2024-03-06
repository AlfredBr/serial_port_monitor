using Iot.Device.Ft232H;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System.Device.Gpio;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace IOTLib;

/// <summary>
/// Service for handling GPIO operations.
/// </summary>
public class GPIOService : BackgroundService
{
	private readonly ILogger<GPIOService> _logger;
	private readonly GPIOConfig _gpioConfig;
	private readonly Dictionary<int, EventHandler<GPIOEventArgs>> _pinHandlers;
	private readonly Dictionary<int, bool> _pinStates;

	/// <summary>
	/// Gets or sets the delay between GPIO operations in milliseconds.
	/// </summary>
	public int Delay { get; set; } = 1;

	/// <summary>
	/// Initializes a new instance of the <see cref="GPIOService"/> class.
	/// </summary>
	/// <param name="logger">The logger.</param>
	/// <param name="configOptions">The GPIO configuration options.</param>
	public GPIOService(ILogger<GPIOService> logger, IOptions<GPIOConfig> configOptions)
	{
		_logger = logger;
		_gpioConfig = configOptions.Value;
		_pinHandlers = new Dictionary<int, EventHandler<GPIOEventArgs>>();
		_pinStates = new Dictionary<int, bool>();

		foreach (var handler in _gpioConfig.PinHandlers)
		{
			RegisterPin(handler.Key, handler.Value);
		}
	}

	/// <summary>
	/// Registers a pin with the specified handler.
	/// </summary>
	/// <param name="pin">The pin number.</param>
	/// <param name="handler">The event handler for the pin.</param>
	private void RegisterPin(int pin, EventHandler<GPIOEventArgs> handler)
	{
		_pinStates.Add(pin, false);
		_pinHandlers.Add(pin, handler);
	}

	/// <summary>
	/// Executes the GPIO service asynchronously.
	/// </summary>
	/// <param name="stoppingToken">The cancellation token.</param>
	/// <returns>A task representing the asynchronous operation.</returns>
	protected async override Task ExecuteAsync(CancellationToken stoppingToken)
	{
		_logger.LogInformation("Running GPIOService at: {time}", DateTimeOffset.Now);

		var gpio = new GPIO();
		var gpioController = gpio.CreateController();
		var pins = _pinStates.Keys;

		while (!stoppingToken.IsCancellationRequested)
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
			await Task.Delay(Delay, stoppingToken);
		}
	}
}
