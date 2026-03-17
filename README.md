# RWA E-trgovina (Student Project)

This repository contains a student e-commerce project built for a university class.  
The project demonstrates how to build both:

- a **server-rendered web shop** (`WebApp`), and
- a **REST API backend** (`WebAPI`),

using a shared SQL Server database schema.

---

## Project overview

The solution is built with **.NET 8** and contains two applications:

- `E-trgovina/WebApp` – ASP.NET Core MVC app with Razor views (storefront + admin pages)
- `E-trgovina/WebAPI` – ASP.NET Core Web API with JWT auth, Swagger, and a small static admin UI for API login/log viewing
- `Database/Database.sql` – database schema + seed data

Both applications connect to the same e-commerce database and use Entity Framework Core models.

---

## What the project does

### Customer-facing functionality (WebApp)

- User registration and login (cookie authentication)
- Product browsing and product detail pages
- Shopping cart management
- Order checkout and order history
- Profile editing and password change

### Admin functionality (WebApp)

- Category management (CRUD)
- Country management (CRUD)
- Product management (CRUD + pagination/search/filtering)
- Order overview and order details

### API functionality (WebAPI)

- Authentication endpoints (register, login, change password)
- Category, country, and product endpoints
- Role-protected endpoints for admin operations
- JWT-based authorization
- Swagger/OpenAPI documentation
- Log endpoints + logging service persisted in database

---

## Domain model (database)

Main entities used in the project:

- `User`
- `Category`
- `Product`
- `Country`
- `Order`
- `OrderItem`
- `Log`

Notable relationship:

- **Many-to-many** between `Product` and `Country` through `ProductCountry`.

The SQL script in `Database/Database.sql` creates schema objects and inserts initial seed data for categories, countries, and products.

---

## Tech stack

- **.NET 8**
- **ASP.NET Core MVC** (WebApp)
- **ASP.NET Core Web API** (WebAPI)
- **Entity Framework Core 8** + SQL Server
- **Cookie Authentication** (WebApp)
- **JWT Bearer Authentication** (WebAPI)
- **Swagger / Swashbuckle** (WebAPI)
- HTML/CSS/JavaScript for API static pages (`wwwroot`)

---

## Solution structure

```text
RWA-E-trgovina/
├─ E-trgovina/
│  ├─ E-trgovina.sln
│  ├─ WebApp/
│  │  ├─ Controllers/
│  │  ├─ Models/
│  │  ├─ ViewModels/
│  │  ├─ Views/
│  │  └─ wwwroot/
│  └─ WebAPI/
│     ├─ Controllers/
│     ├─ DTOs/
│     ├─ Models/
│     ├─ Security/
│     ├─ Services/
│     └─ wwwroot/
└─ Database/
   └─ Database.sql
```

---

## How to run locally

> Prerequisites: .NET 8 SDK + SQL Server.

1. **Create database objects**  
   Run `Database/Database.sql` against your SQL Server instance.

2. **Configure connection strings**  
   Add `ConnectionStrings:EcommerceConnStr` in:
   - `E-trgovina/WebApp/appsettings.json`
   - `E-trgovina/WebAPI/appsettings.json`

3. **Configure JWT settings** (WebAPI)  
   Add these settings to `E-trgovina/WebAPI/appsettings.json`:
   - `JWT:SecureKey`
   - `JWT:Issuer`
   - `JWT:Audience`
   - `JWT:ExpirationMinutes`

4. **Run applications**
   - WebApp: `dotnet run --project E-trgovina/WebApp`
   - WebAPI: `dotnet run --project E-trgovina/WebAPI`

Default development URLs from launch settings:

- WebApp: `http://localhost:5129`
- WebAPI: `http://localhost:5255`

---

## Notes on current architecture

This codebase is organized in a practical, student-friendly way with MVC/API projects that directly use EF Core DbContext and entity models.

That is perfectly fine for learning and medium-small class projects, but as the system grows, separation of concerns can become harder to maintain.

---

## How I would redesign it into a multilayer architecture

If this project were evolved into a more production-oriented architecture, I would split responsibilities into layers using a **Clean/Layered Architecture** style.

### 1) Proposed layers

1. **Presentation layer**
   - `WebApp` (MVC/UI)
   - `WebAPI` (HTTP endpoints)
   - Responsibilities: request/response handling, authentication entry points, validation of API contracts.

2. **Application layer**
   - Use cases and orchestration (e.g., `CreateOrder`, `AddToCart`, `UpdateProductStock`, `RegisterUser`)
   - DTOs/commands/queries
   - Transaction boundaries

3. **Domain layer**
   - Core business entities and rules (order totals, stock checks, country availability, status transitions)
   - Domain services and invariants
   - No dependency on EF, ASP.NET, or infrastructure details

4. **Infrastructure layer**
   - EF Core DbContext/repositories
   - Security providers (JWT creation, password hashing adapter)
   - Logging persistence
   - External integrations (email, payment provider, etc.)

### 2) What to move where

- Keep controllers thin: map HTTP -> command/query -> result
- Move business logic from controllers into **Application services/use cases**
- Move domain rules into domain entities/value objects/services
- Keep EF-only concerns in infrastructure repositories
- Use interfaces in Application layer, implemented in Infrastructure

### 3) Suggested project split

```text
src/
├─ ECommerce.Domain/
├─ ECommerce.Application/
├─ ECommerce.Infrastructure/
├─ ECommerce.WebApi/
└─ ECommerce.WebApp/
```

### 4) Benefits for this project

- Easier testing of business rules without web/database setup
- Better long-term maintainability for student project extensions
- Cleaner role/security handling and reuse between MVC and API
- Clear boundaries for future additions (payments, shipping, notifications)

### 5) Practical migration plan

1. Introduce `Application` project and move one feature first (e.g., product browse/search).
2. Add interfaces (`IProductService`, `IOrderService`) and use DI in controllers.
3. Extract business rules from controllers into application/domain classes.
4. Move data access behind repositories or query services.
5. Add unit tests at application/domain level.
6. Continue feature-by-feature until controllers are mostly orchestration only.

---

## Educational value of the current project

As a class project, this repository already covers many important concepts:

- Relational modeling and SQL seed scripts
- EF Core mapping and navigation properties
- Cookie vs JWT authentication approaches
- Role-based authorization
- MVC + API coexistence in one solution
- Basic logging and admin tooling

It is a strong foundation that can be incrementally refactored into a multilayer architecture as complexity grows.
