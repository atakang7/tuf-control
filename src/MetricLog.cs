using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace TufControl;

public sealed class MetricLog(string folder, int retentionDays = 30)
{
	public static readonly string[] Columns = new string[16]
	{
		"time", "profile", "ac", "cpu_temp_c", "gpu_temp_c", "cpu_load_pct", "gpu_load_pct", "cpu_clock_ghz", "gpu_clock_ghz", "gpu_power_w",
		"cpu_fan_rpm", "gpu_fan_rpm", "mem_pct", "battery_flow_w", "vram_gb", "battery_pct"
	};

	private static readonly JsonSerializerOptions Json = new JsonSerializerOptions
	{
		WriteIndented = false
	};

	private DateTime lastRow = DateTime.MinValue;

	public string Folder { get; } = folder;

	public TimeSpan RowInterval { get; set; } = TimeSpan.FromSeconds(15L);

	public string SegmentsPath => Path.Combine(Folder, "segments.jsonl");

	public string CurrentPath => Path.Combine(Folder, "current-segment.json");

	public string CsvPath(DateTime day)
	{
		return Path.Combine(Folder, $"metrics-{day:yyyy-MM-dd}.csv");
	}

	private static string F(double? v)
	{
		if (v.HasValue)
		{
			double d = v.GetValueOrDefault();
			if (double.IsFinite(d))
			{
				return d.ToString("0.##", CultureInfo.InvariantCulture);
			}
		}
		return "";
	}

	private static string Q(string s)
	{
		if (s.IndexOfAny(new char[4] { ',', '"', '\n', '\r' }) < 0)
		{
			return s;
		}
		return "\"" + s.Replace("\"", "\"\"") + "\"";
	}

	public bool Row(DateTime time, string profile, bool? onAc, double?[] values)
	{
		if (time - lastRow < RowInterval)
		{
			return false;
		}
		try
		{
			Directory.CreateDirectory(Folder);
			string path = CsvPath(time);
			bool flag = !File.Exists(path);
			StringBuilder sb = new StringBuilder();
			if (flag)
			{
				sb.AppendLine(string.Join(',', Columns));
			}
			StringBuilder stringBuilder = sb.Append(time.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)).Append(',').Append(Q(profile))
				.Append(',');
			string value;
			if (onAc.HasValue)
			{
				value = ((onAc == true) ? "1" : "0");
			}
			else
			{
				value = "";
			}
			stringBuilder.Append(value);
			foreach (double? v in values)
			{
				sb.Append(',').Append(F(v));
			}
			sb.AppendLine();
			File.AppendAllText(path, sb.ToString(), Encoding.UTF8);
			lastRow = time;
			return true;
		}
		catch
		{
			return false;
		}
	}

	public void WriteSegment(SegmentSummary s)
	{
		try
		{
			Directory.CreateDirectory(Folder);
			File.AppendAllText(SegmentsPath, JsonSerializer.Serialize(s, Json) + Environment.NewLine, Encoding.UTF8);
		}
		catch
		{
		}
	}

	public void WriteCurrent(SegmentSummary s)
	{
		try
		{
			Directory.CreateDirectory(Folder);
			string text = CurrentPath + ".tmp";
			File.WriteAllText(text, JsonSerializer.Serialize(s, new JsonSerializerOptions
			{
				WriteIndented = true
			}), Encoding.UTF8);
			File.Move(text, CurrentPath, overwrite: true);
		}
		catch
		{
		}
	}

	public int Cleanup(DateTime now)
	{
		int removed = 0;
		try
		{
			if (!Directory.Exists(Folder))
			{
				return 0;
			}
			string[] files = Directory.GetFiles(Folder, "metrics-*.csv");
			foreach (string f in files)
			{
				if (DateTime.TryParseExact(Path.GetFileNameWithoutExtension(f).Substring("metrics-".Length), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var day) && now.Date - day.Date > TimeSpan.FromDays(retentionDays))
				{
					File.Delete(f);
					removed++;
				}
			}
		}
		catch
		{
		}
		return removed;
	}

	public long SizeBytes()
	{
		try
		{
			return Directory.Exists(Folder) ? Directory.GetFiles(Folder).Sum((string f) => new FileInfo(f).Length) : 0;
		}
		catch
		{
			return 0L;
		}
	}
}
