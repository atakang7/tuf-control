using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace TufControl;

public sealed class Settings
{
	public int Version { get; set; } = 1;

	public List<Profile> Profiles { get; set; }

	public string AcProfile { get; set; }

	public string BatteryProfile { get; set; }

	public bool AutoApply { get; set; }

	public List<AppRule> AppRules { get; set; }

	public string Language { get; set; }

	public string StartupMode { get; set; }

	public string StartupProfile { get; set; }

	public string LastProfile { get; set; }

	public bool ExternalStandard { get; set; }

	public bool RecordHistory { get; set; }

	public bool BootEarly { get; set; }

	public string? StartupTarget(bool onAc, Func<string, bool> running)
	{
		string auto = Desired(onAc, running);
		return StartupMode switch
		{
			"firmware" => null, 
			"last" => Profiles.Any((Profile p) => p.Name == LastProfile) ? LastProfile : auto, 
			"profile" => Profiles.Any((Profile p) => p.Name == StartupProfile) ? StartupProfile : auto, 
			_ => auto, 
		};
	}

	public void Validate()
	{
		bool flag = Version != 1 || Profiles == null;
		if (!flag)
		{
			int count = Profiles.Count;
			bool flag2 = ((count < 1 || count > 20) ? true : false);
			flag = flag2;
		}
		if (flag)
		{
			throw new InvalidDataException("Profil dosyası biçimi desteklenmiyor.");
		}
		foreach (Profile profile in Profiles)
		{
			profile.Validate();
		}
		if (Profiles.Select((Profile p) => p.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count() != Profiles.Count)
		{
			throw new InvalidDataException("Profil isimleri benzersiz olmalı.");
		}
		if (!Profiles.Any((Profile p) => p.Name == AcProfile) || !Profiles.Any((Profile p) => p.Name == BatteryProfile))
		{
			throw new InvalidDataException("Priz/batarya profili bulunamadı.");
		}
		if (AppRules == null)
		{
			List<AppRule> list = (AppRules = new List<AppRule>());
		}
		string language = Language;
		if ((!(language == "tr") && !(language == "en")) || 1 == 0)
		{
			Language = "tr";
		}
		switch (StartupMode)
		{
		case "auto":
		case "last":
		case "profile":
		case "firmware":
			flag = true;
			break;
		default:
			flag = false;
			break;
		}
		if (!flag)
		{
			StartupMode = "auto";
		}
		if (StartupProfile == null)
		{
			language = (StartupProfile = "");
		}
		if (LastProfile == null)
		{
			language = (LastProfile = "");
		}
		if (AppRules.Count > 20)
		{
			throw new InvalidDataException("En fazla 20 uygulama kuralı olabilir.");
		}
		foreach (AppRule r in AppRules)
		{
			if (string.IsNullOrWhiteSpace(r.Process) || r.Process.Length > 64 || r.Process.IndexOfAny(new char[3] { '\\', '/', ':' }) >= 0)
			{
				throw new InvalidDataException("Uygulama adı geçersiz.");
			}
			if (!Profiles.Any((Profile p) => p.Name == r.Profile))
			{
				throw new InvalidDataException(r.Process + " kuralının profili bulunamadı.");
			}
		}
		if (AppRules.Select((AppRule appRule) => appRule.Process).Distinct(StringComparer.OrdinalIgnoreCase).Count() != AppRules.Count)
		{
			throw new InvalidDataException("Aynı uygulama için birden fazla kural var.");
		}
	}

	public string Desired(bool onAc, Func<string, bool> running)
	{
		string text = AppRules.FirstOrDefault((AppRule r) => running(r.Process))?.Profile;
		if (text == null)
		{
			if (!onAc)
			{
				return BatteryProfile;
			}
			text = AcProfile;
		}
		return text;
	}

	public Settings()
	{
		int num = 1;
		List<Profile> list = new List<Profile>(num);
		CollectionsMarshal.SetCount(list, num);
		CollectionsMarshal.AsSpan(list)[0] = new Profile
		{
			Name = "Profilim"
		};
		Profiles = list;
		AcProfile = "Profilim";
		BatteryProfile = "Profilim";
		AppRules = new List<AppRule>();
		Language = "tr";
		StartupMode = "auto";
		StartupProfile = "";
		LastProfile = "";
		ExternalStandard = true;
		RecordHistory = true;
	}
}
