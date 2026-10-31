using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace TufControl;

public sealed class Controller(IHardware hw, Action<string> log, IThermal? thermal = null, IWindowsPower? windows = null)
{
	private int? sessionBaselineTemperature;

	private bool thermalWasApplied;

	private bool windowsWasApplied;

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
				// Another controller may have changed the mode behind our back; always rewrite it.
				lastMode = null;
				log("Armoury Crate / ASUSOptimization / G-Helper etkin; ayarlar üstüne yazılıp geri okunarak doğrulanacak.");
			}
			Capabilities cap = Probe();
			if (!cap.Mode || (p.CustomFans && (!cap.CpuCurve || !cap.GpuCurve)) || (p.LimitPower && !cap.Power) || (p.LimitTemperature && !cap.Thermal) || (p.LimitBoost && !cap.Boost) || ((p.TuneGpu || p.LimitGpuTemp) && !cap.Gpu) || (p.GpuMode != 0 && !cap.Eco) || (p.TuneWindows && windows == null))
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
				ApplyWindows(p);
				VerifyItem[] mismatched = Verify(p, thermalDeferred).Where((VerifyItem v) => v.Match == false).ToArray();
				if (mismatched.Length != 0)
				{
					log("Geri okuma eşleşmedi (" + Describe(mismatched) + "); bir kez daha yazılıyor.");
					WriteAll();
					ApplyWindows(p);
					if (ReassertDelayMs > 0)
					{
						Thread.Sleep(350);
					}
					mismatched = Verify(p, thermalDeferred).Where((VerifyItem v) => v.Match == false).ToArray();
					if (mismatched.Length != 0)
					{
						string fail = "Doğrulama başarısız: " + Describe(mismatched) + ". Başka bir uygulama değerleri değiştiriyor olabilir.";
						log(p.Name + ": " + fail);
						return new ApplyResult(Success: false, fail);
					}
				}
				string msg ="Mod ve seçili fan/güç ayarları firmware tarafından kabul edildi (aktif değer geri okuması yok)." + (p.LimitTemperature ? " CPU sıcaklık limiti SMU tablosundan doğrulandı." : "") + (p.TuneWindows ? " Windows işlemci ayarları güç planından geri okunup doğrulandı." : "") + gpuNote + (thermalDeferred ? " CPU sıcaklık hedefi, sürücü hazır olunca uygulanacak." : "");
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

	// Writes the profile's Windows processor settings to the active power plan. A profile that leaves
	// them off hands back Windows defaults, but only if this session changed them, so a cap from one
	// profile never sticks to the next.
	private void ApplyWindows(Profile p)
	{
		if (windows == null)
		{
			return;
		}
		try
		{
			if (p.TuneWindows)
			{
				WindowsCpu v = WindowsCpu.From(p);
				windows.Write(v, v);
				windowsWasApplied = true;
				log("Windows işlemci ayarları yazıldı (" + windows.PlanName + "): " + v.Describe());
			}
			else if (windowsWasApplied)
			{
				windows.Write(WindowsCpu.AcDefaults, WindowsCpu.DcDefaults);
				windowsWasApplied = false;
				log("Windows işlemci ayarları varsayılana döndürüldü (" + windows.PlanName + ").");
			}
		}
		catch (Exception ex)
		{
			// Verify reports the mismatch; firmware settings stay applied.
			log("Windows işlemci ayarları yazılamadı: " + ex.Message);
		}
	}

	// Reads back every setting the firmware exposes and compares it to the profile.
	// Only GPU mode and the CPU temperature limit (SMU) have a real readback; mode, power
	// limits and fan curves are confirmed by the firmware accepting the write.
	public VerifyItem[] Verify(Profile p, bool thermalDeferred = false)
	{
		List<VerifyItem> items = new List<VerifyItem>();
		items.Add(new VerifyItem("Temel mod", p.Mode.ToString(), "?", null));
		if (p.GpuMode != 0)
		{
			int expected = (p.GpuMode == 2) ? 1 : 0;
			int? actual = hw.Read(589856u);
			items.Add(new VerifyItem("GPU modu (Eco)", expected.ToString(), actual?.ToString() ?? "okunamadı", actual.HasValue ? new bool?(actual == expected) : null));
		}
		if (p.CustomFans)
		{
			// DSTS on the curve endpoints returns the factory default curve for the mode, not the
			// active custom curve (confirmed on FA607NUQ), so there is nothing to compare against.
			items.Add(new VerifyItem("CPU fan eğrisi", Convert.ToHexString(Profile.Encode(p.Cpu)), "?", null));
			items.Add(new VerifyItem("GPU fan eğrisi", Convert.ToHexString(Profile.Encode(p.Gpu)), "?", null));
		}
		if (p.LimitPower)
		{
			items.Add(new VerifyItem("CPU güç sınırı", $"{p.CpuWatts} W", "?", null));
		}
		if (p.LimitBoost)
		{
			items.Add(new VerifyItem("CPU boost sınırı", $"{p.CpuSlowWatts}/{p.CpuFastWatts} W", "?", null));
		}
		if (p.TuneGpu)
		{
			items.Add(new VerifyItem("GPU boost", $"{p.GpuBoostWatts} W", "?", null));
		}
		if (p.LimitGpuTemp)
		{
			items.Add(new VerifyItem("GPU sıcaklık hedefi", $"{p.GpuTempTarget} °C", "?", null));
		}
		if (p.LimitTemperature && !thermalDeferred && thermal != null)
		{
			try
			{
				int actual2 = thermal.Read();
				items.Add(new VerifyItem("CPU sıcaklık sınırı", $"{p.CpuTarget} °C", $"{actual2} °C", actual2 == p.CpuTarget));
			}
			catch (Exception ex)
			{
				items.Add(new VerifyItem("CPU sıcaklık sınırı", $"{p.CpuTarget} °C", ex.Message, null));
			}
		}
		if (p.TuneWindows && windows != null)
		{
			WindowsCpu expected2 = WindowsCpu.From(p);
			try
			{
				WindowsCpu ac = windows.Read(ac: true);
				WindowsCpu dc = windows.Read(ac: false);
				string actual3 = (ac == dc) ? ac.Describe() : ("AC " + ac.Describe() + " / DC " + dc.Describe());
				items.Add(new VerifyItem("Windows işlemci ayarları", expected2.Describe(), actual3, ac == expected2 && dc == expected2));
			}
			catch (Exception ex2)
			{
				items.Add(new VerifyItem("Windows işlemci ayarları", expected2.Describe(), ex2.Message, false));
			}
		}
		return items.ToArray();
	}

	public static string Describe(IEnumerable<VerifyItem> items)
	{
		return string.Join(", ", items.Select((VerifyItem v) => $"{v.Name}: beklenen {v.Expected}, okunan {v.Actual}"));
	}

	private void Set(uint id, int value)
	{
		hw.Write(id, BitConverter.GetBytes(value));
	}
}
