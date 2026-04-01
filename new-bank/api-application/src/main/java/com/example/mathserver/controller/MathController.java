package com.example.mathserver.controller;

import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.math.BigInteger;
import java.util.Map;

@RestController
@RequestMapping("/api")
@CrossOrigin
public class MathController {

    @GetMapping("/fac/{n}")
    public ResponseEntity<?> factorial(@PathVariable int n) {
        if (n < 0) {
            return ResponseEntity.badRequest().body(Map.of("error", "n must be non-negative"));
        }
        return ResponseEntity.ok(Map.of("input", n, "result", fac(n).toString()));
    }

    @GetMapping("/fib/{n}")
    public ResponseEntity<?> fibonacci(@PathVariable int n) {
        if (n < 0) {
            return ResponseEntity.badRequest().body(Map.of("error", "n must be non-negative"));
        }
        return ResponseEntity.ok(Map.of("input", n, "result", fib(n).toString()));
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
