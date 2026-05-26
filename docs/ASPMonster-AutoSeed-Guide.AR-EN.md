# CinemaVerse Auto-Migrate + Auto-Seed (ASPMonster)

## English

### What happens on deploy
1. The API starts.
2. EF Core migrations run automatically (`db.Database.Migrate()`).
3. If `SeedData:Enabled` is `true`, a safe idempotent seeder runs once per startup.

### How to enable / disable
- Config section: `SeedData`
- Enable seeding:
  - Set `SeedData:Enabled=true`
- Production guard:
  - By default, seeding is blocked in Production.
  - To allow seeding in Production, set `SeedData:AllowInProduction=true` (not recommended).
- Disable seeding:
  - Set `SeedData:Enabled=false`

### Seeded data (when enabled)
- Users:
  - Admin: `SeedData:AdminEmail` / `SeedData:AdminPassword`
  - User: `SeedData:UserEmail` / `SeedData:UserPassword`
- Catalog:
  - Genres (Action, Drama, Sci-Fi, ...)
  - Movies (3 demo movies)
  - MovieGenres links
- Venues:
  - Branches (2 demo branches)
  - Halls (2 per branch, fixed capacity by HallType)
  - Seats (auto-generated layout via `HallTypeLayoutConfig`)
- Showtimes:
  - 3 future showtimes per hall

### Optional sample transactions
- If `SeedData:IncludeSampleTransactions=true`:
  - Creates 1 confirmed booking for the seeded user
  - Creates booking seats + tickets + a completed payment
  - Creates 1 review (if not already present)

### Idempotency rules (safe re-run)
- Users are created only if email does not already exist.
- Genres are created only if name does not already exist.
- Movies are added only if there are no movies.
- Branches are added only if there are no branches.
- Halls are added only if a branch has no halls.
- Seats are added only if a hall has no seats.
- Showtimes are added only if there are no showtimes.
- Sample transactions are added only if there are no bookings.

### Where the code lives
- Startup hook: `src/CinemaVerse/Program.cs`
- Options: `src/CinemaVerse/Options/SeedDataOptions.cs`
- Seeder: `src/CinemaVerse/Infrastructure/DatabaseSeeder.cs`

### Notes for ASPMonster
- Set your real connection string in `ConnectionStrings:DefaultConnection` (ASPMonster panel).
- Provide seed passwords via environment variables (recommended):
  - `SeedData__AdminPassword`
  - `SeedData__UserPassword`
- Keep `SeedData:Enabled=false` for production unless you explicitly want demo data.

---

## العربية

### ماذا يحدث عند النشر
1. يبدأ تشغيل الـ API.
2. يتم تشغيل ترحيلات EF Core تلقائيا (`db.Database.Migrate()`).
3. إذا كانت `SeedData:Enabled` = `true` فسيتم تشغيل Seeder بشكل آمن (Idempotent) عند الإقلاع.

### كيفية التفعيل / الإيقاف
- قسم الإعدادات: `SeedData`
- لتفعيل الـ Seeding:
  - اجعل `SeedData:Enabled=true`
- حماية الإنتاج:
  - افتراضيا يتم منع الـ Seeding في Production.
  - للسماح في Production اجعل `SeedData:AllowInProduction=true` (غير مستحسن).
- لإيقاف الـ Seeding:
  - اجعل `SeedData:Enabled=false`

### البيانات التي يتم إضافتها (عند التفعيل)
- المستخدمون:
  - Admin: `SeedData:AdminEmail` / `SeedData:AdminPassword`
  - User: `SeedData:UserEmail` / `SeedData:UserPassword`
- الكتالوج:
  - Genres (Action, Drama, Sci-Fi, ...)
  - Movies (3 افلام تجريبية)
  - ربط MovieGenres
- الفروع والقاعات:
  - Branches (فرعين تجريبيين)
  - Halls (قاعتين لكل فرع، السعة ثابتة حسب HallType)
  - Seats (توليد تلقائي للصفوف/المقاعد عبر `HallTypeLayoutConfig`)
- مواعيد العرض:
  - 3 مواعيد مستقبلية لكل قاعة

### بيانات تجريبية اختيارية (Transactions)
- إذا كانت `SeedData:IncludeSampleTransactions=true`:
  - إنشاء حجز مؤكد واحد للمستخدم التجريبي
  - إنشاء BookingSeats + Tickets + Payment (Completed)
  - إنشاء Review واحد (إذا لم يكن موجودا)

### قواعد الأمان (Idempotency)
- يتم إنشاء المستخدمين فقط إذا لم يكن البريد موجودا.
- يتم إنشاء التصنيفات فقط إذا لم يكن الاسم موجودا.
- يتم إضافة الأفلام فقط إذا لم توجد أفلام.
- يتم إضافة الفروع فقط إذا لم توجد فروع.
- يتم إضافة القاعات فقط إذا لم يوجد قاعات داخل الفرع.
- يتم إضافة المقاعد فقط إذا لم يوجد مقاعد داخل القاعة.
- يتم إضافة مواعيد العرض فقط إذا لم توجد مواعيد.
- يتم إضافة المعاملات التجريبية فقط إذا لم توجد حجوزات.

### مكان الكود
- Hook في الإقلاع: `src/CinemaVerse/Program.cs`
- Options: `src/CinemaVerse/Options/SeedDataOptions.cs`
- Seeder: `src/CinemaVerse/Infrastructure/DatabaseSeeder.cs`

### ملاحظات لـ ASPMonster
- ضع Connection String الحقيقي في `ConnectionStrings:DefaultConnection` من لوحة ASPMonster.
- ضع كلمات المرور عبر Environment Variables (مستحسن):
  - `SeedData__AdminPassword`
  - `SeedData__UserPassword`
- اجعل `SeedData:Enabled=false` في الإنتاج إلا إذا أردت بيانات تجريبية.
