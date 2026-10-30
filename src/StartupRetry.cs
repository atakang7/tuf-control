using System;

namespace TufControl;

public static class StartupRetry
{
	public static readonly int[] Delays = new int[7] { 3, 6, 10, 15, 25, 40, 60 };

	public static DateTimeOffset? Next(int failureCount, DateTimeOffset now)
	{
		if (failureCount < 1 || failureCount > 7)
		{
			return null;
		}
		return now.AddSeconds(Delays[failureCount - 1]);
	}
}
