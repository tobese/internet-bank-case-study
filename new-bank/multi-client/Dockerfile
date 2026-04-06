# ── Runtime stage (pre-built WASM artifacts) ───────────────────────────────
# The WASM client is built locally via build-client.sh / dotnet publish.
# Run build-client.sh before docker-compose up --build to refresh the output.
FROM nginx:stable-alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY InternetBankCalculator/InternetBankCalculator/bin/Release/net10.0-browserwasm/publish/wwwroot /usr/share/nginx/html
# Override the Uno-generated service worker with a passthrough version.
# The generated SW caches aggressively and causes stale-asset issues in container deployments.
COPY service-worker-passthrough.js /usr/share/nginx/html/service-worker.js
COPY sw-reset.html /usr/share/nginx/html/sw-reset.html
RUN find /usr/share/nginx/html -type f -exec chmod 644 {} \; \
 && find /usr/share/nginx/html -type d -exec chmod 755 {} \;
EXPOSE 80
