using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    // Inspector'dan topun hızını değiştirebiliyorum.
    public float speed = 10f;
    private Rigidbody rb;

    private void Start()
    {
        // Topun Rigidbody bileşenini alıyorum.
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        // Yatay ve dikey tuşlardan hareket yönünü buluyorum.
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(h, 0f, v);
        // Rigidbody kullandığım için top fizik kurallarına göre yuvarlanıyor.
        rb.AddForce(movement * speed);

        // Top alandan düşerse bölümü baştan yüklüyorum.
        if (transform.position.y < -10f)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.instance == null) return;

        if (other.CompareTag("Green"))
        {
            // Nesnenin puanını alıp ekliyorum, sonra nesneyi sahneden siliyorum.
            Collectible item = other.GetComponent<Collectible>();
            GameManager.instance.AddScore(item != null ? item.Points : 10);
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Red"))
        {
            // Kırmızı nesne puan düşürüyor.
            Collectible item = other.GetComponent<Collectible>();
            GameManager.instance.AddScore(item != null ? item.Points : -5);
            Destroy(other.gameObject);
        }
    }
}
