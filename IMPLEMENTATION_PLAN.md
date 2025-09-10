# TAE App Implementation Plan

This document describes the phased plan to implement HeroApp features into the new `tae_app` project using Tailwind CSS and PostgreSQL, with a modern UI and the copied/adapted models `Member`, `Job`, and `EmailSetting`.

---

## Checklist (requirements)
- Tailwind CSS + modern UI
- PostgreSQL (EF Core + Npgsql)
- Copy/adapt models: `Member`, `Job`, `EmailSetting`
- ASP.NET Identity (authentication), roles & permissions
- Role/permission-based authorization 
- CRUD admin UI (Razor Views) for Users, Roles, Permissions
- Public-facing pages: membership registration and public job applications
- File uploads for member visa IDs and job application documents
- Email sending support (`EmailSender` helper + SMTP config)
- EF Core migrations + DB seeding for roles/users/permissions
- Implement in phases with QA and acceptance criteria

---

## High-level plan (phases)

### Phase 0 — Prep & prerequisites
- Ensure .NET SDK, Node/npm, and `dotnet-ef` are installed.
- Choose secrets strategy (environment variables, user-secrets, or vault).
- Add `.gitignore` entries for `wwwroot/uploads` and config templates.

Deliverables:
- Tool checklist and `.env.example` (or docs).

### Phase 1 — Core data & Identity
- Add packages: `Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.EntityFrameworkCore.Design`, `Microsoft.AspNetCore.Identity.EntityFrameworkCore`.
- Add `Data/ApplicationDbContext.cs` and `Models/ApplicationUser.cs`.
- Copy/adapt models: `Models/Member.cs`, `Models/Job.cs`, `Models/EmailSetting.cs`.
- Add `Data/SeedData.cs` for roles + admin user seeding.
- Configure PostgreSQL connection in `appsettings.json` (use env var fallback).
- Create migrations and apply: `dotnet ef migrations add InitialCreate` and `dotnet ef database update`.

Acceptance criteria:
- Project builds, EF migrations generate, and DB updated locally.

### Phase 2 — Auth & permissions
- Implement roles and permissions
-

Acceptance criteria:
- Can create roles and assign permission claims; protected endpoints enforce permissions.

### Phase 3 — Tailwind + UI shell
- Initialize Tailwind (npm, PostCSS). Add `src/tailwind.css` and build to `wwwroot/css/site.css`.
- Create a modern layout (`Views/Shared/_Layout.cshtml`) using Tailwind utilities (header, sidebar, responsive grid).
- Add component styles for forms, tables, cards, and buttons.

Commands:
```bash
npm init -y
npm install -D tailwindcss postcss autoprefixer
npx tailwindcss init -p
# add src/tailwind.css and scripts, then:
npm run build:css
```

Acceptance criteria:
- `wwwroot/css/site.css` present and layout renders with Tailwind classes.

### Phase 4 — CRUD & public pages
- Implement Admin controllers / Razor Pages:
  - `UsersController`, `RolesController`, `PermissionsController`, `MembersController`, `JobsController`, `JobApplicationsController`, `EmailSettingsController`.
- Implement Public controllers/pages:
  - `RegisterController` (membership), `PublicJobApplicationController`.
- Implement services: `MemberService`, `JobService`, `EmailService`.
- Add server-side validation and paging/search on lists.

Acceptance criteria:
- Admin can CRUD Members and Jobs; public can register and submit job applications.

### Phase 5 — File uploads & Email
- Create `Services/FileStorage/IFileStorage.cs` and `LocalFileStorage.cs` to save files under `wwwroot/uploads` (organized by folder/date) with GUID filenames.
- Validate file types (image/pdf) and size (configurable, e.g., 10MB).
- Implement `Services/Email/IEmailSender.cs` and `SmtpEmailSender.cs` using SMTP config; support retries.
- Wire uploads to `Member.VisaIdFilePath` and job application document fields.

Security notes:
- Store SMTP credentials in environment variables or secret manager — do not commit.
- Sanitize and validate uploaded files; restrict public access if sensitive.

Acceptance criteria:
- Uploads persist to disk and path saved in DB; email sending is testable with configured SMTP.

### Phase 6 — QA, tests, docs, CI
- Add unit tests for services and at least one integration test for registration.
- Add `README.md` with run and build steps (Tailwind build, env vars, DB commands).
- Add CI tasks to run `dotnet build`, `npm run build:css`, `dotnet ef database update` (optional), and tests.

Acceptance criteria:
- CI pipeline runs build, CSS build, and unit tests successfully.

---

## Models (contracts)

### Member (adapted)
- Id (int)
- ApplicationUserId (string | FK to AspNetUsers)
- Code (string)
- UserId (string) — optional
- FirstName, MiddleName, LastName (string)
- Nationality (string)
- VisaType (string)
- Address (string)
- Permit (string)
- Emirate (string)
- PhoneNumber (string)
- EmailAddress (string)
- EmploymentStatus (string)
- CompanyName (string)
- CreatedAt (DateTime)
- VisaIdFilePath (string)
- DoYouKnowAboutTAE (bool)
- Advice (string)

### Job
- Id (int)
- JobTypeId (int) / JobTypeName (string)
- Title (string)
- Description (text)
- PostedAt (DateTime)
- DeadLineDate (DateTime)
- IsBlocked (bool)

### EmailSetting
- Id (int)
- SmtpServer (string)
- SmtpPort (int)
- Username (string)
- Password (string) — do not store plaintext in repo
- UseSsl (bool)
- FromAddress (string)

---

## Authorization design
- Roles: e.g., `SuperUser`, `Admin`, `MemberEditor`, `MemberViewer`, `JobEditor`, etc.
- Permissions: claim-based strings like `members.create`, `jobs.edit`.
- `PermissionPolicyProvider` maps policies to `PermissionRequirement` instances.
- `PermissionAuthorizationHandler` checks claims/roles and enforces permission logic.

Usage example:
```csharp
[Authorize(Policy = "Permission:members.create")]
public IActionResult Create() { ... }
```

---

## File upload rules
- Allowed MIME types: `image/jpeg`, `image/png`, `application/pdf`.
- Max size: default 10 MB (configurable in `appsettings`).
- Store files with GUID filenames under `wwwroot/uploads/{type}/{yyyy}/{mm}/`.
- Validate and sanitize inputs server-side.

---

## Email strategy
- `IEmailSender.SendAsync(EmailMessage)` implemented by `SmtpEmailSender`.
- Read SMTP config from `EmailSetting` or environment variables.
- Use background queue or retry policy for transient failures.

---

## Seeding & startup
- Implement `Data/SeedData.Initialize(IServiceProvider)` to ensure DB is migrated and roles + admin user are seeded.
- Call `SeedData.Initialize` from `Program.Main` during host startup.

Example:
```csharp
using (var scope = host.Services.CreateScope())
{
  var services = scope.ServiceProvider;
  await SeedData.Initialize(services);
}
```

---

## Key commands

Add packages & tools:
```bash
cd /path/to/tae_app
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet tool install --global dotnet-ef
```

Tailwind init & build:
```bash
npm init -y
npm install -D tailwindcss postcss autoprefixer
npx tailwindcss init -p
# add src/tailwind.css and package.json scripts, then:
npm run build:css
```

Migrations & DB:
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Run the app:
```bash
dotnet run
```

---

## Quality gates & acceptance criteria (summary)
- `dotnet build` succeeds.
- `npm run build:css` produces `wwwroot/css/site.css`.
- EF Core migrations can be generated and applied locally.
- Seeded admin user exists and roles are present.
- Admin CRUD flows for Members and Jobs work end-to-end.
- Public registration saves the `VisaIdFilePath` and rejects bad uploads.
- Email sending is testable with configured SMTP credentials.
- Unit tests for core services and one integration test for registration pass.

---

## Risks & mitigations
- Secrets leakage — use env vars or a vault; never commit credentials.
- File upload abuse — validate MIME & size; consider virus-scan integration.
- Postgres migration differences — test locally and pin EF Core packages.

---

## Timeline (estimate)
- Phase 1 (MVP registration + NIDA): 3–5 days
- Phase 2 (events & dynamic forms): 2–4 days
- Phase 3 (jobs/business/news/resources): 3–5 days
- Phase 4 (challenges, full events features): 2–4 days
- Phase 5 (uploads, email, i18n): 2–3 days
- Phase 6 (QA, tests, deploy): 2–3 days

Total estimate: ~12–24 working days depending on scope and integration complexity.

---

## Client requirements (understanding)
The client runs a diaspora community for Tanzanians in the United Arab Emirates. They want a platform with the following capabilities:

1. Members registration, with automatic generation of ID number.
2. Per-event custom registration forms (dynamic extra fields). If a registrant is not a member they must register first.
3. Job opportunities page for UAE job listings.
4. Business section for trade and services (UAE <-> Tanzania).
5. News page.
6. Awareness and education page for diaspora (resources, guides).
7. Challenges & opportunities submission and tracking (feedback loop).
8. Events and gatherings management for the diaspora.

Special workflow to implement first (priority):
- Registration flow must allow members to opt-in for NIDA services ("Wanaotaka huduma za NIDA"). When opting in: present payable service option; after successful payment the member receives an appointment (appointment scheduling flow).

---

## Improvements to phases (prioritized for client requirements)

The original phase plan is updated to prioritize the membership + NIDA flow as MVP, then add event-driven dynamic forms, public content (jobs, news), and business/challenge features.

### Phase 0 — Prep & alignment
- Tools, secrets strategy, environment, minimal UX sketches for registration and NIDA flow.

### Phase 1 — MVP: Membership + NIDA flow (critical path)
Goal: public registration, member record creation with generated MemberID, opt-in for NIDA service, payment, and appointment issuance.

Deliverables:
- `Member` model with required fields (see below).
- Registration public page with modern Tailwind UI and client-side validation.
- Payment integration (Stripe or PayPal sandbox) for NIDA service payments.
- Appointment generation (simple scheduler) and notification (email/SMS placeholder).
- Admin dashboard minimal: view members, view payments, assign/adjust appointments.

Why first: the client explicitly needs the NIDA service workflow for members and it drives immediate value.

Acceptance criteria:
- A non-member can register and optionally choose NIDA service.
- If NIDA is chosen, payment is processed and an appointment record is created and emailed.
- Member record saved in Postgres and MemberID auto-generated.

### Phase 2 — Event system with dynamic forms
Goal: create an events engine where each event can define an arbitrary set of extra fields for registration. Event registrations must require membership or inline registration.

Deliverables:
- `Event` entity and `EventFormField` entity to describe dynamic fields (type, required, options).
- Event admin page to create events and configure form fields.
- Public event registration form generation at runtime; enforces membership requirement.
- Event registration storage and CSV export.

Acceptance criteria:
- Admin can create an event and add fields (text, number, select, checkbox, file upload).
- Public registration validates dynamic fields and associates registration with a `Member`.

### Phase 3 — Jobs, Business, News, Awareness pages
Goal: public-facing content management for Jobs, Business listings, News posts, and Awareness resources.

Deliverables:
- `Job` CRUD and public listing with apply flow (job application entity with file uploads).
- `BusinessListing` entity + admin UI for listing trade/services (with categories, contact info, location, images).
- `News` CMS section (admin create/edit/publish) and public news feed.
- `Resources` / `Awareness` page with content categories and downloadable guides.

Acceptance criteria:
- Jobs page lists vacancies; visitors can apply; admin sees applications.
- Business listings searchable and filterable.

### Phase 4 — Challenges & Opportunities, Events full features
Goal: feedback collection and structured follow-up; full events lifecycle (RSVP, check-in, reporting).

Deliverables:
- `Challenge` entity + submission workflow, admin responses, status tracking.
- Event check-in flow (QR code or admin check-in), attendee lists, post-event reports.
- Dashboard widgets summarizing challenges, event attendance, jobs posted.

Acceptance criteria:
- Users can submit challenges and admins can respond/track.
- Event organizers can mark attendance and export attendee lists.

### Phase 5 — File uploads, email, internationalization & polish
Goal: secure file storage, email reliability, multi-language support (Swahili/English), accessibility & design polish.

Deliverables:
- `IFileStorage` abstraction, optional cloud storage adapter.
- `IEmailSender` with retry and templating.
- Language resource files and UI copy in Swahili + English.

Acceptance criteria:
- File uploads validated & retrievable; emails reliably sent; UI supports both languages.

### Phase 6 — QA, tests, docs, CI/CD, deploy
Standard QA, unit and integration tests, documentation, and CI pipeline.

---

## Required fields per feature (recommended)

### Member (registration fields)
- MemberId (generated string e.g., TAE-{YYYY}-{NNNN})
- ApplicationUserId (optional) — link to Identity user
- FirstName (required)
- MiddleName
- LastName (required)
- DateOfBirth
- Gender
- Nationality
- PassportNumber
- EmiratesId (if available)
- Address (text)
- City
- Emirate
- PhoneNumber (required)
- EmailAddress (required)
- EmploymentStatus
- CompanyName
- DoYouKnowAboutTAE (bool)
- VisaType
- VisaIdFilePath (file upload)
- CreatedAt (timestamp)
- OptInNidaService (bool)
- NidaServiceStatus (enum: PendingPayment, Paid, AppointmentScheduled, Completed)

Notes: `PassportNumber` and `EmiratesId` help identity verification; VisaIdFile enables verification uploads.

### NIDA service related
- NidaServiceOptionId (if multiple service types)
- PaymentReference
- AmountPaid
- PaymentMethod
- AppointmentId (FK to Appointment table)

### Event / EventFormField
- Event: Id, Title, Description, Venue, StartDate, EndDate, IsPublic, MemberOnly (bool), Capacity
- EventFormField: Id, EventId, Label, FieldType (text, number, select, checkbox, file), IsRequired, Options (for select), Order
- EventRegistration: Id, EventId, MemberId, FormResponses (JSON), CreatedAt

### Job
- Job: Id, Title, Company, Location, Description, PostedAt, Deadline, SalaryRange, IsActive
- JobApplication: Id, JobId, MemberId (or Applicant info), CVFilePath, CoverLetter, AppliedAt

### BusinessListing
- Id, Title, OwnerMemberId, Description, Category, ContactEmail, ContactPhone, Website, Country, Images[]

### News / Resource
- News: Id, Title, Slug, Body, Excerpt, ImagePath, PublishedAt, AuthorId, Tags

### Challenge / Opportunity
- Id, MemberId (optional), Title, Description, Category, Status, AdminResponse, CreatedAt

### Appointment (for NIDA)
- Id, MemberId, ServiceType, ScheduledAt, Location, Status, CreatedAt

---

## UX / flow notes (important)
- Registration must be minimal and mobile-friendly; validation and progressive disclosure for optional fields.
- If a user attempts event registration and is not a member, present an inline registration modal or redirect to registration; after registration, continue event registration.
- Payment flows should support sandbox mode for testing and environment variables for real credentials.
- Communications: send confirmation emails after registration, payment, appointment scheduling, and event registration.

---

## Data privacy & compliance
- Store minimal PII and secure sensitive fields (passport, Emirates ID). Consider encryption at rest for sensitive columns.
- Add privacy policy and terms pages.

---

## Implementation checklist to start (MVP first)
1. Wire DB + Identity + Member model (Phase 1).
2. Public registration page with NIDA opt-in + payment sandbox + appointment generation.
3. Admin view for payments and appointment management.
4. Basic jobs listing and application endpoint (Phase 3 minimal).
5. Events skeleton with admin create & dynamic fields (Phase 2 minimal).

---

## Next steps for me
- I can start implementing Phase 1 now (create Member model, registration page, NIDA flow, payment stub, seed data) — this will create the core MVP.
- Or I can produce detailed API contracts and wireframes for the registration + event dynamic forms before coding.

Reply with `Start Phase 1` to begin implementation or `Wireframes` to get API/UX sketches first.
