# SzabiApp - Szabadságkezelő Rendszer Projekt Áttekintés (Részletes)

Ez a dokumentum a SzabiApp teljes architektúrájának, technológiai stékjének és üzleti logikájának részletes leírása, projekt dokumentáció generálásához.

---

## 1. A Projekt Célja és Hatóköre
A SzabiApp egy komplex vállalati megoldás, amely a humánerőforrás-gazdálkodás (HR) egyik legkritikusabb folyamatát, a szabadságok és távollétek kezelését digitalizálja. A rendszer célja a papíralapú vagy e-mail alapú ügyintézés kiváltása egy központi, transzparens felülettel.

---

## 2. Technológiai Architektúra

### Frontend (Kliens oldal)
- **Framework:** Vue 3 (Composition API) a reaktivitás és a komponens-alapú fejlesztés érdekében.
- **Nyelv:** TypeScript a típusbiztonság és a jobb kódminőség (IntelliSense támogatás) miatt.
- **Build Tool:** Vite a gyors fejlesztési ciklusokhoz (HMR).
- **Stílus:** Tailwind CSS + PostCSS a modern, reszponzív designhoz, egyedi üveghatású (Glassmorphism) stílusokkal kiegészítve (`src/style.css`).
- **Állapotkezelés:** Pinia (moduláris store-ok: `auth`, `toast`).
- **Routing:** Vue Router (kliens oldali navigáció és hozzáférés-védelem).
- **HTTP Kliens:** Axios, egyedi interceptorokkal a JWT token automatikus csatolásához és a 401-es hibák (lejárt session) kezeléséhez.

### Backend (Szerver oldal - Inferred)
- **Framework:** ASP.NET Core 8 Web API.
- **Adatbázis:** Entity Framework Core (Code-First megközelítés).
- **Security:** Microsoft.AspNetCore.Authentication.JwtBearer.
- **Data Transfer:** DTO (Data Transfer Object) minta a kliens és szerver közötti adatforgalom minimalizálására és biztonságossá tételére.

---

## 3. Adatmodellek és Üzleti Logika

### Felhasználói Szerepkörök (RBAC)
A rendszer szerepkör-alapú hozzáférés-szabályozást (RBAC) alkalmaz:
- `Employee`: Alapszintű hozzáférés, saját adatok kezelése.
- `Manager`: Beosztottak kérelmeinek jóváhagyása/elutasítása.
- `Admin`: Teljes hozzáférés a rendszer konfigurációjához (felhasználók, munkarendek, keretek).

### Entitások és DTO-k (`src/types/index.ts`)
1.  **User:** Tartalmazza a profiladatokat, szerepkört és a közvetlen vezetőt (`managerId`).
2.  **LeaveRequest:**
    - `category`: Annual (Éves), Sick (Beteg), Unpaid (Fizetés nélküli), Paternity (Apasági), Maternity (Anyasági), Other.
    - `status`: Pending, Approved, Denied, Cancelled.
    - Időintervallum tárolása (`startDate`, `endDate`) és automatikus nap-kalkuláció.
3.  **LeaveAllowance:** Éves kvóta kezelése felhasználónként, év szerint lebontva.
4.  **WorkSchedule:** Meghatározza a munkahét hosszát, ami alapvető a szabadságnapok számításánál.

---

## 4. Modulok és Komponensek Részletezése

### Hitelesítés és Biztonság
- **Auth Store:** Kezeli a bejelentkezési állapotot, a JWT token tárolását (localStorage) és a token dekódolását (`decodeJwt`).
- **Route Guards:** Megakadályozzák az illetéktelen hozzáférést a védett oldalakhoz (pl. admin felület).
- **Axios Interceptor:** Minden kimenő kéréshez hozzáadja az `Authorization: Bearer <token>` fejlécet.

### UI Komponens Réteg (`src/components/ui/`)
- **AppCalendar:** Egyedi fejlesztésű, interaktív naptár komponens. Támogatja a dátum-intervallum kijelölést, megjeleníti az ünnepnapokat és a már rögzített szabadságokat státusz-színezett pöttyökkel.
- **AppModal:** Portál-alapú modális ablakok a megerősítésekhez és adatbevitelhez.
- **AppBadge:** Színkódolt címkék a státuszok és kategóriák gyors felismeréséhez.
- **AppToastStack:** Globális értesítési rendszer a sikeres műveletekhez és hibaüzenetekhez.

### Nézetek (Views)
- **Dashboard:** Összesített statisztika (felhasznált/maradt napok), vizuális folyamatjelző sávok és a legutóbbi kérelmek listája.
- **MyLeaves:** Interaktív naptár és lista nézet váltás.
- **NewLeave:** Többlépcsős folyamat naptári kijelöléssel és validációval.
- **ReviewView (Manager):** Speciális felület a függőben lévő kérelmek listázására és tömeges vagy egyenkénti bírálatára.
- **Admin (Users/Holidays):** CRUD műveletek a rendszer törzsadataihoz.

---

## 5. Frontend Mappaszerkezet Részletesen
```text
src/
├── assets/             # Statikus erőforrások (pl. hero.png, ikonok)
├── components/
│   ├── layout/         # Alkalmazás váz (Sidebar, TopBar, reszponzív navigáció)
│   └── ui/             # Alapszintű UI építőkockák (Buttons, Inputs, Modals, Calendar)
├── router/             # Útvonalak definiálása és jogosultság-ellenőrzés
├── services/           # API hívások (auth, leaves, users, holidays, allowances)
├── stores/             # Pinia állapotkezelők (Auth, Toast)
├── types/              # Közös TypeScript interfészek és Enumok
├── utils/              # Helper függvények (dátumformázás, hiba kezelés)
└── views/              # Oldal-szintű komponensek (konténer komponensek)
    ├── admin/          # Adminisztrációs felületek
    ├── auth/           # Bejelentkezés
    ├── dashboard/      # Kezdőlap
    ├── leaves/         # Szabadságkezelés (Saját)
    └── manager/        # Vezetői jóváhagyó felület
```

---

## 6. Adatfolyam és Kommunikáció
1.  **Kérés:** A felhasználó kezdeményez egy műveletet (pl. szabadság igénylése).
2.  **Validáció:** Frontend oldali ellenőrzés (dátumok helyessége, kötelező mezők).
3.  **API Hívás:** A `leaves.service.ts` meghívja a backend végpontot az Axios példányon keresztül.
4.  **Auth:** A kérés tartalmazza a JWT tokent.
5.  **Szerver:** Feldolgozza a kérést, ellenőrzi az üzleti szabályokat (pl. van-e elég keret), majd válaszol.
6.  **Frissítés:** A frontend megkapja a választ, frissíti a Pinia store-t vagy a lokális állapotot, és visszajelzést ad (Toast).

---

## 7. Telepítés és Üzemeltetés

### Fejlesztői környezet
```bash
# Függőségek telepítése
npm install

# Fejlesztői szerver indítása (Vite)
npm run dev

# Gyártási verzió készítése
npm run build
```

### Konfiguráció
A rendszer környezeti változókat használ (`.env.development`, `.env.example`):
- `VITE_API_BASE_URL`: Az API szerver elérhetősége.
