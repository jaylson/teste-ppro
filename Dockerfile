# Stage 1: Build Backend (.NET 9)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS backend-build
WORKDIR /src/backend

COPY src/backend/ ./
RUN dotnet restore
RUN dotnet publish -c Release -o /app/backend

# Stage 2: Build Frontend (React + Vite)
FROM node:20-alpine AS frontend-build
WORKDIR /src/frontend

COPY src/frontend/package*.json ./
RUN npm ci

COPY src/frontend/ ./
RUN npm run build

# Stage 3: Runtime - Backend + Nginx for Frontend
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install Nginx
RUN apt-get update && apt-get install -y nginx && rm -rf /var/lib/apt/lists/*

# Copy backend
COPY --from=backend-build /app/backend ./backend

# Copy frontend build to Nginx
COPY --from=frontend-build /src/frontend/dist /var/www/html

# Configure Nginx to proxy backend and serve frontend
RUN rm /etc/nginx/sites-enabled/default
COPY nginx.conf /etc/nginx/sites-enabled/default

# Create startup script
RUN echo '#!/bin/bash\n\
nginx -g "daemon off;" &\n\
cd /app/backend\n\
dotnet PartnershipManager.API.dll' > /app/start.sh && chmod +x /app/start.sh

EXPOSE 80

CMD ["/app/start.sh"]
