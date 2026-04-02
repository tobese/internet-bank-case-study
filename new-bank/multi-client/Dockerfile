# ── Build stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
RUN apt-get update && apt-get install -y --no-install-recommends python3 && \
    ln -s /usr/bin/python3 /usr/bin/python && \
    rm -rf /var/lib/apt/lists/*
RUN dotnet workload install wasm-tools

WORKDIR /src

# Copy solution-level files for restore
COPY InternetBankCalculator/global.json             InternetBankCalculator/
COPY InternetBankCalculator/Directory.Build.props    InternetBankCalculator/
COPY InternetBankCalculator/Directory.Build.targets  InternetBankCalculator/
COPY InternetBankCalculator/Directory.Packages.props InternetBankCalculator/
COPY InternetBankCalculator/InternetBankCalculator.sln InternetBankCalculator/
COPY InternetBankCalculator/InternetBankCalculator/InternetBankCalculator.csproj \
     InternetBankCalculator/InternetBankCalculator/

RUN --mount=type=tmpfs,target=/tmp \
    dotnet restore InternetBankCalculator/InternetBankCalculator/InternetBankCalculator.csproj \
    -p:TargetFramework=net10.0-browserwasm

# Copy full source and publish the WASM target
COPY InternetBankCalculator/ InternetBankCalculator/
RUN --mount=type=tmpfs,target=/tmp \
    dotnet publish InternetBankCalculator/InternetBankCalculator/InternetBankCalculator.csproj \
    -c Release \
    -f net10.0-browserwasm \
    --no-restore \
    -o /app/publish

# ── Runtime stage ────────────────────────────────────────────────────────────
FROM nginx:alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
EXPOSE 80
