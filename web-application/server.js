const http = require("http");
const fs = require("fs");
const path = require("path");

const PORT = 3000;
const PUBLIC_DIR = path.join(__dirname, "public");

const MIME_TYPES = {
  ".html": "text/html",
  ".css": "text/css",
  ".js": "application/javascript",
  ".json": "application/json",
  ".png": "image/png",
  ".svg": "image/svg+xml",
};

const RESOURCES_DIR = path.join(__dirname, "resources");

const server = http.createServer((req, res) => {
  let filePath;
  if (req.url.startsWith("/resources/")) {
    filePath = path.join(RESOURCES_DIR, req.url.replace(/^\/resources/, ""));
  } else {
    filePath = path.join(PUBLIC_DIR, req.url === "/" ? "index.html" : req.url);
  }

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
});
