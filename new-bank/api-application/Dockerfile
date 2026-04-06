# ── Build stage (Chainguard Wolfi-based — 0 CVEs) ────────────────────────────
FROM cgr.dev/chainguard/maven:latest-dev AS build
WORKDIR /app
RUN wget -q -O /app/opentelemetry-javaagent.jar \
    https://github.com/open-telemetry/opentelemetry-java-instrumentation/releases/latest/download/opentelemetry-javaagent.jar
COPY --chown=nonroot:nonroot pom.xml .
RUN mvn dependency:go-offline -q
COPY --chown=nonroot:nonroot src src
RUN mvn package -DskipTests -q

# ── Runtime stage (distroless — no shell, no package manager) ─────────────────
FROM gcr.io/distroless/java21-debian12:nonroot
WORKDIR /app
COPY --from=build /app/target/*.jar app.jar
COPY --from=build /app/opentelemetry-javaagent.jar opentelemetry-javaagent.jar
EXPOSE 8282
ENTRYPOINT ["/usr/bin/java", "-jar", "app.jar"]
