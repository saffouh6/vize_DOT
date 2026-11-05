using UnityEngine;

public class Collectible : MonoBehaviour
{
    // Yeşil nesne +10, kırmızı nesne -5 puan taşıyor.
    [SerializeField] private int points = 10;

    public int Points { get { return points; } }

    public void SetPoints(int value)
    {
        points = value;
    }
}
