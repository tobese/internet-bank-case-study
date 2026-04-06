package com.example.mathserver.service;

import com.fasterxml.jackson.core.type.TypeReference;
import com.fasterxml.jackson.databind.ObjectMapper;
import org.springframework.data.redis.core.StringRedisTemplate;
import org.springframework.stereotype.Service;

import java.time.Duration;
import java.util.List;
import java.util.Optional;

@Service
public class PrimeCacheService {

    private static final String KEY_PREFIX = "primes:";
    private static final Duration TTL = Duration.ofHours(24);

    private final StringRedisTemplate redis;
    private final ObjectMapper objectMapper = new ObjectMapper();

    public PrimeCacheService(StringRedisTemplate redis) {
        this.redis = redis;
    }

    public Optional<List<Integer>> load(int limit) {
        String json = redis.opsForValue().get(KEY_PREFIX + limit);
        if (json == null) {
            return Optional.empty();
        }
        try {
            return Optional.of(objectMapper.readValue(json, new TypeReference<>() {}));
        } catch (Exception e) {
            return Optional.empty();
        }
    }

    @SuppressWarnings("null")
    public void save(int limit, List<Integer> primes) {
        try {
            String json = objectMapper.writeValueAsString(primes);
            redis.opsForValue().set(KEY_PREFIX + limit, json, TTL);
        } catch (Exception ignored) {
        }
    }
}
