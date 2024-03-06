using System.IO.Ports;

namespace serial_port_monitor;

public static class SerialPortMonitor
{
	public static void Main(string[] args)
	{
		try
		{
			var portName = string.Empty;
			var ports = SerialPort.GetPortNames();

			// if there are no ports, exit
			if (ports.Length == 0)
			{
				Console.WriteLine("No serial ports found.");
				return;
			}

			// check if the user specified a port name on the command line
			if (args.Length > 0)
			{
				// if the user specified a port name, use it only if it's in the list
				if (!ports.Contains(args[0]))
				{
					Console.WriteLine($"Serial port '{args[0].ToUpperInvariant()}' not found.");
					return;
				}
				portName = args[0].ToUpper();
			}
			else if (ports.Length == 1)
			{
				// if there's only one port, use it
				portName = ports[0];
			}
			else
			{
				// if there's more than one port, let the user choose
				ports = ports.OrderBy(p => p).ToArray();
				var menu = new ConsoleMenu(ports, (int i) => portName = ports[i]);
				menu.Show("Select a serial port:");
			}

			// if we still don't have a port name, exit
			if (string.IsNullOrEmpty(portName))
			{
				Console.WriteLine("No serial port selected.");
				return;
			}

			// create the serial port
			var mySerialPort = new SerialPort(portName)
			{
				BaudRate = 9600,
				Parity = Parity.None,
				StopBits = StopBits.One,
				DataBits = 8,
				Handshake = Handshake.None
			};

			// set up the data received event handler
			mySerialPort.DataReceived += (object sender, SerialDataReceivedEventArgs e) =>
			{
				var serialPort = (SerialPort)sender;
				string incomingData = serialPort.ReadExisting();
				Console.Write(incomingData);
			};

			// open the port
			mySerialPort.Open();

			// display the port configuration
			Console.WriteLine($"{Environment.NewLine}Listening on {mySerialPort.ConfigString()}");

			// Set up Ctrl-C capture
			var shutdownEvent = new ManualResetEvent(false);
			Console.CancelKeyPress += (sender, e) =>
			{
				e.Cancel = true;
				shutdownEvent.Set();
			};

			// wait for Ctrl-C
			shutdownEvent.WaitOne();

			// close the port
			mySerialPort.Close();
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
		}
	}
}