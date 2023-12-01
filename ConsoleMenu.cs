using System.IO.Ports;

namespace serial_port_monitor;

public class ConsoleMenu
{
	public string Prompt { get; set; } = "Select an option:";
	public Action<int> OnSelection { get; set; } = (int i) => { };
	public string[] Items { get; set; } = [];
	public ConsoleMenu()
	{
		// intentionally left blank
	}
	public ConsoleMenu(string[] items, Action<int> callback) : this()
	{
		Items = items;
		OnSelection = callback;
	}
	public int Show(string? prompt = null)
	{
		Console.WriteLine();
		Console.WriteLine(prompt ?? Prompt);
		DisplayMenu();
		SetMenuIndicatorToPosition(0);
		return WaitForUserMenuSelection();
	}
	public int WaitForUserMenuSelection()
	{
		try
		{
			var p = 0;
			ConsoleKeyInfo keyInfo;
			do
			{
				Console.CursorVisible = false;
				keyInfo = Console.ReadKey(true);

				switch (keyInfo.Key)
				{
					case ConsoleKey.DownArrow:
						p = Math.Min(++p, Items.Length - 1);
						SetMenuIndicatorToPosition(p);
						break;
					case ConsoleKey.UpArrow:
						p = Math.Max(0, --p);
						SetMenuIndicatorToPosition(p);
						break;
					case ConsoleKey.Enter:
						this.OnSelection?.Invoke(p);
						return p;
					default:
						// do nothing on other keys (for now)
						break;
				}
			} while (keyInfo.Key != ConsoleKey.Escape);
			return -1;
		}
		finally
		{
			Console.CursorVisible = true;
		}
	}
	private void DisplayMenu()
	{
		Console.WriteLine();
		for (var i = 0; i < Items.Length; i++)
		{
			Console.WriteLine($"   {Items[i]}");
		}
	}
	private void SetMenuIndicatorToPosition(int p)
	{
		var cp = Console.GetCursorPosition();
		Console.SetCursorPosition(0, Math.Max(0, cp.Top - Items.Length));

		for (var i = 0; i < Items.Length; i++)
		{
			var indicator = i == p ? " > " : "   ";
			Console.ForegroundColor = i == p ? ConsoleColor.White : ConsoleColor.Gray;
			Console.WriteLine($"{indicator}{Items[i]}");
		}
	}
}

public static class Extensions
{
	public static string ConfigString(this SerialPort port)
	{
		return $"PortName: {port.PortName}, BaudRate: {port.BaudRate}, Parity: {port.Parity}, StopBits: {port.StopBits}, DataBits: {port.DataBits}, Handshake: {port.Handshake}";
	}
}