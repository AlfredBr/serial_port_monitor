using FtdiSharp;
using FtdiSharp.Protocols;

namespace ftdisharp_demo;

internal static class Program
{
	static void Main(string[] args)
	{
		var devices = FtdiDevices.Scan().Where(t => t.Type.Equals("232H"));
		if (!devices.Any())
		{
			Console.WriteLine("No devices found.");
			return;
		}
		var device = devices.First();
		Console.WriteLine($"Found device: {device}");

		var spi = new SPI(device: device, spiMode: 0, slowDownFactor: 10);

		var clear = (byte)0x76;
		var brightness = new byte[] { (byte)0x7A, (byte)0x1F };

		spi.Write(clear);
		Thread.Sleep(100);
		spi.Write(brightness);
		Thread.Sleep(100);

		int n = 0;
		while (true)
		{
			Console.WriteLine("Pulse");
			var data = (byte)n;
			spi.Write(data);
			Thread.Sleep(100);
			n++;
		}
	}
}
