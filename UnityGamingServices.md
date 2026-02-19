# Unity Gaming Services (UGS) + Multiplayer Ders Notu

Bu doküman, UGS ve multiplayer yapısını hiç bilmeyen bir geliştiriciye öğretmek için hazırlanmıştır.
Amaç: Main Menu tarafında oyuncu verisini güvenli şekilde gösterip, Play butonundan sonra oyuncuyu doğru karakter ve doğru sunucu verisiyle oyuna almak.

---

## 1) Büyük Resim: Neyi Çözmeye Çalışıyoruz?

Bir online oyunda genelde iki farklı veri dünyası vardır:

1. **UI/Görüntü Dünyası (Client tarafı)**
   - Main menu’de oyuncunun ismi, level’ı, gold/elmas değeri gösterilir.
   - Bu veriler kullanıcıya gösterim içindir.

2. **Gerçeklik/Güvenlik Dünyası (Server tarafı)**
   - Oyuncunun gerçek parası, level’ı, envanteri server tarafında doğrulanır.
   - Oyun kuralları server tarafından uygulanır.

Temel prensip:
- **Client gösterir, server karar verir.**

---

## 2) UGS Nedir?

UGS (Unity Gaming Services), Unity’nin hazır backend servisleridir.
Kendi backend sunucunu sıfırdan yazmadan, kimlik, veri saklama, ekonomi, matchmaking gibi ihtiyaçları çözersin.

En önemli servisler:
- **Authentication**: Oyuncu kimliği (login)
- **Cloud Save**: Oyuncu verisi saklama (name, level, xp gibi)
- **Economy**: Para birimleri (gold, gem), inventory
- **Cloud Code**: Server-side script/iş mantığı
- **Lobby / Matchmaker / Relay**: Eşleşme ve bağlantı akışı

---

## 3) Doğru Multiplayer Mimari Zihniyeti

### 3.1 Server Authoritative Nedir?

Server authoritative, kritik kararların server’da verildiği modeldir.

Örnek:
- Yanlış: Client “benim gold’um 999999” deyip gönderiyor ve kabul ediliyor.
- Doğru: Server kendi kayıtlarından gold değerini okuyor.

### 3.2 Neden Önemli?

- Hileyi zorlaştırır.
- Veri tutarlılığı sağlar.
- Rekabetçi oyunlarda adaleti korur.

---

## 4) Senaryonun Uçtan Uca Akışı

Aşağıdaki akış gerçek projelerde en sağlam yaklaşımdır:

1. Oyun açılır.
2. Player Authentication ile giriş yapar.
3. Main Menu açılırken Cloud Save + Economy’den veri çekilir.
4. Kullanıcı karakter seçer.
5. Play’e basınca matchmaking başlar.
6. Oyun sunucusuna bağlanırken ConnectionData içinde **minimal** veri gönderilir (ör. selectedCharacterId + join token).
7. Server token’ı doğrular, gerçek player state’i server/UGS’den okur.
8. Server oyuncuyu doğru karakterle spawn eder.

---

## 5) Hangi Veri Nerede Saklanmalı?

### Client tarafında (geçici/gösterim)
- Seçili karakter ID
- Menüde gösterilecek snapshot (name/level/gold/gem)
- Son bilinen cache

### Server/UGS tarafında (gerçek kaynak)
- Gold, gem
- Level/xp
- Envanter
- Match sonucu ile gelen ödül/ceza

Kural:
- **Ekonomi ve progression hiçbir zaman client’tan “gerçek veri” olarak alınmaz.**

---

## 6) UGS Servisleri Ayrıntılı

## 6.1 Authentication

Ne işe yarar:
- Oyuncuyu tekil bir ID ile tanımlar.

Başlangıç için:
- Anonymous Sign-In ile başlanabilir.
- Sonradan Apple/Google/Steam hesapları ile link yapılır.

İyi pratikler:
- Oturum açık mı kontrol et.
- Oturum süresi dolarsa yeniden login akışı uygula.

---

## 6.2 Cloud Save

Ne işe yarar:
- Oyuncu bazlı basit verileri saklar.

Örnek alanlar:
- name
- avatarId
- level
- xp
- selectedCharacterId (opsiyonel)

Dikkat:
- Cloud Save’e yazılan her şeyin güvenlik modelini düşün.
- Kritik hesaplamaları client’ta değil Cloud Code’da yapmak daha güvenli olur.

---

## 6.3 Economy

Ne işe yarar:
- Currency (gold/gem) ve inventory yönetimi.

Önemli prensip:
- Para artırma/azaltma işlemleri server-side kurallarla yapılmalı.

Örnek:
- Match kazandıysan +100 gold, bunu Cloud Code/Economy üzerinden uygula.

---

## 6.4 Cloud Code

Ne işe yarar:
- Server-side iş kuralları.

Nerede kullanılır:
- Join token oluşturma
- Match sonucu ödül dağıtma
- Günlük görev kontrolü
- Ekonomi mutasyonları

Neden kritik:
- Client’a güvenmeyi azaltır.

---

## 6.5 Lobby / Matchmaker / Relay (Kısa bakış)

- **Lobby**: Oyuncuların bir araya geldiği bekleme odası.
- **Matchmaker**: Benzer oyuncuları eşleştirir.
- **Relay**: Oyuncuların NAT/firewall problemlerini aşarak bağlanmasını kolaylaştırır.

Pratikte:
- Play butonunda Matchmaker ticket açılır.
- Eşleşme bulununca Relay allocation/join code alınır.
- Client ve host/server bu bilgiyle oyuna bağlanır.

---

## 7) Main Menu Veri Çekme Akışı (Adım Adım)

1. `UnityServices.InitializeAsync()`
2. `Authentication` ile sign-in
3. Cloud Save’den profile/progression çek
4. Economy’den currency çek
5. Tek bir `PlayerStateSnapshot` modelinde birleştir
6. UI’ı bu modelle doldur
7. İstersen local cache’e yaz

Bu model neden iyi:
- UI tek kaynaktan beslenir.
- Kod okunur olur.
- Hata ayıklaması kolaylaşır.

---

## 8) Play ve Bağlantı Akışı (ConnectionData Doğru Kullanımı)

ConnectionData, bağlanma anında gönderilen küçük `byte[]` veridir.

Doğru kullanım:
- Sadece gerekli küçük veriler gönder:
  - selectedCharacterId
  - join token
  - build version (opsiyonel)

Yanlış kullanım:
- Tüm player state’i JSON olarak şişirip göndermek.
- Gold/level gibi kritik değerleri client’tan taşımak.

---

## 9) Join Token Mantığı

### Neden token?

Client’ın “ben buyum” demesini tek başına güvenilir kabul etmeyiz.
Token ile server, isteğin gerçek bir oyuncudan geldiğini doğrular.

### Akış

1. Client, Play’de Cloud Code’dan token ister.
2. Cloud Code token üretir (kısa ömür, playerId bağlı).
3. Client token’ı ConnectionData ile yollar.
4. Server approval’da token doğrular.
5. Geçerliyse onay verir.

---

## 10) Oyun Sunucusunda Spawn ve State Uygulama

Bağlantı onaylandıktan sonra:

1. Server, client için doğrulanmış state’i map’ten alır.
2. Player prefab spawn eder.
3. Seçili karakteri uygular.
4. Level/economy gibi verileri server-truth olarak set eder.

Not:
- Eğer karakter modeli sadece görselse, modelin `NetworkObject` olmasına gerek yok.
- Sadece Player networked olabilir; karakter görseli client’larda aynı ID’ye göre oluşturulabilir.

---

## 11) Veri Modelleme Önerisi

`PlayerStateSnapshot` gibi bir birleşik model kullanmak çok faydalıdır.

İç gruplar önerisi:
- Identity: playerId, name, selectedCharacterId
- Progression: level, xp
- Economy: gold, gem

Ama bu model:
- Veri taşıma modeli olmalı.
- İş kuralları bu modelin içine yığılmamalı.

---

## 12) Güvenlik Kontrol Listesi

- Client’tan gelen tüm veriyi doğrula.
- ConnectionData parse hatalarını yönet.
- Payload boyutunu kontrol et.
- Version mismatch’i kontrol et.
- Invalid token’da bağlantıyı reddet.
- Currency değişimlerini server-side yap.
- Kritik event’leri logla.

---

## 13) Hata Senaryoları ve Dayanıklılık

Main menu için:
- Ağ yoksa cache göster, “senkron bekleniyor” durumunu belirt.
- Cloud Save başarısızsa varsayılan değerle UI aç, retry ver.

Bağlantı için:
- Approval başarısızsa kullanıcıya net reason göster.
- Token expired ise tekrar token aldır.

---

## 14) Versionlama ve Uyumluluk

Live oyunda istemci sürümleri farklı olabilir.

Öneri:
- ConnectionData içinde `buildVersion` taşı.
- Server supported version listesi tut.
- Uyuşmazlıkta net hata göster (zorunlu güncelleme).

---

## 15) Performans ve Ölçek

- Main menu açılışında bağımsız istekleri paralel çek (profil + economy).
- Gereksiz sık polling yapma.
- Tekrarlayan veriler için kısa süreli cache kullan.
- Ağ paketlerini küçük tut.

---

## 16) Sık Yapılan Hatalar

1. Tüm oyunu client-trust modelde kurmak.
2. ConnectionData’ya gereksiz büyük JSON basmak.
3. Approval’da başarısız durumda response set etmeyi unutmak.
4. Ekonomi mutasyonunu client’ta yapmak.
5. Hata durumunda kullanıcıya hiç bilgi vermemek.

---

## 17) MVP Yol Haritası (Uygulanabilir Plan)

Aşama 1 (Temel):
- Authentication
- Cloud Save + Economy’den menu verisi göster
- Character seçimi local store

Aşama 2 (Güvenli giriş):
- Play’de Cloud Code’dan join token
- ConnectionData ile token + characterId
- Approval’da doğrulama

Aşama 3 (Üretim kalitesi):
- Retry/backoff
- Offline cache
- Telemetry/logging
- Version control

---

## 18) Kısa Özet

- UGS, backend’i hızlı kurmak için güçlü bir servis setidir.
- Main menu verisi UGS’den okunur, ama gerçek otorite server’dır.
- Oyuna girişte ConnectionData minimal tutulur.
- Player state’in kritik parçaları server/Cloud Code/Economy ile doğrulanır.
- Böylece hem güvenli hem ölçeklenebilir bir multiplayer temel elde edilir.

---

## 19) Sonraki Adım (Pratik)

Bu dokümandan sonra en faydalı uygulama sırası:
1. Authentication + menu data yükleme
2. Character seçimi + local snapshot
3. Join token üretimi
4. Approval doğrulaması + spawn
5. Match sonu economy güncelleme

Bu 5 adımı düzgün kurarsan, üretime yakın bir temel multiplayer omurgan olur.

---

## 20) Örnek Yapı (Kopyala-Uyarla Şablon)

Bu bölüm, anlattığımız mimarinin somut bir örnek iskeletidir.

### 20.1 Önerilen Klasör Yapısı

```text
Assets/_GAME/Scripts/
   Core/
      Bootstrap/
         GameBootstrapper.cs
   Services/
      Ugs/
         UgsInitializer.cs
         UgsAuthService.cs
         UgsProfileService.cs
         UgsEconomyService.cs
         UgsCloudCodeService.cs
   PlayerState/
      Models/
         PlayerStateSnapshot.cs
         JoinConnectionPayload.cs
      MainMenu/
         MainMenuPlayerStateStore.cs
         MainMenuPlayerStateBootstrapper.cs
   UI/
      Menu/
         MainMenuPresenter.cs
         MultiplayerManagerMenu.cs
   Networking/
      Server/
         ServerApprovalHandler.cs
         ServerPlayerSpawnService.cs
```

---

### 20.2 Sınıf Sorumlulukları (Kim ne yapar?)

#### `UgsInitializer`
- Unity Services başlatır.
- Oyun boyunca tek sefer initialize garantisi verir.

#### `UgsAuthService`
- Authentication sign-in yönetir.
- Gerekirse tekrar oturum açma yapar.

#### `UgsProfileService`
- Cloud Save üzerinden isim/level/xp gibi verileri okur.

#### `UgsEconomyService`
- Economy üzerinden gold/gem bakiyesini okur.

#### `UgsCloudCodeService`
- `CreateJoinToken` ve `ValidateJoinToken` gibi endpoint çağrılarını yapar.

#### `MainMenuPlayerStateStore`
- Main menu’nun okuduğu tek state kaynağıdır.
- UI doğrudan UGS çağırmaz; store’dan beslenir.

#### `MainMenuPlayerStateBootstrapper`
- Menu açıldığında profile + economy verisini toplayıp store’a yazar.

#### `MultiplayerManagerMenu`
- Oyuncu karakter seçimi + Play butonu yönetimi.
- Play’de join token ister, payload hazırlayıp client bağlantısını başlatır.

#### `ServerApprovalHandler`
- ConnectionData parse eder.
- Token doğrular.
- Geçerli oyuncular için bağlantı onayı verir.

#### `ServerPlayerSpawnService`
- Onaylı client için player prefab spawn eder.
- Seçili karakteri player’a uygular.

---

### 20.3 Veri Modelleri Örneği

```csharp
[System.Serializable]
public sealed class PlayerStateSnapshot
{
      public string PlayerId;
      public string Name;
      public int Level;
      public int Xp;
      public long Gold;
      public long Gem;
      public ulong SelectedCharacterId;
}
```

```csharp
[System.Serializable]
public struct JoinConnectionPayload
{
      public ulong CharacterId;
      public string JoinToken;
      public string BuildVersion;
}
```

Not:
- Payload minimal tutulur.
- Gold/Gem/Level payload ile taşınmaz.

---

### 20.4 Main Menu Açılış Akışı (Pseudo Code)

```csharp
await UgsInitializer.InitializeAsync();
await UgsAuthService.SignInAsync();

var profileTask = UgsProfileService.LoadProfileAsync();
var economyTask = UgsEconomyService.LoadWalletAsync();

await Task.WhenAll(profileTask, economyTask);

var snapshot = BuildSnapshot(profileTask.Result, economyTask.Result);
mainMenuStore.Set(snapshot);
mainMenuPresenter.Refresh(snapshot);
```

Amaç:
- UI’ı hızlı açmak
- Veriyi tek modelde toplamak

---

### 20.5 Play Butonu Akışı (Pseudo Code)

```csharp
async void OnClickPlay()
{
      ulong selectedCharacterId = characterSelection.CurrentId;

      var tokenResult = await UgsCloudCodeService.CreateJoinTokenAsync(selectedCharacterId);

      var payload = new JoinConnectionPayload
      {
            CharacterId = selectedCharacterId,
            JoinToken = tokenResult.Token,
            BuildVersion = Application.version
      };

      string json = JsonUtility.ToJson(payload);
      NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.UTF8.GetBytes(json);

      NetworkManager.Singleton.StartClient();
}
```

---

### 20.6 Server Approval Akışı (Pseudo Code)

```csharp
async void ApprovalCheck(Request req, Response res)
{
      var payload = Parse(req.Payload);

      if (payload == null)
      {
            Reject(res, "Payload invalid");
            return;
      }

      if (!IsSupportedVersion(payload.BuildVersion))
      {
            Reject(res, "Version mismatch");
            return;
      }

      var validation = await UgsCloudCodeService.ValidateJoinTokenAsync(payload.JoinToken, payload.CharacterId);
      if (!validation.IsValid)
      {
            Reject(res, "Token invalid");
            return;
      }

      clientStateMap[req.ClientNetworkId] = validation.ServerState;

      res.Approved = true;
      res.CreatePlayerObject = false;
}
```

---

### 20.7 Spawn Akışı (Pseudo Code)

```csharp
void OnClientConnected(ulong clientId)
{
      if (!clientStateMap.TryGetValue(clientId, out var state))
      {
            return;
      }

      var player = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
      player.SetupCharacterById(state.SelectedCharacterId);
      player.ApplyServerState(state.Level, state.Gold, state.Gem);

      player.NetworkObject.SpawnAsPlayerObject(clientId, true);
}
```

---

### 20.8 Hızlı Kontrol Listesi

- `NetworkObject` sadece Player’da var mı?
- Character seçimi payload’da var mı?
- Payload parse/doğrulama var mı?
- Token doğrulama var mı?
- Economy/progression server-truth mu?
- Approval başarısızlığında net reason dönülüyor mu?

---

### 20.9 Bu Şablonu Ne Zaman Genişletirsin?

Şu durumlarda ek bileşen ekleyebilirsin:
- Party sistemi: partyId ve leader akışı
- Dereceli mod: MMR tabanlı matchmaking
- Çok sunuculu bölge: region-aware queue
- Canlı operasyon: remote config ile feature flag

Öneri:
- Önce bu iskeleti stabil kur.
- Sonra modüler şekilde genişlet.
