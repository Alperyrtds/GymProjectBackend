# GoalController - Üye (Customer) Tarafından Yapılabilecekler

## 📋 Mevcut Özellikler

### 1. **Hedef Ekleme**
**Endpoint:** `POST /api/Goal/AddGoal`
- Yeni hedef oluşturma
- Hedef tipi seçimi (Weight, Measurement, Exercise, General)
- Hedef adı, hedef değer, mevcut değer
- Hedef tarih belirleme
- Notlar ekleme

### 2. **Hedefleri Görüntüleme**
**Endpoint:** `GET /api/Goal/GetMyGoals`
- Tüm hedefleri listeleme
- İlerleme yüzdesi gösterimi
- Hedef durumu (Aktif/Tamamlanmış)
- Hedef tarihi ve başlangıç tarihi

### 3. **Hedef Güncelleme**
**Endpoint:** `POST /api/Goal/UpdateGoal`
- Mevcut değeri güncelleme
- Hedef değeri değiştirme
- Hedef tarihini güncelleme
- Notları düzenleme
- Otomatik tamamlanma kontrolü (hedef değere ulaşıldığında)

### 4. **Hedef Silme**
**Endpoint:** `POST /api/Goal/DeleteGoal`
- Hedefi silme

### 5. **Tamamlanan Hedefler**
**Endpoint:** `GET /api/Goal/GetCompletedGoals`
- Tamamlanan hedefleri görüntüleme
- Tamamlanma tarihleri

### 6. **Hedefi Tamamlandı Olarak İşaretleme**
**Endpoint:** `POST /api/Goal/MarkGoalAsCompleted`
- Hedefi manuel olarak tamamlandı olarak işaretleme

---

## 🎨 Frontend'de Gösterilebilecek Özellikler

### 1. **Hedefler Listesi Sayfası**
- **Aktif Hedefler** kartı
  - Hedef adı
  - Hedef tipi (Weight, Measurement, vb.)
  - Mevcut değer / Hedef değer
  - İlerleme çubuğu (progress bar)
  - İlerleme yüzdesi (%)
  - Kalan gün sayısı (TargetDate - Bugün)
  - Durum rozeti (Aktif/Yakında/Tamamlandı)

- **Tamamlanan Hedefler** kartı
  - Tamamlanan hedef sayısı
  - Son tamamlanan hedefler listesi
  - Başarı rozetleri

### 2. **Hedef Detay Sayfası**
- Hedef bilgileri
  - Hedef adı ve tipi
  - Başlangıç tarihi
  - Hedef tarih
  - Mevcut değer / Hedef değer
  - İlerleme yüzdesi
  - Kalan gün sayısı
  - Notlar
- İlerleme grafiği (zaman içinde değişim)
- Güncelleme geçmişi (isteğe bağlı)

### 3. **Yeni Hedef Ekleme Formu**
- Hedef tipi seçimi (dropdown)
  - Kilo (Weight)
  - Ölçü (Measurement)
  - Antrenman (Exercise)
  - Genel (General)
- Hedef adı input
- Mevcut değer input
- Hedef değer input
- Hedef tarih seçici (date picker)
- Notlar textarea
- Kaydet butonu

### 4. **Hedef Güncelleme Formu**
- Mevcut değer güncelleme
- Hedef değer güncelleme (isteğe bağlı)
- Hedef tarih güncelleme
- Notlar düzenleme
- Güncelle butonu

### 5. **Hedef Kartları (Dashboard/Liste)**
- **Kart Tasarımı:**
  - Hedef adı (başlık)
  - Hedef tipi badge'i
  - İlerleme çubuğu
  - Mevcut / Hedef değer
  - İlerleme yüzdesi
  - Kalan gün sayısı
  - Durum göstergesi
  - Hızlı işlem butonları (Düzenle, Sil, Tamamla)

### 6. **İlerleme Göstergeleri**
- **Progress Bar (İlerleme Çubuğu)**
  - Yüzde bazlı görsel gösterim
  - Renk kodlaması (kırmızı: 0-30%, sarı: 30-70%, yeşil: 70-100%)
  
- **Circular Progress (Dairesel İlerleme)**
  - Dairesel progress göstergesi
  - Ortada yüzde değeri

### 7. **Filtreleme ve Sıralama**
- **Filtreleme:**
  - Aktif hedefler
  - Tamamlanan hedefler
  - Hedef tipine göre (Weight, Measurement, vb.)
  - Tarihe göre (Bu ay, Bu yıl, vb.)

- **Sıralama:**
  - Tarihe göre (Yeni → Eski, Eski → Yeni)
  - İlerleme yüzdesine göre
  - Hedef tarihine göre (Yakında bitenler önce)

### 8. **Hedef İstatistikleri**
- Toplam hedef sayısı
- Aktif hedef sayısı
- Tamamlanan hedef sayısı
- Tamamlanma oranı (%)
- Ortalama tamamlanma süresi
- En çok belirlenen hedef tipi

### 9. **Hedef Kategorileri**
- **Kilo Hedefleri:**
  - Kilo verme
  - Kilo alma
  - Vücut yağ oranı azaltma
  
- **Ölçü Hedefleri:**
  - Göğüs ölçüsü artırma
  - Bel ölçüsü azaltma
  - Kol ölçüsü artırma
  - Bacak ölçüsü artırma
  
- **Antrenman Hedefleri:**
  - Bench press ağırlığı artırma
  - Squat ağırlığı artırma
  - Koşu mesafesi artırma
  - Antrenman sıklığı artırma

### 10. **Hedef Hatırlatıcıları**
- Hedef tarihine yaklaşıldığında bildirim
- Hedef güncellemesi hatırlatıcısı
- Tamamlanan hedef kutlaması

### 11. **Hızlı İşlemler**
- **Hedef Kartında:**
  - Düzenle butonu
  - Sil butonu
  - Tamamla butonu
  - Detay görüntüle butonu

### 12. **Hedef Özeti (Dashboard Widget)**
- Aktif hedef sayısı
- En yakın hedef (en yakın bitiş tarihi)
- Ortalama ilerleme yüzdesi
- Bu ay tamamlanan hedef sayısı

### 13. **Hedef Geçmişi**
- Hedef oluşturulma tarihi
- Son güncelleme tarihi
- Tamamlanma tarihi (varsa)
- İlerleme geçmişi (isteğe bağlı)

### 14. **Hedef Paylaşımı (İsteğe Bağlı)**
- Hedefi antrenöre gösterme
- Hedef ilerlemesini paylaşma

---

## 📱 Sayfa Yapısı Önerisi

### **Hedefler Ana Sayfası**
1. **Üst Kısım:**
   - "Hedeflerim" başlığı
   - Yeni Hedef Ekle butonu
   - İstatistik kartları (Toplam, Aktif, Tamamlanan)

2. **Filtreleme Bar:**
   - Aktif/Tamamlanan toggle
   - Hedef tipi dropdown
   - Sıralama dropdown

3. **Hedef Listesi:**
   - Hedef kartları (grid/liste görünümü)
   - Her kart için ilerleme çubuğu
   - Hızlı işlem butonları

### **Hedef Detay Sayfası**
1. **Üst Kısım:**
   - Hedef adı
   - Durum badge'i
   - Düzenle/Sil butonları

2. **İlerleme Bölümü:**
   - İlerleme çubuğu
   - Mevcut / Hedef değer
   - İlerleme yüzdesi
   - Kalan gün sayısı

3. **Detaylar:**
   - Hedef tipi
   - Başlangıç tarihi
   - Hedef tarih
   - Notlar

4. **Grafik:**
   - İlerleme grafiği (zaman içinde değişim)

### **Yeni Hedef Ekleme Sayfası**
1. **Form:**
   - Hedef tipi seçimi
   - Hedef adı
   - Mevcut değer
   - Hedef değer
   - Hedef tarih
   - Notlar
   - Kaydet butonu

### **Hedef Güncelleme Sayfası**
1. **Form:**
   - Mevcut değer güncelleme (ana alan)
   - Hedef değer (isteğe bağlı)
   - Hedef tarih (isteğe bağlı)
   - Notlar
   - Güncelle butonu

---

## 🎯 Kullanım Senaryoları

1. **Kullanıcı yeni bir kilo verme hedefi belirliyor**
   → AddGoal endpoint'i ile hedef oluşturulur
   → Frontend'de hedefler listesine eklenir

2. **Kullanıcı haftalık kilo ölçümü yapıyor**
   → UpdateGoal endpoint'i ile CurrentValue güncellenir
   → İlerleme yüzdesi otomatik hesaplanır
   → Hedef değere ulaşıldığında otomatik tamamlanır

3. **Kullanıcı hedeflerini görüntülüyor**
   → GetMyGoals endpoint'i ile tüm hedefler getirilir
   → İlerleme yüzdeleri gösterilir
   → Aktif/Tamamlanan ayrımı yapılır

4. **Kullanıcı bir hedefi tamamlandı olarak işaretliyor**
   → MarkGoalAsCompleted endpoint'i ile hedef tamamlanır
   → Tamamlanan hedefler listesine eklenir

5. **Kullanıcı tamamlanan hedeflerini görüntülüyor**
   → GetCompletedGoals endpoint'i ile tamamlanan hedefler getirilir
   → Başarı rozetleri gösterilir

---

## 💡 Eklenebilecek Özellikler (Gelecekte)

1. **Hedef Şablonları**
   - Önceden tanımlı hedef şablonları
   - Hızlı hedef oluşturma

2. **Hedef Grupları**
   - Hedefleri kategorilere ayırma
   - Grup bazlı görüntüleme

3. **Hedef Hatırlatıcıları**
   - Push notification
   - E-posta bildirimleri

4. **Hedef Paylaşımı**
   - Antrenöre hedef paylaşma
   - Sosyal medya paylaşımı

5. **Hedef Geçmişi**
   - İlerleme değişim geçmişi
   - Güncelleme logları

6. **Hedef Önerileri**
   - AI bazlı hedef önerileri
   - Benzer kullanıcıların hedefleri

