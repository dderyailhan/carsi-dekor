namespace CarsiDekor.Web.Data;

/// <summary>
/// Sitenin her yerinde kullanılan iletişim bilgileri.
/// Değiştirmek istediğin bir bilgi olursa sadece bu dosyayı düzenlemen yeterli.
/// </summary>
public static class SiteInfo
{
    public const string Name = "Çarşı Dekor";

    // Ekranda görünen telefon
    public const string Phone = "0532 713 02 45";

    // Tıklayınca arama başlatan format: + ve ülke kodu ile, boşluksuz
    public const string PhoneLink = "+905327130245";

    // WhatsApp numarası: başında + ve 0 olmadan, ülke koduyla (90 ile başlar)
    public const string WhatsApp = "905327130245";

    public const string Email = "carsidekor@gmail.com";

    // Adresin parçaları (Google'a "yerel işletme" bilgisi verirken kullanılır)
    public const string Street = "Mustafa Kemal Paşa Mah. Çetin Sokak No:103";
    public const string District = "Avcılar";
    public const string City = "İstanbul";
    public const string Address = Street + " " + District + "/" + City;

    // Haritada aranacak metin: tam adres yazınca pin doğru yere düşer.
    public const string MapQuery = "Çarşı Dekor, " + Address;

    public const string WorkingHours = "Pazartesi - Cumartesi, 09:00 - 19:30";

    // Sosyal medya hesapların varsa buraya yaz; Google'a "sameAs" olarak bildirilir.
    public static readonly string[] SocialLinks = Array.Empty<string>();
}
