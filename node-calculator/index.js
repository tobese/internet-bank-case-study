const readline = require("readline");
const http = require("http");

const JAVA_API = "http://localhost:8282/api";

function callJavaApi(path) {
  return new Promise((resolve, reject) => {
    http
      .get(`${JAVA_API}${path}`, (res) => {
        let data = "";
        res.on("data", (chunk) => (data += chunk));
        res.on("end", () => {
          try {
            const json = JSON.parse(data);
            if (res.statusCode !== 200) {
              reject(new Error(json.error || "API error"));
            } else {
              resolve(json.result);
            }
          } catch {
            reject(new Error("Invalid response from Java API"));
          }
        });
      })
      .on("error", (err) => {
        reject(
          new Error(
            `Could not reach Java API at ${JAVA_API} – is the server running?\n  ${err.message}`
          )
        );
      });
  });
}

function basicCalc(expression) {
  // Supports: number op number  (op = + - * /)
  const match = expression.match(
    /^\s*(-?\d+(?:\.\d+)?)\s*([+\-*/])\s*(-?\d+(?:\.\d+)?)\s*$/
  );
  if (!match) return null;

  const a = parseFloat(match[1]);
  const op = match[2];
  const b = parseFloat(match[3]);

  switch (op) {
    case "+":
      return a + b;
    case "-":
      return a - b;
    case "*":
      return a * b;
    case "/":
      return b === 0 ? "Error: division by zero" : a / b;
  }
}

async function evaluate(input) {
  const trimmed = input.trim().toLowerCase();

  // fac(n) or fib(n)
  const fnMatch = trimmed.match(/^(fac|fib)\((\d+)\)$/);
  if (fnMatch) {
    const fn = fnMatch[1];
    const n = fnMatch[2];
    return callJavaApi(`/${fn}/${n}`);
  }

  // Basic arithmetic
  const result = basicCalc(trimmed);
  if (result !== null) return result;

  return 'Unknown expression. Supported: "3 + 4", "fac(5)", "fib(10)"';
}

// ── REPL ────────────────────────────────────────────────
const rl = readline.createInterface({
  input: process.stdin,
  output: process.stdout,
  prompt: "calc> ",
});

console.log("Calculator  –  supports +, -, *, /, fac(n), fib(n)");
console.log('Type "exit" to quit.\n');
rl.prompt();

rl.on("line", async (line) => {
  const input = line.trim();
  if (input === "exit" || input === "quit") {
    rl.close();
    return;
  }
  if (!input) {
    rl.prompt();
    return;
  }

  try {
    const result = await evaluate(input);
    console.log(`  = ${result}`);
  } catch (err) {
    console.error(`  Error: ${err.message}`);
  }
  rl.prompt();
});

rl.on("close", () => {
  console.log("Bye!");
  process.exit(0);
});
