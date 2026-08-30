#!/bin/bash

set -e

APP_DIR="/docker/choir-music-system"
BACKUP_DIR="$APP_DIR/backups"
TIMESTAMP=$(date +"%Y%m%d-%H%M%S")

echo "======================================"
echo " Choir Music System - Production Deploy"
echo "======================================"

cd "$APP_DIR"

echo ""
echo "[1/6] Backing up database..."
mkdir -p "$BACKUP_DIR"

if [ -f "$APP_DIR/Data/choir.db" ]; then
    cp "$APP_DIR/Data/choir.db" \
       "$BACKUP_DIR/choir-$TIMESTAMP.db"
fi

echo ""
echo "[2/6] Pulling latest code..."
git pull

echo ""
echo "[3/6] Stopping current application..."
docker compose down

echo ""
echo "[4/6] Building Docker image..."
docker compose build

echo ""
echo "[5/6] Starting application..."
docker compose up -d

echo ""
echo "[6/6] Checking application..."
docker compose ps

echo ""
echo "Cleaning old Docker images..."
docker image prune -f

echo ""
echo "======================================"
echo " Deployment complete"
echo "======================================"