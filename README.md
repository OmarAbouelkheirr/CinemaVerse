# CinemaVerse

[![Angular](https://img.shields.io/badge/Angular-21-DD0031?style=flat&logo=angular)](https://angular.dev/)
[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com/)
[![SQL Server](https://img.shields.io/badge/SQL_Server-2019-CC2927?style=flat&logo=microsoftsqlserver)](https://www.microsoft.com/en-us/sql-server)
[![Stripe](https://img.shields.io/badge/Stripe-Payments-635BFF?style=flat&logo=stripe)](https://stripe.com/)
[![License](https://img.shields.io/badge/License-MIT-green?style=flat)](LICENSE)

> A complete cinema ticket booking platform built with **Angular 21** (frontend) and **ASP.NET Core 9.0** (backend), featuring user authentication, movie browsing, seat selection, Stripe payments, QR code tickets, and a full admin panel.

---

## Table of Contents

- [Features](#features)
  - [User Features](#user-features)
  - [Admin Features](#admin-features)
- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Database](#database)
- [Authentication](#authentication)
- [Getting Started](#getting-started)
- [API Documentation](#api-documentation)

---

## Features

### User Features

#### Movie Browsing

Browse movies with search, genre, and language filters. View movie info, cast, images, and available showtimes.

| Home Page | Movie Listing | Movie Detail |
|:---------:|:-------------:|:------------:|
| <img src="docs/Images/user%20home.png" width="300"> | <img src="docs/Images/user%20movies.png" width="300"> | <img src="docs/Images/user%20movie%20details.png" width="300"> |

---

#### Seat Selection & Booking

Interactive seat grid with real-time availability, Stripe payment integration, and QR code ticket generation.

| Booking | Payment |
|---------|---------|
| ![Booking](docs/Images/User%20movie%20booking.png) | ![Payment](docs/Images/user%20payment.png) |

---

### Admin Features

#### Dashboard

KPI cards, revenue charts, booking trends, and occupancy rate at a glance.

| Dashboard |
|-----------|
| ![Dashboard](docs/Images/Dashboard.png) |

---

#### Movies Management

Full CRUD with media upload, genre assignment, and detailed movie views.

| Movies List | View Movie |
|-------------|------------|
| ![Movies List](docs/Images/admin%20movies.png) | ![View Movie](docs/Images/admin%20view%20movies.png) |

---

#### Branches & Halls

Manage cinema branches, hall configurations, seat layouts, and hall types (2D, 3D, IMAX, VIP).

| Branches | View Branch | Edit Hall |
|----------|-------------|-----------|
| ![Branches](docs/Images/Admin%20branch.png) | ![View Branch](docs/Images/admin%20view%20branch.png) | ![Edit Hall](docs/Images/admin%20edit%20hall.png) |

---

#### Showtimes

Schedule showtimes with conflict detection across halls and movies.

| Showtimes |
|-----------|
| ![Showtimes](docs/Images/admin%20showtimes.png) |

---

#### Users Management

View, create, edit, activate/deactivate user accounts with role-based access control.

| Users | View User |
|-------|-----------|
| ![Users](docs/Images/Admin%20User.png) | ![View User](docs/Images/Admin%20View%20user.png) |

---

#### Bookings Management

View all bookings, update status, and export to CSV.

| Bookings |
|----------|
| ![Bookings](docs/Images/admn%20bookings.png) |

---

#### Tickets Management

QR code lookup and check-in management for ticket validation.

| Tickets | View Ticket |
|---------|-------------|
| ![Tickets](docs/Images/admin%20tickets.png) | ![View Ticket](docs/Images/admin%20view%20ticket.png) |

---

## Tech Stack

| Frontend | Backend |
|----------|---------|
| Angular 21.2 | ASP.NET Core 9.0 |
| TypeScript 5.9 | Entity Framework Core 9.0 |
| SCSS + Bootstrap 5.3 | SQL Server |
| Chart.js 4.5 | JWT Bearer 8.15 |
| RxJS 7.8 | Stripe.net 50.2 |
| Angular Signals | Hangfire 1.8 |
| Vitest 4.0 | MailKit 4.1 / RazorLight 2.2 |
| | BCrypt.Net 4.0 / Serilog 8.0 |

---

## Architecture

### System Overview

```mermaid
graph LR
    subgraph Frontend["Frontend — Angular 21"]
        A[Angular SPA]
    end

    subgraph Backend["Backend — ASP.NET Core 9"]
        B[API Controllers]
        C[Services / Business Logic]
        D[EF Core / Repositories]
    end

    subgraph External["External Services"]
        E[(SQL Server)]
        F[Stripe]
        G[Hangfire]
        H[MailKit / SMTP]
    end

    A -->|HTTP + JWT| B
    B --> C
    C --> D
    D --> E
    C --> F
    C --> G
    C --> H
```

> **Note:** The frontend communicates with the backend via RESTful APIs using JWT authentication. External services like Stripe for payments, Hangfire for background jobs, and MailKit for emails are integrated at the service layer.

---

### Backend — Clean Architecture

The backend follows Clean Architecture with three distinct layers, ensuring separation of concerns and testability.

```mermaid
graph TB
    subgraph Presentation["Presentation Layer — CinemaVerse"]
        P1[Controllers]
        P2[Middleware]
        P3[Filters]
        P4[Infrastructure]
    end

    subgraph Business["Business Layer — CinemaVerse.Services"]
        S1[Interfaces]
        S2[Implementations]
        S3[DTOs]
        S4[Mappers]
        S5[Constants]
    end

    subgraph Data["Data Layer — CinemaVerse.Data"]
        D1[AppDbContext]
        D2[Models / Entities]
        D3[Configurations]
        D4[Repositories]
        D5[Migrations]
    end

    P1 --> S1
    P2 --> S1
    P3 --> S1
    S1 --> S2
    S2 --> D1
    S2 --> D2
    D4 --> D1
```

> **Key Principles:**
> - **Dependency Inversion:** Business layer defines interfaces; implementation details live in outer layers
> - **Repository Pattern:** Data access is abstracted behind repository interfaces
> - **Unit of Work:** Transaction management across multiple repositories

---

### Frontend — Angular Architecture

The frontend follows a modular architecture with lazy-loaded feature modules and shared components.

```mermaid
graph TB
    subgraph Core["Core Module"]
        C1[Auth]
        C2[Config]
        C3[Guards]
        C4[HTTP Client]
        C5[Interceptors]
    end

    subgraph Layout["Layout Module"]
        L1[Header]
        L2[Router Outlet]
        L3[Footer]
    end

    subgraph Shared["Shared Module"]
        SH1[Pagination]
        SH2[Modals]
        SH3[Design System UI]
    end

    subgraph Features["Feature Modules"]
        F1[Auth — Login/Register]
        subgraph User["User Features"]
            U1[Home]
            U2[Movie Detail]
            U3[Movie Booking]
            U4[Bookings]
            U5[Profile]
        end
        subgraph Admin["Admin Features"]
            A1[Dashboard]
            A2[Movies]
            A3[Users]
            A4[Branches & Halls]
            A5[Showtimes]
            A6[Genres]
            A7[Bookings]
            A8[Payments]
            A9[Tickets]
        end
    end

    Layout --> Core
    Features --> Core
    Features --> Shared
```

> **Key Principles:**
> - **Standalone Components:** Angular 21 standalone components without NgModules
> - **Signal-Based State:** Using Angular Signals for reactive state management
> - **Lazy Loading:** Feature modules are lazy-loaded for optimal bundle size

---

## Getting Started

### Prerequisites

- .NET SDK 9.0+, Node.js 18+, SQL Server 2019+, Stripe account

### Backend Setup

```bash
cd "CinemaVerse Backend/src"
dotnet restore
dotnet ef database update --project CinemaVerse.Data --startup-project CinemaVerse
cd CinemaVerse
dotnet run
```

> **Tip:** Update `appsettings.json` with your connection string and Stripe keys before running migrations.

### Frontend Setup

```bash
cd "CinmaVerse Front"
npm install
npm start
```

> **Note:** Update the API URL in `src/app/core/config/api.config.ts` for local development.

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
| User (Profile, Bookings, Tickets, Payments, Reviews) | 17 | Required |
| Public (Movies, Seats) | 3 | Public |
| Admin (Users, Movies, Branches, Halls, Genres, Showtimes, Bookings, Payments, Tickets, Media) | 66 | Admin |
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

```mermaid
sequenceDiagram
    participant U as User
    participant F as Frontend
    participant B as Backend
    participant DB as Database

    U->>F: Enter credentials
    F->>B: POST /api/auth/login
    B->>DB: Validate user
    DB-->>B: User record
    B-->>F: { accessToken, refreshToken, userId, email, role }
    F->>F: Store tokens in localStorage

    loop Every Authenticated Request
        F->>B: GET /api/... + Bearer token
        B->>B: Validate JWT
        B-->>F: Protected resource
    end

    U->>F: Click "My Profile"
    F->>B: GET /api/me + Bearer token
    B-->>F: { userProfile }
```

> **Note:** Access tokens expire after 60 minutes. Refresh tokens are valid for 7 days and enable silent token renewal without requiring the user to log in again.

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

The database consists of 17 entities with clear relationships for users, movies, bookings, and cinema management.

```mermaid
erDiagram
    User ||--o{ Booking : "creates"
    User ||--o{ Review : "writes"
    Booking ||--o{ BookingSeat : "has"
    Booking ||--o{ Ticket : "generates"
    Booking ||--o{ BookingPayment : "paid via"
    Booking }o--|| MovieShowTime : "for"
    MovieShowTime }o--|| Movie : "shows"
    MovieShowTime }o--|| Hall : "in"
    Hall }o--|| Branch : "belongs to"
    Hall ||--o{ Seat : "contains"
    Seat ||--o{ BookingSeat : "reserved in"
    Seat ||--o{ Ticket : "assigned to"
    Movie ||--o{ MovieGenre : "tagged with"
    Genre ||--o{ MovieGenre : "categorizes"
    Movie ||--o{ MovieImage : "has"
    Movie ||--o{ MovieCastMember : "features"
```

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

## Authors

| Name | Role | GitHub |
|------|------|--------|
| **Nour Eldeen Mahmoud** | Backend | [@NourEldeenMahmoud](https://github.com/NourEldeenMahmoud) |
| **Omar Aboelkheir** | Backend | [@OmarAbouelkheirr](https://github.com/OmarAbouelkheirr) |
| **Ahmed Kamal** | Frontend | [@butalib](https://github.com/butalib) |

---

## Acknowledgments

- [Angular](https://angular.dev/) — Frontend framework
- [ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/) — Backend framework
- [Stripe](https://stripe.com/docs) — Payment processing
- [Hangfire](https://hangfire.io/) — Background job processing
- [MailKit](https://github.com/jstedfast/MailKit) — Email sending
- [Chart.js](https://www.chartjs.org/) — Dashboard charts
