// Service worker passthrough - no caching for container deployment.
// On each install, wipe all caches and claim existing clients immediately
// so the browser always fetches fresh assets from nginx.
//
// No fetch handler: without one, all requests pass through to the network
// natively. Adding event.respondWith(fetch(...)) causes "Failed to fetch"
// errors on navigation requests while the SW is activating.

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
