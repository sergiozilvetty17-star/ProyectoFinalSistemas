const CACHE_NAME = "examsecure-static-v1";

const STATIC_ASSETS = [
    "/css/examsecure.css",
    "/css/site.css",
    "/js/site.js",
    "/js/examsecure.js",
    "/manifest.json",
    "/icons/icon.svg",
    "/favicon.ico"
];

self.addEventListener("install", event => {
    event.waitUntil(
        caches.open(CACHE_NAME)
            .then(cache => cache.addAll(STATIC_ASSETS))
            .then(() => self.skipWaiting())
    );
});

self.addEventListener("activate", event => {
    event.waitUntil(
        caches.keys()
            .then(keys =>
                Promise.all(
                    keys
                        .filter(key => key !== CACHE_NAME)
                        .map(key => caches.delete(key))
                )
            )
            .then(() => self.clients.claim())
    );
});

self.addEventListener("fetch", event => {
    if (event.request.method !== "GET") {
        return;
    }

    const url = new URL(event.request.url);

    if (url.origin !== self.location.origin) {
        return;
    }

    if (event.request.destination === "document") {
        return;
    }

    event.respondWith(
        caches.match(event.request)
            .then(cached => cached || fetch(event.request))
    );
});
