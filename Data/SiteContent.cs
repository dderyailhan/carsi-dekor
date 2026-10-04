namespace CarsiDekor.Web.Data;

/// <summary>
/// Sık sorulan sorular, müşteri yorumları ve vaka çalışmaları burada durur.
/// Metinleri bu dosyadan değiştirebilirsin; veritabanı gerekmez.
/// </summary>
public static class SiteContent
{
    public record Faq(string Question, string Answer);

    // ---------- Sık sorulan sorular (5 adet) ----------
    // Cevapları kendi çalışma şekline göre kontrol et; yanlış bir söz verilmesin.
    public static readonly Faq[] Faqs =
    {
        new("Vitrin siparişi nasıl veriliyor?",
            "Telefonla, WhatsApp'tan ya da iletişim formundan bize ulaşabilirsiniz. Dükkânınızın ölçüsünü, " +
            "sergileyeceğiniz ürünleri ve beklentinizi öğrenip size tasarım önerisi ve fiyat teklifi hazırlarız."),
        new("Ürünleri dükkânımın ölçüsüne göre mi yapıyorsunuz?",
            "Evet. Vitrin, banko ve niş işlerini mağazanızın ölçüsüne göre tasarlıyor, atölyemizde marangozluk işçiliğiyle üretiyoruz."),
        new("Hangi ürünler için sergileme çözümü üretiyorsunuz?",
            "Yüzüklük, küpelik, kolyelik, bileklik, tesbihlik, manken, tabla ve döner motor standı gibi sergileme ürünlerinin yanında " +
            "vitrin, banko ve niş işleri yapıyoruz. Güncel işlerimizi Kategoriler sayfasında görebilirsiniz."),
        new("Fiyat nasıl belirleniyor?",
            "Fiyat; işin ölçüsüne, kullanılacak malzemeye, adede ve istenen detaylara göre değişir. " +
            "Net bir teklif için ölçü ve ürün bilgisini bize iletmeniz yeterli."),
        new("Teslim süresi ve montaj nasıl işliyor?",
            "Süre işin kapsamına ve yoğunluğa göre değişir; teklif aşamasında size net bir teslim tarihi veririz. " +
            "Teslimat ve montaj ayrıntılarını da sipariş öncesinde birlikte netleştiririz."),
    };

    // ---------- Müşteri yorumları ----------
    // ÖNEMLİ: Sadece GERÇEK müşteri yorumlarını yaz ve Published = true yap.
    // Published = false olan yorumlar sitede görünmez.
    public record Testimonial(string Name, string Business, string Text, bool Published);

    public static readonly Testimonial[] Testimonials =
    {
        new("[Müşteri adı]", "[Kuyumcu adı, ilçe]", "[Müşterinin kendi cümleleriyle gerçek yorumu buraya yazılacak.]", false),
        new("[Müşteri adı]", "[Kuyumcu adı, ilçe]", "[Müşterinin kendi cümleleriyle gerçek yorumu buraya yazılacak.]", false),
        new("[Müşteri adı]", "[Kuyumcu adı, ilçe]", "[Müşterinin kendi cümleleriyle gerçek yorumu buraya yazılacak.]", false),
    };

    // ---------- Vaka çalışmaları ----------
    // Gerçek bir işi anlatan şablon. Doldurup Published = true yapınca sitede görünür.
    // CategorySlug: işin ilgili olduğu kategori (iç linkleme için), ör. "vitrinler"
    public record CaseStudy(string Slug, string Title, string Place, string Problem, string Solution, string Result,
                            string CategorySlug, string? Image, bool Published);

    public static readonly CaseStudy[] CaseStudies =
    {
        new("ornek-vitrin-yenileme",
            "[İşin başlığı, ör. Kuyumcu mağazası için komple vitrin yenileme]",
            "[İlçe, İstanbul]",
            "[Müşterinin başlangıçtaki sorunu: eski vitrin, yetersiz alan, ürünlerin öne çıkmaması...]",
            "[Ne yaptınız: ölçü, malzeme, aydınlatma, üretim...]",
            "[Sonuç: teslim süresi, müşterinin memnuniyeti, önce/sonra fotoğrafı...]",
            "vitrinler", null, false),
    };
}
