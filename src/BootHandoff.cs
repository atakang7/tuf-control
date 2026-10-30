using System;
using System.IO;
using System.Text.Json;

namespace TufControl;

public static class BootHandoff
{
	public static string PathIn(string dir)
	{
		return Path.Combine(dir, "boot-state.json");
	}

	public static void Write(string dir, BootState s)
	{
		Directory.CreateDirectory(dir);
		string text = PathIn(dir) + ".tmp";
		File.WriteAllText(text, JsonSerializer.Serialize(s));
		File.Move(text, PathIn(dir), overwrite: true);
	}

	public static BootState? Read(string dir)
	{
		try
		{
			return File.Exists(PathIn(dir)) ? JsonSerializer.Deserialize<BootState>(File.ReadAllText(PathIn(dir))) : null;
		}
		catch
		{
			return null;
		}
	}

	public static bool IsFresh(BootState s, DateTime appStartUtc)
	{
		TimeSpan age = appStartUtc - s.AppliedAt;
		if (age >= TimeSpan.Zero)
		{
			return age <= TimeSpan.FromMinutes(10L);
		}
		return false;
	}
}
