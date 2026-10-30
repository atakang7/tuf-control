using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace TufControl;

public sealed class Controller(IHardware hw, Action<string> log, IThermal? thermal = null)
{
	private int? sessionBaselineTemperature;

	private bool thermalWasApplied;

	private int? lastMode;

	public int ReassertDelayMs { get; set; } = 1100;

	public int? ThermalBaseline => sessionBaselineTemperature;

	public bool ThermalActive => thermalWasApplied;

	public Func<string?>? GpuBusy { get; set; }

	public void ForgetMode()
	{
		lastMode = null;
	}

	public void AssumeMode(int mode)
	{
		lastMode = mode;
	}

	public void AssumeThermal(int baselineCelsius)
	{
		thermalWasApplied = true;
		sessionBaselineTemperature = baselineCelsius;
	}

	public Capabilities Probe()
	{
		int? num = hw.Read(1179765u);
		bool flag = num.HasValue && num.GetValueOrDefault() >= 0;
		bool flag2 = ValidCurve(hw.ReadCurve(1114148u, 0));
		bool flag3 = ValidCurve(hw.ReadCurve(1114149u, 0));
		num = hw.Read(1179811u);
		bool flag4 = num.HasValue && num.GetValueOrDefault() >= 0;
		bool flag5 = thermal?.Available ?? false;
		num = hw.Read(1179808u);
		int num2;
		if (num.HasValue && num.GetValueOrDefault() >= 0)
		{
			num = hw.Read(1179841u);
			num2 = ((num.HasValue && num.GetValueOrDefault() >= 0) ? 1 : 0);
		}
		else
		{
			num2 = 0;
		}
		bool flag6 = (byte)num2 != 0;
		num = hw.Read(1179840u);
		int num3;
		if (num.HasValue && num.GetValueOrDefault() >= 0)
		{
			num = hw.Read(1179842u);
			num3 = ((num.HasValue && num.GetValueOrDefault() >= 0) ? 1 : 0);
		}
		else
		{
			num3 = 0;
		}
		bool flag7 = (byte)num3 != 0;
		int? num4 = hw.Read(589856u);
		bool flag8;
		if (num4.HasValue)
		{
			int valueOrDefault = num4.GetValueOrDefault();
			if ((uint)valueOrDefault <= 1u)
			{
				flag8 = true;
				goto IL_0175;
			}
		}
		flag8 = false;
		goto IL_0175;
		IL_0175:
		bool mode = flag;
		bool cpuCurve = flag2;
		bool gpuCurve = flag3;
		bool power = flag4;
		bool flag9 = flag5;
		bool boost = flag6;
		bool gpu = flag7;
		int eco;
		if (flag8)
		{
			num = hw.Read(589846u);
			eco = ((!num.HasValue || num.GetValueOrDefault() != 0) ? 1 : 0);
		}
		else
		{
			eco = 0;
		}
		return new Capabilities(mode, cpuCurve, gpuCurve, power, flag9, boost, gpu, (byte)eco != 0);
	}

	public static bool ValidCurve(byte[] b)
	{
		if (b.Length == 16 && b.Take(8).All((byte v) => v <= 110) && b.Skip(8).All((byte v) => v <= 100) && b.Take(8).Distinct().Count() >= 4)
		{
			return b.Skip(8).Any((byte v) => v > 0);
		}
		return false;
	}

	public ApplyResult Apply(Profile p, bool modelAllowed, bool conflict, bool deferThermal = false)
	{
		try
		{
			p.Validate();
			if (!modelAllowed)
			{
				return new ApplyResult(Success: false, "Bu model için donanım yazma kapalı.");
			}
			if (conflict)
			{
				lastMode = null;
			}
			if (conflict)
			{
				return new ApplyResult(Success: false, "Armoury Crate / ASUSOptimization / G-Helper etkin. Çakışma çözülmeden uygulama yapılmadı.");
			}
			Capabilities cap = Probe();
			if (!cap.Mode || (p.CustomFans && (!cap.CpuCurve || !cap.GpuCurve)) || (p.LimitPower && !cap.Power) || (p.LimitTemperature && !cap.Thermal) || (p.LimitBoost && !cap.Boost) || ((p.TuneGpu || p.LimitGpuTemp) && !cap.Gpu) || (p.GpuMode != 0 && !cap.Eco))
			{
				return new ApplyResult(Success: false, "Seçili özelliklerden biri firmware tarafından sunulmuyor. Hiçbir ayar uygulanmadı.");
			}
			int? ecoWanted = p.GpuMode switch
			{
				1 => (int?)0, 
				2 => 1, 
				_ => null, 
			};
			if (ecoWanted.HasValue)
			{
				int w = ecoWanted.GetValueOrDefault();
				if (hw.Read(589856u) == w)
				{
					ecoWanted = null;
				}
			}
			if (ecoWanted == 1)
			{
				string busy = GpuBusy?.Invoke();
				if (busy != null)
				{
					return new ApplyResult(Success: false, "RTX şu an kullanılıyor (" + busy + "). Eco moduna geçilmedi; uygulamayı kapatıp tekrar dene. Hiçbir ayar uygulanmadı.", GpuBusy: true);
				}
			}
			string gpuNote = "";
			bool thermalDeferred = false;
			int? oldTemp = null;
			if (p.LimitTemperature || thermalWasApplied)
			{
				try
				{
					oldTemp = thermal.Prepare();
				}
				catch (Exception) when (deferThermal && p.LimitTemperature)
				{
					thermalDeferred = true;
				}
			}
			if (p.LimitTemperature && !thermalDeferred)
			{
				int? num = sessionBaselineTemperature;
				if (!num.HasValue)
				{
					sessionBaselineTemperature = oldTemp;
				}
			}
			try
			{
				if (ecoWanted.HasValue)
				{
					int eco = ecoWanted.GetValueOrDefault();
					Set(589856u, eco);
					if (hw.Read(589856u) != eco)
					{
						throw new IOException("GPU modu geri okunamadı.");
					}
					log((eco == 1) ? "RTX kapatıldı (Eco)." : "RTX etkin (Standart).");
				}
				bool flag = lastMode != p.Mode;
				Stopwatch sinceMode = Stopwatch.StartNew();
				if (flag)
				{
					Set(1179765u, p.Mode);
				}
				else
				{
					log("Temel mod değişmedi; firmware moduna yeniden yazılmadı (güç sıçraması önlenir).");
				}
				lastMode = p.Mode;
				bool caps = p.LimitPower || p.LimitBoost || p.TuneGpu;
				WriteAll();
				if (flag && (caps || p.LimitGpuTemp || p.LimitTemperature || p.CustomFans))
				{
					log($"Mod geçişi: ayarlar moddan {sinceMode.ElapsedMilliseconds} ms sonra yazıldı; firmware toparlanınca yeniden yazılacak.");
					if (ReassertDelayMs > 0)
					{
						Thread.Sleep(ReassertDelayMs);
					}
					WriteAll();
				}
				if (p.LimitTemperature && !thermalDeferred)
				{
					bool ok = false;
					for (int attempt = 0; attempt < 4; attempt++)
					{
						if (ok)
						{
							break;
						}
						if (thermal.Read() == p.CpuTarget)
						{
							ok = true;
							break;
						}
						if (ReassertDelayMs > 0)
						{
							Thread.Sleep(350);
						}
						thermal.Set(p.CpuTarget);
					}
					if (!ok && thermal.Read() != p.CpuTarget)
					{
						throw new IOException("Sıcaklık sınırı geri okuması eşleşmedi.");
					}
				}
				thermalWasApplied = p.LimitTemperature && !thermalDeferred;
				string msg = "Mod ve seçili fan/güç ayarları firmware tarafından kabul edildi (aktif değer geri okuması yok)." + (p.LimitTemperature ? " CPU sıcaklık limiti SMU tablosundan doğrulandı." : "") + gpuNote + (thermalDeferred ? " CPU sıcaklık hedefi, sürücü hazır olunca uygulanacak." : "");
				log(p.Name + ": " + msg);
				return new ApplyResult(Success: true, msg, GpuBusy: false, thermalDeferred);
			}
			catch (Exception ex2)
			{
				bool restored = true;
				lastMode = null;
				try
				{
					Set(1179765u, 0);
					lastMode = 0;
				}
				catch
				{
					restored = false;
				}
				if (oldTemp.HasValue)
				{
					try
					{
						thermal.Restore(oldTemp.Value);
					}
					catch
					{
						restored = false;
					}
				}
				string msg2 = "Uygulama tamamlanamadı: " + ex2.Message + (restored ? " Dengeli firmware moduna dönüş gönderildi ve varsa önceki sıcaklık limiti geri yüklendi. Fan/güç sıfırlamasının geri okuması yok; kontrol edin." : " Geri dönüş de başarısız. Otomatik uygulama durduruldu; Armoury Crate üzerinden kontrol edin.");
				log(msg2);
				return new ApplyResult(Success: false, msg2);
			}
			void WriteAll()
			{
				WriteCaps();
				if (p.CustomFans)
				{
					hw.Write(1114148u, Profile.Encode(p.Cpu));
					hw.Write(1114149u, Profile.Encode(p.Gpu));
				}
				if (p.LimitGpuTemp)
				{
					try
					{
						Set(1179842u, p.GpuTempTarget);
					}
					catch (IOException) when (p.GpuTempTarget < 75)
					{
						Set(1179842u, 75);
						gpuNote = $" GPU sıcaklık hedefi {p.GpuTempTarget} °C firmware tarafından kabul edilmedi; {75} °C uygulandı.";
					}
				}
				if (p.LimitTemperature)
				{
					if (!thermalDeferred)
					{
						thermal.Set(p.CpuTarget);
					}
				}
				else if (thermalWasApplied)
				{
					int? num2 = sessionBaselineTemperature;
					if (num2.HasValue)
					{
						int baseline = num2.GetValueOrDefault();
						thermal.Restore(baseline);
					}
				}
			}
		}
		catch (Exception ex3)
		{
			log(ex3.Message);
			return new ApplyResult(Success: false, ex3.Message);
		}
		void WriteCaps()
		{
			if (p.LimitPower)
			{
				Set(1179811u, p.CpuWatts);
			}
			if (p.LimitBoost)
			{
				Set(1179808u, p.CpuSlowWatts);
				Set(1179841u, p.CpuFastWatts);
			}
			if (p.TuneGpu)
			{
				Set(1179840u, p.GpuBoostWatts);
			}
		}
	}

	private void Set(uint id, int value)
	{
		hw.Write(id, BitConverter.GetBytes(value));
	}
}
