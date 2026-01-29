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

# Copy SSL certificate for MySQL
COPY ca-certificate.pem /app/

# Copy backend
COPY --from=backend-build /app/backend ./backend

# Copy frontend build to Nginx
COPY --from=frontend-build /src/frontend/dist /var/www/html

# Configure Nginx to proxy backend and serve frontend
RUN rm -f /etc/nginx/sites-enabled/default
COPY nginx.conf /etc/nginx/sites-enabled/default

# Create supervisor config
RUN mkdir -p /etc/supervisor/conf.d && mkdir -p /var/log/supervisor
RUN echo '[supervisord]\n\
nodaemon=true\n\
user=root\n\
logfile=/var/log/supervisor/supervisord.log\n\
loglevel=info\n\
\n\
[program:nginx]\n\
command=/usr/sbin/nginx -g "daemon off;"\n\
autostart=true\n\
autorestart=true\n\
startsecs=5\n\
stopasgroup=true\n\
stdout_logfile=/dev/stdout\n\
stdout_logfile_maxbytes=0\n\
stderr_logfile=/dev/stderr\n\
stderr_logfile_maxbytes=0\n\
\n\
[program:dotnet]\n\
command=/bin/bash -c "dotnet PartnershipManager.API.dll"\n\
directory=/app/backend\n\
environment=ASPNETCORE_URLS="http://0.0.0.0:5000"\n\
autostart=true\n\
autorestart=true\n\
startsecs=15\n\
stopasgroup=true\n\
stdout_logfile=/dev/stdout\n\
stdout_logfile_maxbytes=0\n\
stderr_logfile=/dev/stderr\n\
stderr_logfile_maxbytes=0' > /etc/supervisor/conf.d/services.conf

EXPOSE 80

CMD ["/usr/bin/supervisord", "-c", "/etc/supervisor/conf.d/../supervisord.conf"]
