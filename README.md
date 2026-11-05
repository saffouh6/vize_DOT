3D Top Toplama Oyunu

Ad Soyad: Saffouh Haj Obıd Alhajar
Öğrenci No: 2502020033
Oynanış videosu: https://youtu.be/1I6OGScpWGg

Oyun

Mavi bir küreyi WASD veya ok tuşlarıyla yuvarlıyoruz. Yeşil nesneler 10 puan veriyor, kırmızılar 5 puan götürüyor. Hedef puana ulaşınca sarı çıkış alanına girip sonraki seviyeye geçiyoruz. Puan yetmezse sahne değişmiyor, sadece uyarı çıkıyor. Hedefler Level 1'de 30, Level 2'de 50, Level 3'te 70. Her seviyede puan sıfırdan başlıyor. Son seviyeyi bitirince "Oyunu Tamamladınız!" yazısı çıkıyor.

Scriptler
PlayerController: GetComponent ile Rigidbody'yi alıp AddForce ile topu hareket ettiriyor. OnTriggerEnter içinde CompareTag ile yeşil ve kırmızı nesneleri ayırıyor, puanı GameManager'a gönderip nesneyi Destroy ile siliyor.
Collectible: Nesnenin puan değerini tutuyor (+10 veya -5).
GameManager: Puanı, hedefi ve ekrandaki yazıları yönetiyor. CompleteLevel() ile SceneManager üzerinden sıradaki sahneyi yüklüyor.
ExitZone: Çıkışa girilince puanı kontrol ediyor.
Sahne geçişi algoritması
Oyuncu çıkış alanına girer.
Puan hedeften küçükse uyarı gösterilir, sahne değişmez.
Yeterliyse CompleteLevel() çağrılır.
Mevcut sahnenin sırasına 1 eklenir. Böyle bir sahne varsa yüklenir ve puan sıfırlanır.
Yoksa bu son seviyedir, bitiş mesajı gösterilir.
Hata ve çözümü

Olası bir hata, nesnelere çarpılınca hiçbir şey olmamasıdır. Bunun için toplanabilirlerde Is Trigger açık olmalı, oyuncuda Rigidbody bulunmalı, Green, Red ve Exit tag'leri atanmış olmalı. Bunları kontrol ettim. Ayrıca sahneleri Build Settings'e sırayla eklemezsem LoadScene hata verirdi, onları Level1, Level2, Level3 diye ekledim.

## Çalıştırma

Projeyi Unity 6000.3.15f1 ile açın, `Assets/Scenes/Level1.unity` sahnesini seçip Play'e basın. Sahneler eksikse **Top Toplama Oyunu → Generate Assignment Scenes** menüsünü çalıştır