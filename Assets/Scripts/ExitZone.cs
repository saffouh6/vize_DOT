using UnityEngine;

public class ExitZone : MonoBehaviour
{
    private bool levelEnded;

    private void OnTriggerEnter(Collider other)
    {
        TryExit(other);
    }

    private void OnTriggerStay(Collider other)
    {
        // Puan çıkışın içinde dururken tamamlanırsa da sonraki bölüme geçsin.
        if (GameManager.instance != null && GameManager.instance.HasEnoughScore())
            TryExit(other);
    }

    private void TryExit(Collider other)
    {
        // Çıkışa sadece Player tag'li top girdiğinde kontrol yapıyorum.
        if (levelEnded || !other.CompareTag("Player") || GameManager.instance == null) return;

        if (GameManager.instance.HasEnoughScore())
        {
            levelEnded = true;
            GameManager.instance.CompleteLevel();
        }
        else
        {
            // Puan yetmiyorsa burada sadece uyarı veriyorum, sahneyi yüklemiyorum.
            GameManager.instance.ShowMessage("Yetersiz puan! Hedefe ulaş, sonra çıkışa gel.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && GameManager.instance != null &&
            GameManager.instance.HasEnoughScore() == false)
            GameManager.instance.ShowMessage("");
    }
}
