using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace TufControl;

public sealed class Metric(string name, string unit, string format, double floor, double ceiling)
{
	public const int Capacity = 1300;

	private readonly List<Sample> samples = new List<Sample>();

	private double? peak;

	public string Name { get; } = name;

	public string Info { get; set; } = "";

	public string Unit { get; } = unit;

	public string Format { get; } = format;

	public double Floor { get; } = floor;

	public double Ceiling { get; private set; } = ceiling;

	public Brush Brush { get; set; } = Brushes.White;

	public double? Last { get; private set; }

	public IReadOnlyList<Sample> Samples => samples;

	public RunningStats Segment { get; } = new RunningStats();

	public void Push(DateTime time, double? value)
	{
		Last = value;
		if (!value.HasValue)
		{
			return;
		}
		double v = value.GetValueOrDefault();
		if (double.IsFinite(v))
		{
			Segment.Add(v);
			samples.Add(new Sample(time, v));
			if (samples.Count > 1300)
			{
				samples.RemoveAt(0);
			}
			double? num = peak;
			if (!num.HasValue || v > peak)
			{
				peak = v;
			}
			if (v > Ceiling)
			{
				Ceiling = Math.Ceiling(v / 10.0) * 10.0;
			}
		}
	}

	public MetricStats Stats(TimeSpan window, DateTime now)
	{
		double min = double.MaxValue;
		double max = double.MinValue;
		double sum = 0.0;
		int n = 0;
		DateTime from = now - window;
		int i = samples.Count - 1;
		while (i >= 0 && samples[i].Time >= from)
		{
			double v = samples[i].Value;
			min = Math.Min(min, v);
			max = Math.Max(max, v);
			sum += v;
			n++;
			i--;
		}
		if (n != 0)
		{
			return new MetricStats(Last, min, sum / (double)n, max, peak, n);
		}
		return new MetricStats(Last, null, null, null, peak, 0);
	}

	public string Show(double? v)
	{
		if (!v.HasValue)
		{
			return "—";
		}
		return v.GetValueOrDefault().ToString(Format) + ((Unit.Length > 0) ? (" " + Unit) : "");
	}
}
