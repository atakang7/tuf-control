namespace TufControl;

public static class AdviceText
{
	public const string EcoTitle = "Bataryada RTX gereksiz uyanık kalıyor";

	public const string EcoWhy = "Bataryadayken zamanın %{0}'inde RTX uyanıktı; boştayken ortalama {1} W çekti. Bu, ortalama yaklaşık {2} W ek pil tüketimi demek.";

	public const string EcoFix = "Batarya profilinde (‘{3}’) GPU modunu Eco yap; RTX tamamen kapanır.";

	public const string EcoAction = "‘{0}’ için Eco'yu aç";

	public const string FanTitle = "Boşta fan gereğinden hızlı dönüyor";

	public const string FanWhy = "Düşük yükte ve ortalama {1} °C'de CPU fanı ortalama {0} rpm döndü (toplam {2} dk).";

	public const string FanFix = "‘{3}’ profilinde 60 °C'ye kadar fanı düşük tutan sessiz eğriyi uygula; 70 °C üstü korumalı kalır.";

	public const string FanAction = "Sessiz eğriyi uygula";

	public const string HeadTitle = "‘{0}’ profilinde CPU güç payı kullanılmıyor";

	public const string HeadWhy = "Yüksek yükte (toplam {3} dk) CPU ortalama {1} °C, zamanın %95'inde {2} °C altında kaldı; sürekli güç sınırı {4} W.";

	public const string HeadFix = "Sürekli güç sınırını {5} W'a çıkarmak performansı artırır; sıcaklık hâlâ güvenli kalır.";

	public const string HeadAction = "Sürekli gücü {0} W yap";

	public const string CapTitle = "‘{0}’ profilinde sıcaklık hedefi performansı sınırlıyor";

	public const string CapWhy = "Yüksek yükte sıcaklık hedefe yaslanıyor: hedef {1} °C, zamanın %95'inde {2} °C'ye çıktı (toplam {3} dk).";

	public const string CapFix = "Hedefi {4} °C'ye çıkarırsan CPU daha az yavaşlar; fan biraz daha yüksek çalışabilir.";

	public const string CapAction = "Hedefi {0} °C yap";

	public const string GpuHotTitle = "GPU yük altında sıcak";

	public const string GpuHotWhy = "GPU yükteyken (toplam {2} dk) ortalama {0} °C, en yüksek {1} °C.";

	public const string GpuHotFix = "70 °C üstünde fan eğrisini yükselt; Dynamic Boost'u düşürmek de ısıyı azaltır.";

	public const string GpuHotAction = "‘{1}’ için Boost'u {0} W yap";

	public const string GpuCapTitle = "GPU sıcaklık hedefi performansı sınırlıyor";

	public const string GpuCapWhy = "‘{0}’ profilinde GPU yükteyken zamanın %95'inde {2} °C'ye çıkıyor; hedef {1} °C (toplam {3} dk).";

	public const string GpuCapFix = "Hedefi {4} °C'ye çıkarmak GPU'nun yavaşlamasını azaltır.";

	public const string GpuCapAction = "Hedefi {0} °C yap";

	public const string BatTitle = "Bataryada daha verimli bir profil var";

	public const string BatWhy = "Bataryada boştayken ‘{0}’ ortalama {1} W, ‘{2}’ ise {3} W çekti (fark {4} W).";

	public const string BatFix = "Batarya profilini ‘{2}’ yap.";

	public const string BatAction = "Batarya profili: {0}";

	public const string MemTitle = "Bellek sık sık dolu";

	public const string MemWhy = "Ortalama bellek kullanımı %{0}, en yüksek %{1}.";

	public const string MemFix = "Kullanmadığın uygulamaları kapat; bellek dolunca bilgisayar yavaşlar ve daha çok ısınır.";

	public const string GoodTitle = "Ayarların verilere göre dengeli görünüyor";

	public const string GoodWhy = "Kayıtlı {0} saatlik kullanımda dikkat çeken bir sorun bulunmadı.";

	public const string GoodFix = "Daha fazla kullanım verisi biriktikçe öneriler güncellenir.";
}
