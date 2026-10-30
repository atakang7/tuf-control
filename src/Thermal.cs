using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using Microsoft.Win32;
using PawnIO;

namespace TufControl;

public sealed class Thermal : IThermal, IDisposable
{
	private readonly PawnIOWrapper io = new PawnIOWrapper();

	private bool ready;

	private readonly bool readOnly;

	public bool Available
	{
		get
		{
			if (Machine.ModelAllowed)
			{
				return Registry.GetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\PawnIO", "ImagePath", null) is string;
			}
			return false;
		}
	}

	public Thermal(bool readOnly = false)
	{
		this.readOnly = readOnly;
	}

	public int Prepare()
	{
		if (readOnly)
		{
			throw new InvalidOperationException("Salt okunur oturum: SMU modülü yüklenmedi.");
		}
		if (!Available)
		{
			throw new IOException("Mevcut PawnIO sürücüsü bulunamadı. CPU sıcaklık kontrolü kullanılamıyor.");
		}
		if (!ready)
		{
			if (io.Connect() != PawnIOWrapper.ConnectResult.OK)
			{
				throw new IOException("PawnIO erişilemiyor; yönetici yetkisi gerekebilir.");
			}
			using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("TufControl.RyzenSMU.bin");
			using MemoryStream ms = new MemoryStream();
			stream.CopyTo(ms);
			if (Convert.ToHexString(SHA256.HashData(ms.ToArray())) != "B84ECA7F32C63B3D8C14B2C6D45482706DF8683AA6F43EB8BEAD9DC62181D38F")
			{
				throw new IOException("SMU modülü bütünlük kontrolünden geçmedi.");
			}
			if (!io.LoadModule(ms.ToArray()))
			{
				throw new IOException("İmzalı SMU modülü yüklenemedi.");
			}
			ulong[] code = new ulong[1];
			if (!io.Execute("ioctl_get_code_name", null, code) || code[0] != 10)
			{
				throw new IOException("İşlemci Rembrandt olarak doğrulanamadı; sıcaklık kontrolü kapalı.");
			}
			ready = true;
		}
		return Read();
	}

	public int Read()
	{
		if (!ready || readOnly)
		{
			throw new IOException("SMU oturumu hazır değil.");
		}
		ulong[] info = new ulong[2];
		if (!io.Execute("ioctl_resolve_pm_table", null, info))
		{
			throw new IOException("SMU tablo adresi okunamadı.");
		}
		uint family = (uint)info[0] >> 16;
		if (family != 69 && family != 64)
		{
			throw new IOException("Tanımsız SMU tablo sürümü; tahmini adres kullanılmadı.");
		}
		if (!io.Execute("ioctl_update_pm_table", null, null))
		{
			throw new IOException("SMU tablosu güncellenemedi.");
		}
		Thread.Sleep(120);
		ulong[] words = new ulong[64];
		if (!io.Execute("ioctl_read_pm_table", null, words))
		{
			throw new IOException("SMU tablosu okunamadı.");
		}
		float temp = MemoryMarshal.Cast<ulong, float>(words)[16];
		if (!float.IsFinite(temp) || temp < 60f || temp > 110f)
		{
			throw new IOException("SMU sıcaklık limiti geçersiz.");
		}
		return (int)Math.Round(temp);
	}

	public void Set(int celsius)
	{
		if ((celsius < 60 || celsius > 90) ? true : false)
		{
			throw new ArgumentOutOfRangeException("celsius");
		}
		Send(celsius);
	}

	public void Restore(int celsius)
	{
		if ((celsius < 60 || celsius > 110) ? true : false)
		{
			throw new ArgumentOutOfRangeException("celsius");
		}
		Send(celsius);
		if (Read() != celsius)
		{
			throw new IOException("Önceki sıcaklık limiti geri okunamadı.");
		}
	}

	private void Send(int value)
	{
		if (readOnly || !ready)
		{
			throw new InvalidOperationException("SMU yazma yetkisi yok.");
		}
		using Mutex mutex = new Mutex(initiallyOwned: false, "Global\\TufControl.SMU");
		bool acquired = false;
		try
		{
			try
			{
				acquired = mutex.WaitOne(2000);
			}
			catch (AbandonedMutexException)
			{
				acquired = true;
			}
			if (!acquired)
			{
				throw new IOException("SMU meşgul.");
			}
			Wait(61932920u);
			WriteRegister(61932920u, 0u);
			for (uint i = 0u; i < 6; i++)
			{
				WriteRegister(61933976 + i * 4, (i == 0) ? ((uint)value) : 0u);
			}
			WriteRegister(61932840u, 25u);
			if (Wait(61932920u) != 1)
			{
				throw new IOException("SMU sıcaklık limitini reddetti.");
			}
		}
		finally
		{
			if (acquired)
			{
				mutex.ReleaseMutex();
			}
		}
	}

	private uint Wait(uint address)
	{
		Stopwatch watch = Stopwatch.StartNew();
		while (watch.ElapsedMilliseconds < 250)
		{
			ulong[] value = new ulong[1];
			if (!io.Execute("ioctl_read_smu_register", new ulong[1] { address }, value))
			{
				throw new IOException("SMU yanıtı okunamadı.");
			}
			if (value[0] != 0L)
			{
				return (uint)value[0];
			}
			Thread.Sleep(2);
		}
		throw new IOException("SMU yanıt süresi aşıldı.");
	}

	private void WriteRegister(uint address, uint value)
	{
		if (!io.Execute("ioctl_write_smu_register", new ulong[2] { address, value }, null))
		{
			throw new IOException("SMU komutu başarısız.");
		}
	}

	public void Dispose()
	{
		io.Dispose();
	}
}
