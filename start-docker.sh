#!/bin/bash

# TAE Community Platform - Docker Startup Script

echo "🚀 Starting TAE Community Platform with Docker..."

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker is not running. Please start Docker Desktop first."
    exit 1
fi

# Build and start the containers
echo "📦 Building Docker containers..."
docker-compose down --remove-orphans
docker-compose build --no-cache

echo "🔄 Starting containers..."
docker-compose up -d

# Wait for services to be healthy
echo "⏳ Waiting for services to start..."
sleep 10

# Check container status
echo "📊 Container Status:"
docker-compose ps

# Show logs
echo "📝 Application Logs (press Ctrl+C to exit):"
echo "🌐 Application will be available at: http://localhost:8080"
echo "🗄️  PostgreSQL will be available at: localhost:5432"
echo ""
echo "📋 Default Admin Account:"
echo "   Email: admin@tae.ae"
echo "   Password: TaeAdmin123!"
echo ""
echo "🔧 Useful Commands:"
echo "   Stop containers: docker-compose down"
echo "   View logs: docker-compose logs -f"
echo "   Restart: docker-compose restart"
echo ""

# Follow logs
docker-compose logs -f web
