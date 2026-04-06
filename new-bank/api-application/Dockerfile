# ── Build stage ──────────────────────────────────────────────────────────────
FROM maven:3.9-eclipse-temurin-21-alpine AS build
WORKDIR /app
COPY pom.xml .
COPY .mvn .mvn
COPY mvnw .
RUN chmod +x mvnw && ./mvnw dependency:go-offline -q
COPY src src
RUN ./mvnw package -DskipTests -q

# ── OTel agent stage ─────────────────────────────────────────────────────────
FROM eclipse-temurin:21-jre AS otel
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/* \
    && curl -sSL -o /opentelemetry-javaagent.jar \
    https://github.com/open-telemetry/opentelemetry-java-instrumentation/releases/latest/download/opentelemetry-javaagent.jar

# ── Runtime stage ─────────────────────────────────────────────────────────────
FROM gcr.io/distroless/java21-debian12:nonroot
WORKDIR /app
COPY --from=build /app/target/*.jar app.jar
COPY --from=otel /opentelemetry-javaagent.jar opentelemetry-javaagent.jar
EXPOSE 8282
ENTRYPOINT ["/usr/bin/java", "-jar", "app.jar"]
