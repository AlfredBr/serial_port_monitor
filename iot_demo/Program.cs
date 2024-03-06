using Iot.Device.Ft232H;
using System.Device.Gpio;
using System.Device.Spi;
using System.Device.I2c;
using System.Diagnostics;
using System.Text;
using IOTLib;

namespace iot_demo;

internal partial class Program
{
    static async Task Main(string[] args)
    {
        //BuiltInSPIDemo();
        //BitBangSPIDemo();
        //BuiltInI2CDemo();
        //BlinkLED();
        //Serial7Segment();
        //ButtonPress();
        //FrequencyCounter();
        await OneEventPerPulse();
    }

    private static void BuiltInI2CDemo()
    {
        var devices = Ft232HDevice.GetFt232H();
        Debug.Assert(devices != null, "No devices found.");
        Debug.Assert(devices.Any(), "Device list is empty.");
        var device = new Ft232HDevice(devices[0]);
        Debug.Assert(device != null, "Device initialization failed.");

        // I2C device address and bus ID
        var i2cConnectionSettings = new I2cConnectionSettings(busId: 0, deviceAddress: 0x3C);
        var i2cDevice = device.CreateI2cDevice(i2cConnectionSettings);
        Debug.Assert(i2cDevice != null, "I2C device creation failed.");

        // Sending commands and data to the I2C device
        byte[] command = new byte[] { 0x00, 0xAE }; // Example command (e.g., display off for an OLED screen)
        i2cDevice.Write(command);

        for (int i = 0; i < 0xFF; i++)
        {
            Console.WriteLine(i);
            byte[] data = new byte[] { 0x40, (byte)i }; // Example data (e.g., a byte to display on an OLED screen)
            i2cDevice.Write(data);
            Thread.Sleep(10);
        }
    }

	/// <summary>
	/// Demonstrates the Bit Bang SPI functionality.
	/// </summary>
    private static void BitBangSPIDemo()
    {
        var spi = new SPI(clk: 7, mosi: 6, miso: -1, cs: 5, delay: 10);

        ReadOnlySpan<byte> clearDisplay = stackalloc byte[] { (byte)'\x76' };
        spi.Send(clearDisplay[0]);

        byte[] msg = Encoding.ASCII.GetBytes("9876");

        while (true)
        {
            foreach (byte b in msg)
            {
                spi.Send(b);
            }
        }
    }

    private static void BuiltInSPIDemo()
    {
        var devices = Ft232HDevice.GetFt232H();
        Debug.Assert(devices != null, "No devices found.");
        Debug.Assert(devices.Any(), "Device list is empty.");
        var device = new Ft232HDevice(devices[0]);
        Debug.Assert(device != null, "Device initialization failed.");

        var spiConnectionSettings = new SpiConnectionSettings(0, 3)
        {
            ClockFrequency = 1200,
            Mode = SpiMode.Mode0,
            DataFlow = DataFlow.MsbFirst,
            DataBitLength = 8,
            ChipSelectLineActiveState = PinValue.Low
        };

        var spiDevice = device.CreateSpiDevice(spiConnectionSettings);
        Debug.Assert(spiDevice != null, "SPI device creation failed.");

        ReadOnlySpan<byte> setBrightness = stackalloc byte[] { (byte)'\x7A', (byte)'\x1F' };
        spiDevice.Write(setBrightness);
        ReadOnlySpan<byte> clearDisplay = stackalloc byte[] { (byte)'\x76' };
        spiDevice.Write(clearDisplay);

        //byte[] msg = Encoding.ASCII.GetBytes("1234");

        while (true)
        {
            spiDevice.Write(clearDisplay);
            var msg = Encoding.ASCII.GetBytes(DateTime.Now.ToString("mmss"));
            foreach (byte b in msg)
            {
                spiDevice.WriteByte(b);
            }
            Thread.Sleep(1000);
        }
    }

    private static void BlinkLED()
    {
        var devices = Ft232HDevice.GetFt232H();
        Debug.Assert(devices != null, "No devices found.");
        Debug.Assert(devices.Any(), "Device list is empty.");
        var device = new Ft232HDevice(devices[0]);
        Debug.Assert(device != null, "Device initialization failed.");
        var gpioController = device.CreateGpioController();
        Debug.Assert(gpioController != null, "Failed to create GPIO controller.");
        Debug.Assert(gpioController.PinCount > 0, "Controller pin count is Zero.");
        var pin = gpioController.OpenPin(7);
        Debug.Assert(pin != null, "Failed to open pin.");
        pin.SetPinMode(PinMode.Output);
        while (true)
        {
            pin.Write(1);
            Thread.Sleep(200);
            pin.Write(0);
            Thread.Sleep(100);
        }
    }

    private static void Serial7Segment()
    {
        ReadOnlySpan<byte> ros(string str) => new ReadOnlySpan<byte>(Encoding.ASCII.GetBytes(str));
        var devices = Ft232HDevice.GetFt232H();
        Debug.Assert(devices != null, "No devices found.");
        Debug.Assert(devices.Any(), "Device list is empty.");
        var device = new Ft232HDevice(devices[0]);
        Debug.Assert(device != null, "Device initialization failed.");
        var spiConnectionSettings = new SpiConnectionSettings(0, 3)
        {
            ClockFrequency = 1200,
            Mode = SpiMode.Mode0,
            DataFlow = DataFlow.MsbFirst,
            DataBitLength = 8,
            ChipSelectLineActiveState = PinValue.Low
        };
        var spiDevice = device.CreateSpiDevice(spiConnectionSettings);
        Debug.Assert(spiDevice != null, "SPI device creation failed.");

        ReadOnlySpan<byte> clearDisplay = stackalloc byte[] { (byte)'\x76' };
        ReadOnlySpan<byte> setBrightness = stackalloc byte[] { (byte)'\x7A', (byte)'\x1F' };
        // for (int x = 0; x < 10; x++)
        // {
        // 	spiDevice.WriteByte((byte)0b00010001);
        // 	Thread.Sleep(100);
        // }
        spiDevice.Write(clearDisplay);
        spiDevice.Write(setBrightness);
        //spiDevice.Write(ros("1234"));

        int n = 0;
        while (true)
        {
            spiDevice.Write(ros(n.ToString("D4")));
            n++;
            Thread.Sleep(1000);
        }
    }

    private static void ButtonPress()
    {
        var gpio = new GPIO();
        var gpioController = gpio.CreateController();

        var pin = gpioController.OpenPin(8);
        Debug.Assert(pin != null, "Failed to open pin.");
        pin.SetPinMode(PinMode.InputPullUp);

        while (true)
        {
            Console.WriteLine($"Pin value: {pin.Read()}");
            Thread.Sleep(100);
        }
    }

    private static void FrequencyCounter()
    {
        var gpio = new GPIO();
        var gpioController = gpio.CreateController();

        var inputPin = gpioController.OpenPin(6); // D6
        Debug.Assert(inputPin != null, "Failed to open inputPin.");
        inputPin.SetPinMode(PinMode.Input);

        var outputPin = gpioController.OpenPin(7); // D7
        Debug.Assert(outputPin != null, "Failed to open outputPin.");
        outputPin.SetPinMode(PinMode.Output);

        Stopwatch stopwatch = new Stopwatch();
        var isPulseStart = false;

        while (true)
        {
            var input = inputPin.Read();
            outputPin.Write(input);

            if (input == 1 && !isPulseStart)
            {
                stopwatch.Start();
                isPulseStart = true;
            }
            else if (input == 0 && isPulseStart)
            {
                stopwatch.Stop();
                isPulseStart = false;
                Console.WriteLine($"Frequency: {(1 / (stopwatch.Elapsed.TotalSeconds * 2d)):N2}Hz, ET: {stopwatch.ElapsedMilliseconds}ms");
                stopwatch.Reset();
            }
        }
    }

    public static async Task OneEventPerPulse()
    {
        var gpio = new GPIO();
        var gpioController = gpio.CreateController();

        var outputPin = gpioController.OpenPin(GPIOPin.D7);
        Debug.Assert(outputPin != null, "Failed to open outputPin.");
        outputPin.SetPinMode(PinMode.Output);

        gpio.RegisterPin(GPIOPin.D6, (o, e) => {
            Console.WriteLine($"{e}");
            outputPin.Write(e.State);
        });
        _ = gpio.ScanPinsAsync();
        Console.WriteLine("Okay");
        Console.ReadLine();
    }
}