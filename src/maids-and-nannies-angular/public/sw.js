const CACHE = 'rafeeqa-v2';

self.addEventListener('install', () => self.skipWaiting());

self.addEventListener('activate', (e) => {
    e.waitUntil(
        caches.keys()
            .then((keys) => Promise.all(keys.filter((k) => k !== CACHE).map((k) => caches.delete(k))))
            .then(() => self.clients.claim())
    );
});

self.addEventListener('fetch', (e) => {
    const req = e.request;

    // التنقلات (index.html): network-first دائماً — لا يشترط تشغيل نسخة قديمة أبداً
    if (req.mode === 'navigate') {
        e.respondWith(
            fetch(req)
                .then((res) => {
                    const copy = res.clone();
                    caches.open(CACHE).then((c) => c.put(req, copy));
                    return res;
                })
                .catch(() => caches.match(req).then((r) => r || caches.match('/index.html')))
        );
        return;
    }

    // الملفات الثابتة فقط (حِزم Angular ذات الهاش): cache-first ثم شبكة
    if (req.method === 'GET' && req.url.startsWith(self.location.origin)) {
        e.respondWith(
            caches.match(req).then(
                (r) => r || fetch(req).then((res) => {
                    if (res.status === 200) {
                        const copy = res.clone();
                        caches.open(CACHE).then((c) => c.put(req, copy));
                    }
                    return res;
                })
            )
        );
    }
});