# TAE Community Platform - Setup Instructions

## Phase 1 MVP - Complete ✅

The Phase 1 implementation is now complete with the following features:

### ✅ Implemented Features

1. **Member Registration System**
   - Complete registration form with all required fields
   - Automatic Member ID generation (TAE-YYYY-NNNN format)
   - File upload support for visa documents
   - Email validation and form validation

2. **NIDA Service Workflow**
   - Optional NIDA service opt-in during registration
   - Payment processing flow (sandbox/demo)
   - Automatic appointment scheduling
   - Appointment confirmation system

3. **Database & Models**
   - PostgreSQL/EF Core setup with migrations
   - Complete data models: Member, Appointment, Job, Event, etc.
   - Identity integration with roles and users
   - Seed data for admin user and roles

4. **Admin Dashboard**
   - Member management and viewing
   - Appointment management with status updates
   - Dashboard with statistics
   - CSV export functionality

5. **UI & Views**
   - Bootstrap-based responsive design
   - Registration flow with conditional fields
   - Payment and confirmation pages
   - Admin interface

## 🚀 Quick Start (Without Database)

The application will run even without PostgreSQL configured. It will show connection errors but remain functional for UI testing.

```bash
cd /Users/admin/Documents/projects/tae_projects/tae_app
dotnet run
```

Then visit: http://localhost:5294

## 📋 Full Setup (With Database)

### Prerequisites

1. **Install PostgreSQL**
   ```bash
   # macOS with Homebrew
   brew install postgresql
   brew services start postgresql
   
   # Create database and user
   createdb tae_app
   psql -c "CREATE USER postgres WITH PASSWORD 'password';"
   psql -c "GRANT ALL PRIVILEGES ON DATABASE tae_app TO postgres;"
   ```

2. **Update Connection String** (if needed)
   Edit `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=tae_app;Username=postgres;Password=your_password"
     }
   }
   ```

3. **Run Migrations**
   ```bash
   export PATH="$PATH:/Users/admin/.dotnet/tools"
   dotnet ef database update
   ```

4. **Start Application**
   ```bash
   dotnet run
   ```

### 🔑 Default Admin Account

- **Email:** admin@tae.ae
- **Password:** TaeAdmin123!
- **Roles:** SuperAdmin

## 📱 Testing the Application

### Registration Flow Test
1. Go to `/Registration`
2. Fill out member registration form
3. Optionally check "NIDA Service" option
4. Submit registration
5. If NIDA selected: proceed through payment flow
6. View appointment confirmation

### Admin Dashboard Test
1. Go to `/Admin/Dashboard` (requires login)
2. View member statistics
3. Manage members at `/Admin/Members`
4. Manage appointments at `/Admin/Appointments`

## 🎯 What's Working

- ✅ Member registration with file uploads
- ✅ NIDA service opt-in and payment flow
- ✅ Appointment scheduling and management
- ✅ Admin dashboard and member management
- ✅ CSV export functionality
- ✅ Identity with role-based access
- ✅ Database migrations and seeding

## 🔧 Configuration Options

### File Upload Settings
- Default path: `wwwroot/uploads/visa/`
- Accepted formats: JPG, PNG, PDF
- Max size: Configurable (default: 10MB)

### Payment Settings
- Currently: Demo/sandbox mode
- Payment methods: Cash, Bank Transfer, Credit Card (demo)
- NIDA service fee: AED 50

### Email Settings
- Currently: Not configured (placeholder)
- Needs SMTP configuration for production

## 📊 Database Schema

### Core Tables
- `Members` - Member registration data
- `Appointments` - NIDA service appointments
- `AspNetUsers` - Identity users
- `AspNetRoles` - System roles
- `Events` - Community events (Phase 2)
- `Jobs` - Job postings (Phase 3)

## 🚀 Next Steps

### Phase 2 - Events System
- Dynamic event form builder
- Event registration flow
- Member-only event enforcement

### Phase 3 - Jobs & Business
- Job posting and application system
- Business directory
- News and awareness content

### Phase 4 - Advanced Features
- Challenges & opportunities system
- Advanced reporting
- Email notifications

### Phase 5 - Production Ready
- Tailwind CSS integration
- Email service configuration
- File storage optimization
- Multi-language support (English/Swahili)

## 📞 Current URLs

- **Home:** `/`
- **Registration:** `/Registration`
- **NIDA Payment:** `/Registration/NidaPayment/{memberId}`
- **Admin Dashboard:** `/Admin/Dashboard`
- **Admin Members:** `/Admin/Members`
- **Admin Appointments:** `/Admin/Appointments`

## 🔐 Security Notes

- File uploads are validated for type and size
- Payment processing is currently demo/sandbox
- Admin routes require authentication
- Connection strings should use environment variables in production

---

**Status:** Phase 1 MVP Complete ✅
**Next:** Ready to start Phase 2 (Events) or configure production database
