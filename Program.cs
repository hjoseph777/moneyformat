using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace CurrencyTray;

internal static class Program
{
	[STAThread]
	private static void Main()
	{
		// Prevent duplicate tray icons by allowing only one running instance.
		using var singleInstanceMutex = new Mutex(initiallyOwned: true, name: @"Local\CurrencyTray.SingleInstance", createdNew: out var createdNew);
		if (!createdNew)
		{
			return;
		}

		ApplicationConfiguration.Initialize();
		Application.Run(new CurrencyTrayContext());
	}
}

internal sealed class CurrencyTrayContext : ApplicationContext
{
	private const string IntlRegistryKey = @"Control Panel\International";
	private const uint WmSettingChange = 0x001A;
	private const uint SmtoAbortIfHung = 0x0002;
	private static readonly nint HwndBroadcast = new(0xFFFF);

	private readonly NotifyIcon _notifyIcon;
	private readonly Icon _appIcon;
	private readonly ToolStripMenuItem _formatItem;
	private readonly ToolStripMenuItem _localeItem;
	private readonly ToolStripMenuItem _decimalItem;
	private readonly ToolStripMenuItem _thousandsItem;
	private readonly ToolStripMenuItem _symbolItem;
	private readonly ToolStripMenuItem _digitsItem;
	private readonly ToolStripMenuItem _changeFormatItem;
	private readonly System.Windows.Forms.Timer _pollTimer;

	public CurrencyTrayContext()
	{
		var menu = new ContextMenuStrip();

		_formatItem = new ToolStripMenuItem
		{
			Enabled = false
		};

		_localeItem = new ToolStripMenuItem
		{
			Enabled = false
		};

		_decimalItem = new ToolStripMenuItem
		{
			Enabled = false
		};

		_thousandsItem = new ToolStripMenuItem
		{
			Enabled = false
		};

		_symbolItem = new ToolStripMenuItem
		{
			Enabled = false
		};

		_digitsItem = new ToolStripMenuItem
		{
			Enabled = false
		};

		_changeFormatItem = new ToolStripMenuItem("Change format");
		AddLocaleOption("English Canada (en-CA)", "en-CA");
		AddLocaleOption("French Canada (fr-CA)", "fr-CA");
		AddLocaleOption("English US (en-US)", "en-US");
		AddLocaleOption("French France (fr-FR)", "fr-FR");

		var refreshItem = new ToolStripMenuItem("Refresh", null, (_, _) => RefreshValue());
		var copyItem = new ToolStripMenuItem("Copy", null, (_, _) => CopyValue());
		var exitItem = new ToolStripMenuItem("Exit", null, (_, _) => ExitThread());

		menu.Items.Add(new ToolStripMenuItem("Currency Format Monitor") { Enabled = false });
		menu.Items.Add(new ToolStripSeparator());
		menu.Items.Add(_formatItem);
		menu.Items.Add(_localeItem);
		menu.Items.Add(_decimalItem);
		menu.Items.Add(_thousandsItem);
		menu.Items.Add(_symbolItem);
		menu.Items.Add(_digitsItem);
		menu.Items.Add(new ToolStripSeparator());
		menu.Items.Add(_changeFormatItem);
		menu.Items.Add(new ToolStripSeparator());
		menu.Items.Add(refreshItem);
		menu.Items.Add(copyItem);
		menu.Items.Add(new ToolStripSeparator());
		menu.Items.Add(exitItem);

		_notifyIcon = new NotifyIcon
		{
			Icon = (_appIcon = CreateDollarIcon()),
			Visible = true,
			Text = "Currency Tray",
			ContextMenuStrip = menu
		};

		_notifyIcon.DoubleClick += (_, _) => CopyValue();

		_pollTimer = new System.Windows.Forms.Timer
		{
			Interval = 2000
		};
		// Keep the menu details in sync even if locale settings change elsewhere.
		_pollTimer.Tick += (_, _) => RefreshValue();
		_pollTimer.Start();

		RefreshValue();
	}

	private string GetUserLocaleName()
	{
		using var key = Registry.CurrentUser.OpenSubKey(IntlRegistryKey, writable: false);
		var localeName = key?.GetValue("LocaleName") as string;

		if (!string.IsNullOrWhiteSpace(localeName))
		{
			return localeName;
		}

		return CultureInfo.CurrentCulture.Name;
	}

	private string GetCurrencyValue(CultureInfo culture)
	{
		var amount = 12345.67m;
		return amount.ToString("C", culture);
	}

	private void RefreshValue()
	{
		var localeName = GetUserLocaleName();
		CultureInfo culture;

		try
		{
			culture = CultureInfo.GetCultureInfo(localeName);
		}
		catch (CultureNotFoundException)
		{
			culture = CultureInfo.CurrentCulture;
			localeName = culture.Name;
		}

		var nfi = culture.NumberFormat;
		var value = GetCurrencyValue(culture);
		var placement = GetSymbolPlacement(value, nfi.CurrencySymbol);
		var thousands = DescribeSeparator(nfi.CurrencyGroupSeparator);

		_formatItem.Text = $"Format  : {value}";
		_localeItem.Text = $"Locale  : {localeName}";
		_decimalItem.Text = $"Decimal : {nfi.CurrencyDecimalSeparator}";
		_thousandsItem.Text = $"Thousands: {thousands}";
		_symbolItem.Text = $"Symbol  : {nfi.CurrencySymbol} ({placement})";
		_digitsItem.Text = $"Digits  : {nfi.CurrencyDecimalDigits} decimal places";

		_notifyIcon.Text = TruncateTooltip($"Currency Tray - {value} ({localeName})");
		UpdateCheckedLocale(localeName);
	}

	private void CopyValue()
	{
		var localeName = GetUserLocaleName();
		var culture = CultureInfo.GetCultureInfo(localeName);
		var value = GetCurrencyValue(culture);
		Clipboard.SetText(value);
		_notifyIcon.ShowBalloonTip(
			1500,
			"Currency Copied",
			value,
			ToolTipIcon.Info);
	}

	private void AddLocaleOption(string text, string localeName)
	{
		var item = new ToolStripMenuItem(text)
		{
			Tag = localeName
		};

		item.Click += (_, _) => ApplyLocale(localeName);
		_changeFormatItem.DropDownItems.Add(item);
	}

	private void ApplyLocale(string localeName)
	{
		// LocaleName under HKCU applies per-user and does not require admin rights.
		using (var key = Registry.CurrentUser.OpenSubKey(IntlRegistryKey, writable: true))
		{
			key?.SetValue("LocaleName", localeName, RegistryValueKind.String);
		}

		// Broadcast so open apps can react without waiting for sign-out.
		var section = Marshal.StringToHGlobalUni("intl");
		try
		{
			SendMessageTimeout(
				HwndBroadcast,
				WmSettingChange,
				nuint.Zero,
				section,
				SmtoAbortIfHung,
				2000,
				out _);
		}
		finally
		{
			Marshal.FreeHGlobal(section);
		}

		RefreshValue();
	}

	private void UpdateCheckedLocale(string localeName)
	{
		foreach (ToolStripItem dropDownItem in _changeFormatItem.DropDownItems)
		{
			if (dropDownItem is ToolStripMenuItem localeItem && localeItem.Tag is string tag)
			{
				localeItem.Checked = string.Equals(tag, localeName, StringComparison.OrdinalIgnoreCase);
			}
		}
	}

	private static string GetSymbolPlacement(string value, string symbol)
	{
		var symbolIndex = value.IndexOf(symbol, StringComparison.Ordinal);
		if (symbolIndex < 0)
		{
			return "unknown";
		}

		var firstDigitIndex = value.IndexOfAny(['0', '1', '2', '3', '4', '5', '6', '7', '8', '9']);
		if (firstDigitIndex < 0)
		{
			return "unknown";
		}

		return symbolIndex < firstDigitIndex ? "prefix" : "suffix";
	}

	private static string DescribeSeparator(string separator)
	{
		if (separator == "\u00A0")
		{
			return "(non-breaking space)";
		}

		if (separator == "\u202F")
		{
			return "(thin space)";
		}

		if (string.IsNullOrWhiteSpace(separator))
		{
			return "(space)";
		}

		return separator;
	}

	private static string TruncateTooltip(string input)
	{
		const int maxLength = 63;
		return input.Length <= maxLength ? input : input[..maxLength];
	}

	protected override void ExitThreadCore()
	{
		_pollTimer.Stop();
		_pollTimer.Dispose();
		_notifyIcon.Visible = false;
		_notifyIcon.Dispose();
		_appIcon.Dispose();
		base.ExitThreadCore();
	}

	private static Icon CreateDollarIcon()
	{
		// Draw a simple custom icon so it stands out in the tray overflow list.
		using var bitmap = new Bitmap(32, 32);
		using (var graphics = Graphics.FromImage(bitmap))
		{
			graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
			graphics.Clear(Color.Transparent);

			using var bgBrush = new SolidBrush(Color.FromArgb(40, 128, 70));
			using var fgBrush = new SolidBrush(Color.White);
			using var font = new Font("Segoe UI", 18, FontStyle.Bold, GraphicsUnit.Pixel);
			using var format = new StringFormat
			{
				Alignment = StringAlignment.Center,
				LineAlignment = StringAlignment.Center
			};

			graphics.FillEllipse(bgBrush, 0, 0, 31, 31);
			graphics.DrawString("$", font, fgBrush, new RectangleF(0, 0, 32, 32), format);
		}

		var hIcon = bitmap.GetHicon();
		try
		{
			using var tempIcon = Icon.FromHandle(hIcon);
			return (Icon)tempIcon.Clone();
		}
		finally
		{
			DestroyIcon(hIcon);
		}
	}

	[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern nint SendMessageTimeout(
		nint hWnd,
		uint msg,
		nuint wParam,
		nint lParam,
		uint fuFlags,
		uint uTimeout,
		out nuint lpdwResult);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool DestroyIcon(nint hIcon);
}
