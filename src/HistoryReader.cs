using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace TufControl;

public static class HistoryReader
{
	public static List<HistoryRow> Load(string folder, DateTime since)
	{
		List<HistoryRow> rows = new List<HistoryRow>();
		if (!Directory.Exists(folder))
		{
			return rows;
		}
		foreach (string file in from f in Directory.GetFiles(folder, "metrics-*.csv")
			orderby f
			select f)
		{
			if (DateTime.TryParseExact(Path.GetFileNameWithoutExtension(file).Substring("metrics-".Length), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var day) && day.AddDays(1.0) < since)
			{
				continue;
			}
			string[] lines;
			try
			{
				lines = File.ReadAllLines(file);
			}
			catch
			{
				continue;
			}
			foreach (string item in lines.Skip(1))
			{
				HistoryRow r = Parse(item);
				if ((object)r != null && r.Time >= since)
				{
					rows.Add(r);
				}
			}
		}
		return rows.OrderBy((HistoryRow historyRow) => historyRow.Time).ToList();
	}

	public static HistoryRow? Parse(string line)
	{
		List<string> f = Split(line);
		if (f.Count < MetricLog.Columns.Length)
		{
			return null;
		}
		if (!DateTime.TryParse(f[0], CultureInfo.InvariantCulture, DateTimeStyles.None, out var time))
		{
			return null;
		}
		DateTime time2 = time;
		string profile = f[1];
		bool? ac;
		if (f[2] == "1")
		{
			ac = true;
		}
		else
		{
			ac = ((f[2] == "0") ? new bool?(false) : ((bool?)null));
		}
		return new HistoryRow(time2, profile, ac, D(3), D(4), D(5), D(6), D(7), D(8), D(9), D(10), D(11), D(12), D(13), D(14), D(15));
		double? D(int i)
		{
			if (!double.TryParse(f[i], NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
			{
				return null;
			}
			return v;
		}
	}

	private static List<string> Split(string line)
	{
		List<string> parts = new List<string>();
		StringBuilder sb = new StringBuilder();
		bool quoted = false;
		for (int i = 0; i < line.Length; i++)
		{
			char c = line[i];
			if (quoted)
			{
				if (c == '"')
				{
					if (i + 1 < line.Length && line[i + 1] == '"')
					{
						sb.Append('"');
						i++;
					}
					else
					{
						quoted = false;
					}
				}
				else
				{
					sb.Append(c);
				}
				continue;
			}
			switch (c)
			{
			case '"':
				quoted = true;
				break;
			case ',':
				parts.Add(sb.ToString());
				sb.Clear();
				break;
			default:
				sb.Append(c);
				break;
			}
		}
		parts.Add(sb.ToString());
		return parts;
	}
}
