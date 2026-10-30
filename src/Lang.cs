using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace TufControl;

public static class Lang
{
	private sealed class Original(string text)
	{
		public string Text = text;
	}

	public static class Info
	{
		public const string CpuTemp = "CPU'nun anlık sıcaklığı. 60 °C altı serin, 60–80 °C yük altında normal, 80 °C üstü sıcak. Ryzen işlemciler 95 °C'ye kadar dayanacak şekilde tasarlanır; sürekli 90 °C üstü fan eğrisini ya da güç sınırını gözden geçirmen gerektiğini gösterir.";

		public const string CpuUsage = "Tüm çekirdeklerin ortalama yükü. Boşta %0–10 normaldir. Düşük kullanımda yüksek sıcaklık, arka planda bir şeyin çalıştığını ya da fanların yetersiz kaldığını gösterebilir.";

		public const string CpuLimit = "Uygulanan sürekli güç sınırı (SPL). 'Firmware' yazıyorsa sınırı ASUS'un modu belirliyor. Düşük sınır daha serin, sessiz ve pil dostudur ama yük altında daha yavaştır.";

		public const string CpuClock = "İşlemcinin anlık ortalama saat hızı. Boşta düşer, yükte yükselir. Yük altındayken hızın belirgin düşmesi, işlemcinin ısı ya da güç sınırına takıldığını gösterir.";

		public const string GpuTemp = "RTX'in sıcaklığı. 'RTX uykuda' ya da 'Eco' görüyorsan kart kullanılmıyor ve güç çekmiyor; pil için ideal olan budur. Oyunda 65–80 °C normaldir, NVIDIA kartı 87 °C'de yavaşlatır.";

		public const string GpuUsage = "RTX'in ne kadar meşgul olduğu. Oyunda %90 ve üstü normaldir. Masaüstünde %0 olmalı; değilse bir uygulama RTX'i boşuna çalıştırıyordur.";

		public const string GpuPower = "RTX'in anlık güç tüketimi. Boşta birkaç watt, oyunda 60–115 W olabilir. Bataryadayken yüksek görmek pili hızla bitirir.";

		public const string GpuClock = "RTX'in grafik çekirdeği saat hızı. Yükte yükselir; oyunda ani düşüşler ısı ya da güç sınırına işaret eder.";

		public const string Vram = "Ekran kartının belleği. %100'e yaklaşması oyunlarda takılmaya yol açabilir; doku kalitesini düşürmek yardımcı olur.";

		public const string Fans = "Fanların dakikadaki devri. Yaklaşık 2.500 rpm altı sessiz, 4.500 rpm üstü belirgin gürültülüdür. 0 rpm, fanın eğrine göre durduğu anlamına gelir.";

		public const string Adapter = "Şarj adaptörü takılı mı. Otomatik geçiş açıksa takılıyken priz profili, çıkarılınca batarya profili uygulanır.";

		public const string PowerFlow = "Bataryadan akan güç. Eksi (−) değer, bataryadayken bütün laptopun o anki toplam tüketimidir: ne kadar küçükse pil o kadar uzun gider. Artı (+) değer şarj gücüdür. Prizdeyken toplam tüketim ölçülemez.";

		public const string Remaining = "Windows'un mevcut tüketime göre tahmin ettiği kalan pil süresi. Tüketim değiştikçe güncellenir.";

		public const string History = "Son dakikalardaki CPU/GPU sıcaklığı, CPU kullanımı ve GPU gücü aynı 0–100 ölçeğinde. Çizgilerin birlikte yükselmesi yükün ısıya dönüştüğünü gösterir. Altında seçili aralığın min / ort / maks değerleri var.";

		public const string Cooling = "Seçili profilin sıcaklık hedefleri ve fan eğrisi. Grafikte yatay eksen sıcaklık, dikey eksen fan hızıdır (%).";

		public const string Memory = "Kullanılan RAM. Sürekli %85 üstü bilgisayarı yavaşlatır; açık uygulamaları azaltmak yardımcı olur.";

		public const string Storage = "Sistem diskinin doluluğu. %90 üstü Windows güncellemelerini ve performansı olumsuz etkileyebilir.";

		public const string Status = "Genel özet. Sıcaklık: en sıcak bileşen. Gürültü: fan hızlarından tahmin. Çakışma: Armoury Crate / G-Helper açık mı. Donanım koruması: güvenli sınırlar her zaman açıktır.";

		public const string FanCurve = "Her nokta, o sıcaklıkta fanın yüzde kaçta döneceğini belirler. 70 °C'ye kadar tamamen serbestsin; sonrası güvenlik için sınırlıdır. Kesikli dikey çizgi şu anki sıcaklıktır.";

		public const string CustomFans = "Açıkken senin eğrin kullanılır, kapalıyken firmware'in temel moda ait eğrisi.";

		public const string CpuTarget = "İşlemci bu sıcaklığa ulaşınca hızını düşürerek orada tutar. Düşük hedef daha serin ve sessizdir ama daha yavaştır. Bataryada 70–80 °C iyi bir dengedir.";

		public const string GpuTarget = "RTX'in sıcaklık hedefi. Düşürmek kartı daha serin ama daha yavaş yapar; kart hedefe ulaşınca güç/hız düşürülür. ASUS firmware 75–87 °C aralığını belgeler; 75 altı denenir, firmware kabul etmezse 75 uygulanır ve bu mesajla bildirilir. NVIDIA varsayılanı 87 °C'dir.";

		public const string Spl = "Uzun süreli yükte işlemcinin çekebileceği en yüksek güç. Pil ömrü ve sessizlik için en etkili ayar budur; bataryada 15–25 W iyi sonuç verir.";

		public const string Boost = "sPPT: birkaç saniyelik kısa yüklerde izin verilen güç. fPPT: anlık zirve gücü. Yüksek değerler uygulamaları daha çevik yapar ama ısıyı ve tüketimi kısa süre artırır.";

		public const string PowerChart = "Ayarladığın üç güç sınırının yük altında zamanla nasıl devreye girdiğini gösterir. Kırmızı kesikli çizgi, uygulamanın asla geçmediği güvenli tavandır.";

		public const string GpuMode = "Eco: RTX tamamen kapanır, pil ömrü belirgin uzar. Standart: RTX gerektiğinde devreye girer. Değiştirme: profil GPU moduna dokunmaz.";

		public const string DynBoost = "Oyunda CPU'dan GPU'ya aktarılabilecek ek güç. Yüksek değer oyunda daha fazla FPS, ama daha fazla ısı ve tüketim demektir.";

		public const string Score = "Optimizasyon testindeki sürdürülebilir hesaplama hızı (milyon işlem/sn). Yüzde, en iyi denemeye göre oranıdır; yüksek olan daha güçlüdür.";

		public const string AutoSwitch = "Prize takılıyken, bataryadayken ve listelediğin uygulamalar açıkken hangi profilin kendiliğinden uygulanacağını belirler.";

		public const string Startup = "Açıkken TUF Control oturum açıldığında bildirim alanında başlar ve profilini uygular.";

		public const string BaseMode = "ASUS firmware'inin temel güç modu. Senin ayarların bunun üzerine uygulanır. Dikkat: temel mod değişince firmware o modun kendi güç sınırlarını ve fan eğrisini bir an yeniden yükler; yük altında bu kısa bir güç/fan sıçraması yapabilir. Sık geçiş yapacağın profillerde temel modu aynı tut (ör. hepsi Dengeli); o zaman geçişte sıçrama olmaz.";

		public const string ExternalStd = "HDMI çıkışı bu laptopta doğrudan RTX'e bağlıdır; Eco'da RTX kapandığı için harici ekran görüntü alamaz. Bu seçenek açıkken harici ekran takılı olduğu sürece Eco profilleri Standart modda çalışır, ekran çıkarılınca Eco'ya geri dönülür. USB-C ile bağlanan ekranlar da harici sayılır.";

		public const string StartupProfile = "Bilgisayar açıldığında (Windows ile başlat açıkken) hangi ayarın uygulanacağını belirler. 'Bilgisayarın kendi ayarı': hiçbir şey uygulanmaz, her şey ASUS firmware'inde kalır. 'Priz / batarya profilim': Otomatik geçişteki profil. 'En son uyguladığım': en son kullandığın profil. 'Sabit profil': her açılışta aynı profil.";

		public const string HistoryLog = "Her 15 saniyede bir sensör değerleri (sıcaklık, kullanım, güç, fan, batarya…) ve o anki profil günlük bir CSV dosyasına yazılır. Bilgisayar uykuya girerken, kapanırken ya da uygulama kapanırken o dönemin her metrik için min / ortalama / maks özeti ve profil başına süre, ortalama sıcaklık ve batarya tüketimi ayrı bir dosyaya eklenir. Hiçbir şey bu bilgisayardan çıkmaz; 30 günden eski günlük dosyalar silinir.";

		public const string BootEarly = "Windows açılırken, sen oturum açmadan önce, küçük bir görev (SYSTEM yetkisiyle) seçili açılış profilini uygular; böylece açılışın ilk dakikasındaki yüksek fan hızı ve sıcaklık azalır. Görev, uygulamanın Program Files altındaki korumalı bir kopyasını kullanır (normal bir uygulama o dosyayı değiştiremez); kopyayı uygulama kendisi güncel tutar. Oturum açılınca uygulama bu ayarı tekrar yazmaz, devralır. Kapatınca görev ve kopya silinir. Açılış ayarı 'Bilgisayarın kendi ayarı' ise hiçbir şey yapılmaz. Eco, oturum açıldıktan sonra uygulanır.";

		public const string Advice = "Uygulamanın kaydettiği ölçümleri (sıcaklık, kullanım, güç, fan, batarya çekişi) profil bazında inceler ve kanıt göstererek öneri çıkarır: bataryada RTX'in gereksiz uyanık kalması, boşta hızlı fanlar, kullanılmayan güç payı, performansı sınırlayan sıcaklık hedefleri, daha verimli batarya profili gibi. Her öneri güvenli sınırlar içinde kalır; 'uygula' düğmesi yalnızca profilini günceller, donanıma göndermek için profili ayrıca uygularsın. Veri yoksa ya da azsa öneri üretilmez.";
	}

	private static readonly ConditionalWeakTable<object, Original> texts = new ConditionalWeakTable<object, Original>();

	private static readonly ConditionalWeakTable<object, Original> tips = new ConditionalWeakTable<object, Original>();

	public static readonly Dictionary<string, string> En;

	public static bool English { get; private set; }

	public static void Set(string code)
	{
		English = code == "en";
	}

	public static string T(string tr)
	{
		if (!English || !En.TryGetValue(tr, out string en))
		{
			return tr;
		}
		return en;
	}

	public static string F(string tr, params object?[] args)
	{
		return string.Format(T(tr), args);
	}

	public static void Apply(DependencyObject root)
	{
		HashSet<DependencyObject> seen = new HashSet<DependencyObject>();
		Walk(root);
		void Walk(DependencyObject d)
		{
			if (seen.Add(d))
			{
				if (!(d is TextBlock tb))
				{
					if (d is ContentControl cc && !(cc is Window) && cc.Content is string s && !BindingOperations.IsDataBound((DependencyObject)(object)cc, ContentControl.ContentProperty))
					{
						cc.Content = Swap(texts, cc, s);
					}
				}
				else if (!BindingOperations.IsDataBound((DependencyObject)(object)tb, TextBlock.TextProperty))
				{
					tb.Text = Swap(texts, tb, tb.Text);
				}
				if (d is FrameworkElement { ToolTip: string tip } fe)
				{
					fe.ToolTip = Swap(tips, fe, tip);
				}
				foreach (object child in LogicalTreeHelper.GetChildren(d))
				{
					DependencyObject c = (DependencyObject)((child is DependencyObject) ? child : null);
					if (c != null)
					{
						Walk(c);
					}
				}
				if (d is Visual || d is Visual3D)
				{
					for (int i = 0; i < VisualTreeHelper.GetChildrenCount(d); i++)
					{
						Walk(VisualTreeHelper.GetChild(d, i));
					}
				}
			}
		}
	}

	private static string Swap(ConditionalWeakTable<object, Original> table, object owner, string current)
	{
		if (table.TryGetValue(owner, out Original o))
		{
			if (current != o.Text && (!En.TryGetValue(o.Text, out string prev) || !(current == prev)))
			{
				o.Text = current;
			}
		}
		else
		{
			o = new Original(current);
			table.Add(owner, o);
		}
		return T(o.Text);
	}

	static Lang()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary["Pano"] = "Dashboard";
		dictionary["İzleme"] = "Monitor";
		dictionary["Soğutma"] = "Cooling";
		dictionary["Güç"] = "Power";
		dictionary["Optimizasyon"] = "Optimize";
		dictionary["Günlük"] = "Log";
		dictionary["Ayarlar"] = "Settings";
		dictionary["Bildirim alanına gizle"] = "Hide to tray";
		dictionary["ASUS ile bağlantılı değildir"] = "Not affiliated with ASUS";
		dictionary["Profilleri düzenle / yeni profil"] = "Edit profiles / new profile";
		dictionary["Tıkla ve uygula"] = "Click to apply";
		dictionary[" · otomatik: "] = " · auto: ";
		dictionary["prizde"] = "on AC";
		dictionary["bataryada"] = "on battery";
		dictionary[".exe açıkken"] = ".exe running";
		dictionary["Sıcaklık"] = "Temperature";
		dictionary["Kullanım"] = "Usage";
		dictionary["Güç sınırı"] = "Power limit";
		dictionary["Saat"] = "Clock";
		dictionary["Firmware"] = "Firmware";
		dictionary["VRAM"] = "VRAM";
		dictionary["Fanlar"] = "Fans";
		dictionary["CPU fanı"] = "CPU fan";
		dictionary["GPU fanı"] = "GPU fan";
		dictionary["Batarya / Güç"] = "Battery / Power";
		dictionary["Adaptör"] = "Adapter";
		dictionary["Güç akışı"] = "Power flow";
		dictionary["Kalan süre"] = "Time left";
		dictionary["Takılı"] = "Plugged in";
		dictionary["Çıkarılmış"] = "Unplugged";
		dictionary["Şarjda"] = "Charging";
		dictionary["{0} sa {1} dk"] = "{0} h {1} min";
		dictionary["Sistem geçmişi"] = "System history";
		dictionary["Son 2 dakika"] = "Last 2 minutes";
		dictionary["Son 6 dakika"] = "Last 6 minutes";
		dictionary["Son 10 dakika"] = "Last 10 minutes";
		dictionary["CPU sıcaklık (°C)"] = "CPU temp (°C)";
		dictionary["GPU sıcaklık (°C)"] = "GPU temp (°C)";
		dictionary["CPU kullanım (%)"] = "CPU usage (%)";
		dictionary["GPU güç (W)"] = "GPU power (W)";
		dictionary["Düzenle"] = "Edit";
		dictionary["CPU hedefi"] = "CPU target";
		dictionary["GPU hedefi"] = "GPU target";
		dictionary["Fan modu"] = "Fan mode";
		dictionary["Fan eğrisi"] = "Fan curve";
		dictionary["Fan eğrisi · {0}"] = "Fan curve · {0}";
		dictionary["Otomatik"] = "Auto";
		dictionary["Tam hız"] = "Full speed";
		dictionary["Özel"] = "Custom";
		dictionary["Bellek (RAM)"] = "Memory (RAM)";
		dictionary["Depolama (C:)"] = "Storage (C:)";
		dictionary["Durum"] = "Status";
		dictionary["İyi"] = "Good";
		dictionary["Yüksek"] = "High";
		dictionary["Kritik"] = "Critical";
		dictionary["Gürültü seviyesi"] = "Noise level";
		dictionary["Sessiz"] = "Quiet";
		dictionary["Orta"] = "Medium";
		dictionary["Kontrol çakışması"] = "Control conflict";
		dictionary["Yok"] = "None";
		dictionary["Var"] = "Yes";
		dictionary["Donanım koruması"] = "Hardware protection";
		dictionary["Etkin"] = "Active";
		dictionary["RTX uykuda"] = "RTX asleep";
		dictionary["Eco · RTX kapalı"] = "Eco · RTX off";
		dictionary["veri yok"] = "no data";
		dictionary["min {0} · ort {1} · maks {2}"] = "min {0} · avg {1} · max {2}";
		dictionary["Ayrıntılı izleme"] = "Detailed monitoring";
		dictionary["Oturum başlıyor"] = "Session starting";
		dictionary["1 dk"] = "1 min";
		dictionary["5 dk"] = "5 min";
		dictionary["15 dk"] = "15 min";
		dictionary["60 dk"] = "60 min";
		dictionary["Grafiklerin üzerine gelerek o anın değerini gör. Gölgeli bant her noktadaki min–maks aralığıdır."] = "Hover a chart to read that moment. The shaded band is the min–max range at each point.";
		dictionary["Uyanıştan beri {0}  ·  {1} örnek  ·  3 sn aralık"] = "Since wake-up {0}  ·  {1} samples  ·  every 3 s";
		dictionary["Uyanıştan beri"] = "Since wake-up";
		dictionary["Ölçüm geçmişini kaydet"] = "Keep a measurement history";
		dictionary["Klasörü aç"] = "Open folder";
		dictionary["Sensör değerleri ve her kullanım dönemi (açılış/uyanıştan uykuya) için min, ortalama ve maks özetleri yalnızca bu bilgisayarda saklanır; ayarlarını sonradan iyileştirmek için kullanılır."] = "Sensor values and a min, average and max summary for every stretch of use (boot or wake-up until sleep) are stored only on this computer, to help you tune your settings later.";
		dictionary["{0} MB kullanılıyor · 30 günden eski ölçümler silinir"] = "{0} MB used · measurements older than 30 days are deleted";
		dictionary["CPU sıcaklığı"] = "CPU temperature";
		dictionary["GPU sıcaklığı"] = "GPU temperature";
		dictionary["CPU kullanımı"] = "CPU usage";
		dictionary["GPU kullanımı"] = "GPU usage";
		dictionary["CPU saat hızı"] = "CPU clock";
		dictionary["GPU saat hızı"] = "GPU clock";
		dictionary["GPU gücü"] = "GPU power";
		dictionary["Bellek kullanımı"] = "Memory usage";
		dictionary["Batarya akışı"] = "Battery flow";
		dictionary["VRAM kullanımı"] = "VRAM usage";
		dictionary["Min"] = "Min";
		dictionary["Ort"] = "Avg";
		dictionary["Maks"] = "Max";
		dictionary["Oturum zirvesi"] = "Session peak";
		dictionary["Veri toplanıyor…"] = "Collecting data…";
		dictionary["Canlı örnekler toplanıyor…"] = "Collecting live samples…";
		dictionary["{0} dk önce"] = "{0} min ago";
		dictionary["{0} sn önce"] = "{0} s ago";
		dictionary["{0} önce"] = "{0} ago";
		dictionary["şimdi"] = "now";
		dictionary["maks"] = "max";
		dictionary["min"] = "min";
		dictionary["ort"] = "avg";
		dictionary["Profil"] = "Profile";
		dictionary["Ad"] = "Name";
		dictionary["Temel mod"] = "Base mode";
		dictionary["Dengeli"] = "Balanced";
		dictionary["Turbo"] = "Turbo";
		dictionary["Geri al"] = "Revert";
		dictionary["Kaydet"] = "Save";
		dictionary["Uygula"] = "Apply";
		dictionary["Bu ayarlardan yeni profil"] = "New profile from these settings";
		dictionary["Profili sil"] = "Delete profile";
		dictionary["Fan eğrileri"] = "Fan curves";
		dictionary["Noktaları sürükle. Eğri her zaman güvenlik kurallarına uyar."] = "Drag the points. The curve always respects the safety rules.";
		dictionary["Özel fan eğrilerini kullan"] = "Use custom fan curves";
		dictionary["Kapalıyken firmware temel modun eğrisini kullanır."] = "When off, the firmware uses the base mode's curve.";
		dictionary["HAZIR EĞRİ"] = "PRESET";
		dictionary["Performans"] = "Performance";
		dictionary["Sıcaklık hedefleri"] = "Temperature targets";
		dictionary["Hedefe ulaşınca işlemci yavaşlatılarak sıcaklık sabitlenir."] = "At the target the processor slows down to hold the temperature.";
		dictionary["CPU sıcaklık hedefi"] = "CPU temperature target";
		dictionary["GPU sıcaklık hedefi"] = "GPU temperature target";
		dictionary["60–90 °C"] = "60–90 °C";
		dictionary["60–90 °C · düşük hedef daha serin, daha yavaş"] = "60–90 °C · lower is cooler and slower";
		dictionary["65–87 °C · 75 altı deneysel"] = "65–87 °C · below 75 is experimental";
		dictionary["Canlı"] = "Live";
		dictionary["CPU sıcaklık · fan"] = "CPU temp · fan";
		dictionary["GPU sıcaklık · fan"] = "GPU temp · fan";
		dictionary["Şu an {0}°C"] = "Now {0}°C";
		dictionary["◀ serbest bölge"] = "◀ free zone";
		dictionary["korumalı ▶"] = "protected ▶";
		dictionary["70 °C'ye kadar eğri tamamen senin: %0–100, istediğin şekilde. Korumalı bölge: 70 °C'den itibaren en az %30, 80 °C'den itibaren en az %70, 90 °C ve üzeri %100. Firmware'in termal korumaları her zaman etkindir."] = "Up to 70 °C the curve is entirely yours: 0–100 %, any shape. Protected zone: at least 30 % from 70 °C, at least 70 % from 80 °C, 100 % from 90 °C. The firmware's thermal protection is always on.";
		dictionary["Firmware bu ayarı desteklediğini bildirmiyor"] = "The firmware does not report support for this setting";
		dictionary["CPU güç sınırları"] = "CPU power limits";
		dictionary["Sürekli güç (SPL)"] = "Sustained power (SPL)";
		dictionary["Uzun yükte sabit tavan · 15–45 W"] = "Ceiling for long loads · 15–45 W";
		dictionary["Boost sınırları"] = "Boost limits";
		dictionary["sPPT ≤ 54 W · fPPT ≤ 65 W"] = "sPPT ≤ 54 W · fPPT ≤ 65 W";
		dictionary["GPU"] = "GPU";
		dictionary["GPU modu"] = "GPU mode";
		dictionary["Değiştirme"] = "Keep";
		dictionary["Standart"] = "Standard";
		dictionary["Eco"] = "Eco";
		dictionary["Eco: RTX tamamen kapanır, görüntüyü dahili Radeon verir; pil ömrü uzar. Standart: RTX gerektiğinde devreye girer. Batarya profiline Eco, oyun profiline Standart seç."] = "Eco: the RTX powers off completely and the built-in Radeon drives the display; battery life improves. Standard: the RTX joins when needed. Pick Eco for your battery profile and Standard for gaming.";
		dictionary["Firmware Eco modunu bildirmiyor (ya da MUX yalnızca RTX modunda)."] = "The firmware does not report Eco mode (or the MUX is in RTX-only mode).";
		dictionary["Dynamic Boost"] = "Dynamic Boost";
		dictionary["CPU'dan GPU'ya aktarılan ek güç · 5–20 W"] = "Extra power shifted from CPU to GPU · 5–20 W";
		dictionary["Anlık GPU gücü"] = "GPU power now";
		dictionary["Donanım koruması açık"] = "Hardware protection on";
		dictionary["Değerler üç katmanda denetlenir: arayüz, profil doğrulaması ve donanım yazma katmanı. Güvenli aralık dışındaki hiçbir değer firmware'e gönderilmez. Firmware desteklemediği bir ayarı kabul etmez."] = "Values are checked in three layers: the interface, profile validation and the hardware write layer. Nothing outside the safe range is ever sent to the firmware, and the firmware rejects settings it does not support.";
		dictionary["Güvenli tavan {0} W"] = "Safe ceiling {0} W";
		dictionary["fPPT · anlık"] = "fPPT · instant";
		dictionary["sPPT · kısa süre"] = "sPPT · short";
		dictionary["SPL · sürekli"] = "SPL · sustained";
		dictionary["Firmware varsayılanı"] = "Firmware default";
		dictionary["Otomatik optimizasyon"] = "Automatic optimization";
		dictionary["İşlemciye tüm çekirdeklerde sabit bir yük verilir ve 7 farklı ayar sırayla denenir: firmware modları, farklı güç sınırları, sıcaklık hedefleri ve fan eğrileri. Her ayarda sürdürülebilir performans, sıcaklık ve fan hızı ölçülür; sonunda senin için Performans, Dengeli ve Sessiz önerileri çıkar."] = "A steady all-core load runs while 7 settings are tried in turn: firmware modes, power limits, temperature targets and fan curves. Each is measured for sustained performance, temperature and fan speed, and you get Performance, Balanced and Quiet recommendations at the end.";
		dictionary["Hızlı · ~5 dk"] = "Quick · ~5 min";
		dictionary["Kapsamlı · ~10 dk"] = "Thorough · ~10 min";
		dictionary["Optimizasyonu başlat"] = "Start optimization";
		dictionary["İptal et"] = "Cancel";
		dictionary["Hazır. Başlamadan önce sağdaki kontrollerin hepsi yeşil olmalı."] = "Ready. All checks on the right must be green before starting.";
		dictionary["Güvenlik kontrolleri"] = "Safety checks";
		dictionary["Yalnızca doğrulanmış güvenli aralıktaki ayarlar denenir. CPU 95 °C'ye ulaşır, priz çekilir ya da bir ayar uygulanamazsa test anında durur. Bitince ya da iptal edince önceki ayarın geri yüklenir."] = "Only settings inside the validated safe range are tried. The test stops at once if the CPU reaches 95 °C, the charger is unplugged or a setting fails. Your previous setting is restored when it ends or is cancelled.";
		dictionary["CPU sıcaklığı (test boyunca)"] = "CPU temperature (during test)";
		dictionary["Performans puanı (Mops/sn)"] = "Performance score (Mops/s)";
		dictionary["Sonuçlar"] = "Results";
		dictionary["AYAR"] = "SETTING";
		dictionary["PUAN"] = "SCORE";
		dictionary["ORT. SICAKLIK"] = "AVG TEMP";
		dictionary["MAKS. SICAKLIK"] = "MAX TEMP";
		dictionary["CPU FANI"] = "CPU FAN";
		dictionary["SAAT"] = "CLOCK";
		dictionary["ÖNERİ"] = "PICK";
		dictionary["Henüz test çalıştırılmadı."] = "No test run yet.";
		dictionary["Prize takılı"] = "Plugged in";
		dictionary["Çakışan uygulama"] = "Conflicting app";
		dictionary["Normal mod"] = "Normal mode";
		dictionary["Desteklenen model"] = "Supported model";
		dictionary["Yönetici yetkisi"] = "Administrator";
		dictionary["Başlangıç sıcaklığı"] = "Start temperature";
		dictionary["Evet"] = "Yes";
		dictionary["Hayır"] = "No";
		dictionary["Açık"] = "On";
		dictionary["Salt okunur"] = "Read-only";
		dictionary["Yok · sıcaklık hedefi atlanır"] = "No · temp-target runs skipped";
		dictionary["{0} °C · soğuma beklenir"] = "{0} °C · will cool down first";
		dictionary["Sensör yok"] = "No sensor";
		dictionary["Önerilen · {0}"] = "Recommended · {0}";
		dictionary["Profil olarak kaydet"] = "Save as profile";
		dictionary["Puan"] = "Score";
		dictionary["Ort. sıcaklık"] = "Avg temp";
		dictionary["En yüksek sürdürülebilir performans."] = "Highest sustained performance.";
		dictionary["Performans, sıcaklık ve fan sesi arasında en iyi denge."] = "Best balance of performance, temperature and fan noise.";
		dictionary["En iyi puanın en az %80'ini en düşük fan hızıyla verir."] = "At least 80 % of the best score at the lowest fan speed.";
		dictionary["Olay günlüğü"] = "Event log";
		dictionary["Çakışma denetimi"] = "Conflict check";
		dictionary["Donanım desteği"] = "Hardware support";
		dictionary["Yenile"] = "Refresh";
		dictionary["Tanı raporu"] = "Diagnostics";
		dictionary["Denetleniyor…"] = "Checking…";
		dictionary["Windows ile başlat"] = "Start with Windows";
		dictionary["Kontrol ediliyor…"] = "Checking…";
		dictionary["Oturum açıldıktan birkaç saniye sonra bildirim alanında başlar. Açıp kapatmak için yönetici yetkisi gerekir."] = "Starts in the tray a few seconds after sign-in. Changing it needs administrator rights.";
		dictionary["Yönetici olarak yeniden aç"] = "Reopen as administrator";
		dictionary["Otomatik profil geçişi"] = "Automatic profile switching";
		dictionary["Hangi profilin ne zaman devreye gireceğini belirle. Açılışta ve uykudan dönüşte de uygulanır."] = "Choose which profile applies when. Also applied at startup and after sleep.";
		dictionary["Prize takılıyken"] = "When plugged in";
		dictionary["Bataryadayken"] = "On battery";
		dictionary["Uygulama kuralları"] = "App rules";
		dictionary["Listelenen oyun ya da uygulama açıkken seçtiğin profil devreye girer; kapanınca priz/batarya profiline dönülür. Birden fazla uygulama açıksa üstteki kural geçerlidir."] = "While a listed game or app runs, its profile takes over; when it closes, the AC/battery profile returns. If several are running, the top rule wins.";
		dictionary["Gözat…"] = "Browse…";
		dictionary["Ekle"] = "Add";
		dictionary["Kuralı sil"] = "Delete rule";
		dictionary["Açık uygulamalardan birini seç ya da Gözat ile bir .exe dosyası ekle."] = "Pick a running app or add an .exe with Browse.";
		dictionary["Otomatik geçişi etkinleştir"] = "Enable automatic switching";
		dictionary["Kapalıyken profiller yalnızca elle uygulanır."] = "When off, profiles apply only by hand.";
		dictionary["Değişiklikler anında kaydedilir."] = "Changes save instantly.";
		dictionary["Profil yedeği"] = "Profile backup";
		dictionary["İçe aktarma otomatik uygulamayı kapatır."] = "Importing turns automatic switching off.";
		dictionary["İçe aktar"] = "Import";
		dictionary["Dışa aktar"] = "Export";
		dictionary["Hakkında"] = "About";
		dictionary["TUF Control v2.1 · GPL-3.0. G-Helper ve PawnIO.Modules kaynaklarından uyarlanan bileşenler içerir. ASUS ile bağlantılı değildir."] = "TUF Control v2.1 · GPL-3.0. Contains components adapted from G-Helper and PawnIO.Modules. Not affiliated with ASUS.";
		dictionary["Dil"] = "Language";
		dictionary["Uygulama dili. Seçim anında uygulanır."] = "App language. Applies instantly.";
		dictionary["2 dk"] = "2 min";
		dictionary["6 dk"] = "6 min";
		dictionary["10 dk"] = "10 min";
		dictionary["Özel eğri kapalı · firmware yönetiyor"] = "Custom curve off · firmware in control";
		dictionary["Şu bileşenler açık olduğu için ayar uygulanamaz:"] = "Settings can't be applied while these are running:";
		dictionary["Armoury Crate, ASUSOptimization ve G-Helper kapalı. Ayarlar uygulanabilir."] = "Armoury Crate, ASUSOptimization and G-Helper are closed. Settings can be applied.";
		dictionary["Performans modu"] = "Performance mode";
		dictionary["CPU SPL"] = "CPU SPL";
		dictionary["CPU sPPT / fPPT"] = "CPU sPPT / fPPT";
		dictionary["GPU boost / sıcaklık hedefi"] = "GPU boost / temp target";
		dictionary["GPU Eco modu"] = "GPU Eco mode";
		dictionary["CPU sıcaklık hedefi (PawnIO)"] = "CPU temp target (PawnIO)";
		dictionary["var"] = "yes";
		dictionary["yok"] = "no";
		dictionary["{0} zaten uygulanmış; değişiklik yok."] = "{0} is already applied; nothing changed.";
		dictionary["GPU kullanımı %{0}"] = "GPU usage {0} %";
		dictionary["GPU kullanımı %{0}: {1}"] = "GPU usage {0} %: {1}";
		dictionary["RTX boşta; bağlı uygulamalar dahili GPU'ya taşınıyor: {0}"] = "RTX idle; attached apps move to the built-in GPU: {0}";
		dictionary["Oturum açılışında etkin"] = "Enabled at sign-in";
		dictionary["Başlangıç görevi devre dışı"] = "Startup task disabled";
		dictionary["Başlangıç görevi yok"] = "No startup task";
		dictionary["İzleme hazır."] = "Monitoring ready.";
		dictionary["Henüz bir ayar uygulanmadı"] = "Nothing applied yet";
		dictionary["Uygulandı: {0} · {1}"] = "Applied: {0} · {1}";
		dictionary["{0} uygulanıyor…"] = "Applying {0}…";
		dictionary["{0} uygulandı."] = "{0} applied.";
		dictionary["{0} kaydedildi."] = "{0} saved.";
		dictionary["{0} yüklendi."] = "{0} loaded.";
		dictionary["Kaydedilmemiş değişiklikler var."] = "You have unsaved changes.";
		dictionary["Değişiklikler geri alındı."] = "Changes reverted.";
		dictionary["Salt okunur oturum: donanıma ayar gönderilmez."] = "Read-only session: nothing is sent to the hardware.";
		dictionary["Otomatik: {0} uygulandı ({1})."] = "Auto: {0} applied ({1}).";
		dictionary["prize takılı"] = "plugged in";
		dictionary["{0} açık"] = "{0} running";
		dictionary["Kaydedildi · prizde {0}, bataryada {1}."] = "Saved · {0} on AC, {1} on battery.";
		dictionary["Kaydedildi · otomatik geçiş kapalı."] = "Saved · automatic switching off.";
		dictionary["Optimizasyon sürüyor; bitmesini bekle ya da iptal et."] = "Optimization running; wait for it or cancel.";
		dictionary["Armoury Crate / G-Helper açık; uygulanmadı. Ayrıntılar Günlük sayfasında."] = "Armoury Crate / G-Helper is running; nothing applied. Details on the Log page.";
		dictionary["CPU'nun anlık sıcaklığı. 60 °C altı serin, 60–80 °C yük altında normal, 80 °C üstü sıcak. Ryzen işlemciler 95 °C'ye kadar dayanacak şekilde tasarlanır; sürekli 90 °C üstü fan eğrisini ya da güç sınırını gözden geçirmen gerektiğini gösterir."] = "Current CPU temperature. Under 60 °C is cool, 60–80 °C is normal under load, above 80 °C is hot. Ryzen chips are built to handle up to 95 °C; staying above 90 °C means you should revisit the fan curve or power limit.";
		dictionary["Tüm çekirdeklerin ortalama yükü. Boşta %0–10 normaldir. Düşük kullanımda yüksek sıcaklık, arka planda bir şeyin çalıştığını ya da fanların yetersiz kaldığını gösterebilir."] = "Average load across all cores. 0–10 % at idle is normal. High temperature at low usage can mean something runs in the background or the fans are too slow.";
		dictionary["Uygulanan sürekli güç sınırı (SPL). 'Firmware' yazıyorsa sınırı ASUS'un modu belirliyor. Düşük sınır daha serin, sessiz ve pil dostudur ama yük altında daha yavaştır."] = "The applied sustained power limit (SPL). 'Firmware' means the ASUS mode sets it. A lower limit is cooler, quieter and easier on the battery, but slower under load.";
		dictionary["İşlemcinin anlık ortalama saat hızı. Boşta düşer, yükte yükselir. Yük altındayken hızın belirgin düşmesi, işlemcinin ısı ya da güç sınırına takıldığını gösterir."] = "Current average CPU clock. It drops at idle and rises under load. A clear drop while loaded means the CPU is hitting a thermal or power limit.";
		dictionary["RTX'in sıcaklığı. 'RTX uykuda' ya da 'Eco' görüyorsan kart kullanılmıyor ve güç çekmiyor; pil için ideal olan budur. Oyunda 65–80 °C normaldir, NVIDIA kartı 87 °C'de yavaşlatır."] = "RTX temperature. 'RTX asleep' or 'Eco' means the card is unused and draws no power, which is ideal on battery. 65–80 °C is normal in games; NVIDIA throttles at 87 °C.";
		dictionary["RTX'in ne kadar meşgul olduğu. Oyunda %90 ve üstü normaldir. Masaüstünde %0 olmalı; değilse bir uygulama RTX'i boşuna çalıştırıyordur."] = "How busy the RTX is. 90 %+ is normal in games. It should be 0 % on the desktop; otherwise an app is keeping the RTX busy for nothing.";
		dictionary["RTX'in anlık güç tüketimi. Boşta birkaç watt, oyunda 60–115 W olabilir. Bataryadayken yüksek görmek pili hızla bitirir."] = "Current RTX power draw. A few watts at idle, 60–115 W in games. Seeing it high on battery drains the battery fast.";
		dictionary["RTX'in grafik çekirdeği saat hızı. Yükte yükselir; oyunda ani düşüşler ısı ya da güç sınırına işaret eder."] = "RTX graphics clock. It rises under load; sudden drops in games point to a thermal or power limit.";
		dictionary["Ekran kartının belleği. %100'e yaklaşması oyunlarda takılmaya yol açabilir; doku kalitesini düşürmek yardımcı olur."] = "Graphics memory. Getting close to 100 % can cause stutter in games; lowering texture quality helps.";
		dictionary["Fanların dakikadaki devri. Yaklaşık 2.500 rpm altı sessiz, 4.500 rpm üstü belirgin gürültülüdür. 0 rpm, fanın eğrine göre durduğu anlamına gelir."] = "Fan speed in revolutions per minute. Below about 2,500 rpm is quiet, above 4,500 rpm is clearly loud. 0 rpm means your curve has stopped the fan.";
		dictionary["Şarj adaptörü takılı mı. Otomatik geçiş açıksa takılıyken priz profili, çıkarılınca batarya profili uygulanır."] = "Whether the charger is connected. With automatic switching on, the AC profile applies when plugged in and the battery profile when unplugged.";
		dictionary["Bataryadan akan güç. Eksi (−) değer, bataryadayken bütün laptopun o anki toplam tüketimidir: ne kadar küçükse pil o kadar uzun gider. Artı (+) değer şarj gücüdür. Prizdeyken toplam tüketim ölçülemez."] = "Power flowing through the battery. A minus (−) value on battery is the whole laptop's draw right now: the smaller, the longer the battery lasts. A plus (+) value is the charging power. Total draw can't be measured while plugged in.";
		dictionary["Windows'un mevcut tüketime göre tahmin ettiği kalan pil süresi. Tüketim değiştikçe güncellenir."] = "Windows' estimate of battery time left at the current draw. It updates as the draw changes.";
		dictionary["Son dakikalardaki CPU/GPU sıcaklığı, CPU kullanımı ve GPU gücü aynı 0–100 ölçeğinde. Çizgilerin birlikte yükselmesi yükün ısıya dönüştüğünü gösterir. Altında seçili aralığın min / ort / maks değerleri var."] = "CPU/GPU temperature, CPU usage and GPU power over the last minutes on one 0–100 scale. Lines rising together show load turning into heat. Min / avg / max for the selected range are listed below.";
		dictionary["Seçili profilin sıcaklık hedefleri ve fan eğrisi. Grafikte yatay eksen sıcaklık, dikey eksen fan hızıdır (%)."] = "Temperature targets and fan curve of the selected profile. In the chart, the horizontal axis is temperature and the vertical axis is fan speed (%).";
		dictionary["Kullanılan RAM. Sürekli %85 üstü bilgisayarı yavaşlatır; açık uygulamaları azaltmak yardımcı olur."] = "RAM in use. Staying above 85 % slows the PC down; closing apps helps.";
		dictionary["Sistem diskinin doluluğu. %90 üstü Windows güncellemelerini ve performansı olumsuz etkileyebilir."] = "How full the system drive is. Above 90 % can hurt Windows updates and performance.";
		dictionary["Genel özet. Sıcaklık: en sıcak bileşen. Gürültü: fan hızlarından tahmin. Çakışma: Armoury Crate / G-Helper açık mı. Donanım koruması: güvenli sınırlar her zaman açıktır."] = "Overall summary. Temperature: the hottest component. Noise: estimated from fan speeds. Conflict: whether Armoury Crate / G-Helper is running. Hardware protection: the safe limits are always on.";
		dictionary["Her nokta, o sıcaklıkta fanın yüzde kaçta döneceğini belirler. 70 °C'ye kadar tamamen serbestsin; sonrası güvenlik için sınırlıdır. Kesikli dikey çizgi şu anki sıcaklıktır."] = "Each point sets the fan speed at that temperature. Up to 70 °C you are completely free; above that it is limited for safety. The dashed vertical line is the current temperature.";
		dictionary["Açıkken senin eğrin kullanılır, kapalıyken firmware'in temel moda ait eğrisi."] = "When on, your curve is used; when off, the firmware uses the base mode's curve.";
		dictionary["İşlemci bu sıcaklığa ulaşınca hızını düşürerek orada tutar. Düşük hedef daha serin ve sessizdir ama daha yavaştır. Bataryada 70–80 °C iyi bir dengedir."] = "When the CPU reaches this temperature it slows down to stay there. A lower target is cooler and quieter but slower. 70–80 °C is a good balance on battery.";
		dictionary["RTX'in sıcaklık hedefi. Düşürmek kartı daha serin ama daha yavaş yapar; kart hedefe ulaşınca güç/hız düşürülür. ASUS firmware 75–87 °C aralığını belgeler; 75 altı denenir, firmware kabul etmezse 75 uygulanır ve bu mesajla bildirilir. NVIDIA varsayılanı 87 °C'dir."] = "The RTX temperature target. Lowering it makes the card cooler but slower; at the target the card cuts power/speed. ASUS documents 75–87 °C for the firmware; values below 75 are tried, and if the firmware refuses one, 75 is applied and you are told. NVIDIA's default is 87 °C.";
		dictionary["Uzun süreli yükte işlemcinin çekebileceği en yüksek güç. Pil ömrü ve sessizlik için en etkili ayar budur; bataryada 15–25 W iyi sonuç verir."] = "The highest power the CPU may draw under long loads. This is the most effective setting for battery life and quietness; 15–25 W works well on battery.";
		dictionary["sPPT: birkaç saniyelik kısa yüklerde izin verilen güç. fPPT: anlık zirve gücü. Yüksek değerler uygulamaları daha çevik yapar ama ısıyı ve tüketimi kısa süre artırır."] = "sPPT: power allowed for short loads of a few seconds. fPPT: instant peak power. Higher values feel snappier but briefly raise heat and draw.";
		dictionary["Ayarladığın üç güç sınırının yük altında zamanla nasıl devreye girdiğini gösterir. Kırmızı kesikli çizgi, uygulamanın asla geçmediği güvenli tavandır."] = "Shows how your three power limits take effect over time under load. The red dashed line is the safe ceiling the app never exceeds.";
		dictionary["Eco: RTX tamamen kapanır, pil ömrü belirgin uzar. Standart: RTX gerektiğinde devreye girer. Değiştirme: profil GPU moduna dokunmaz."] = "Eco: the RTX powers off completely for much longer battery life. Standard: the RTX joins when needed. Keep: the profile does not touch the GPU mode.";
		dictionary["Oyunda CPU'dan GPU'ya aktarılabilecek ek güç. Yüksek değer oyunda daha fazla FPS, ama daha fazla ısı ve tüketim demektir."] = "Extra power that can shift from CPU to GPU in games. Higher means more FPS, but more heat and draw.";
		dictionary["Optimizasyon testindeki sürdürülebilir hesaplama hızı (milyon işlem/sn). Yüzde, en iyi denemeye göre oranıdır; yüksek olan daha güçlüdür."] = "Sustained compute speed in the optimization test (million operations/s). The percentage is relative to the best run; higher is stronger.";
		dictionary["Prize takılıyken, bataryadayken ve listelediğin uygulamalar açıkken hangi profilin kendiliğinden uygulanacağını belirler."] = "Sets which profile applies by itself when plugged in, on battery, and while the apps you list are running.";
		dictionary["Açıkken TUF Control oturum açıldığında bildirim alanında başlar ve profilini uygular."] = "When on, TUF Control starts in the tray at sign-in and applies your profile.";
		dictionary["ASUS firmware'inin temel güç modu. Senin ayarların bunun üzerine uygulanır. Dikkat: temel mod değişince firmware o modun kendi güç sınırlarını ve fan eğrisini bir an yeniden yükler; yük altında bu kısa bir güç/fan sıçraması yapabilir. Sık geçiş yapacağın profillerde temel modu aynı tut (ör. hepsi Dengeli); o zaman geçişte sıçrama olmaz."] = "The ASUS firmware's base power mode. Your settings apply on top of it. Note: when the base mode changes, the firmware briefly reloads that mode's own power limits and fan curve, which can cause a short power/fan spike under load. Keep the same base mode in profiles you switch between often (e.g. all Balanced) and switching causes no spike.";
		dictionary["HDMI çıkışı bu laptopta doğrudan RTX'e bağlıdır; Eco'da RTX kapandığı için harici ekran görüntü alamaz. Bu seçenek açıkken harici ekran takılı olduğu sürece Eco profilleri Standart modda çalışır, ekran çıkarılınca Eco'ya geri dönülür. USB-C ile bağlanan ekranlar da harici sayılır."] = "The HDMI output on this laptop is wired directly to the RTX; in Eco the RTX is off, so an external display gets no picture. While this is on and an external display is connected, Eco profiles run in Standard mode and return to Eco when it is unplugged. Displays connected over USB-C count as external too.";
		dictionary["Harici ekran takılıyken RTX'i açık tut"] = "Keep the RTX on while an external display is connected";
		dictionary["HDMI bu laptopta RTX'e bağlıdır. Harici ekran varken Eco profilleri Standart çalışır; ekran çıkarılınca Eco'ya dönülür."] = "HDMI on this laptop is wired to the RTX. With an external display connected, Eco profiles run in Standard and return to Eco when it is removed.";
		dictionary["Harici ekran bağlandı."] = "External display connected.";
		dictionary["Harici ekran çıkarıldı."] = "External display disconnected.";
		dictionary["Harici ekran bağlı: HDMI için RTX açık (Standart) tutuluyor."] = "External display connected: RTX kept on (Standard) for HDMI.";
		dictionary["Kaydedildi."] = "Saved.";
		dictionary["RTX meşgul; diğer ayarlar uygulandı, Eco birazdan yeniden denenecek."] = "RTX busy; other settings applied, Eco will be retried shortly.";
		dictionary["Uygulanamadı; kısa süre sonra yeniden denenecek."] = "Could not apply; will retry shortly.";
		dictionary["Uygulanamadı; {0} sn sonra yeniden denenecek."] = "Could not apply; retrying in {0} s.";
		dictionary["TUF Control yalnızca Windows'ta çalışır."] = "TUF Control runs on Windows only.";
		dictionary["TUF Control yalnızca x64 Windows'ta çalışır (bu sistem: {0})."] = "TUF Control runs on x64 Windows only (this system: {0}).";
		dictionary["Windows 10 2004 (yapı {0}) veya daha yenisi gerekli; bu sistem yapı {1}."] = "Windows 10 2004 (build {0}) or newer is required; this system is build {1}.";
		dictionary["Bu model ({0}) doğrulanmadı; uygulama salt okunur çalışır. Desteklenen: {1}."] = "This model ({0}) is not verified; the app runs read-only. Supported: {1}.";
		dictionary["BIOS {0} test edilmedi (test edilen: {1}). Ayarlar uygulanır ama sorun görürsen bildir."] = "BIOS {0} is untested (tested: {1}). Settings are applied, but please report problems.";
		dictionary["PawnIO sürücüsü kurulu değil; CPU sıcaklık hedefi kullanılamaz (diğer özellikler çalışır)."] = "PawnIO driver is not installed; the CPU temperature target is unavailable (everything else works).";
		dictionary["Düzenlenen profil"] = "Editing profile";
		dictionary["Düzenlenen: {0} · Henüz etkin profil yok. Uygula'ya basınca bu profil etkin olur."] = "Editing: {0} · No profile is active yet. Press Apply to make this one active.";
		dictionary["Etkin profil {0} düzenleniyor · kaydedilmemiş değişiklikler Uygula'ya basana kadar donanıma gitmez."] = "Editing the active profile {0} · unsaved changes reach the hardware only when you press Apply.";
		dictionary["Etkin profil {0} düzenleniyor · gördüğün değerler şu an donanımda."] = "Editing the active profile {0} · the values you see are what the hardware is running.";
		dictionary["Düzenlenen: {0} · Şu an etkin: {1}. Buradaki değerler {0} profiline aittir; Uygula'ya basana kadar donanıma gitmez."] = "Editing: {0} · Active now: {1}. These values belong to {0}; they reach the hardware only when you press Apply.";
		dictionary[" · şu an etkin"] = " · active now";
		dictionary[" · şu an düzenleniyor"] = " · being edited";
		dictionary["{0} uygulandı. Editörde {1} için kaydedilmemiş değişiklikler var; o yüzden editör {1} üzerinde kaldı."] = "{0} applied. The editor has unsaved changes for {1}, so it stays on {1}.";
		dictionary["Armoury Crate / G-Helper açık; ayarlar üstüne yazılıp doğrulanacak."] = "Armoury Crate / G-Helper is running; settings will be overridden and verified.";
		dictionary["Şu bileşenler açık; TufControl ayarlarını üstüne yazıp geri okuyarak doğruluyor:"] = "These components are running; TufControl overrides them and verifies its settings by reading them back:";
		dictionary["Ayarlar dışarıdan değiştirilmiş; yeniden uygulanıyor:"] = "Settings were changed externally; re-applying:";
		dictionary["Ayarlar yeniden uygulandı ve doğrulandı."] = "Settings re-applied and verified.";
		dictionary["Mod geçişi: ayarlar moddan {0} ms sonra yazıldı; firmware toparlanınca yeniden yazılacak."] = "Mode switch: settings written {0} ms after the mode; they are rewritten once the firmware settles.";
		dictionary["RTX hâlâ meşgul; Eco'ya geçilemedi. Profili elle uygulayabilirsin."] = "RTX still busy; could not switch to Eco. You can apply the profile by hand.";
		dictionary["Eco uygulandı."] = "Eco applied.";
		dictionary["CPU sıcaklık hedefi için sürücü henüz hazır değil; diğer ayarlar uygulandı, hedef birazdan yeniden denenecek."] = "The driver for the CPU temperature target is not ready yet; other settings are applied and the target will be retried shortly.";
		dictionary["CPU sıcaklık hedefi uygulandı."] = "CPU temperature target applied.";
		dictionary["CPU sıcaklık hedefi uygulanamadı; sürücü hazır olmadı. Profili elle uygulayabilirsin."] = "Could not apply the CPU temperature target; the driver never became ready. You can apply the profile by hand.";
		dictionary["Bilgisayar açıldığında (Windows ile başlat açıkken) hangi ayarın uygulanacağını belirler. 'Bilgisayarın kendi ayarı': hiçbir şey uygulanmaz, her şey ASUS firmware'inde kalır. 'Priz / batarya profilim': Otomatik geçişteki profil. 'En son uyguladığım': en son kullandığın profil. 'Sabit profil': her açılışta aynı profil."] = "Decides which settings apply when the PC starts (with Start with Windows on). 'The laptop's own setting': nothing is applied and everything stays on the ASUS firmware. 'My AC / battery profile': the one from automatic switching. 'Last applied': the profile you used most recently. 'Fixed profile': the same profile at every start.";
		dictionary["Açılışta uygulanacak profil"] = "Profile at startup";
		dictionary["Bilgisayar açılınca hangi ayarın kullanılacağını seç."] = "Choose which setting is used when the PC starts.";
		dictionary["Bilgisayarın kendi ayarı (hiçbir şey uygulanmaz)"] = "The laptop's own setting (nothing applied)";
		dictionary["Priz / batarya profilim (otomatik)"] = "My AC / battery profile (automatic)";
		dictionary["En son uyguladığım profil"] = "Last applied profile";
		dictionary["Sabit profil: {0}"] = "Fixed profile: {0}";
		dictionary["Her 15 saniyede bir sensör değerleri (sıcaklık, kullanım, güç, fan, batarya…) ve o anki profil günlük bir CSV dosyasına yazılır. Bilgisayar uykuya girerken, kapanırken ya da uygulama kapanırken o dönemin her metrik için min / ortalama / maks özeti ve profil başına süre, ortalama sıcaklık ve batarya tüketimi ayrı bir dosyaya eklenir. Hiçbir şey bu bilgisayardan çıkmaz; 30 günden eski günlük dosyalar silinir."] = "Every 15 seconds the sensor values (temperature, load, power, fans, battery…) and the active profile are written to a daily CSV file. When the computer goes to sleep, shuts down or the app closes, a min / average / max summary for every metric plus time, average temperature and battery draw per profile is appended to a separate file. Nothing leaves this computer; daily files older than 30 days are deleted.";
		dictionary["Uygulamanın kaydettiği ölçümleri (sıcaklık, kullanım, güç, fan, batarya çekişi) profil bazında inceler ve kanıt göstererek öneri çıkarır: bataryada RTX'in gereksiz uyanık kalması, boşta hızlı fanlar, kullanılmayan güç payı, performansı sınırlayan sıcaklık hedefleri, daha verimli batarya profili gibi. Her öneri güvenli sınırlar içinde kalır; 'uygula' düğmesi yalnızca profilini günceller, donanıma göndermek için profili ayrıca uygularsın. Veri yoksa ya da azsa öneri üretilmez."] = "Looks at the measurements the app records (temperature, load, power, fans, battery draw) per profile and gives advice backed by evidence: the RTX staying awake on battery, fast fans at idle, unused power headroom, temperature targets that limit performance, a more efficient battery profile and so on. Every suggestion stays inside the safe limits; the apply button only updates your profile, and you apply the profile to the hardware yourself. With no or too little data nothing is suggested.";
		dictionary["Öneriler"] = "Recommendations";
		dictionary["Kaydedilen ölçümlere bakarak ayarların için öneriler."] = "Advice for your settings, based on the recorded measurements.";
		dictionary["24 saat"] = "24 hours";
		dictionary["7 gün"] = "7 days";
		dictionary["30 gün"] = "30 days";
		dictionary["Kayıtlı süre"] = "Recorded time";
		dictionary["Bataryada geçen süre"] = "Time on battery";
		dictionary["Bataryada ort. çekiş"] = "Avg battery draw";
		dictionary["Ölçüm sayısı"] = "Samples";
		dictionary["Profil karşılaştırması"] = "Profile comparison";
		dictionary["Seçili dönemde her profilin gerçekte nasıl çalıştığı."] = "How each profile actually behaved in the chosen period.";
		dictionary["PROFİL"] = "PROFILE";
		dictionary["SÜRE"] = "TIME";
		dictionary["ORT. CPU SICAKLIK"] = "AVG CPU TEMP";
		dictionary["ORT. CPU YÜKÜ"] = "AVG CPU LOAD";
		dictionary["ORT. CPU FANI"] = "AVG CPU FAN";
		dictionary["BATARYADA BOŞTA ÇEKİŞ"] = "IDLE DRAW ON BATTERY";
		dictionary["Henüz kayıt yok."] = "No records yet.";
		dictionary["Ölçüm geçmişi kapalı. Ayarlar'dan açarsan öneriler için veri toplanır."] = "Measurement history is off. Turn it on in Settings to collect data for recommendations.";
		dictionary["Öneri çıkarabilmek için en az {0} dk kayıt gerekiyor; şimdilik {1} dk var. Uygulama açık kaldıkça kayıt birikir."] = "At least {0} min of records are needed for advice; there are {1} min so far. Records build up while the app stays open.";
		dictionary["Önce Soğutma/Güç sayfasındaki kaydedilmemiş değişiklikleri kaydet ya da geri al."] = "First save or revert the unsaved changes on the Cooling/Power page.";
		dictionary["Öneri uygulandı: {0} profili güncellendi."] = "Advice applied: profile {0} updated.";
		dictionary["{0} profili güncellendi. Donanıma göndermek için profili uygula."] = "Profile {0} updated. Apply the profile to send it to the hardware.";
		dictionary["Bataryada RTX gereksiz uyanık kalıyor"] = "The RTX stays awake on battery for nothing";
		dictionary["Bataryadayken zamanın %{0}'inde RTX uyanıktı; boştayken ortalama {1} W çekti. Bu, ortalama yaklaşık {2} W ek pil tüketimi demek."] = "On battery the RTX was awake {0} % of the time and drew {1} W on average while idle. That is about {2} W of extra battery drain on average.";
		dictionary["Batarya profilinde (‘{3}’) GPU modunu Eco yap; RTX tamamen kapanır."] = "Set the GPU mode of the battery profile ('{3}') to Eco; the RTX then powers off completely.";
		dictionary["‘{0}’ için Eco'yu aç"] = "Turn on Eco for '{0}'";
		dictionary["Boşta fan gereğinden hızlı dönüyor"] = "Fans spin faster than needed at idle";
		dictionary["Düşük yükte ve ortalama {1} °C'de CPU fanı ortalama {0} rpm döndü (toplam {2} dk)."] = "At low load and {1} °C on average, the CPU fan ran at {0} rpm on average ({2} min in total).";
		dictionary["‘{3}’ profilinde 60 °C'ye kadar fanı düşük tutan sessiz eğriyi uygula; 70 °C üstü korumalı kalır."] = "Apply the quiet curve to the '{3}' profile: it keeps the fan low up to 60 °C; the protection above 70 °C stays.";
		dictionary["Sessiz eğriyi uygula"] = "Apply the quiet curve";
		dictionary["‘{0}’ profilinde CPU güç payı kullanılmıyor"] = "Unused CPU power headroom in '{0}'";
		dictionary["Yüksek yükte (toplam {3} dk) CPU ortalama {1} °C, zamanın %95'inde {2} °C altında kaldı; sürekli güç sınırı {4} W."] = "Under high load ({3} min in total) the CPU averaged {1} °C and stayed below {2} °C 95 % of the time; the sustained power limit is {4} W.";
		dictionary["Sürekli güç sınırını {5} W'a çıkarmak performansı artırır; sıcaklık hâlâ güvenli kalır."] = "Raising the sustained power limit to {5} W improves performance while the temperature stays safe.";
		dictionary["Sürekli gücü {0} W yap"] = "Set sustained power to {0} W";
		dictionary["‘{0}’ profilinde sıcaklık hedefi performansı sınırlıyor"] = "The temperature target limits performance in '{0}'";
		dictionary["Yüksek yükte sıcaklık hedefe yaslanıyor: hedef {1} °C, zamanın %95'inde {2} °C'ye çıktı (toplam {3} dk)."] = "Under high load the temperature leans on the target: target {1} °C, it reached {2} °C 95 % of the time ({3} min in total).";
		dictionary["Hedefi {4} °C'ye çıkarırsan CPU daha az yavaşlar; fan biraz daha yüksek çalışabilir."] = "Raising the target to {4} °C makes the CPU slow down less; the fan may run a little higher.";
		dictionary["Hedefi {0} °C yap"] = "Set the target to {0} °C";
		dictionary["GPU yük altında sıcak"] = "The GPU runs hot under load";
		dictionary["GPU yükteyken (toplam {2} dk) ortalama {0} °C, en yüksek {1} °C."] = "While the GPU was loaded ({2} min in total) it averaged {0} °C and peaked at {1} °C.";
		dictionary["70 °C üstünde fan eğrisini yükselt; Dynamic Boost'u düşürmek de ısıyı azaltır."] = "Raise the fan curve above 70 °C; lowering Dynamic Boost also reduces heat.";
		dictionary["‘{1}’ için Boost'u {0} W yap"] = "Set Boost to {0} W for '{1}'";
		dictionary["GPU sıcaklık hedefi performansı sınırlıyor"] = "The GPU temperature target limits performance";
		dictionary["‘{0}’ profilinde GPU yükteyken zamanın %95'inde {2} °C'ye çıkıyor; hedef {1} °C (toplam {3} dk)."] = "In '{0}' the GPU reaches {2} °C 95 % of the time under load; the target is {1} °C ({3} min in total).";
		dictionary["Hedefi {4} °C'ye çıkarmak GPU'nun yavaşlamasını azaltır."] = "Raising the target to {4} °C makes the GPU slow down less.";
		dictionary["Hedefi {0} °C yap"] = "Set the target to {0} °C";
		dictionary["Bataryada daha verimli bir profil var"] = "A more efficient profile exists for battery";
		dictionary["Bataryada boştayken ‘{0}’ ortalama {1} W, ‘{2}’ ise {3} W çekti (fark {4} W)."] = "Idle on battery, '{0}' drew {1} W on average and '{2}' drew {3} W (a difference of {4} W).";
		dictionary["Batarya profilini ‘{2}’ yap."] = "Set the battery profile to '{2}'.";
		dictionary["Batarya profili: {0}"] = "Battery profile: {0}";
		dictionary["Bellek sık sık dolu"] = "Memory is often full";
		dictionary["Ortalama bellek kullanımı %{0}, en yüksek %{1}."] = "Average memory use is {0} %, peak {1} %.";
		dictionary["Kullanmadığın uygulamaları kapat; bellek dolunca bilgisayar yavaşlar ve daha çok ısınır."] = "Close apps you are not using; when memory fills up the PC slows down and runs hotter.";
		dictionary["Ayarların verilere göre dengeli görünüyor"] = "Your settings look balanced for the data so far";
		dictionary["Kayıtlı {0} saatlik kullanımda dikkat çeken bir sorun bulunmadı."] = "Nothing stood out in {0} hours of recorded use.";
		dictionary["Daha fazla kullanım verisi biriktikçe öneriler güncellenir."] = "Advice gets updated as more usage data builds up.";
		dictionary["Windows açılırken, sen oturum açmadan önce, küçük bir görev (SYSTEM yetkisiyle) seçili açılış profilini uygular; böylece açılışın ilk dakikasındaki yüksek fan hızı ve sıcaklık azalır. Görev, uygulamanın Program Files altındaki korumalı bir kopyasını kullanır (normal bir uygulama o dosyayı değiştiremez); kopyayı uygulama kendisi güncel tutar. Oturum açılınca uygulama bu ayarı tekrar yazmaz, devralır. Kapatınca görev ve kopya silinir. Açılış ayarı 'Bilgisayarın kendi ayarı' ise hiçbir şey yapılmaz. Eco, oturum açıldıktan sonra uygulanır."] = "Before you sign in, while Windows starts, a small task (running as SYSTEM) applies the chosen startup profile, which reduces the high fan speed and temperature in the first minute. The task uses a protected copy of the app under Program Files (a normal app cannot change that file), and the app keeps that copy up to date itself. After sign-in the app takes the state over instead of writing it again. Turning it off removes the task and the copy. If the startup setting is 'The laptop's own setting', nothing is applied. Eco is applied after sign-in.";
		dictionary["Erken başlatma (oturum açılmadan önce)"] = "Early start (before sign-in)";
		dictionary["Windows açılırken, daha sen oturum açmadan, seçili açılış profilinin güç ve fan ayarları uygulanır; ilk dakikadaki yüksek fan ve sıcaklık azalır."] = "While Windows starts, before you even sign in, the chosen startup profile's power and fan settings are applied; the high fan and temperature in the first minute are reduced.";
		dictionary["Etkin"] = "On";
		dictionary["Kapalı"] = "Off";
		dictionary["son açılışta uygulanan: {0} ({1})"] = "applied at last boot: {0} ({1})";
		dictionary["Açıp kapatmak için uygulamayı yönetici olarak aç (Windows ile başlat kartındaki düğme)."] = "Reopen the app as administrator to change this (button on the Start with Windows card).";
		dictionary["Profil, oturum açılmadan önce uygulanmıştı: {0}."] = "The profile was already applied before sign-in: {0}.";
		dictionary["Erken başlatma kopyası bu sürümle güncellendi."] = "The early-start copy was updated to this version.";
		dictionary["Erken başlatma etkinleştirildi."] = "Early start turned on.";
		dictionary["Erken başlatma kapatıldı."] = "Early start turned off.";
		dictionary["Erken başlatma etkin: bir sonraki açılışta profil oturum açılmadan önce uygulanır."] = "Early start is on: at the next boot the profile is applied before sign-in.";
		En = dictionary;
	}
}
