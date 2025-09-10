# 🐳 Docker Setup for TAE Community Platform

## Quick Start

### Option 1: Automated Setup (Recommended)
```bash
cd /Users/admin/Documents/projects/tae_projects/tae_app
./start-docker.sh
```

### Option 2: Manual Setup
```bash
cd /Users/admin/Documents/projects/tae_projects/tae_app

# Build and start containers
docker-compose up --build -d

# View logs
docker-compose logs -f
```

## 🌐 Access the Application

Once containers are running:

- **Web Application:** http://localhost:8080
- **PostgreSQL Database:** localhost:5432
- **Admin Dashboard:** http://localhost:8080/Admin/Dashboard

## 🔐 Default Credentials

**Admin Account:**
- Email: `admin@tae.ae`
- Password: `TaeAdmin123!`

**Database:**
- Host: `localhost`
- Port: `5432`
- Database: `tae_app`
- Username: `postgres`
- Password: `password`

## 📋 Container Architecture

### Services

1. **web** (TAE Application)
   - Port: 8080
   - Runs ASP.NET Core app
   - Auto-applies database migrations
   - Health checks enabled

2. **db** (PostgreSQL)
   - Port: 5432
   - Persistent data storage
   - Auto-creates database and user
   - Health checks enabled

## 🔧 Docker Commands

### Basic Operations
```bash
# Start containers
docker-compose up -d

# Stop containers
docker-compose down

# Restart containers
docker-compose restart

# View logs
docker-compose logs -f

# View specific service logs
docker-compose logs -f web
docker-compose logs -f db
```

### Development Commands
```bash
# Rebuild containers (after code changes)
docker-compose up --build

# Reset everything (removes data)
docker-compose down -v
docker-compose up --build

# Execute commands in containers
docker-compose exec web bash
docker-compose exec db psql -U postgres -d tae_app
```

### Database Management
```bash
# Access PostgreSQL shell
docker-compose exec db psql -U postgres -d tae_app

# Backup database
docker-compose exec db pg_dump -U postgres tae_app > backup.sql

# Restore database
docker-compose exec -T db psql -U postgres -d tae_app < backup.sql
```

## 📁 File Structure

```
tae_app/
├── Dockerfile              # App container definition
├── docker-compose.yml      # Multi-container setup
├── .dockerignore           # Files to exclude from build
├── init-db.sql            # Database initialization
├── start-docker.sh        # Automated startup script
└── wwwroot/uploads/       # Persistent file uploads
```

## 🔍 Troubleshooting

### Container Won't Start
```bash
# Check container status
docker-compose ps

# View detailed logs
docker-compose logs web
docker-compose logs db

# Check Docker daemon
docker info
```

### Database Connection Issues
```bash
# Verify PostgreSQL is ready
docker-compose exec db pg_isready -U postgres

# Check database exists
docker-compose exec db psql -U postgres -l
```

### Port Conflicts
If ports 8080 or 5432 are in use, modify `docker-compose.yml`:

```yaml
services:
  web:
    ports:
      - "8081:8080"  # Change external port
  db:
    ports:
      - "5433:5432"  # Change external port
```

### Reset Everything
```bash
# Stop and remove all containers, networks, and volumes
docker-compose down -v --remove-orphans

# Remove Docker images (optional)
docker image prune -a

# Start fresh
docker-compose up --build
```

## 🔄 Data Persistence

- **Database:** Data persists in Docker volume `postgres_data`
- **File Uploads:** Mapped to local `./wwwroot/uploads` directory

## 📈 Health Checks

Both containers have health checks:

- **Web App:** HTTP check on http://localhost:8080/
- **PostgreSQL:** `pg_isready` check

View health status:
```bash
docker-compose ps
```

## 🚀 Production Considerations

For production deployment, update:

1. **Environment Variables:**
   ```yaml
   environment:
     - ASPNETCORE_ENVIRONMENT=Production
     - ConnectionStrings__DefaultConnection=Host=db;Database=tae_app;Username=postgres;Password=SECURE_PASSWORD
   ```

2. **Security:**
   - Change default passwords
   - Use environment files (.env)
   - Enable HTTPS
   - Configure proper firewall rules

3. **Scaling:**
   - Use external database (AWS RDS, etc.)
   - Configure load balancing
   - Set up proper logging and monitoring

## 🎯 Testing the Setup

1. **Start containers:** `./start-docker.sh`
2. **Wait for startup:** Check logs show "Application started"
3. **Test registration:** http://localhost:8080/Registration
4. **Test admin:** http://localhost:8080/Admin/Dashboard
5. **Test NIDA flow:** Register with NIDA service option

## 💡 Tips

- Use `docker-compose logs -f web` to monitor app logs in real-time
- Database data persists between container restarts
- File uploads are saved to local `wwwroot/uploads/` directory
- The app automatically applies database migrations on startup

---

**Status:** Docker setup complete ✅  
**Next:** Run `./start-docker.sh` and visit http://localhost:8080
