# Combined Dockerfile for TeleHook API + Web
FROM node:18-alpine AS web-deps
WORKDIR /app
COPY apps/web/package.json apps/web/pnpm-lock.yaml* ./
RUN npm install -g pnpm@latest && \
    pnpm install --frozen-lockfile --prod=false && \
    npm cache clean --force

FROM node:18-alpine AS web-builder
WORKDIR /app
COPY --from=web-deps /app/node_modules ./node_modules
COPY apps/web/ ./
RUN npm install -g pnpm@latest && \
    NODE_ENV=production pnpm build && \
    rm -rf node_modules/.cache

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS api-builder
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY apps/api/TeleHook.Api/TeleHook.Api.csproj apps/api/TeleHook.Api/
RUN dotnet restore apps/api/TeleHook.Api/TeleHook.Api.csproj
COPY apps/api/ ./apps/api/
WORKDIR /src/apps/api/TeleHook.Api
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/api \
    --self-contained false \
    --no-restore && \
    # Remove CodeAnalysis assemblies (runtime compilation bloat)
    find /app/api -name "Microsoft.CodeAnalysis*.dll" -delete

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
ARG APP_UID=1001

# Install Node.js runtime and supervisord (minimal packages)
RUN apk add --no-cache nodejs supervisor curl && \
    apk add --no-cache --virtual .build-deps && \
    apk del .build-deps

# Create non-root user
RUN addgroup -g $APP_UID appgroup && \
    adduser -D -u $APP_UID -G appgroup appuser

# Create necessary directories with proper permissions
RUN mkdir -p /app/api /app/web /data /app/logs /etc/supervisor/conf.d /var/log/supervisor && \
    chown -R appuser:appgroup /app /data && \
    chmod -R 755 /data

# Copy .NET API
COPY --from=api-builder --chown=appuser:appgroup /app/api /app/api

# Copy Next.js app
COPY --from=web-builder --chown=appuser:appgroup /app/.next/standalone /app/web/
COPY --from=web-builder --chown=appuser:appgroup /app/.next/static /app/web/.next/static
COPY --from=web-builder --chown=appuser:appgroup /app/public /app/web/public

# Create supervisord configuration
COPY <<EOF /etc/supervisor/conf.d/telehook.conf
[supervisord]
nodaemon=true
user=root
logfile=/dev/stdout
logfile_maxbytes=0
pidfile=/var/run/supervisord.pid

[program:api]
command=dotnet /app/api/TeleHook.Api.dll
directory=/app/api
user=appuser
autostart=true
autorestart=true
stdout_logfile=/dev/stdout
stdout_logfile_maxbytes=0
stderr_logfile=/dev/stderr
stderr_logfile_maxbytes=0
environment=ASPNETCORE_URLS="http://+:8080"

[program:web]
command=node server.js
directory=/app/web
user=appuser
autostart=true
autorestart=true
stdout_logfile=/dev/stdout
stdout_logfile_maxbytes=0
stderr_logfile=/dev/stderr
stderr_logfile_maxbytes=0
environment=PORT="3000",HOSTNAME="0.0.0.0"
EOF

# Expose ports
EXPOSE 8080 3000

# Health check for both services
HEALTHCHECK --interval=30s --timeout=10s --start-period=15s --retries=3 \
    CMD curl -f http://localhost:8080/health && curl -f http://localhost:3000/ || exit 1

# Run supervisord
CMD ["/usr/bin/supervisord", "-c", "/etc/supervisor/conf.d/telehook.conf"]