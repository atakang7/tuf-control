using System.IO;
using System.Linq;
using System.Text.Json;

namespace TufControl;

public sealed class Profile
{
	public const int MinCpuWatts = 15;

	public const int MaxCpuWatts = 45;

	public const int MaxSlowWatts = 54;

	public const int MaxFastWatts = 65;

	public const int MinGpuBoost = 5;

	public const int MaxGpuBoost = 20;

	public const int MaxGpuTarget = 87;

	public const int FirmwareGpuTargetFloor = 75;

	public const int MinGpuTarget = 65;

	public string Name { get; set; } = "Dengeli";

	public int Mode { get; set; }

	public bool CustomFans { get; set; }

	public bool LimitPower { get; set; }

	public int CpuWatts { get; set; } = 35;

	public bool LimitTemperature { get; set; }

	public int CpuTarget { get; set; } = 85;

	public bool LimitBoost { get; set; }

	public int CpuSlowWatts { get; set; } = 45;

	public int CpuFastWatts { get; set; } = 54;

	public bool TuneGpu { get; set; }

	public int GpuBoostWatts { get; set; } = 15;

	public bool LimitGpuTemp { get; set; }

	public int GpuMode { get; set; }

	public int GpuTempTarget { get; set; } = 87;

	public FanPoint[] Cpu { get; set; } = DefaultCurve();

	public FanPoint[] Gpu { get; set; } = DefaultCurve();

	public static FanPoint[] DefaultCurve()
	{
		return new FanPoint[8]
		{
			new FanPoint(30, 20),
			new FanPoint(40, 25),
			new FanPoint(50, 35),
			new FanPoint(60, 45),
			new FanPoint(70, 60),
			new FanPoint(80, 80),
			new FanPoint(90, 100),
			new FanPoint(100, 100)
		};
	}

	public Profile Clone()
	{
		return JsonSerializer.Deserialize<Profile>(JsonSerializer.Serialize(this));
	}

	public Profile ForDisplays(bool externalConnected, bool enabled)
	{
		Profile p = Clone();
		if ((externalConnected & enabled) && p.GpuMode == 2)
		{
			p.GpuMode = 1;
		}
		return p;
	}

	public void Validate()
	{
		if (string.IsNullOrWhiteSpace(Name) || Name.Length > 40)
		{
			throw new InvalidDataException("Profil adı 1–40 karakter olmalı.");
		}
		int mode = Mode;
		if ((mode < 0 || mode > 2) ? true : false)
		{
			throw new InvalidDataException("Geçersiz çalışma modu.");
		}
		mode = CpuWatts;
		if ((mode < 15 || mode > 45) ? true : false)
		{
			throw new InvalidDataException("CPU güç sınırı 15–45 W olmalı.");
		}
		mode = CpuTarget;
		if ((mode < 60 || mode > 90) ? true : false)
		{
			throw new InvalidDataException("CPU hedefi 60–90 °C olmalı.");
		}
		if (CpuSlowWatts < 15 || CpuSlowWatts > 54)
		{
			throw new InvalidDataException($"sPPT {15}–{54} W olmalı.");
		}
		if (CpuFastWatts < 15 || CpuFastWatts > 65)
		{
			throw new InvalidDataException($"fPPT {15}–{65} W olmalı.");
		}
		if (LimitBoost && CpuFastWatts < CpuSlowWatts)
		{
			throw new InvalidDataException("fPPT, sPPT'den düşük olamaz.");
		}
		if (LimitBoost && LimitPower && CpuSlowWatts < CpuWatts)
		{
			throw new InvalidDataException("sPPT, sürekli güç sınırından (SPL) düşük olamaz.");
		}
		mode = GpuBoostWatts;
		if ((mode < 5 || mode > 20) ? true : false)
		{
			throw new InvalidDataException($"Dynamic Boost {5}–{20} W olmalı.");
		}
		mode = GpuTempTarget;
		if ((mode < 65 || mode > 87) ? true : false)
		{
			throw new InvalidDataException($"GPU sıcaklık hedefi {65}–{87} °C olmalı.");
		}
		mode = GpuMode;
		if ((mode < 0 || mode > 2) ? true : false)
		{
			throw new InvalidDataException("Geçersiz GPU modu.");
		}
		if (GpuMode == 2 && (TuneGpu || LimitGpuTemp))
		{
			throw new InvalidDataException("Eco modunda RTX kapalı olduğu için GPU boost / sıcaklık hedefi kullanılamaz.");
		}
		ValidateCurve(Cpu);
		ValidateCurve(Gpu);
	}

	public static void ValidateCurve(FanPoint[] curve)
	{
		if (curve == null || curve.Length != 8)
		{
			throw new InvalidDataException("Fan eğrisi 8 nokta içermeli.");
		}
		for (int i = 0; i < 8; i++)
		{
			FanPoint p = curve[i];
			int temperature = p.Temperature;
			bool flag = ((temperature < 20 || temperature > 100) ? true : false);
			bool flag2 = flag;
			if (!flag2)
			{
				int percent = p.Percent;
				bool flag3 = ((percent < 0 || percent > 100) ? true : false);
				flag2 = flag3;
			}
			if (flag2)
			{
				throw new InvalidDataException("Sıcaklık 20–100 °C, fan %0–100 aralığında olmalı.");
			}
			if (i > 0 && p.Temperature <= curve[i - 1].Temperature)
			{
				throw new InvalidDataException("Nokta sıcaklıkları soldan sağa artmalı.");
			}
			if (p.Temperature >= 90 && p.Percent < 100)
			{
				throw new InvalidDataException("90 °C ve üzeri için fan %100 olmalı.");
			}
			if (p.Temperature >= 80 && p.Percent < 70)
			{
				throw new InvalidDataException("80 °C ve üzeri için en az %70 fan gerekir.");
			}
			if (p.Temperature >= 70 && p.Percent < 30)
			{
				throw new InvalidDataException("70 °C ve üzeri için en az %30 fan gerekir.");
			}
		}
		if (!curve.Any((FanPoint fanPoint) => fanPoint.Temperature <= 90 && fanPoint.Percent == 100))
		{
			throw new InvalidDataException("En geç 90 °C'de %100 fan gerekir.");
		}
	}

	public static byte[] Encode(FanPoint[] curve)
	{
		ValidateCurve(curve);
		return curve.Select((FanPoint p) => (byte)p.Temperature).Concat(curve.Select((FanPoint p) => (byte)p.Percent)).ToArray();
	}
}
