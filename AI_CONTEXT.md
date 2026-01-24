# CinemaVerse — AI Context (Arabic)

هذا الملف معمول مخصوص علشان أي AI (زي ChatGPT) يفهم مشروع **CinemaVerse** بسرعة وبشكل “عملي”:
- إيه موجود فعلًا في الكود؟
- إيه اللي مكتمل/جزئي/ناقص؟
- الدومين (Entities) وعلاقاتها.
- تدفّق الـ Use-cases (تصفح أفلام / تفاصيل / حجز / مقاعد / تذاكر / دفع / إيميل).
- مشاكل/تناقضات حالية قد تمنع التشغيل أو تسبب أخطاء.

> ملاحظة: المشروع حاليًا فيه Services وData جاهزين بنسبة كبيرة، لكن **طبقة الـ API ما فيهاش Controllers** (يعني مفيش Endpoints).

---

## 1) صورة عامة سريعة

### الهدف الوظيفي (Domain Goal)
نظام سينما يدير:
- أفلام (Movies) + صور + أنواع (Genres) + مواعيد عرض (ShowTimes)
- فروع (Branches) + قاعات (Halls) + مقاعد (Seats)
- حجوزات (Bookings) + مقاعد الحجز (BookingSeats) + عمليات دفع (BookingPayments)
- تذاكر (Tickets) مع QR Token

### المعمارية (Architecture)
المشروع مبني على **Layered Architecture**:
- **`CinemaVerse.API`**: تشغيل Web API + Dependency Injection + Middleware.
- **`CinemaVerse.Services`**: Business Logic + DTOs.
- **`CinemaVerse.Data`**: EF Core + Identity + Entities + Configurations + Repositories + UnitOfWork + Transactions.

### تدفّق الطلب المتوقع (Request Flow)
حاليًا التدفق النظري كده (لكن Controllers غير موجودة):

`HTTP Request` → `Controller (missing)` → `Service` → `UnitOfWork` → `Repository` → `AppDbContext` → `SQL Server`

---

## 2) هيكل الحل (Solution Structure)

### ملفات/مشاريع مهمة
- **API**
  - `src/CinemaVerse/Program.cs`
  - `src/CinemaVerse/appsettings.json`
  - `src/CinemaVerse/CinemaVerse.API.csproj`
- **Services**
  - `src/CinemaVerse.Services/Implementations/*.cs`
  - `src/CinemaVerse.Services/Interfaces/*.cs`
  - `src/CinemaVerse.Services/DTOs/**`
- **Data**
  - `src/CinemaVerse.Data/Data/AppDbContext.cs`
  - `src/CinemaVerse.Data/Models/*.cs`
  - `src/CinemaVerse.Data/Data/Configurations/*.cs`
  - `src/CinemaVerse.Data/Repositories/**`

### Tech Stack
- .NET: `net9.0`
- ORM: `Microsoft.EntityFrameworkCore` + SQL Server provider
- Identity: `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
- Logging: استخدام `ILogger<>` في Repos/Services + وجود `Serilog` package في Data
- OpenAPI: `Microsoft.AspNetCore.OpenApi`

---

## 3) طبقة API (CinemaVerse.API)

### Program.cs (Dependency Injection + Pipeline)
الموجود فعليًا في `Program.cs`:
- تسجيل `AppDbContext` على SQL Server باستخدام ConnectionString اسمها `DefaultConnection`
- تسجيل `IUnitOfWork`
- تسجيل خدمات:
  - `IMovieService → MovieService`
  - `ITicketService → TicketService`
  - `IHallSeatService → HallSeatService`
- `AddControllers()` و `AddOpenApi()`
- Middleware: `UseHttpsRedirection()` + `UseAuthorization()` + `MapControllers()`

### Configuration
`appsettings.json`:
- `DefaultConnection` شغال على localdb:
  - `Server=(localdb)\mssqllocaldb;Database=CinemaVerseDb;Trusted_Connection=True;MultipleActiveResultSets=true`

### حالة التنفيذ في API
- **مكتمل**: تشغيل الـ Host + DI الأساسي + DbContext + OpenAPI.
- **ناقص (Blocker)**: **لا توجد Controllers** (ملفات Controller = 0).
- **ناقص**: `IBookingService` غير مسجل في DI رغم وجود `BookingService`.
- **ناقص/غير واضح**: لا يوجد `UseAuthentication()` أو إعداد JWT/Identity endpoints في `Program.cs`.

---

## 4) طبقة Data (CinemaVerse.Data)

### 4.1) AppDbContext
`AppDbContext : IdentityDbContext<ApplicationUser>`
- فيه DbSets لمعظم Entities:
  - `Movies`, `Genres`, `MovieShowTimes`, `MovieGenres`, `MovieImages`
  - `Branches`, `Halls`, `Seats`
  - `Bookings`, `BookingSeats`, `BookingPayments`
  - `Tickets`
- `OnModelCreating` بيعمل:
  - `builder.ApplyConfigurationsFromAssembly(...)` لتطبيق ملفات الـ Configurations تلقائيًا.

### 4.2) Entities (Models) + العلاقات

#### Movie
ملف: `src/CinemaVerse.Data/Models/Movie.cs`
- **Fields**: `Id`, `MovieName`, `MovieDescription`, `MovieDuration (TimeSpan)`, `MovieCast (List<string>)`, `MovieRating (decimal)`, `MovieAgeRating`, `ReleaseDate (DateOnly)`, `TrailerUrl`
- **Relations**:
  - `MovieImages` (1:N)
  - `MovieShowTimes` (1:N)
  - `MovieGenres` (1:N) → many-to-many مع Genre عبر جدول وسيط

#### Genre / MovieGenre (Many-to-Many)
ملفات: `Genre.cs`, `MovieGenre.cs`
- `MovieGenre` مفتاح مركّب: `{MovieID, GenreID}`
- Cascade delete على العلاقة.

#### MovieImage
ملف: `MovieImage.cs`
- (N:1) مع Movie

#### Branch / Hall / Seat
ملفات: `Branch.cs`, `Hall.cs`, `Seat.cs`
- Branch (1:N) Halls
- Hall (1:N) Seats
- Hall (1:N) MovieShowTimes
- Seat (1:N) Tickets
- Seat (N:M) Bookings عبر `BookingSeat`

#### MovieShowTime
ملف: `MovieShowTime.cs`
- **Fields**: `MovieId`, `HallId`, `ShowStartTime`, `ShowEndTime`, `Price`, `CreatedAt`
- **Relations**:
  - (N:1) Movie
  - (N:1) Hall
  - (1:N) Bookings

#### Booking / BookingSeat / BookingPayment / Ticket
ملفات: `Booking.cs`, `BookingSeat.cs`, `BookingPayment.cs`, `Ticket.cs`
- Booking:
  - `UserId (string)` (IdentityUser.Id)
  - `MovieShowTimeId`
  - `Status`, `TotalAmount`, `CreatedAt`
  - Relations: User / MovieShowTime / BookingPayments / Tickets / BookingSeats
- BookingSeat:
  - مفتاح مركّب `{BookingId, SeatId}`
- BookingPayment:
  - مربوط بـ Booking (N:1)، وفيه `PaymentIntentId`, `Currency`, `Status`, `TransactionDate`
- Ticket:
  - `TicketNumber` + `QrToken` + `BookingId` + `SeatId` + `Price` + `Status` + `CreatedAt`
  - Unique على:
    - `TicketNumber`
    - `{BookingId, SeatId}`
    - `QrToken`

#### ApplicationUser
ملف: `ApplicationUser.cs`
- يرث من `IdentityUser`
- حقول إضافية: `FirstName`, `LastName`, `DateOfBirth`, `Gender?`, `Address?`, `City?`, `CreatedAt`
- Relation: (1:N) Bookings

### 4.3) Enums
ملفات: `src/CinemaVerse.Data/Enums/*.cs`
- **BookingStatus**: Pending(1), Cancelled(2), Completed(3)
- **PaymentStatus**: Pending(1), Completed(2), Failed(3), Cancelled(4)
- **TicketStatus**: Active(1), Used(2), Cancelled(3)
- **HallStatus**: Available(1), Maintenance(2), Closed(3)
- **HallType**: TwoD(1), ThreeD(2), IMAX(3), VIP(4), ScreenX(5)
- **Genders**: Male(1), Female(2)
- **MovieStatus**: NowPlaying, ComingSoon (ملحوظة: غير مستخدم واضحًا في الـ Models الحالية)

### 4.4) EF Core Configurations (قيود/جداول/Indexes)
الموجود في `src/CinemaVerse.Data/Data/Configurations/`:
- **MovieConfiguration**
  - جدول `Movies`
  - Required + MaxLength لعدة خصائص
  - (⚠️) يوجد سطر `builder.Property(m => m.Rating)...` بينما الـ Model اسمه `MovieRating` (تفاصيل في “مشاكل معروفة”)
- **MovieShowTimeConfiguration**
  - جدول `MovieShowTimes`
  - Unique Index: `{HallId, MovieId, ShowStartTime}`
  - علاقات Cascade مع Movie وHall
- **SeatConfiguration**
  - جدول `Seats`
  - Unique Index: `{SeatLabel, HallId}`
- **BookingSeatConfiguration**
  - Composite PK: `{BookingId, SeatId}`
- **TicketsConfiguration**
  - جدول `Tickets`
  - Unique Index: `TicketNumber`, `{BookingId, SeatId}`, `QrToken`
  - `TicketNumber` MaxLength(10) (⚠️ يتعارض مع الـ Generator الحالي في TicketService)
  - `QrToken` MaxLength(128)
  - OnDelete Restrict مع Booking وSeat
- **BookingConfiguration**
  - جدول `Bookings`
  - Default Status Pending + CreatedAt GETUTCDATE()
  - OnDelete:
    - User → Bookings: Cascade
    - MovieShowTime → Bookings: Restrict
- **BookingPaymentConfiguration**
  - جدول `BookingPayments`
  - Default Currency = `EGP`
  - Default Status Pending
  - OnDelete Restrict مع Booking
- **HallConfiguration**
  - جدول `Halls`
  - HallStatus/HallType محفوظين كـ int
  - Branch → Halls: Cascade
- **BranchConfiguration**
  - جدول اسمه `Branchs` (ملحوظة لغوية)
- **ApplicationUserConfiguration**
  - جدول `ApplicationUsers`
  - Required FirstName/LastName/DateOfBirth + CreatedAt GETUTCDATE()

### 4.5) Repository Pattern + UnitOfWork

#### IRepository<T> + Repository<T>
ملفات: `Repositories/Interfaces/IRepository.cs`, `Repositories/Implementations/Repository.cs`
- CRUD + Queries + Pagination:
  - `GetAllAsync`, `GetByIdAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`
  - `AnyAsync`, `CountAsync` (Predicate أو Queryable)، `FirstOrDefaultAsync`, `FindAllAsync`
  - `GetQueryable()`
  - `GetPagedAsync(...)` مع:
    - `orderBy`
    - `skip/take`
    - `includeProperties` كسلسلة `"Nav1,Nav2.Nested"`

#### Repositories متخصصة
- **MovieRepository**
  - `GetMovieWithDetailsByIdAsync`: Include صور + شو تايمز + Hall/Branch + Genres
  - `GetByGenreAsync`
  - `SearchByNameAsync`
- **BookingRepository**
  - `GetBookingWithDetailsAsync`: User + Tickets + MovieShowTime.Movie + Payments
  - `GetUserBookingsAsync`
  - `GetBookingsByStatusAsync`
- **MovieShowTimeRepository**
  - `GetMovieShowTimeWithDetailsAsync`: Movie + Genres + Images + Hall/Branch + Bookings + Tickets.Seat
  - `GetAvailableSeatsAsync`: (Hall.Seats) - (SeatIds المحجوزة في Tickets)
  - `GetReservedSeatsAsync`: join Tickets مع Seats مع استبعاد Cancelled
  - `IsSeatReservedAsync`
- **TicketsRepository**
  - `GetIssuedSeatIdsAsync` (idempotency)
  - `GetByTicketNumberAsync`
  - `GetTicketWithDetailsAsync`
  - `GetUserTicketsAsync` (⚠️ مشكلة نوع UserId موضحة لاحقًا)
- **HallRepository**
  - Halls حسب Branch + تفاصيل Seats/ShowTimes + تحقق HallNumber

#### UnitOfWork
ملف: `src/CinemaVerse.Data/Repositories/UnitOfWork.cs`
- بيوفر properties لكل repo، وبيعمل Lazy init
- بيدير Transactions:
  - `BeginTransactionAsync`
  - `CommitTransactionAsync`
  - `RollbackTransactionAsync`
- `SaveChangesAsync`

---

## 5) طبقة Services (CinemaVerse.Services)

### 5.1) الموجود في DI حاليًا
المسجل في `Program.cs`:
- `IMovieService`, `ITicketService`, `IHallSeatService`, `IUnitOfWork`

غير مسجل رغم وجوده:
- `IBookingService` (تنفيذ موجود)

Interfaces موجودة بدون Implementations:
- `IPaymentService`
- `IEmailService`
- `IAuthService` (واجهة فاضية)

### 5.2) DTOs (شكل البيانات للـ API)

#### Movies
- `BrowseMoviesRequestDto`: فلترة + Pagination
- `BrowseMoviesResponseDto`: قائمة `MovieCardDto` + `TotalCount/TotalPages`
- `MovieDetailsDto`: تفاصيل الفيلم + Genres + Images + ShowTimes
- `MovieCardDto`, `MovieImageDto`, `MovieShowTimeDto`

#### Booking
- `CreateBookingRequestDto`: `MovieShowTimeId` + `SeatIds`
- `BookingDetailsDto`: `BookingId`, `Status`, `TotalAmount`, `CreatedAt`, `ExpiresAt?`, `Showtime`, `BookedSeats`, `Tickets`
- `BookingListDto`: ملخص سريع للحجز
- `ShowtimeDto`: معلومات مختصرة عن العرض داخل تفاصيل الحجز

#### Hall/Seats
- `HallWithSeatsDto`: بيانات القاعة + Available/Reserved Seats
- `SeatDto`: SeatLabel + Row/Column

#### Tickets
- `TicketDetailsDto`: كل التفاصيل اللازمة لعرض التذكرة + QR token

#### Payment
- `CreatePaymentIntentRequestDto` (⚠️ موجود في namespace `CinemaVerse.Services.DTOs.Payment.NewFolder`)
- `CreatePaymentIntentResponseDto`
- `ConfirmPaymentRequestDto`
- `RefundPaymentRequestDto`

#### Email
- Base: `BaseEmailDto` (To/Subject/Body)
- `SendEmailDto` (عام)
- `WelcomeEmailDto`, `PasswordResetEmailDto`
- `BookingConfirmationEmailDto`, `PaymentConfirmationEmailDto`, `BookingCancellationEmailDto`
- `TicketInfoDto` (داخل إيميل التأكيد)

### 5.3) الخدمات المنفذة (Implementations)

#### MovieService (مكتمل وظيفيًا)
ملف: `src/CinemaVerse.Services/Implementations/MovieService.cs`
- **BrowseMoviesAsync**
  - Validations: `Page/PageSize`، `Rating` بين 0..10، التأكد من GenreId لو موجود
  - بيستخدم `GetQueryable()` لتكوين query في DB
  - بيعمل `CountAsync(query)` قبل pagination
  - بيجيب `GetPagedAsync` مع include: `"MovieImages,MovieGenres.Genre"`
  - Mapping إلى `MovieCardDto`
- **GetMovieDetailsAsync**
  - Validation لـ movieId
  - بيستخدم `Movies.GetMovieWithDetailsByIdAsync`
  - Mapping لـ `MovieDetailsDto` (بما فيها ShowTimes مع Hall/Branch)

#### BookingService (جزئي)
ملف: `src/CinemaVerse.Services/Implementations/BookingService.cs`
- **CreateBookingAsync (مكتمل)**:
  - Transaction لحماية حجز المقاعد
  - Validations قوية + Hall availability + Showtime not started
  - التأكد إن كل SeatId يتبع نفس Hall
  - منع Duplicate seatIds
  - check per-seat reservation (`IsSeatReservedAsync`)
  - إنشاء `Booking` Status=Pending + `BookingSeat` لكل مقعد
  - Commit/Rollback
  - يرجع `BookingDetailsDto` لكن:
    - `Tickets` فاضية (طبيعي قبل الدفع)
    - `BookedSeats` غير معمولة mapping حاليًا
    - `ExpiresAt` غير محسوبة حاليًا
- **CancelUserBookingAsync**: غير منفذ
- **GetUserBookingByIdAsync**: غير منفذ
- **GetUserBookingsAsync**: غير منفذ

#### TicketService (قريب من المكتمل لكن يعتمد على سلامة الموديل/الكونفيج)
ملف: `src/CinemaVerse.Services/Implementations/TicketService.cs`
- **IssueTicketsAsync**
  - Validation: BookingId > 0
  - جلب Booking بالتفاصيل
  - شرط: Booking.Status لازم `Completed`
  - Idempotency: عدم إصدار Tickets لمقاعد اتصدر لها قبل كده (`GetIssuedSeatIdsAsync`)
  - Transaction + إنشاء Ticket لكل Seat
  - إنشاء `TicketNumber` و `QrToken` آمن
  - Commit/Rollback
- **ملاحظة مهمة**: في Config `TicketNumber` MaxLength(10) لكن الـ Generator بيرجع قيمة أطول (مشكلة تشغيلية)

#### HallSeatService (مكتمل وظيفيًا)
ملف: `src/CinemaVerse.Services/Implementations/HallSeatService.cs`
- **GetHallWithSeatsAsync**
  - Validation: MovieShowTimeId > 0
  - جلب showtime بالتفاصيل (Hall/Branch)
  - جلب AvailableSeats + ReservedSeats من repo
  - Mapping لـ `HallWithSeatsDto`

---

## 6) “إيه اتعمل وإيه لسه” — Status Matrix

### مكتمل (Functional)
- **Data Layer**
  - Entities + DbContext + معظم Configurations
  - Generic Repository + Repos متخصصة
  - UnitOfWork + Transactions
- **Services**
  - Movies: Browse + Details
  - Hall seats: available/reserved
  - Tickets: إصدار tickets (من ناحية منطق البزنس)
  - Booking: إنشاء حجز (Pending) + حجز seats عبر BookingSeat

### جزئي (Partial / TODO)
- **BookingService**
  - Get user bookings / get booking by id / cancel booking: غير منفذين
  - `BookingDetailsDto.BookedSeats` و`ExpiresAt`: غير مُستخدمين في الإرجاع الحالي
- **Payment**
  - DTOs + interface موجودين، لكن **لا يوجد PaymentService**
  - لا يوجد ربط بين الدفع وتغيير `BookingStatus` إلى Completed
  - لا يوجد إنشاء `BookingPayment` في `CreateBookingAsync` حاليًا
- **Email**
  - DTOs + interface موجودين، لكن **لا يوجد EmailService**
- **Auth**
  - `ApplicationUser` + IdentityDbContext موجودين
  - لا يوجد AuthService ولا إعدادات Auth pipeline

### ناقص (Blockers)
- **Controllers/Endpoints**: لا يوجد أي Controller فعليًا → لا يوجد API قابل للاستهلاك.

---

## 7) مشاكل/تناقضات معروفة (قد تمنع Build/Runtime)

هذه نقاط مهمة جدًا لأي AI/Developer لأنها “تفسر” ليه بعض الـ flows مش شغالة حتى لو المنطق موجود:

1) **MovieConfiguration يستخدم `m.Rating`**
   - في `Movie.cs` الخاصية اسمها `MovieRating`
   - ده غالبًا هيكسر build أو migrations.

2) **TicketNumber length mismatch**
   - `TicketsConfiguration`: `TicketNumber` MaxLength = 10
   - `TicketService.GenerateTicketNumber()` بيرجع قيمة طولها تقريبًا 19 (مثال: `tk--20260124-ABCDEF`)
   - ده هيعمل database error عند الحفظ.

3) **TicketsRepository.GetUserTicketsAsync نوع الـ UserId**
   - `Booking.UserId` نوعه `string`
   - الدالة بتاخد `Guid UserId` وبتقارن: `t.Booking.UserId == UserId`
   - ده غالبًا compile error (أو على الأقل تصميم غلط).

4) **MovieCast نوعه `List<string>` لكن Configuration بتعاملها كعمود string**
   - `MovieConfiguration`: `builder.Property(m => m.MovieCast).IsRequired().HasMaxLength(500);`
   - بدون ValueConverter واضح، EF Core لا يحفظ List<string> مباشرة.

5) **Seat.cs فيه `using static CinemaVerse.Data.Models.Booking;`**
   - غالبًا غير مؤثر، لكنه غير منطقي وقد يكون بقايا refactor.

6) **Branch table name**
   - `BranchConfiguration`: جدول `Branchs` (تهجئة) — مش خطأ تقنيًا لكنه قد يسبب لخبطة في SQL.

7) **API pipeline**
   - `UseAuthorization()` موجود لكن لا يوجد `UseAuthentication()` أو تسجيل Identity/JWT → أي Authorization حقيقي مش جاهز.

---

## 8) Use-cases المتوقعة (كيف المفروض النظام يشتغل)

### 8.1) Browse Movies
- Input: `BrowseMoviesRequestDto`
- Processing: `MovieService.BrowseMoviesAsync`
- Output: `BrowseMoviesResponseDto` (قائمة cards + pagination)

### 8.2) Movie Details
- Input: movieId
- Processing: `MovieService.GetMovieDetailsAsync`
- Output: `MovieDetailsDto` (Genres/Images/ShowTimes)

### 8.3) Hall Seats for Showtime
- Input: MovieShowTimeId
- Processing: `HallSeatService.GetHallWithSeatsAsync`
- Output: `HallWithSeatsDto` (Available/Reserved)

### 8.4) Create Booking (Pending)
- Input: userId + `CreateBookingRequestDto(MovieShowTimeId, SeatIds)`
- Processing: `BookingService.CreateBookingAsync`
- Output: `BookingDetailsDto` (Tickets empty)

### 8.5) Payment Flow (غير منفذ حاليًا)
التدفق المتوقع:
1) `CreatePaymentIntent(BookingId)` → يرجع `PaymentIntentId`/`ClientSecret`
2) `ConfirmPayment(PaymentIntentId, BookingId)`:
   - إنشاء `BookingPayment` Status=Completed
   - تحويل `Booking.Status` إلى Completed
   - استدعاء `TicketService.IssueTicketsAsync(BookingId)`
   - إرسال Email confirmation

### 8.6) Issue Tickets (بعد اكتمال الحجز)
- شرط: Booking.Status == Completed
- يمنع إصدار مكرر (idempotent)

---

## 9) اقتراح خريطة Endpoints مستقبلية (لما Controllers تتعمل)

> مجرد اقتراح شكل، لأن Controllers غير موجودة حاليًا.

- `GET /api/movies` → BrowseMovies
- `GET /api/movies/{id}` → MovieDetails
- `GET /api/showtimes/{id}/hall` → HallWithSeats
- `POST /api/bookings` → CreateBooking (يتطلب Auth user)
- `GET /api/bookings` → GetUserBookings (TODO)
- `GET /api/bookings/{id}` → GetUserBookingById (TODO)
- `POST /api/bookings/{id}/cancel` → Cancel (TODO)
- `POST /api/payments/intent` → CreatePaymentIntent (TODO)
- `POST /api/payments/confirm` → ConfirmPayment (TODO)
- `POST /api/tickets/issue` → IssueTickets (عادة تُستدعى داخليًا بعد الدفع)

---

## 10) أسئلة شائعة يقدر الـ AI يجاوبها بالملف ده

- “إزاي المقاعد بتتحدد متاحة/محجوزة؟”
- “إمتى Booking يبقى Completed وازاي التذاكر بتتولد؟”
- “إيه القيود (unique indexes) اللي تحمي البيانات من التكرار؟”
- “فين transaction boundaries؟”
- “إيه الخدمات اللي ناقصة؟”
- “ليه إصدار التذاكر ممكن يفشل في قاعدة البيانات؟”

---

## 11) TODOs المقترحة (Roadmap عملي)

1) إضافة Controllers وربطها بالخدمات (MVP).
2) إصلاح مشاكل الـ EF Config / Model inconsistencies (MovieRating, MovieCast converter, TicketNumber length, UserId type).
3) تنفيذ `PaymentService` (Stripe/Paymob/… حسب المطلوب) وربطها بـ BookingPayment/BookingStatus.
4) تنفيذ `EmailService` (SMTP/SendGrid) واستخدام DTOs الحالية.
5) تنفيذ Auth (JWT/Identity endpoints) وتفعيل `UseAuthentication()`.
6) إكمال BookingService (cancel/list/get-by-id) + سياسات الإلغاء/refund.

