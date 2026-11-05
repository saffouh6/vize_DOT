using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// Oyunun puanını, hedef puanı ve arayüz mesajlarını yöneten sınıf.
// Her seviye sahnesinde bir tane GameManager nesnesi bulunur.
public class GameManager : MonoBehaviour
{
    // Diğer scriptlerin (PlayerController gibi) GameManager'a kolayca erişmesi için
    // statik referans (basit singleton yapısı).
    public static GameManager instance;

    // Bu seviyede ulaşılması gereken puan (Inspector'dan 30 / 50 / 70 olarak ayarlanır).
    public int targetScore = 30;

    // Arayüzdeki metin alanları (Inspector'dan atanır).
    public TextMeshProUGUI scoreText;    // "Puan: X" yazısı
    public TextMeshProUGUI targetText;   // "Hedef: Y" yazısı
    public TextMeshProUGUI messageText;  // Bilgi / uyarı mesajları

    // Oyuncunun mevcut puanı. Dışarıdan doğrudan değiştirilemez (private).
    private int score;

    // Sahne yüklendiğinde Start'tan önce çalışır.
    private void Awake()
    {
        // Bu sahnedeki GameManager'ı statik referansa atıyoruz.
        instance = this;

        // Önceki seviyede oyun durdurulmuş olabilir (timeScale = 0), bu yüzden zamanı normale döndürüyoruz.
        Time.timeScale = 1f;

        // Her sahnede yeni bir GameManager olduğu için puan burada sıfırlanıyor.
        score = 0;
    }

    // İlk karede bir kez çalışır; arayüzü başlangıç haline getirir.
    private void Start()
    {
        // Referans atanmamışsa NullReferenceException oluşmasın diye null kontrolü yapılıyor.
        if (targetText != null) targetText.text = "Hedef: " + targetScore;

        // Başlangıçta mesaj alanını temizle.
        ShowMessage("");

        // Puan yazısını ilk kez güncelle.
        UpdateUI();
    }

    // Puan ekler veya çıkarır (yeşil için +10, kırmızı için -5 gönderilir).
    public void AddScore(int amount)
    {
        // Puan değişmeden önce hedefe ulaşılmış mıydı, onu saklıyoruz.
        bool hedefeUlasmisti = HasEnoughScore();

        // Puanı güncelle.
        score += amount;
        UpdateUI();

        // Hedefe ilk ulaştığım anda çıkışın yerini hatırlatıyorum.
        if (!hedefeUlasmisti && HasEnoughScore())
            ShowMessage("Hedef tamam! Çıkış kapısına git.");
        // Kırmızı nesne yüzünden hedefin altına düşülürse oyuncu uyarılır.
        else if (hedefeUlasmisti && !HasEnoughScore())
            ShowMessage("Hedef puanın altına düştün. Yeşil nesne topla.");
    }

    // Ekrandaki puan metnini günceller.
    private void UpdateUI()
    {
        if (scoreText != null) scoreText.text = "Puan: " + score;
    }

    // Puanın hedefe ulaşıp ulaşmadığını döndürür (çıkış kapısı bu kontrolü kullanır).
    public bool HasEnoughScore()
    {
        return score >= targetScore;
    }

    // Mesaj alanına verilen yazıyı yazar.
    public void ShowMessage(string message)
    {
        if (messageText != null) messageText.text = message;
    }

    // Çıkış alanında yeterli puanla çağrılır; sonraki seviyeye geçer ya da oyunu bitirir.
    public void CompleteLevel()
    {
        // Sahneler Build Settings'te Level1, Level2, Level3 sırasıyla duruyor.
        // Mevcut sahnenin sıra numarasına 1 ekleyerek sonraki sahneyi buluyoruz.
        int next = SceneManager.GetActiveScene().buildIndex + 1;

        // Build Settings'te sonraki bir sahne varsa onu yükle.
        if (next < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(next);
        else
        {
            // Sonraki sahne yoksa bu son seviyedir: bitiş mesajı gösterilir.
            ShowMessage("Oyunu Tamamladınız!");

            // Son bölüm bitince topun ve fizik nesnelerinin hareketi duruyor.
            Time.timeScale = 0f;
        }
    }
}