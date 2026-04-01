const http = require("http");
const fs = require("fs");
const path = require("path");

const PORT = process.env.PORT || 3000;
const API_URL = process.env.API_URL || "http://localhost:8282";
const PUBLIC_DIR = path.join(__dirname, "public");

const MIME_TYPES = {
  ".html": "text/html",
  ".css": "text/css",
  ".js": "application/javascript",
  ".json": "application/json",
  ".png": "image/png",
  ".svg": "image/svg+xml",
};

const server = http.createServer((req, res) => {
  // Proxy /api/ requests to the API service
  if (req.url.startsWith("/api/")) {
    const apiBase = new URL(API_URL);
    const options = {
      hostname: apiBase.hostname,
      port: apiBase.port || 80,
      path: req.url,
      method: req.method,
      headers: { ...req.headers, host: apiBase.host },
    };
    const proxyReq = http.request(options, (proxyRes) => {
      res.writeHead(proxyRes.statusCode, proxyRes.headers);
      proxyRes.pipe(res);
    });
    proxyReq.on("error", (err) => {
      res.writeHead(502, { "Content-Type": "text/plain" });
      res.end("Bad Gateway: " + err.message);
    });
    req.pipe(proxyReq);
    return;
  }

  const filePath = path.join(PUBLIC_DIR, req.url === "/" ? "index.html" : req.url);

  const ext = path.extname(filePath);
  const contentType = MIME_TYPES[ext] || "application/octet-stream";

  fs.readFile(filePath, (err, data) => {
    if (err) {
      res.writeHead(404, { "Content-Type": "text/plain" });
      res.end("404 Not Found");
      return;
    }
    res.writeHead(200, { "Content-Type": contentType });
    res.end(data);
  });
});

server.listen(PORT, () => {
  console.log(`Web server running at http://localhost:${PORT}`);
  console.log(`Proxying /api/ -> ${API_URL}`);
});
