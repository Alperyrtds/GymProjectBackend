# WorkoutLogController - Kullanıcı Tarafına Eklenebilecek Özellikler

## 📋 Mevcut Özellikler
- ✅ Antrenman ekleme (`AddWorkoutLog`)
- ✅ Tüm antrenmanları görüntüleme (`GetMyWorkoutLogs`)
- ✅ Belirli hareket için ilerleme (`GetProgressByMovement`)
- ✅ Antrenman güncelleme (`UpdateWorkoutLog`)
- ✅ Antrenman silme (`DeleteWorkoutLog`)

---

## 🆕 Eklenebilecek Özellikler

### 1. **Filtreleme ve Arama**

#### a) Tarih Aralığına Göre Filtreleme
**Endpoint:** `POST /api/WorkoutLog/GetMyWorkoutLogsByDateRange`
- Bugün, bu hafta, bu ay, özel tarih aralığı
- Request: `{ "startDate": "2024-01-01", "endDate": "2024-01-31" }`

#### b) Hareket Adına Göre Arama/Filtreleme
**Endpoint:** `POST /api/WorkoutLog/GetMyWorkoutLogsByMovement`
- Hareket adına göre arama
- Request: `{ "movementName": "Bench Press" }`

#### c) Son N Antrenmanı Getirme
**Endpoint:** `GET /api/WorkoutLog/GetMyRecentWorkouts?count=10`
- Son 5, 10, 20 antrenman gibi

---

### 2. **İstatistikler ve Özetler**

#### a) Antrenman İstatistikleri
**Endpoint:** `GET /api/WorkoutLog/GetMyWorkoutStatistics`
- Toplam antrenman sayısı
- Toplam antrenman süresi (dakika/saat)
- Ortalama antrenman süresi
- Toplam hacim (weight × sets × reps)
- Ortalama hacim
- En uzun antrenman
- En kısa antrenman

#### b) Günlük/Haftalık/Aylık Özet
**Endpoint:** `POST /api/WorkoutLog/GetMyWorkoutSummary`
- Request: `{ "period": "daily|weekly|monthly", "date": "2024-01-15" }`
- O dönemdeki antrenman sayısı
- Toplam süre
- Yapılan hareketler
- Toplam hacim

#### c) En Çok Yapılan Hareketler
**Endpoint:** `GET /api/WorkoutLog/GetMyTopMovements?limit=10`
- En çok yapılan N hareket
- Her hareket için toplam yapılma sayısı
- Son yapılma tarihi

---

### 3. **Kişisel Rekorlar (Personal Records)**

#### a) Kişisel Rekorlar
**Endpoint:** `GET /api/WorkoutLog/GetMyPersonalRecords`
- **Max Weight:** Her hareket için maksimum ağırlık
- **Max Reps:** Her hareket için maksimum tekrar
- **Max Volume:** Her hareket için maksimum hacim (weight × sets × reps)
- **Max Sets:** Her hareket için maksimum set sayısı
- **Max Duration:** En uzun antrenman süresi
- Her rekor için tarih bilgisi

#### b) Belirli Hareket için Rekorlar
**Endpoint:** `POST /api/WorkoutLog/GetMovementRecords`
- Request: `{ "movementId": "..." }`
- O hareket için tüm rekorlar

---

### 4. **Antrenman Detayı ve Yönetimi**

#### a) Antrenman Detayı
**Endpoint:** `POST /api/WorkoutLog/GetWorkoutLogDetail`
- Tek bir antrenman kaydının detaylı bilgisi
- Movement bilgileri
- Tüm set/rep/weight detayları
- Notlar

#### b) Antrenman Kopyalama
**Endpoint:** `POST /api/WorkoutLog/DuplicateWorkoutLog`
- Geçmiş bir antrenmanı kopyalayıp yeni tarihle ekleme
- Request: `{ "workoutLogId": "...", "newDate": "2024-01-20" }`

---

### 5. **Gelişmiş Filtreleme**

#### a) Çoklu Filtre
**Endpoint:** `POST /api/WorkoutLog/GetMyWorkoutLogsFiltered`
- Tarih aralığı + hareket + ağırlık aralığı + süre aralığı
- Request:
```json
{
  "startDate": "2024-01-01",
  "endDate": "2024-01-31",
  "movementId": "...",
  "minWeight": 50,
  "maxWeight": 100,
  "minDuration": 30,
  "maxDuration": 120
}
```

#### b) Notlara Göre Arama
**Endpoint:** `POST /api/WorkoutLog/SearchWorkoutLogsByNotes`
- Notlar içinde arama
- Request: `{ "searchTerm": "zor" }`

---

### 6. **Antrenman Şablonları (Templates)**

#### a) Şablon Oluşturma
**Endpoint:** `POST /api/WorkoutLog/CreateWorkoutTemplate`
- Sık kullanılan antrenmanları şablon olarak kaydetme
- Request: `{ "templateName": "Göğüs Günü", "movementId": "...", "setCount": 4, "reps": 10 }`

#### b) Şablonlardan Antrenman Oluşturma
**Endpoint:** `POST /api/WorkoutLog/CreateWorkoutFromTemplate`
- Şablondan hızlıca antrenman oluşturma

#### c) Şablonları Listeleme
**Endpoint:** `GET /api/WorkoutLog/GetMyWorkoutTemplates`

---

### 7. **Haftalık/Aylık Takip**

#### a) Haftalık Antrenman Takvimi
**Endpoint:** `GET /api/WorkoutLog/GetMyWeeklyCalendar?weekStartDate=2024-01-15`
- Haftanın hangi günlerinde antrenman yapıldı
- Günlük antrenman sayıları
- Günlük toplam süre

#### b) Aylık Antrenman Takvimi
**Endpoint:** `GET /api/WorkoutLog/GetMyMonthlyCalendar?year=2024&month=1`
- Ayın hangi günlerinde antrenman yapıldı
- Günlük özetler

---

### 8. **İlerleme Takibi**

#### a) Hareket İlerleme Grafiği Verisi
**Endpoint:** `POST /api/WorkoutLog/GetMovementProgressData`
- Belirli bir hareket için zaman içinde ağırlık/rep/set değişimi
- Grafik için veri formatında

#### b) Toplam Hacim İlerlemesi
**Endpoint:** `GET /api/WorkoutLog/GetVolumeProgress?movementId=...`
- Zaman içinde toplam hacim değişimi

---

### 9. **Hızlı Erişim**

#### a) Bugünün Antrenmanları
**Endpoint:** `GET /api/WorkoutLog/GetTodayWorkouts`
- Bugün yapılan antrenmanlar

#### b) Bu Haftanın Antrenmanları
**Endpoint:** `GET /api/WorkoutLog/GetThisWeekWorkouts`

#### c) Bu Ayın Antrenmanları
**Endpoint:** `GET /api/WorkoutLog/GetThisMonthWorkouts`

---

### 10. **Antrenman Analizi**

#### a) Antrenman Sıklığı Analizi
**Endpoint:** `GET /api/WorkoutLog/GetWorkoutFrequencyAnalysis`
- Haftada kaç gün antrenman yapılıyor
- Ortalama antrenman sıklığı
- En aktif günler

#### b) Hareket Çeşitliliği
**Endpoint:** `GET /api/WorkoutLog/GetMovementDiversity`
- Kaç farklı hareket yapılmış
- En çok çeşitlilik olan dönem

---

## 🎯 Öncelikli Öneriler

### **Yüksek Öncelik:**
1. ✅ **Tarih aralığına göre filtreleme** - Çok kullanışlı
2. ✅ **Kişisel rekorlar** - Motivasyon için önemli
3. ✅ **Antrenman istatistikleri** - Dashboard için gerekli
4. ✅ **Son N antrenman** - Hızlı erişim

### **Orta Öncelik:**
5. ✅ **Antrenman detayı** - Tek bir kaydın detayı
6. ✅ **Antrenman kopyalama** - Pratik özellik
7. ✅ **Bugün/Bu hafta/Bu ay** - Hızlı erişim

### **Düşük Öncelik (Gelecekte):**
8. ⏳ **Antrenman şablonları** - İleri seviye özellik
9. ⏳ **Çoklu filtreleme** - Karmaşık ama güçlü
10. ⏳ **Haftalık/Aylık takvim** - Görselleştirme için

---

## 📝 Örnek Request Modelleri

```csharp
// Tarih aralığı
public class DateRangeRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

// Filtreleme
public class FilterWorkoutLogsRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? MovementId { get; set; }
    public string? MovementName { get; set; }
    public decimal? MinWeight { get; set; }
    public decimal? MaxWeight { get; set; }
    public int? MinDuration { get; set; }
    public int? MaxDuration { get; set; }
}

// Özet
public class WorkoutSummaryRequest
{
    public string Period { get; set; } = "weekly"; // daily, weekly, monthly
    public DateTime? Date { get; set; }
}
```

---

## 💡 Kullanım Senaryoları

1. **Kullanıcı bugünkü antrenmanını görmek istiyor**
   → `GetTodayWorkouts`

2. **Kullanıcı geçen ay ne kadar antrenman yaptığını görmek istiyor**
   → `GetMyWorkoutLogsByDateRange` veya `GetThisMonthWorkouts`

3. **Kullanıcı bench press için kişisel rekorunu görmek istiyor**
   → `GetMovementRecords`

4. **Kullanıcı en çok hangi hareketleri yaptığını görmek istiyor**
   → `GetMyTopMovements`

5. **Kullanıcı geçmiş bir antrenmanı tekrar yapmak istiyor**
   → `DuplicateWorkoutLog`

