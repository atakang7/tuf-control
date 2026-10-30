using System;
using System.IO;
using System.Text.Json;

namespace TufControl;

public sealed class Store(string directory)
{
	public string DirectoryPath { get; } = directory;

	public string FilePath => Path.Combine(DirectoryPath, "profiles.json");

	public Settings Load()
	{
		if (!File.Exists(FilePath))
		{
			return new Settings();
		}
		Settings? settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(FilePath)) ?? throw new InvalidDataException("Boş profil dosyası.");
		settings.Validate();
		return settings;
	}

	public void Save(Settings settings)
	{
		settings.Validate();
		Directory.CreateDirectory(DirectoryPath);
		JsonSerializerOptions options = new JsonSerializerOptions
		{
			WriteIndented = true
		};
		string intended = JsonSerializer.Serialize(settings, options);
		string temp = FilePath + ".tmp";
		using (FileStream fs = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None))
		{
			JsonSerializer.Serialize(fs, settings, options);
			fs.Flush(flushToDisk: true);
		}
		if (File.Exists(FilePath))
		{
			File.Replace(temp, FilePath, FilePath + ".bak");
		}
		else
		{
			File.Move(temp, FilePath);
		}
		// Read the file back and make sure what landed on disk is exactly what was intended.
		Settings stored = JsonSerializer.Deserialize<Settings>(File.ReadAllText(FilePath)) ?? throw new IOException("Kaydedilen profil dosyası boş.");
		if (JsonSerializer.Serialize(stored, options) != intended)
		{
			throw new IOException("Kaydedilen profil dosyası amaçlanan ayarlarla eşleşmiyor.");
		}
	}

	public void Log(string message)
	{
		try
		{
			Directory.CreateDirectory(DirectoryPath);
			string path = Path.Combine(DirectoryPath, "events.log");
			if (File.Exists(path) && new FileInfo(path).Length > 1000000)
			{
				File.Move(path, path + ".previous", overwrite: true);
			}
			File.AppendAllText(path, $"{DateTimeOffset.Now:O} {message}{Environment.NewLine}");
		}
		catch
		{
		}
	}
}
