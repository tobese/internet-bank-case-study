package com.example.mathserver.controller;

import com.example.mathserver.model.Mathematician;
import com.example.mathserver.service.MathematicianService;
import com.example.mathserver.service.PrimeCacheService;
import io.micrometer.core.instrument.MeterRegistry;
import io.micrometer.core.instrument.Timer;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.HttpStatus;
import org.springframework.http.MediaType;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.server.ResponseStatusException;
import org.springframework.web.servlet.mvc.method.annotation.SseEmitter;

import java.math.BigInteger;
import java.util.Map;
import java.util.concurrent.atomic.AtomicLong;

@RestController
@RequestMapping("/api")
@CrossOrigin
public class MathController {

    @Autowired
    private MathematicianService mathematicianService;

    @Autowired
    private PrimeCacheService primeCacheService;

    @Autowired
    private MeterRegistry meterRegistry;

    @GetMapping("/fac/{n}")
    public ResponseEntity<?> factorial(@PathVariable int n) {
        if (n < 0) {
            return ResponseEntity.badRequest().body(Map.of("error", "n must be non-negative"));
        }
        BigInteger result = Timer.builder("math.calculation")
                .tag("operation", "factorial")
                .tag("n", String.valueOf(n))
                .register(meterRegistry)
                .record(() -> fac(n));
        meterRegistry.counter("math.requests", "operation", "factorial").increment();
        return ResponseEntity.ok(Map.of("input", n, "result", result.toString()));
    }

    @GetMapping("/fib/{n}")
    public ResponseEntity<?> fibonacci(@PathVariable int n) {
        if (n < 0) {
            return ResponseEntity.badRequest().body(Map.of("error", "n must be non-negative"));
        }
        BigInteger result = Timer.builder("math.calculation")
                .tag("operation", "fibonacci")
                .tag("n", String.valueOf(n))
                .register(meterRegistry)
                .record(() -> fib(n));
        meterRegistry.counter("math.requests", "operation", "fibonacci").increment();
        return ResponseEntity.ok(Map.of("input", n, "result", result.toString()));
    }
    
    @GetMapping("/mathematician/random")
    public ResponseEntity<?> randomMathematician() {
        return mathematicianService.getRandom()
                .map(ResponseEntity::ok)
                .orElse(ResponseEntity.notFound().build());
    }

    @GetMapping("/mathematician/daily")
    public ResponseEntity<?> dailyMathematician() {
        return mathematicianService.getDaily()
                .map(ResponseEntity::ok)
                .orElse(ResponseEntity.notFound().build());
    }
    
    @GetMapping("/mathematician/{id}")
    public ResponseEntity<?> getMathematician(@PathVariable Long id) {
        return mathematicianService.getById(id)
                .map(ResponseEntity::ok)
                .orElse(ResponseEntity.notFound().build());
    }
    
    @GetMapping("/mathematicians")
    public ResponseEntity<?> getAllMathematicians() {
        return ResponseEntity.ok(mathematicianService.getAll());
    }

    @GetMapping(value = "/primes/sieve/{limit}", produces = MediaType.TEXT_EVENT_STREAM_VALUE)
    public SseEmitter streamPrimesSieve(@PathVariable int limit) {
        if (limit < 2 || limit > 10_000) {
            throw new ResponseStatusException(HttpStatus.BAD_REQUEST, "limit must be between 2 and 10000");
        }
        meterRegistry.counter("math.requests", "operation", "primes").increment();
        SseEmitter emitter = new SseEmitter(0L);
        Thread.ofVirtual().start(() -> {
            long start = System.nanoTime();
            AtomicLong primeCount = new AtomicLong(0);
            try {
                var cached = primeCacheService.load(limit);
                if (cached.isPresent()) {
                    meterRegistry.counter("math.primes.cache", "result", "hit").increment();
                    for (int prime : cached.get()) {
                        emitter.send(SseEmitter.event().data(prime));
                        primeCount.incrementAndGet();
                        Thread.sleep(50);
                    }
                } else {
                    meterRegistry.counter("math.primes.cache", "result", "miss").increment();
                    boolean[] composite = new boolean[limit + 1];
                    var found = new java.util.ArrayList<Integer>();
                    for (int i = 2; i <= limit; i++) {
                        if (!composite[i]) {
                            found.add(i);
                            emitter.send(SseEmitter.event().data(i));
                            primeCount.incrementAndGet();
                            Thread.sleep(50);
                            for (int j = i * 2; j <= limit; j += i) {
                                composite[j] = true;
                            }
                        }
                    }
                    primeCacheService.save(limit, found);
                }
                meterRegistry.timer("math.primes.duration").record(
                        System.nanoTime() - start, java.util.concurrent.TimeUnit.NANOSECONDS);
                meterRegistry.summary("math.primes.count").record(primeCount.get());
                emitter.complete();
            } catch (Exception e) {
                emitter.completeWithError(e);
            }
        });
        return emitter;
    }

    private BigInteger fac(int n) {
        return facHelper(n);
    }

    private BigInteger facHelper(int n) {
        if (n <= 1) return BigInteger.ONE;
        return BigInteger.valueOf(n).multiply(facHelper(n - 1));
    }

    private BigInteger fib(int n) {
        return fibHelper(n);
    }

    private BigInteger fibHelper(int n) {
        if (n <= 0) return BigInteger.ZERO;
        if (n == 1) return BigInteger.ONE;
        return fibHelper(n - 1).add(fibHelper(n - 2));
    }
}
