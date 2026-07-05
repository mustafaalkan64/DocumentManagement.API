# Doküman Arama ve Yükleme Mimarisi

## Genel Bakış

Bu mimari, React tabanlı kullanıcı arayüzü ile .NET tabanlı Web API arasında çalışan ve PostgreSQL Full-Text Search (FTS) altyapısını kullanan bir doküman yönetim sistemini göstermektedir. Sistem, kullanıcıların doküman yüklemesine ve yüklenen dokümanlar üzerinde hızlı tam metin araması yapmasına olanak sağlar.

---

# Mimari Bileşenleri

## 1. Kullanıcı Arayüzü (React)

React uygulaması, kullanıcı ile sistem arasındaki etkileşimi sağlar.

### Arama Modülü

Kullanıcı arama kutusuna bir metin girer.

```
Arama Kutusu
        │
        ▼
POST /api/documents/search
```

Girilen arama metni REST API üzerinden sunucuya gönderilir.

### Doküman Yükleme Modülü

Kullanıcı dosyasını seçerek sisteme yükler.

```
Yükleme Formu
        │
        ▼
POST /api/documents/upload
```

Yüklenen dosya API katmanına iletilir.

---

# 2. Doküman Yönetim API (.NET)

Sistemin iş kurallarının bulunduğu katmandır.

## DocumentsController

API'nin giriş noktasıdır.

Görevleri:

* HTTP isteklerini almak
* Doğrulama yapmak
* Servis katmanını çağırmak
* Sonucu istemciye döndürmek

Controller doğrudan veritabanına erişmez.

```
DocumentsController
        │
        ▼
IDocumentService
```

---

## IDocumentService

İş mantığının soyutlandığı servis arayüzüdür.

Başlıca görevleri:

* Dosya yükleme işlemleri
* Doküman kayıt işlemleri
* Full Text Search sorguları
* Hash hesaplama
* Tekrar yüklenen dosyaların kontrolü

---

## DocumentService

IDocumentService implementasyonudur.

Başlıca sorumlulukları:

* Dosyayı işlemek
* İçeriği okumak
* SearchVector oluşturmak
* Hash hesaplamak
* PostgreSQL'e kayıt yapmak
* Arama sorgularını çalıştırmak

```
DocumentService
        │
        ▼
AppDbContext
```

---

## AppDbContext

Entity Framework Core üzerinden PostgreSQL ile iletişim kurar.

Görevleri:

* CRUD işlemleri
* LINQ sorguları
* Full Text Search fonksiyonları
* Transaction yönetimi

---

# 3. PostgreSQL Veritabanı

Verilerin kalıcı olarak saklandığı katmandır.

## Document Tablosu

Dokümanlara ait temel bilgiler tutulur.

Örnek alanlar:

* Id
* FileName
* Content
* SearchVector
* Hash
* CreatedDate

---

## SearchVector Alanı

PostgreSQL Full Text Search tarafından kullanılan özel alandır.

Örneğin:

```
to_tsvector('turkish', Content)
```

Bu alan üzerinde GIN Index bulunmaktadır.

Avantajları:

* Çok hızlı arama
* Kelime kökü desteği
* Büyük veri kümelerinde yüksek performans

---

## GIN Index

SearchVector alanı üzerinde oluşturulur.

Örneğin:

```sql
CREATE INDEX IX_Document_SearchVector
ON Documents
USING GIN(SearchVector);
```

Bu indeks sayesinde milyonlarca kayıt arasında milisaniyeler içerisinde arama yapılabilir.

---

## Hash Kontrolü

Her yüklenen dosya için SHA-256 gibi bir hash değeri hesaplanır.

Örnek:

```
SHA256(File)
=
A85F3D9C...
```

Yeni dosya yüklenirken:

1. Hash hesaplanır.
2. Veritabanındaki hash ile karşılaştırılır.
3. Aynı hash varsa dosya tekrar kaydedilmez.

Bu yöntem:

* Depolama alanı tasarrufu sağlar.
* Mükerrer dosyaları engeller.
* Veri bütünlüğünü artırır.

---

# Arama İş Akışı

1. Kullanıcı React arayüzünde arama yapar.
2. İstek `/api/documents/search` endpoint'ine gönderilir.
3. `DocumentsController` isteği alır.
4. `DocumentService` çağrılır.
5. Entity Framework Core üzerinden PostgreSQL Full Text Search sorgusu çalıştırılır.
6. GIN Index kullanılarak en uygun sonuçlar bulunur.
7. Sonuçlar JSON olarak React uygulamasına döndürülür.
8. Kullanıcı arama sonuçlarını görüntüler.

---

# Doküman Yükleme İş Akışı

1. Kullanıcı dosya seçer.
2. Dosya `/api/documents/upload` endpoint'ine gönderilir.
3. `DocumentsController` isteği alır.
4. `DocumentService` dosyanın hash değerini hesaplar.
5. PostgreSQL'de aynı hash değeri kontrol edilir.
6. Eğer dosya daha önce yüklenmemişse:

   * İçerik okunur.
   * SearchVector oluşturulur.
   * Veritabanına kaydedilir.
7. İşlem sonucu kullanıcıya bildirilir.

---

# Katmanlar Arası İletişim

```
React UI
      │
      ▼
DocumentsController
      │
      ▼
IDocumentService
      │
      ▼
DocumentService
      │
      ▼
AppDbContext
      │
      ▼
PostgreSQL
      │
      ├── Document Table
      ├── SearchVector
      └── GIN Index
```

---

# Mimarinin Avantajları

* Katmanlı (Layered Architecture) yapı sayesinde düşük bağımlılık.
* Controller ve iş mantığının ayrılması ile sürdürülebilir kod yapısı.
* Entity Framework Core ile kolay veri erişimi.
* PostgreSQL Full Text Search kullanımı sayesinde yüksek performanslı metin aramaları.
* GIN Index ile büyük veri kümelerinde hızlı sorgulama.
* Hash kontrolü ile mükerrer dosya yüklemenin engellenmesi.
* REST API sayesinde farklı istemciler (React, Blazor, Mobil vb.) tarafından kolay entegrasyon.
* Genişletilebilir servis mimarisi sayesinde yeni özelliklerin kolay eklenebilmesi.

---

# Özet

Bu mimari; React tabanlı modern bir kullanıcı arayüzü, .NET servis katmanı ve PostgreSQL Full Text Search altyapısını bir araya getirerek yüksek performanslı, ölçeklenebilir ve sürdürülebilir bir doküman yönetim sistemi sunmaktadır. Servis katmanının soyutlanması, GIN indeksleriyle optimize edilen arama performansı ve hash tabanlı mükerrer dosya kontrolü sayesinde sistem hem geliştirilebilir hem de kurumsal ölçekte kullanılabilir bir yapı sağlamaktadır.
