# CinemaVerse — Fullstack Cinema Ticket Booking Platform

A complete cinema ticket booking platform built with **Angular 21** (frontend) and **ASP.NET Core 9.0** (backend), featuring user authentication, movie browsing, seat selection, Stripe payments, QR code tickets, and a full admin panel.

---

## Table of Contents

- [Live Demo](#live-demo)
- [Screenshots](#screenshots)
  - [User — Home & Movies](#user--home--movies)
  - [Admin — Dashboard](#admin--dashboard)
  - [Admin — Movies Management](#admin--movies-management)
  - [Admin — Branches & Halls](#admin--branches--halls)
  - [Admin — Showtimes](#admin--showtimes)
  - [Admin — Users Management](#admin--users-management)
  - [Admin — Bookings](#admin--bookings)
  - [Admin — Tickets](#admin--tickets)
- [Tech Stack](#tech-stack)
- [Features](#features)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Backend Setup](#backend-setup)
  - [Frontend Setup](#frontend-setup)
- [API Documentation](#api-documentation)
- [Authentication](#authentication)
- [Database](#database)
- [Background Jobs](#background-jobs)
- [Email System](#email-system)
- [Deployment](#deployment)
- [Test Credentials](#test-credentials)
- [Known Issues](#known-issues)
- [Contributing](#contributing)

---

## Live Demo

| Service | URL |
|---------|-----|
| **Frontend** | [https://butalib.github.io/cinmaVerse/login](https://butalib.github.io/cinmaVerse/login) |
| **Backend API** | [https://cinemaverse.tryasp.net](https://cinemaverse.tryasp.net) |
| **Swagger UI** | [https://cinemaverse.tryasp.net/swagger/index.html](https://cinemaverse.tryasp.net/swagger/index.html) |

---

## Screenshots

### User — Home & Movies

| Home Page | Movie Listing |
|-----------|--------------|
| ![Home Page](docs/Images/user%20home.png) | ![Movie Listing](docs/Images/user%20movies.png) |

| Movie Detail | Booking |
|--------------|---------|
| ![Movie Detail](docs/Images/user%20movie%20details.png) | ![Booking](docs/Images/User%20movie%20booking.png) |

| Payment |
|---------|
| ![Payment](docs/Images/user%20payment.png) |

---

### Admin — Dashboard

| Dashboard |
|-----------|
| ![Dashboard](docs/Images/Dashboard.png) |

---

### Admin — Movies Management

| Movies List | View Movie |
|-------------|------------|
| ![Movies List](docs/Images/admin%20movies.png) | ![View Movie](docs/Images/admin%20view%20movies.png) |

---

### Admin — Branches & Halls

| Branches | View Branch |
|----------|-------------|
| ![Branches](docs/Images/Admin%20branch.png) | ![View Branch](docs/Images/admin%20view%20branch.png) |

| Edit Hall |
|-----------|
| ![Edit Hall](docs/Images/admin%20edit%20hall.png) |

---

### Admin — Showtimes

| Showtimes |
|-----------|
| ![Showtimes](docs/Images/admin%20showtimes.png) |

---

### Admin — Users Management

| Users | View User |
|-------|-----------|
| ![Users](docs/Images/Admin%20User.png) | ![View User](docs/Images/Admin%20View%20user.png) |

---

### Admin — Bookings

| Bookings |
|----------|
| ![Bookings](docs/Images/admn%20bookings.png) |

---

### Admin — Tickets

| Tickets | View Ticket |
|---------|-------------|
| ![Tickets](docs/Images/admin%20tickets.png) | ![View Ticket](docs/Images/admin%20view%20ticket.png) |

---

## Tech Stack

### Frontend

| Technology | Version | Purpose |
|------------|---------|---------|
| Angular | 21.2.x | UI framework (standalone components) |
| TypeScript | 5.9.x | Type-safe JavaScript |
| SCSS | — | Styling with custom design system |
| Bootstrap | 5.3.8 | CSS utility classes |
| Chart.js | 4.5.1 | Dashboard charts |
| RxJS | 7.8.x | Reactive programming |
| Angular Signals | — | State management |
| Vitest | 4.0.8 | Unit testing |

### Backend

| Technology | Version | Purpose |
|------------|---------|---------|
| ASP.NET Core | 9.0 | Web API framework |
| Entity Framework Core | 9.0 | ORM / Database access |
| SQL Server | — | Database |
| JWT Bearer | 8.15.0 | Authentication |
| Stripe.net | 50.2.0 | Payment processing |
| Hangfire | 1.8.23 | Background job processing |
| MailKit | 4.14.1 | Email sending |
| RazorLight | 2.2.0 | Email templates |
| BCrypt.Net | 4.0.3 | Password hashing |
| Serilog | 8.0.3 | Structured logging |
| Swashbuckle | 6.5.0 | API documentation |

---

## Features

### User Features

| Feature | Description |
|---------|-------------|
| **Registration & Login** | JWT-based authentication with email verification |
| **Password Recovery** | Forgot password flow with email reset tokens |
| **Movie Browsing** | Browse movies with search, genre, and language filters |
| **Movie Details** | View movie info, cast, images, and available showtimes |
| **Seat Selection** | Interactive seat grid with real-time availability |
| **Payment** | Stripe-integrated payment with card validation |
| **Booking Management** | View, cancel, and track booking status |
| **Ticket History** | View tickets with QR codes for check-in |
| **Profile Management** | View and edit profile, change password |

### Admin Features

| Feature | Description |
|---------|-------------|
| **Dashboard** | KPI cards, revenue charts, booking trends, occupancy rate |
| **Movies Management** | Full CRUD with media upload, genre assignment |
| **Branches & Halls** | Manage cinema branches and hall configurations |
| **Showtimes** | Schedule showtimes with conflict detection |
| **Genres** | Manage movie genres |
| **Users** | View, create, edit, activate/deactivate users |
| **Bookings** | View all bookings, update status, export to CSV |
| **Payments** | View payment history and summaries |
| **Tickets** | View tickets, QR lookup, check-in management |
| **Hangfire Dashboard** | Monitor background jobs at `/hangfire` |

---

## Architecture

### Frontend — Angular 21 (Clean Architecture)

```
src/app/
├── core/                    ← Singleton services, guards, interceptors
│   ├── auth/                ← Authentication logic, JWT handling
│   ├── config/              ← API configuration
│   ├── guards/              ← Route guards (auth, role)
│   ├── http/                ← API client service
│   └── interceptors/        ← HTTP interceptors (auth, refresh token)
│
├── layout/                  ← App shell (header + router outlet + footer)
│
├── shared/                  ← Reusable components
│   ├── components/          ← Pagination, modals
│   └── ui/                  ← Design system components
│
└── features/                ← Feature modules
    ├── auth/                ← Login, register, password recovery
    ├── user/                ← User-facing features
    │   ├── data-access/     ← API services
    │   ├── models/          ← TypeScript interfaces
    │   ├── pages/           ← Profile, edit profile, change password
    │   ├── store/           ← Signal-based state
    │   └── user-mangment/   ← Feature components
    │       └── feature/
    │           ├── home/           ← Home page with banner
    │           ├── movie-detail/   ← Movie detail with showtimes
    │           ├── movie-booking/  ← Seat selection + payment flow
    │           └── bookings/       ← Booking list, detail, tickets
    │
    └── admin/               ← Admin panel
        ├── admin-layout/    ← Admin shell (sidebar + header)
        ├── features/
        │   ├── dashboard/   ← KPI cards, charts
        │   ├── movies/      ← Movie CRUD
        │   ├── users/       ← User management
        │   ├── branches/    ← Branch & hall management
        │   ├── showtimes/   ← Showtime scheduling
        │   ├── genres/      ← Genre management
        │   ├── bookings/    ← Booking management
        │   ├── payments/    ← Payment history
        │   └── tickets/     ← Ticket management
        └── shared/          ← Admin shared components
```

### Backend — ASP.NET Core 9.0 (Clean Architecture)

```
CinemaVerse Backend/src/
├── CinemaVerse/                    ← API Layer (Presentation)
│   ├── Program.cs                  ← Entry point, DI, middleware
│   ├── Controllers/
│   │   ├── AuthController.cs       ← Public auth endpoints
│   │   ├── MeController.cs         ← User profile endpoints
│   │   ├── MovieController.cs      ← Public movie browsing
│   │   ├── HallSeatController.cs   ← Seat information
│   │   ├── BookingController.cs    ← User booking endpoints
│   │   ├── TicketController.cs     ← User ticket endpoints
│   │   ├── PaymentController.cs    ← Payment processing
│   │   ├── ReviewController.cs     ← Movie reviews
│   │   └── Admin/                  ← 10 admin controllers
│   ├── Middleware/                  ← Global exception handling
│   ├── Filters/                    ← Model validation, Hangfire auth
│   └── Infrastructure/             ← Database seeder, Hangfire jobs
│
├── CinemaVerse.Services/           ← Business Logic Layer
│   ├── DTOs/                       ← Data Transfer Objects
│   │   ├── Common/                 ← Shared DTOs
│   │   ├── UserFlow/               ← User-facing DTOs
│   │   └── AdminFlow/              ← Admin DTOs
│   ├── Interfaces/                 ← Service contracts
│   ├── Implementations/            ← Service logic
│   ├── Mappers/                    ← Entity ↔ DTO mappers
│   └── Constants/                  ← Enums, cache keys, configs
│
└── CinemaVerse.Data/               ← Data Access Layer
    ├── Data/
    │   ├── AppDbContext.cs         ← EF Core DbContext
    │   └── Configurations/        ← Entity configurations
    ├── Models/                     ← 17 entity classes
    ├── Repositories/               ← Repository + Unit of Work
    └── Migrations/                 ← Database migrations
```

---

## Getting Started

### Prerequisites

| Requirement | Version |
|-------------|---------|
| .NET SDK | 9.0+ |
| Node.js | 18+ |
| npm | 9+ |
| SQL Server | 2019+ (or Azure SQL) |
| Stripe Account | For payment processing |

### Backend Setup

```bash
# 1. Navigate to backend
cd "CinemaVerse Backend/src"

# 2. Restore dependencies
dotnet restore

# 3. Update connection string in CinemaVerse/appsettings.json
# "ConnectionStrings": {
#   "DefaultConnection": "Server=.;Database=CinemaVerseDb;Trusted_Connection=true;TrustServerCertificate=true"
# }

# 4. Update Stripe key in appsettings.json
# "Stripe": {
#   "SecretKey": "sk_test_your_stripe_secret_key"
# }

# 5. Run migrations and seed data
dotnet ef database update --project CinemaVerse.Data --startup-project CinemaVerse

# 6. Run the API
cd CinemaVerse
dotnet run

# API will be available at https://localhost:5001 or http://localhost:5000
```

### Frontend Setup

```bash
# 1. Navigate to frontend
cd "CinmaVerse Front"

# 2. Install dependencies
npm install

# 3. Update API URL in src/app/core/config/api.config.ts
# For local development:
# export const API_BASE_URL = 'https://localhost:5001';

# 4. Start development server
npm start

# Frontend will be available at http://localhost:4200
```

---

## API Documentation

### Swagger UI

Access the interactive API documentation at:

```
https://cinemaverse.tryasp.net/swagger/index.html
```

### API Endpoints Summary

| Category | Endpoints | Auth |
|----------|-----------|------|
| Auth | 8 | Public |
| User Profile | 3 | Required |
| Movies (Public) | 2 | Public |
| Hall Seats | 1 | Public |
| User Bookings | 4 | Required |
| User Tickets | 2 | Required |
| User Payments | 3 | Required |
| User Reviews | 5 | Required |
| Admin Dashboard | 7 | Admin |
| Admin Users | 9 | Admin |
| Admin Movies | 6 | Admin |
| Admin Branches | 7 | Admin |
| Admin Halls | 5 | Admin |
| Admin Genres | 5 | Admin |
| Admin Showtimes | 5 | Admin |
| Admin Bookings | 7 | Admin |
| Admin Payments | 3 | Admin |
| Admin Tickets | 6 | Admin |
| Admin Media | 1 | Admin |
| **Total** | **94** | |

### Standard Response Format

```json
// Success (200 OK)
{ "data": "...", "message": "Success" }

// Paginated Success
{
  "items": [...],
  "page": 1,
  "totalCount": 100,
  "pageSize": 10,
  "totalPages": 10
}

// Error (400/401/403/404/500)
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "errors": {
    "FieldName": ["Error message"]
  }
}
```

---

## Authentication

### JWT Configuration

| Setting | Value |
|---------|-------|
| Issuer | `CinemaVerseApi` |
| Audience | `CinemaVerseApiUsers` |
| Access Token Expiry | 60 minutes |
| Refresh Token Expiry | 7 days |
| Signing Algorithm | HMAC-SHA256 |
| Password Hashing | BCrypt (work factor 10+) |

### Auth Flow

```
┌──────────┐     POST /api/auth/login      ┌──────────┐
│ Frontend │ ──────────────────────────────▶│ Backend  │
│          │◀──────────────────────────────│          │
│          │  { accessToken, refreshToken,  │          │
│          │    userId, email, role }        │          │
└──────────┘                               └──────────┘
     │
     │  Store tokens in localStorage
     │  Attach Bearer token to all requests
     ▼
┌──────────┐     GET /api/me (with Bearer)  ┌──────────┐
│ Frontend │ ──────────────────────────────▶│ Backend  │
│          │◀──────────────────────────────│          │
│          │  { userProfile }               │          │
└──────────┘                               └──────────┘
```

### Token Storage

| Key | Purpose |
|-----|---------|
| `cinemaverse_token` | JWT access token |
| `cv_refresh_token` | Refresh token for silent renewal |
| `cv_role` | Cached user role |

### Rate Limiting

Auth endpoints are rate-limited to **5 requests per minute per IP**:
- `POST /api/auth/login`
- `POST /api/auth/refresh-token`
- `POST /api/auth/logout`

---

## Database

### Entity Relationship Diagram

```
Users ──────── Bookings ──────── MovieShowTimes ──────── Movies
  │               │                    │
  │               │                    └─────── Halls ──────── Branches
  │               │
  │               ├─── BookingSeat ──────── Seats
  │               │
  │               ├─── Tickets ──────── Seats
  │               │
  │               └─── BookingPayments
  │
  └─────── Reviews ──────── Movies

Movies ──────── MovieGenres ──────── Genres
Movies ──────── MovieImages
Movies ──────── MovieCastMembers
```

### Entities (17 Total)

| Entity | Key Fields |
|--------|-----------|
| **User** | Id, Email, PasswordHash, FirstName, LastName, Role, IsActive, IsEmailConfirmed |
| **Movie** | Id, MovieName, MovieDescription, MoviePoster, MovieDuration, MovieRating, Status |
| **Branch** | Id, BranchName, BranchLocation |
| **Hall** | Id, BranchId, HallNumber, HallType, Capacity, HallStatus |
| **Seat** | Id, HallId, SeatLabel |
| **Genre** | Id, GenreName |
| **MovieGenre** | MovieID, GenreID (many-to-many) |
| **MovieShowTime** | Id, HallId, MovieId, ShowStartTime, ShowEndTime, Price |
| **Booking** | Id, UserId, MovieShowTimeId, Status, TotalAmount, ExpiresAt |
| **BookingSeat** | BookingId, SeatId (many-to-many) |
| **Ticket** | Id, BookingId, SeatId, Price, Status, TicketNumber, QrToken |
| **BookingPayment** | Id, BookingId, Amount, Currency, PaymentIntentId, Status |
| **Review** | Id, UserId, MovieId, Rating, Comment |
| **MovieImage** | Id, MovieId, ImageUrl |
| **MovieCastMember** | Id, MovieId, PersonName, RoleType, CharacterName |

### Enums

| Enum | Values |
|------|--------|
| `UserRole` | Admin(1), User(2) |
| `MovieStatus` | Draft(0), Active(1), Archived(2), ComingSoon(3) |
| `BookingStatus` | Pending(0), Confirmed(1), Cancelled(2), Expired(3) |
| `TicketStatus` | Active(0), Used(1), Cancelled(2) |
| `PaymentStatus` | Pending(0), Completed(1), Failed(2), Refunded(3) |
| `HallType` | TwoD(0), ThreeD(1), IMAX(2), VIP(3) |
| `HallStatus` | Available(0), UnderMaintenance(1) |
| `MovieAgeRating` | G(0), PG(1), PG13(2), R(3), NC17(4) |
| `Genders` | Male(0), Female(1), Other(2) |
| `CastRoleType` | Actor(0), Director(1), Producer(2) |

---

## Background Jobs

Powered by **Hangfire** with SQL Server storage.

| Job | Schedule | Purpose |
|-----|----------|---------|
| `ExpirePendingBookings` | Every minute | Cancel unpaid bookings after timeout |
| `SendShowReminders` | Every 15 minutes | Email reminders for upcoming shows |

### Hangfire Dashboard

Access at `https://cinemaverse.tryasp.net/hangfire` (Admin-only).

---

## Email System

### Email Types

| Type | Template | Trigger |
|------|----------|---------|
| Welcome | `welcome.cshtml` | After registration |
| Email Verification | `verification.cshtml` | After registration |
| Password Reset | `password-reset.cshtml` | On forgot password request |
| Booking Confirmation | `booking-confirmation.cshtml` | After payment success |
| Booking Cancellation | `booking-cancellation.cshtml` | On booking cancellation |
| Show Reminder | `show-reminder.cshtml` | 2 hours before showtime |
| Payment Confirmation | `payment-confirmation.cshtml` | After payment success |

### SMTP Configuration

| Setting | Value |
|---------|-------|
| Server | `smtp.gmail.com` |
| Port | 587 |
| Security | STARTTLS |
| Authentication | App password |

---

## Deployment

### Frontend (GitHub Pages)

```bash
# Build for production
cd "CinmaVerse Front"
npm run build

# Output: dist/cinmaverse-web/browser/
# Deploy to GitHub Pages via CI/CD or manual upload
```

### Backend (Azure / Any .NET Host)

```bash
# Publish
dotnet publish -c Release -o ./publish

# Deploy publish/ folder to your hosting provider
```

### Environment Variables

| Variable | Purpose | Example |
|----------|---------|---------|
| `ConnectionStrings__DefaultConnection` | Database connection | `Server=...;Database=CinemaVerseDb;...` |
| `Jwt__Secret` | JWT signing key | `your-256-bit-secret` |
| `Stripe__SecretKey` | Stripe API key | `sk_test_...` |
| `Email__From` | Sender email | `noreply@cinemaverse.com` |
| `Email__Password` | SMTP app password | `your-app-password` |
| `SeedData__AllowInProduction` | Allow seeding in prod | `false` |

---

## Test Credentials

| Role | Email | Password |
|------|-------|----------|
| **Admin** | `admin@cinemaverse.local` | `YourAdminPassword123!` |
| **User** | `user@cinemaverse.local` | `User@12345` |

> These are seeded in development. Production credentials may differ.

---

## Known Issues

### Critical

| Issue | Description |
|-------|-------------|
| Payment confirm response shape | Backend returns raw `bool`, frontend expects `{ success, message }` |
| Movie status mapper | Frontend sends `"NowShowing"` / `"Draft"`, backend expects `"Active"` / `"Archived"` |
| User booking missing `userId` query | Detail/cancel calls omit required `?userId=X` parameter |
| Refresh interceptor | Only clears session, doesn't attempt token refresh |

### High

| Issue | Description |
|-------|-------------|
| Login doesn't save refresh token | Logout sends empty refresh token |
| Hardcoded `userId: 1` | Admin booking creation always uses user ID 1 |
| Genre ID mismatch | Frontend uses `id`, backend returns `genreId` |
| Admin user create missing DOB validation | Form allows blank dateOfBirth |

### Medium

| Issue | Description |
|-------|-------------|
| Hall seats branch shape | Backend sends string, frontend expects object |
| Seat column type | Backend sends string, frontend expects number |
| Cast image URL validation | Empty string fails backend `[Url]` validation |

> See `AI_PROJECT_ANALYSIS/FRONTEND_BACKEND_MISMATCHES.md` for full details.

---

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Development Guidelines

- Follow Angular style guide for frontend
- Follow ASP.NET Core conventions for backend
- Write unit tests for new features
- Update API documentation for new endpoints
- Run linter before committing

---

## License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.

---

## Author

**Nour Eldeen** — [GitHub](https://github.com/butalib)

---

## Acknowledgments

- [Angular](https://angular.dev/) — Frontend framework
- [ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/) — Backend framework
- [Stripe](https://stripe.com/docs) — Payment processing
- [Hangfire](https://hangfire.io/) — Background job processing
- [MailKit](https://github.com/jstedfast/MailKit) — Email sending
- [Chart.js](https://www.chartjs.org/) — Dashboard charts
