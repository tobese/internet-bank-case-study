# Internet Bank Case Study

# New Bank

The repo is built and deployed to AWS where you can find the details of the setup.
https://doggerbank.online/

# Old Bank

Two-project setup: a **Java REST API** for factorial and Fibonacci, and a **Node.js CLI calculator** that consumes it.

## Java Server (`java-server/`)

Exposes two endpoints:

- `GET /api/fac/{n}` – factorial of n
- `GET /api/fib/{n}` – nth Fibonacci number

### Run

```bash
cd java-server
./mvnw spring-boot:run        # or: mvn spring-boot:run
```

The server starts on **http://localhost:8080**.

## Node.js Calculator (`node-calculator/`)

An interactive CLI calculator supporting `+`, `-`, `*`, `/`, `fac(n)`, and `fib(n)`.  
`fac` and `fib` call the Java API over HTTP.

### Run

```bash
cd node-calculator
npm start
```

### Example session

```
calc> 3 + 4
  = 7
calc> fac(5)
  = 120
calc> fib(10)
  = 55
calc> exit
Bye!
```

> **Note:** The Java server must be running before using `fac()` or `fib()` in the calculator.
