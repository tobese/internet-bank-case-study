package com.example.mathserver.service;

import com.fasterxml.jackson.core.type.TypeReference;
import com.fasterxml.jackson.databind.ObjectMapper;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Service;

import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.List;
import java.util.Optional;

@Service
public class PrimeCacheService {

    private final Path cacheDir;
    private final ObjectMapper objectMapper = new ObjectMapper();

    public PrimeCacheService(@Value("${primes.cache.dir:/data/primes-cache}") String cacheDir) throws IOException {
        this.cacheDir = Paths.get(cacheDir);
        Files.createDirectories(this.cacheDir);
    }

    public Optional<List<Integer>> load(int limit) {
        Path file = cacheFile(limit);
        if (!Files.exists(file)) {
            return Optional.empty();
        }
        try {
            List<Integer> primes = objectMapper.readValue(file.toFile(), new TypeReference<>() {});
            return Optional.of(primes);
        } catch (IOException e) {
            return Optional.empty();
        }
    }

    public void save(int limit, List<Integer> primes) {
        try {
            objectMapper.writeValue(cacheFile(limit).toFile(), primes);
        } catch (IOException ignored) {
        }
    }

    private Path cacheFile(int limit) {
        return cacheDir.resolve("primes_" + limit + ".json");
    }
}
