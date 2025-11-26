# Ayarlar Sayfası - Özellikler Listesi

## 🔧 Ayarlar Sayfasında Olabilecek Özellikler

### 1. **Profil Bilgileri (Profile Information)**
**Endpoint:** `POST /api/Customer/GetCustomerById` (kendi ID'si ile)
**Update Endpoint:** `POST /api/Customer/UpdateCustomer`

- **Ad (CustomerName)** - Düzenlenebilir
- **Soyad (CustomerSurname)** - Düzenlenebilir
- **E-posta (CustomerEmail)** - Düzenlenebilir (kullanıcı adı olarak da kullanılıyor)
- **Telefon (CustomerPhoneNumber)** - Düzenlenebilir
- **TC Kimlik No (CustomerIdentityNumber)** - Sadece görüntüleme (güvenlik)

### 2. **Şifre İşlemleri (Password Management)**
**Endpoint:** `POST /api/Customer/ChangePassword`

- **Şifre Değiştirme**
  - Mevcut şifre
  - Yeni şifre
  - Yeni şifre tekrar
- **Şifre Gereksinimleri Gösterimi**
  - Minimum 6 karakter
  - Güçlü şifre önerileri

### 3. **Üyelik Bilgileri (Membership Information)**
**Endpoint:** Yeni endpoint oluşturulacak - `GET /api/Customer/GetMyMembershipInfo`

- **Üyelik Başlangıç Tarihi** (CustomerRegistrationStartDate)
- **Üyelik Bitiş Tarihi** (CustomerRegistrationFinishDate)
- **Kalan Gün Sayısı** (hesaplanacak)
- **Üyelik Durumu** (Aktif/Pasif/Süresi Dolmuş)
- **Üyelik Süresi** (ay cinsinden)

### 4. **Güvenlik Ayarları (Security Settings)**
- **Oturum Yönetimi**
  - Aktif oturumlar listesi
  - Tüm cihazlardan çıkış yap
- **İki Faktörlü Doğrulama** (gelecekte eklenebilir)
- **Giriş Geçmişi** (gelecekte eklenebilir)

### 5. **Bildirim Tercihleri (Notification Preferences)**
**Not:** Şu an için model yok, gelecekte eklenebilir

- **E-posta Bildirimleri**
  - Program güncellemeleri
  - Üyelik bitiş uyarıları
  - Hedef hatırlatmaları
- **SMS Bildirimleri** (gelecekte)
- **Push Bildirimleri** (gelecekte)

### 6. **Gizlilik Ayarları (Privacy Settings)**
- **Profil Görünürlüğü**
  - Antrenörlerin görebileceği bilgiler
  - Diğer üyelerin görebileceği bilgiler
- **Veri Paylaşımı**
  - İstatistiklerin paylaşımı
  - İlerleme verilerinin paylaşımı

### 7. **Uygulama Tercihleri (App Preferences)**
- **Dil Seçimi** (Türkçe/İngilizce)
- **Tema** (Açık/Koyu mod)
- **Birimler**
  - Kilo birimi (kg/lbs)
  - Uzunluk birimi (cm/inch)
- **Tarih Formatı** (GG/AA/YYYY vs AA/GG/YYYY)

### 8. **Hesap İşlemleri (Account Actions)**
- **Hesap Silme**
  - Uyarı mesajı
  - Onay mekanizması
  - Veri silme politikası bilgisi
- **Veri İndirme** (GDPR uyumluluğu için)
  - Tüm verilerin JSON/CSV formatında indirilmesi

### 9. **Yardım ve Destek (Help & Support)**
- **SSS (Sık Sorulan Sorular)**
- **İletişim Bilgileri**
  - Spor salonu telefonu
  - E-posta adresi
  - Adres
- **Hata Bildirimi**
- **Geri Bildirim Gönderme**

### 10. **Hakkında (About)**
- **Uygulama Versiyonu**
- **Kullanım Şartları**
- **Gizlilik Politikası**
- **Lisans Bilgileri**

---

## 📡 Mevcut Endpoint'ler

| Endpoint | Method | Açıklama |
|----------|--------|----------|
| `/api/Customer/GetCustomerById` | POST | Profil bilgilerini getir |
| `/api/Customer/UpdateCustomer` | POST | Profil bilgilerini güncelle |
| `/api/Customer/ChangePassword` | POST | Şifre değiştir |

---

## 🆕 Oluşturulması Gereken Endpoint'ler

1. **`GET /api/Customer/GetMyMembershipInfo`** - Üyelik bilgileri ve kalan gün
2. **`POST /api/Customer/UpdateProfile`** - Sadece profil bilgilerini güncelle (şifre hariç)
3. **`POST /api/Customer/DeleteAccount`** - Hesap silme (isteğe bağlı)

---

## 💡 Öneriler

1. **Profil Güncelleme:** E-posta değişikliğinde kullanıcı adının da güncellenmesi gerekir
2. **Üyelik Uyarıları:** Kalan gün 30'dan azsa uyarı göster
3. **Güvenlik:** TC Kimlik No gibi hassas bilgiler sadece görüntüleme modunda olmalı
4. **Validasyon:** Telefon ve e-posta formatları kontrol edilmeli
5. **Responsive:** Mobil uyumlu ayarlar sayfası tasarımı

