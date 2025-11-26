# Üye Dashboard Ekranı - Gösterilebilecek Veriler

## 📊 Dashboard Ana Ekranı İçin Veri Listesi

### 1. **İstatistik Kartları (Stat Cards)**
**Endpoint:** `GET /api/ProgressChart/GetProgressSummary`

- **Toplam Antrenman Sayısı** (`totalWorkouts`)
- **Bu Ay Yapılan Antrenman** (`thisMonthWorkouts`)
- **Toplam Antrenman Süresi** (`totalDurationHours` - saat cinsinden)
- **Toplam Hedef Sayısı** (`totalGoals`)
- **Tamamlanan Hedefler** (`completedGoals`)
- **Aktif Hedefler** (`activeGoals`)
- **Hedef Tamamlanma Oranı** (`completionRate` - %)

### 2. **Aktif Programlar (Active Programs)**
**Endpoint:** `GET /api/CustomersProgram/GetMyProgram`

- Program listesi (aktif olanlar)
- Her program için:
  - Hareket adı (`MovementName`)
  - Set/Rep bilgisi (`SetCount`, `Reps`)
  - Program başlangıç/bitiş tarihi
  - Kalan geçerlilik (`LeftValidity`)

### 3. **Son Antrenmanlar (Recent Workouts)**
**Endpoint:** `GET /api/WorkoutLog/GetMyWorkoutLogs`

- Son 5-10 antrenman kaydı
- Her kayıt için:
  - Tarih (`WorkoutDate`)
  - Hareket adı (`MovementName`)
  - Ağırlık, Set, Rep
  - Süre (`WorkoutDuration` - dakika)
  - Notlar (`Notes`)

### 4. **Aktif Hedefler (Active Goals)**
**Endpoint:** `GET /api/Goal/GetMyGoals`

- Tamamlanmamış hedefler (`IsCompleted: false`)
- Her hedef için:
  - Hedef adı (`GoalName`)
  - Hedef tipi (`GoalType` - Weight, Measurement, vb.)
  - Mevcut değer / Hedef değer
  - İlerleme yüzdesi (`ProgressPercentage`)
  - Hedef tarih (`TargetDate`)
  - Kalan gün sayısı

### 5. **En Çok Yapılan Hareketler (Top Movements)**
**Endpoint:** `GET /api/ProgressChart/GetProgressSummary` (içinde `topMovements`)

- En çok yapılan 5 hareket
- Her hareket için yapılma sayısı

### 6. **Hızlı İstatistikler (Quick Stats)**
**Endpoint:** `GET /api/ProgressChart/GetProgressSummary`

- **Bu Hafta Antrenman Sayısı** (hesaplanabilir)
- **Ortalama Antrenman Süresi** (hesaplanabilir)
- **En Son Antrenman Tarihi** (hesaplanabilir)

### 7. **Grafikler (Charts)**

#### a) **Kilo Grafiği**
**Endpoint:** `GET /api/ProgressChart/GetWeightChart`
- Kilo hedefleri zaman serisi
- Mevcut kilo vs Hedef kilo

#### b) **Ölçü Grafikleri**
**Endpoint:** `GET /api/ProgressChart/GetMeasurementChart`
- Vücut ölçüleri zaman serisi
- İlerleme yüzdesi

#### c) **Antrenman Sıklığı Grafiği**
**Endpoint:** `POST /api/ProgressChart/GetWorkoutFrequencyChart`
- Haftalık veya aylık antrenman sayıları
- Period: "weekly" veya "monthly"

#### d) **Performans Grafiği**
**Endpoint:** `POST /api/ProgressChart/GetPerformanceChart`
- Belirli bir hareket için ağırlık/set/rep ilerlemesi
- Toplam hacim (weight × sets × reps)

### 8. **Üyelik Bilgileri (Membership Info)**
**Endpoint:** `POST /api/Customer/GetCustomerById` (kendi ID'si ile)

- Üye adı soyadı
- E-posta
- Telefon
- Üyelik başlangıç/bitiş tarihi (CustomersRegistration'dan)
- Üyelik durumu (aktif/pasif)

### 9. **Tamamlanan Hedefler (Completed Goals)**
**Endpoint:** `GET /api/Goal/GetCompletedGoals`

- Tamamlanan hedefler listesi
- Tamamlanma tarihleri
- Başarı rozetleri için kullanılabilir

### 10. **Hareket İlerlemesi (Movement Progress)**
**Endpoint:** `POST /api/WorkoutLog/GetProgressByMovement`

- Belirli bir hareket için detaylı ilerleme
- Zaman içinde ağırlık artışı
- Set/Rep artışı

---

##

## 📡 API Endpoint Özeti

| Endpoint | Method | Açıklama |
|----------|--------|----------|
| `/api/ProgressChart/GetProgressSummary` | GET | Dashboard özet verileri |
| `/api/CustomersProgram/GetMyProgram` | GET | Aktif programlar |
| `/api/WorkoutLog/GetMyWorkoutLogs` | GET | Antrenman kayıtları |
| `/api/Goal/GetMyGoals` | GET | Tüm hedefler (ilerleme yüzdesi ile) |
| `/api/Goal/GetCompletedGoals` | GET | Tamamlanan hedefler |
| `/api/ProgressChart/GetWeightChart` | GET | Kilo grafiği |
| `/api/ProgressChart/GetMeasurementChart` | GET | Ölçü grafikleri |
| `/api/ProgressChart/GetWorkoutFrequencyChart` | POST | Antrenman sıklığı |
| `/api/ProgressChart/GetPerformanceChart` | POST | Performans grafiği |
| `/api/WorkoutLog/GetProgressByMovement` | POST | Hareket ilerlemesi |
| `/api/Customer/GetCustomerById` | POST | Üye bilgileri |

---

## 💡 Öneriler

1. **Dashboard yüklenirken:** `GetProgressSummary` endpoint'ini çağır (tüm özet veriler için)
2. **Lazy Loading:** Grafikler ve detaylı listeler scroll olduğunda yüklensin
3. **Caching:** İstatistikler 5-10 dakika cache'lenebilir
4. **Real-time Updates:** Yeni antrenman eklendiğinde dashboard'u güncelle
5. **Empty States:** Veri yoksa kullanıcıyı yönlendiren mesajlar göster

