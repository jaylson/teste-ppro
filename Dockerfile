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

# Install Nginx and supervisor
RUN apt-get update && apt-get install -y nginx supervisor && rm -rf /var/lib/apt/lists/*

# Copy backend
COPY --from=backend-build /app/backend ./backend

# Copy frontend build to Nginx
COPY --from=frontend-build /src/frontend/dist /var/www/html

# Configure Nginx to proxy backend and serve frontend
RUN rm -f /etc/nginx/sites-enabled/default
COPY nginx.conf /etc/nginx/sites-enabled/default

# Create supervisor config
RUN mkdir -p /etc/supervisor/conf.d
RUN echo '[supervisord]\n\
nodaemon=true\n\
logfile=/var/log/supervisor/supervisord.log\n\
\n\
[program:nginx]\n\
command=/usr/sbin/nginx -g "daemon off;"\n\
autostart=true\n\
autorestart=true\n\
startsecs=5\n\
stopasgroup=true\n\
stdout_logfile=/var/log/supervisor/nginx.log\n\
stderr_logfile=/var/log/supervisor/nginx_err.log\n\
\n\
[program:dotnet]\n\
command=bash -c "cd /app/backend && dotnet PartnershipManager.API.dll"\n\
directory=/app/backend\n\
autostart=true\n\
autorestart=true\n\
startsecs=10\n\
stdout_logfile=/var/log/supervisor/dotnet.log\n\
stderr_logfile=/var/log/supervisor/dotnet_err.log\n\
environment=ASPNETCORE_URLS=http://+:5000,ASPNETCORE_ENVIRONMENT=Production' > /etc/supervisor/conf.d/services.conf

EXPOSE 80

CMD ["/usr/bin/supervisord", "-c", "/etc/supervisor/conf.d/../supervisord.conf"]
