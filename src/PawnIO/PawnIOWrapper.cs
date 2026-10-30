using System;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace PawnIO;

public sealed class PawnIOWrapper : IDisposable
{
	private enum Ctl : uint
	{
		Load = 2712805508u,
		Execute = 2712805636u
	}

	public enum ConnectResult
	{
		OK,
		NotInstalled,
		AccessDenied,
		OtherError
	}

	private const int FN_LEN = 32;

	private const uint DEV_TYPE = 2712797184u;

	private nint _raw = IntPtr.Zero;

	private SafeFileHandle? _safe;

	private bool _loaded;

	private bool _disposed;

	public bool IsConnected
	{
		get
		{
			if (_raw != IntPtr.Zero)
			{
				return ((IntPtr)_raw).ToInt64() != -1;
			}
			return false;
		}
	}

	public bool IsModuleLoaded
	{
		get
		{
			if (_loaded && _safe != null)
			{
				return !_safe.IsInvalid;
			}
			return false;
		}
	}

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern nint CreateFile(string n, uint acc, uint share, nint sec, uint disp, uint fl, nint tmpl);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool CloseHandle(nint h);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool DeviceIoControl(nint dev, Ctl code, byte[] inB, uint inSz, byte[] outB, uint outSz, out uint ret, nint ovl);

	[DllImport("kernel32.dll")]
	private static extern bool DeviceIoControl(SafeFileHandle dev, Ctl code, [In] byte[] inB, uint inSz, [Out] byte[] outB, uint outSz, out uint ret, nint ovl);

	public ConnectResult Connect()
	{
		if (IsConnected)
		{
			return ConnectResult.OK;
		}
		_raw = CreateFile("\\\\?\\GLOBALROOT\\Device\\PawnIO", 3221225472u, 3u, IntPtr.Zero, 3u, 0u, IntPtr.Zero);
		if (_raw == IntPtr.Zero || ((IntPtr)_raw).ToInt64() == -1)
		{
			int err = Marshal.GetLastWin32Error();
			_raw = IntPtr.Zero;
			switch (err)
			{
			case 2:
			case 3:
				return ConnectResult.NotInstalled;
			case 5:
				return ConnectResult.AccessDenied;
			default:
				return ConnectResult.OtherError;
			}
		}
		return ConnectResult.OK;
	}

	public bool LoadModule(byte[] data)
	{
		if (!IsConnected || data == null || data.Length == 0)
		{
			return false;
		}
		if (!DeviceIoControl(_raw, Ctl.Load, data, (uint)data.Length, null, 0u, out var _, IntPtr.Zero))
		{
			return false;
		}
		_safe = new SafeFileHandle(_raw, ownsHandle: true);
		_raw = IntPtr.Zero;
		_loaded = true;
		return true;
	}

	public bool Execute(string functionName, ulong[]? input, ulong[]? output)
	{
		if (!IsModuleLoaded)
		{
			return false;
		}
		byte[] nameBytes = Encoding.ASCII.GetBytes(functionName);
		int inputCount = input?.Length ?? 0;
		byte[] buffer = new byte[32 + inputCount * 8];
		Buffer.BlockCopy(nameBytes, 0, buffer, 0, Math.Min(nameBytes.Length, 31));
		if (input != null && inputCount > 0)
		{
			byte[] inputBytes = new byte[inputCount * 8];
			Buffer.BlockCopy(input, 0, inputBytes, 0, inputBytes.Length);
			Buffer.BlockCopy(inputBytes, 0, buffer, 32, inputBytes.Length);
		}
		int outputCount = output?.Length ?? 0;
		byte[] outputBuffer = ((outputCount > 0) ? new byte[outputCount * 8] : null);
		bool flag = DeviceIoControl(_safe, Ctl.Execute, buffer, (uint)buffer.Length, outputBuffer, (uint)(outputBuffer?.Length ?? 0), out var bytesReturned, IntPtr.Zero);
		if (flag && output != null && outputBuffer != null && bytesReturned != 0)
		{
			Buffer.BlockCopy(outputBuffer, 0, output, 0, (int)Math.Min(bytesReturned, (uint)(outputCount * 8)));
		}
		return flag;
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			_loaded = false;
			_safe?.Close();
			if (_raw != IntPtr.Zero && ((IntPtr)_raw).ToInt64() != -1)
			{
				CloseHandle(_raw);
			}
			_disposed = true;
		}
	}
}
