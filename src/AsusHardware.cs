using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace TufControl;

public sealed class AsusHardware : IHardware, IDisposable
{
	private SafeFileHandle handle;

	private bool disposed;

	private readonly bool readOnly;

	public string? LastError { get; private set; }

	public AsusHardware(bool readOnly = false)
	{
		this.readOnly = readOnly;
		handle = CreateFile("\\\\.\\ATKACPI", 3221225472u, 3u, IntPtr.Zero, 3u, 128u, IntPtr.Zero);
	}

	private byte[] Call(uint method, uint id, byte[] data)
	{
		if (disposed)
		{
			throw new ObjectDisposedException("AsusHardware");
		}
		if (handle.IsInvalid || handle.IsClosed)
		{
			handle.Dispose();
			handle = CreateFile("\\\\.\\ATKACPI", 3221225472u, 3u, IntPtr.Zero, 3u, 128u, IntPtr.Zero);
		}
		if (handle.IsInvalid)
		{
			throw new IOException("ASUS System Control Interface erişilemiyor. Yönetici olarak açmanız gerekebilir.");
		}
		if (readOnly && method != 1398035268)
		{
			throw new InvalidOperationException("Salt okunur oturumda yazma yasak.");
		}
		byte[] input = new byte[12 + data.Length];
		byte[] output = new byte[256];
		BitConverter.GetBytes(method).CopyTo(input, 0);
		BitConverter.GetBytes(4 + data.Length).CopyTo(input, 4);
		BitConverter.GetBytes(id).CopyTo(input, 8);
		data.CopyTo(input, 12);
		if (!DeviceIoControl(handle, 2237452u, input, (uint)input.Length, output, (uint)output.Length, out var count, IntPtr.Zero))
		{
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}
		if (count < 4)
		{
			throw new IOException("ASUS sürücüsünden eksik yanıt.");
		}
		return output.Take((int)Math.Min(count, 16u)).ToArray();
	}

	public int? Read(uint id)
	{
		try
		{
			int raw = BitConverter.ToInt32(Call(1398035268u, id, new byte[4]));
			return (raw >= 65536) ? new int?(raw - 65536) : ((int?)null);
		}
		catch
		{
			return null;
		}
	}

	public byte[] ReadCurve(uint id, int mode)
	{
		try
		{
			return Call(1398035268u, id, BitConverter.GetBytes((mode == 1) ? 2 : ((mode == 2) ? 1 : 0)));
		}
		catch (Exception ex)
		{
			LastError = ex.Message;
			return Array.Empty<byte>();
		}
	}

	public void Write(uint id, byte[] payload)
	{
		if (readOnly)
		{
			throw new InvalidOperationException("Salt okunur oturum.");
		}
		if (!new uint[10] { 1179765u, 1114148u, 1114149u, 1179811u, 1179806u, 1179808u, 1179841u, 1179840u, 1179842u, 589856u }.Contains(id))
		{
			throw new InvalidOperationException("İzin verilmeyen endpoint.");
		}
		bool flag = id == 589856;
		if (flag)
		{
			int num = BitConverter.ToInt32(payload);
			bool flag2 = (uint)num <= 1u;
			flag = !flag2;
		}
		if (flag)
		{
			throw new InvalidOperationException("GPU modu yalnızca 0 veya 1 olabilir.");
		}
		if ((id == 1179808 || id == 1179811 || id - 1179840 <= 2) ? true : false)
		{
			int v = BitConverter.ToInt32(payload);
			int max = id switch
			{
				1179811u => 45, 
				1179808u => 54, 
				1179841u => 65, 
				1179840u => 20, 
				_ => 87, 
			};
			if (v < id switch
			{
				1179840u => 5, 
				1179842u => 65, 
				_ => 15, 
			} || v > max)
			{
				throw new InvalidOperationException($"Güvenli aralık dışı değer reddedildi (0x{id:X}: {v}).");
			}
		}
		if (BitConverter.ToInt32(Call(1398162756u, id, payload)) != 1)
		{
			throw new IOException($"Firmware ayarı kabul etmedi (0x{id:X}).");
		}
	}

	public void Dispose()
	{
		disposed = true;
		handle.Dispose();
	}

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern SafeFileHandle CreateFile(string path, uint access, uint share, nint security, uint creation, uint flags, nint template);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool DeviceIoControl(SafeFileHandle file, uint code, byte[] input, uint inputSize, byte[] output, uint outputSize, out uint returned, nint overlapped);
}
