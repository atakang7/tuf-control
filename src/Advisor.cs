using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace TufControl;

public static class Advisor
{
	private sealed record W(HistoryRow R, double Dt);

	public const double MinHours = 0.25;

	private const double MinMinutes = 5.0;

	private const double MinIdleMinutes = 10.0;

	public static readonly FanPoint[] SilentCurve = new FanPoint[8]
	{
		new FanPoint(30, 0),
		new FanPoint(40, 0),
		new FanPoint(50, 25),
		new FanPoint(60, 35),
		new FanPoint(70, 50),
		new FanPoint(80, 70),
		new FanPoint(90, 100),
		new FanPoint(100, 100)
	};

	private static List<W> Weigh(List<HistoryRow> rows)
	{
		List<W> list = new List<W>(rows.Count);
		for (int i = 0; i < rows.Count; i++)
		{
			double dt = ((i < rows.Count - 1) ? Math.Clamp((rows[i + 1].Time - rows[i].Time).TotalSeconds, 0.0, 30.0) : 15.0);
			if (dt > 0.0)
			{
				list.Add(new W(rows[i], dt));
			}
		}
		return list;
	}

	private static double Minutes(IEnumerable<W> ws)
	{
		return ws.Sum((W w) => w.Dt) / 60.0;
	}

	private static double? Avg(IEnumerable<W> ws, Func<HistoryRow, double?> f)
	{
		double sum = 0.0;
		double t = 0.0;
		foreach (W w in ws)
		{
			double? num = f(w.R);
			if (num.HasValue)
			{
				double v = num.GetValueOrDefault();
				sum += v * w.Dt;
				t += w.Dt;
			}
		}
		if (!(t > 0.0))
		{
			return null;
		}
		return sum / t;
	}

	private static double? Max(IEnumerable<W> ws, Func<HistoryRow, double?> f)
	{
		double m = (from w in ws
			select f(w.R) into v
			where v.HasValue
			select v.Value).DefaultIfEmpty(double.NaN).Max();
		if (double.IsNaN(m))
		{
			return null;
		}
		return m;
	}

	private static double? P95(IEnumerable<W> ws, Func<HistoryRow, double?> f)
	{
		List<(double?, double)> v = (from w in ws
			select (Value: f(w.R), Dt: w.Dt) into tuple
			where tuple.Value.HasValue
			orderby tuple.Value
			select tuple).ToList();
		double total = v.Sum(((double? Value, double Dt) tuple) => tuple.Dt);
		if (total <= 0.0)
		{
			return null;
		}
		double acc = 0.0;
		foreach (var x in v)
		{
			acc += x.Item2;
			if (acc >= 0.95 * total)
			{
				return x.Item1;
			}
		}
		return v[v.Count - 1].Item1;
	}

	private static double R1(double? v)
	{
		return Math.Round(v.GetValueOrDefault(), 1);
	}

	private static double R0(double? v)
	{
		return Math.Round(v.GetValueOrDefault());
	}

	public static AdvisorReport Analyze(IReadOnlyList<HistoryRow> source, Settings settings)
	{
		List<W> ws = Weigh(source.ToList());
		double hours = ws.Sum((W w) => w.Dt) / 3600.0;
		List<W> battery = ws.Where((W w) => w.R.Ac == false).ToList();
		double batteryHours = battery.Sum((W w) => w.Dt) / 3600.0;
		double? avgDraw = Avg(battery, (HistoryRow r) => (!(r.BatteryFlow < 0.0)) ? ((double?)null) : (0.0 - r.BatteryFlow));
		List<ProfileStat> stats = (from s in (from w in ws
				group w by w.R.Profile).Select((IGrouping<string, W> g) =>
			{
				List<W> list = g.Where((W w) => w.R.Ac == false).ToList();
				return new ProfileStat(g.Key, g.Sum((W w) => w.Dt) / 3600.0, Avg(g, (HistoryRow r) => r.CpuTemp), P95(g, (HistoryRow r) => r.CpuTemp), Avg(g, (HistoryRow r) => r.CpuLoad), Avg(g, (HistoryRow r) => r.CpuFan), Avg(g, (HistoryRow r) => r.GpuTemp), Avg(list, (HistoryRow r) => (!(r.BatteryFlow < 0.0)) ? ((double?)null) : (0.0 - r.BatteryFlow)), Avg(list.Where((W w) => (w.R.CpuLoad ?? 100.0) < 20.0), (HistoryRow r) => (!(r.BatteryFlow < 0.0)) ? ((double?)null) : (0.0 - r.BatteryFlow)));
			})
			orderby s.Hours descending
			select s).ToList();
		List<Advice> advice = new List<Advice>();
		bool enough = hours >= 0.25;
		if (enough)
		{
			Eco(battery, settings, advice);
			IdleFan(ws, settings, advice);
			foreach (Profile p in settings.Profiles)
			{
				CpuHeadroom(ws, p, advice);
				GpuTargetCap(ws, p, advice);
			}
			GpuHot(ws, settings, advice);
			BatteryProfile(ws, settings, advice);
			Memory(ws, advice);
			if (advice.Count == 0)
			{
				advice.Add(new Advice("good", AdviceKind.Good, "Ayarların verilere göre dengeli görünüyor", "Kayıtlı {0} saatlik kullanımda dikkat çeken bir sorun bulunmadı.", "Daha fazla kullanım verisi biriktikçe öneriler güncellenir.", new object[1] { R1(hours) }));
			}
		}
		return new AdvisorReport(source.Count, hours, batteryHours, avgDraw, enough, stats, advice);
	}

	private static void Eco(List<W> battery, Settings s, List<Advice> advice)
	{
		double total = battery.Sum((W w) => w.Dt);
		if (total < 600.0)
		{
			return;
		}
		List<W> source = battery.Where((W w) => w.R.GpuPower.HasValue).ToList();
		List<W> idleAwake = source.Where((W w) => w.R.GpuLoad.GetValueOrDefault() <= 10.0).ToList();
		double share = source.Sum((W w) => w.Dt) / total;
		if (share < 0.1 || Minutes(idleAwake) < 5.0)
		{
			return;
		}
		double idleW = Avg(idleAwake, (HistoryRow r) => r.GpuPower).GetValueOrDefault();
		if (!(idleW < 3.0))
		{
			double savedW = idleW * idleAwake.Sum((W w) => w.Dt) / total;
			Profile bat = s.Profiles.FirstOrDefault((Profile p) => p.Name == s.BatteryProfile);
			if (bat != null && bat.GpuMode != 2)
			{
				advice.Add(new Advice("eco", AdviceKind.Tip, "Bataryada RTX gereksiz uyanık kalıyor", "Bataryadayken zamanın %{0}'inde RTX uyanıktı; boştayken ortalama {1} W çekti. Bu, ortalama yaklaşık {2} W ek pil tüketimi demek.", "Batarya profilinde (‘{3}’) GPU modunu Eco yap; RTX tamamen kapanır.", new object[4]
				{
					R0(share * 100.0),
					R1(idleW),
					R1(savedW),
					bat.Name
				}, new AdviceAction("‘{0}’ için Eco'yu aç", new object[1] { bat.Name }, "gpumode", bat.Name, 2)));
			}
		}
	}

	private static void IdleFan(List<W> ws, Settings s, List<Advice> advice)
	{
		List<W> idle = ws.Where((W w) => (w.R.CpuLoad ?? 100.0) < 15.0 && w.R.GpuLoad.GetValueOrDefault() < 15.0 && w.R.CpuTemp < 60.0 && w.R.CpuFan.HasValue).ToList();
		if (Minutes(idle) < 10.0)
		{
			return;
		}
		double fan = Avg(idle, (HistoryRow r) => r.CpuFan).GetValueOrDefault();
		if (fan < 3000.0)
		{
			return;
		}
		string main = (from w in idle
			group w by w.R.Profile into g
			where s.Profiles.Any((Profile profile) => profile.Name == g.Key)
			orderby g.Sum((W w) => w.Dt) descending
			select g.Key).FirstOrDefault();
		if (main == null)
		{
			return;
		}
		Profile p = s.Profiles.First((Profile x) => x.Name == main);
		if (!p.CustomFans || !p.Cpu.Where((FanPoint x) => x.Temperature <= 60).All((FanPoint x) => x.Percent <= 35))
		{
			advice.Add(new Advice("idlefan", AdviceKind.Tip, "Boşta fan gereğinden hızlı dönüyor", "Düşük yükte ve ortalama {1} °C'de CPU fanı ortalama {0} rpm döndü (toplam {2} dk).", "‘{3}’ profilinde 60 °C'ye kadar fanı düşük tutan sessiz eğriyi uygula; 70 °C üstü korumalı kalır.", new object[4]
			{
				R0(fan),
				R0(Avg(idle, (HistoryRow r) => r.CpuTemp)),
				R0(Minutes(idle)),
				main
			}, new AdviceAction("Sessiz eğriyi uygula", Array.Empty<object>(), "silentcurve", main, 0)));
		}
	}

	private static void CpuHeadroom(List<W> ws, Profile p, List<Advice> advice)
	{
		List<W> load = ws.Where((W w) => w.R.Profile == p.Name && w.R.CpuLoad.GetValueOrDefault() >= 60.0 && w.R.CpuTemp.HasValue).ToList();
		if (Minutes(load) < 5.0)
		{
			return;
		}
		double avgT = Avg(load, (HistoryRow r) => r.CpuTemp).GetValueOrDefault();
		double p95 = P95(load, (HistoryRow r) => r.CpuTemp).GetValueOrDefault();
		if (p.LimitTemperature && p95 >= (double)(p.CpuTarget - 2))
		{
			int target = Math.Min(90, p.CpuTarget + 3);
			if (target > p.CpuTarget)
			{
				advice.Add(new Advice("cap:" + p.Name, AdviceKind.Tip, "‘{0}’ profilinde sıcaklık hedefi performansı sınırlıyor", "Yüksek yükte sıcaklık hedefe yaslanıyor: hedef {1} °C, zamanın %95'inde {2} °C'ye çıktı (toplam {3} dk).", "Hedefi {4} °C'ye çıkarırsan CPU daha az yavaşlar; fan biraz daha yüksek çalışabilir.", new object[5]
				{
					p.Name,
					p.CpuTarget,
					R0(p95),
					R0(Minutes(load)),
					target
				}, new AdviceAction("Hedefi {0} °C yap", new object[1] { target }, "cputarget", p.Name, target)));
			}
		}
		else if (p.LimitPower && p.CpuWatts < 45 && avgT <= 72.0 && p95 <= 80.0)
		{
			int watts = Math.Min(45, p.CpuWatts + 5);
			advice.Add(new Advice("head:" + p.Name, AdviceKind.Tip, "‘{0}’ profilinde CPU güç payı kullanılmıyor", "Yüksek yükte (toplam {3} dk) CPU ortalama {1} °C, zamanın %95'inde {2} °C altında kaldı; sürekli güç sınırı {4} W.", "Sürekli güç sınırını {5} W'a çıkarmak performansı artırır; sıcaklık hâlâ güvenli kalır.", new object[6]
			{
				p.Name,
				R0(avgT),
				R0(p95),
				R0(Minutes(load)),
				p.CpuWatts,
				watts
			}, new AdviceAction("Sürekli gücü {0} W yap", new object[1] { watts }, "spl", p.Name, watts)));
		}
	}

	private static void GpuTargetCap(List<W> ws, Profile p, List<Advice> advice)
	{
		if (!p.LimitGpuTemp || p.GpuTempTarget >= 87)
		{
			return;
		}
		List<W> load = ws.Where((W w) => w.R.Profile == p.Name && w.R.GpuLoad.GetValueOrDefault() >= 50.0 && w.R.GpuTemp.HasValue).ToList();
		if (!(Minutes(load) < 5.0))
		{
			double p95 = P95(load, (HistoryRow r) => r.GpuTemp).GetValueOrDefault();
			if (!(p95 < (double)(p.GpuTempTarget - 1)))
			{
				int target = Math.Min(87, p.GpuTempTarget + 3);
				advice.Add(new Advice("gpucap:" + p.Name, AdviceKind.Tip, "GPU sıcaklık hedefi performansı sınırlıyor", "‘{0}’ profilinde GPU yükteyken zamanın %95'inde {2} °C'ye çıkıyor; hedef {1} °C (toplam {3} dk).", "Hedefi {4} °C'ye çıkarmak GPU'nun yavaşlamasını azaltır.", new object[5]
				{
					p.Name,
					p.GpuTempTarget,
					R0(p95),
					R0(Minutes(load)),
					target
				}, new AdviceAction("Hedefi {0} °C yap", new object[1] { target }, "gputarget", p.Name, target)));
			}
		}
	}

	private static void GpuHot(List<W> ws, Settings s, List<Advice> advice)
	{
		List<W> load = ws.Where((W w) => w.R.GpuLoad.GetValueOrDefault() >= 50.0 && w.R.GpuTemp.HasValue).ToList();
		if (Minutes(load) < 5.0)
		{
			return;
		}
		double avg = Avg(load, (HistoryRow r) => r.GpuTemp).GetValueOrDefault();
		if (avg < 80.0)
		{
			return;
		}
		string main = (from w in load
			group w by w.R.Profile into g
			where s.Profiles.Any((Profile profile) => profile.Name == g.Key)
			orderby g.Sum((W w) => w.Dt) descending
			select g.Key).FirstOrDefault();
		Profile p = ((main == null) ? null : s.Profiles.First((Profile x) => x.Name == main));
		AdviceAction action = null;
		if (p != null && p.TuneGpu && p.GpuBoostWatts > 5)
		{
			int boost = Math.Max(5, p.GpuBoostWatts - 5);
			action = new AdviceAction("‘{1}’ için Boost'u {0} W yap", new object[2] { boost, p.Name }, "gpuboost", p.Name, boost);
		}
		advice.Add(new Advice("gpuhot", AdviceKind.Warn, "GPU yük altında sıcak", "GPU yükteyken (toplam {2} dk) ortalama {0} °C, en yüksek {1} °C.", "70 °C üstünde fan eğrisini yükselt; Dynamic Boost'u düşürmek de ısıyı azaltır.", new object[3]
		{
			R0(avg),
			R0(Max(load, (HistoryRow r) => r.GpuTemp)),
			R0(Minutes(load))
		}, action));
	}

	private static void BatteryProfile(List<W> ws, Settings s, List<Advice> advice)
	{
		List<(string, double)> idle = (from w in ws
			where w.R.Ac == false && (w.R.CpuLoad ?? 100.0) < 20.0 && w.R.BatteryFlow < 0.0
			group w by w.R.Profile into g
			where Minutes(g) >= 10.0 && s.Profiles.Any((Profile p) => p.Name == g.Key)
			select (Name: g.Key, Draw: Avg(g, (HistoryRow r) => 0.0 - r.BatteryFlow).GetValueOrDefault())).ToList();
		(string, double) current = idle.FirstOrDefault(((string Name, double Draw) x) => x.Name == s.BatteryProfile);
		if (current.Item1 != null)
		{
			(string, double) best = idle.Where(((string Name, double Draw) x) => x.Name != s.BatteryProfile).OrderBy(((string Name, double Draw) x) => x.Draw).FirstOrDefault();
			if (best.Item1 != null && !(current.Item2 - best.Item2 < 2.0))
			{
				advice.Add(new Advice("batprofile", AdviceKind.Tip, "Bataryada daha verimli bir profil var", "Bataryada boştayken ‘{0}’ ortalama {1} W, ‘{2}’ ise {3} W çekti (fark {4} W).", "Batarya profilini ‘{2}’ yap.", new object[5]
				{
					current.Item1,
					R1(current.Item2),
					best.Item1,
					R1(best.Item2),
					R1(current.Item2 - best.Item2)
				}, new AdviceAction("Batarya profili: {0}", new object[1] { best.Item1 }, "batteryprofile", best.Item1, 0)));
			}
		}
	}

	private static void Memory(List<W> ws, List<Advice> advice)
	{
		if (Minutes(ws.Where((W w) => w.R.Mem.HasValue)) < 30.0)
		{
			return;
		}
		double avg = Avg(ws, (HistoryRow r) => r.Mem).GetValueOrDefault();
		if (!(avg < 85.0))
		{
			advice.Add(new Advice("memory", AdviceKind.Tip, "Bellek sık sık dolu", "Ortalama bellek kullanımı %{0}, en yüksek %{1}.", "Kullanmadığın uygulamaları kapat; bellek dolunca bilgisayar yavaşlar ve daha çok ısınır.", new object[2]
			{
				R0(avg),
				R0(Max(ws, (HistoryRow r) => r.Mem))
			}));
		}
	}

	public static Settings Apply(Settings settings, AdviceAction a)
	{
		Settings s = JsonSerializer.Deserialize<Settings>(JsonSerializer.Serialize(settings));
		if (a.Setting == "batteryprofile")
		{
			s.BatteryProfile = a.Profile;
			s.Validate();
			return s;
		}
		Profile p = s.Profiles.First((Profile x) => x.Name == a.Profile);
		switch (a.Setting)
		{
		case "gpumode":
			p.GpuMode = a.Value;
			if (a.Value == 2)
			{
				p.TuneGpu = false;
				p.LimitGpuTemp = false;
			}
			break;
		case "silentcurve":
			p.CustomFans = true;
			p.Cpu = SilentCurve.ToArray();
			p.Gpu = SilentCurve.ToArray();
			break;
		case "spl":
			p.LimitPower = true;
			p.CpuWatts = a.Value;
			if (p.LimitBoost)
			{
				p.CpuSlowWatts = Math.Max(p.CpuSlowWatts, p.CpuWatts);
				p.CpuFastWatts = Math.Max(p.CpuFastWatts, p.CpuSlowWatts);
			}
			break;
		case "cputarget":
			p.LimitTemperature = true;
			p.CpuTarget = a.Value;
			break;
		case "gpuboost":
			p.TuneGpu = true;
			p.GpuBoostWatts = a.Value;
			break;
		case "gputarget":
			p.LimitGpuTemp = true;
			p.GpuTempTarget = a.Value;
			break;
		default:
			throw new ArgumentException("Bilinmeyen öneri eylemi.");
		}
		s.Validate();
		return s;
	}
}
