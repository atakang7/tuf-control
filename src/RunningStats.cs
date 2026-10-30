namespace TufControl;

public sealed class RunningStats
{
	private double sum;

	public double? Min { get; private set; }

	public double? Max { get; private set; }

	public int Count { get; private set; }

	public double? Avg
	{
		get
		{
			if (Count != 0)
			{
				return sum / (double)Count;
			}
			return null;
		}
	}

	public void Add(double v)
	{
		if (double.IsFinite(v))
		{
			Min = ((!Min.HasValue || v < Min) ? new double?(v) : Min);
			Max = ((!Max.HasValue || v > Max) ? new double?(v) : Max);
			sum += v;
			Count++;
		}
	}

	public void Reset()
	{
		sum = 0.0;
		double? min = (Max = null);
		Min = min;
		Count = 0;
	}
}
