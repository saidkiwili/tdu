Diaspora Community Website - Phase Planning (.dm)

Overview
--------
Purpose: produce a phased project plan and acceptance-driven backlog for a Diaspora Community Website targeted at Tanzanian diaspora in the UAE. The site focuses on culture, services, opportunities and leadership engagement (explicitly no political content).

High-level goal: deliver an accessible public website + admin CMS and an optional members area supporting events, services (NIDA), business directory, media, knowledge base, and membership management.

Tech assumptions (current repo): ASP.NET Core 8.0 (Razor/Blazor mix), EF Core + PostgreSQL, Tailwind CSS, Identity with roles, SweetAlert2, existing Admin/Members CRUD features.

Checklist (map to customer's 13 sections)
----------------------------------------
- [ ] 1. Homepage
- [ ] 2. About Us
- [ ] 3. Culture & Heritage
- [ ] 4. Community Services
- [ ] 5. Events & Activities
- [ ] 6. Business & Opportunities
- [ ] 7. Youth & Students
- [ ] 8. Media & Resources
- [ ] 9. Gallery
- [ ] 10. Knowledge Base
- [ ] 11. Membership Area (optional)
- [ ] 12. Contact & Support
- [ ] 13. Safety & Awareness

Phases & Deliverables
---------------------
Phase 0 — Discovery & Setup (1 week)
- Deliverables: final scope doc, MVP definition, wireframe sketches for Homepage + Event flow + Membership sign-up. Data model review (Member entity, Event, Business, Media, Article).
- Acceptance: PO sign-off on MVP and data model.
- Tasks:
  - Map existing Member model fields to membership requirements (VisaID file, NIDA opt-in, contact details, employment info).
  - Inventory current admin pages (Members CRUD done). Identify gaps.

Phase 1 — MVP Public Site + Basic Admin (3 weeks)
- Deliverables:
  - Homepage, About Us, Events listing + single event page, Contact page, simple Media gallery, Basic Knowledge Base (how-to articles), Business directory listing (read-only).
  - Admin pages for Events, Media, Articles, Business listings (CRUD).
  - Membership sign-up (public) and admin member management (existing Members area). File uploads working (visa ID).
- Acceptance criteria:
  - Pages render with responsive Tailwind layout.
  - Admin can create/edit events and upload media.
  - Members can register, upload documents, admin sees member details.
- Tasks:
  - Implement homepage modules: hero, mission, quick links, highlight card for service day.
  - Implement Event model + calendar view (basic list + date filter).
  - Hook file storage (wwwroot/uploads or cloud-compatible path) and show downloads in admin view modal.

Phase 2 — Community Services & NIDA workflows (2–3 weeks)
- Deliverables:
  - Structured Service pages: NIDA, banks, TIC, immigration guidance.
  - Service-days/Campaigns admin module to publish upcoming NIDA registration events.
  - NIDA opt-in flow capture + admin export for attendees.
- Acceptance:
  - Admin can publish service events and view/export participants.
  - Opt-in flags and payment references stored.

Phase 3 — Media, Gallery, Knowledge Base, Search (2 weeks)
- Deliverables:
  - Photo/video gallery with categories, archive pages.
  - Knowledge base with tagging, search, and categories (Labor, Legal, Investment, Banking).
  - Newsletter subscribe (email capture) and basic CMS for monthly newsletter.
- Acceptance:
  - Editors can create articles, tag them, and content is searchable.

Phase 4 — Business Directory, Jobs, Youth features (2 weeks)
- Deliverables:
  - Business listings with contact, category, map link, optional discounts for members.
  - Job board: admin posts + external/remote postings, filtering by location/type.
  - Youth/Students area offering mentor signups and scholarship listings (simple forms).

Phase 5 — Membership Area & Optional Forum (2–4 weeks)
- Deliverables:
  - Members-only directory, volunteer signups, discounts page, simple discussion forum or Slack/Discord integration.
  - Role-based access control: MemberViewer, MemberEditor already in system.
- Acceptance:
  - Members can login, view member-only content, and admin controls respect roles.

Phase 6 — Polish, Security, Testing & Launch (2 weeks)
- Deliverables: accessibility checks (WCAG basics), security hardening (file validation, upload size limits), QA test pass, analytics and SEO meta.
- Acceptance: No critical defects, basic performance acceptance, documented runbook for deploy.

Suggested Sprint Breakdown (2-week sprints)
-------------------------------------------
Sprint 1: Phase 0 + start Phase 1 (Homepage, wireframes, Member model fixes). 
Sprint 2: Phase 1 complete (Events, Admin CRUD, Contact form).
Sprint 3: Phase 2 + Phase 3 work.
Sprint 4: Phase 4 + Phase 5 core features.
Sprint 5: Phase 6 final polish, UAT, launch.

Epics / Example User Stories
----------------------------
Epic: Events
- As a visitor I want to see upcoming events so I can attend service days.
- As an admin I want to create events with date/time, location, capacity, and upload media.

Epic: Membership
- As a user I want to register and upload my visa/ID so the admin can verify documents.
- As an admin I want to view member details and download uploaded files.

Epic: Services (NIDA)
- As a member I want to opt-in to NIDA service and receive confirmation when the registration day is published.
- As an admin I want exportable CSV of opt-ins and payment refs.

Data Model (initial)
--------------------
- Member (existing) -> fields: Id, MemberId, ApplicationUserId, FirstName, MiddleName, LastName, FullName (NotMapped), DateOfBirth, Gender, Nationality, PassportNumber, EmiratesId, Address, City, Emirate, PhoneNumber, EmailAddress, EmploymentStatus, CompanyName, DoYouKnowAboutTAE, VisaType, VisaIdFilePath, Advice, CreatedAt, OptInNidaService, NidaServiceStatus, PaymentReference, AmountPaid, PaymentMethod.
- Event -> Id, Title, Slug, Description, StartDate, EndDate, Location, Capacity, IsServiceDay(bool), CreatedAt, CreatedBy, MediaPaths[]
- Article -> Id, Title, Slug, Content, Excerpt, Tags[], Category, AuthorId, PublishedAt, IsPublic
- BusinessListing -> Id, Name, Category, ContactInfo, Description, Address, Website, IsDiasporaDiscount(bool)
- Media -> Id, FilePath, Type(image/video), Title, Caption, UploadedAt, UploadedBy

APIs & Exports
--------------
- Admin endpoints (Razor pages handlers) for CSV exports: members opt-ins, event participants.
- Public endpoints for event feeds (JSON) and gallery.

Security & Privacy
------------------
- Validate and sanitize all file uploads; restrict file types (pdf, jpg, png) and max size (e.g., 8 MB).
- Serve uploads from wwwroot/uploads with safe filenames or use guid based names.
- Ensure consent for storing personal data; add privacy note on membership registration.
- Role-based access for admin pages (use existing Identity roles).

Localization & Content
----------------------
- Primary language: English (site can include Kiswahili content sections). Provide Kiswahili subtitle or corner.
- SEO: friendly slugs, meta tags, OpenGraph previews for events and articles.

UX & Accessibility
------------------
- Use responsive Tailwind components; keyboard accessible forms and modals; aria labels for dynamic content.
- Ensure high-contrast options for branding colors.

Acceptance Criteria (MVP)
-------------------------
- Main navigation with Home, About, Services, Events, Jobs/Business, Media, Knowledge Base, Contact.
- Event create/edit in admin working, public event list + single event page available.
- Member registration with file upload; admin can view/download file in modal.
- Contact form sends email (or stores message) and shows confirmation.

Risks & Mitigations
-------------------
- Risk: File-storage size and backup — Mitigation: store only essential docs, consider cloud storage later.
- Risk: Sensitive data retention — Mitigation: policy and retention periods, secure access.

Monitoring & Metrics
--------------------
- Track: page visits, event signups, membership signups, newsletter subscriptions.
- Basic analytics: Google Analytics/Matomo + site search logs for popular KB topics.

Tasks (first 10 concrete tasks)
------------------------------
1. Finalize MVP pages & approve wireframes (Homepage, Event, Membership, Contact).
2. Implement Event model + migration + admin CRUD.
3. Hook file upload pipeline (visa docs) and add download link in `Index.cshtml` view modal (done part).
4. Implement homepage hero + highlights module.
5. Create Contact page with form handler and spam protection (honeypot/recaptcha later).
6. Add Knowledge Base Article model + simple listing.
7. Add Gallery model + admin upload + public grid UI.
8. Business directory model + listing page.
9. Membership registration flow end-to-end (validation, file upload, email notification).
10. Add NIDA opt-in export (CSV) and admin page to manage service-day participants.

QA & Tests
----------
- Unit tests: data model validation, exports, and handler logic.
- Integration tests: registration flow + file upload (mock storage), event creation.
- Manual UAT checklist: responsive layout, forms, downloads, role-based access.

Deployment & Runbook
--------------------
- Use environment-specific appsettings for DB and storage paths.
- Migration steps: EF migrations + seeders (roles, sample data).
- Rollback: always take DB backup before major migrations.

Next Steps (what I can do now)
------------------------------
- Convert this .dm planning file into GitHub issues or sprint backlog.
- Start implementing Sprint 1 tasks: finalize homepage wireframe and implement hero module + highlight slot.
- Continue refining Members page features (view modal file download already updated; can add block/unblock handlers next).

Document history
----------------
Created: 2025-09-09
Author: Project planning artifact (generated)

Notes
-----
This plan is an editable authoritative source for the project roadmap. I can split items into tickets, produce wireframes, or generate CRUD scaffolding for prioritized epics on request.
