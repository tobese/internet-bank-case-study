// Service worker passthrough - no caching for container deployment.
// On each install, wipe all caches and claim existing clients immediately
// so the browser always fetches fresh assets from nginx.

self.addEventListener("install", () => {
  self.skipWaiting();
});

self.addEventListener("activate", (event) => {
  event.waitUntil(
    caches
      .keys()
      .then((keys) => Promise.all(keys.map((k) => caches.delete(k))))
      .then(() => self.clients.claim()),
  );
});

self.addEventListener("fetch", (event) => {
  event.respondWith(fetch(event.request));
});
