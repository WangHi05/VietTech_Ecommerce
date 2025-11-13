/* Basic service worker to handle push events and show notifications */
self.addEventListener('push', function (event) {
    let payload = {};
    try {
        payload = event.data ? event.data.json() : {};
    } catch (e) {
        payload = { title: 'Thông báo', body: event.data ? event.data.text() : 'Bạn có thông báo mới' };
    }

    const title = payload.title || 'Thông báo từ VietTech';
    const options = {
        body: payload.body || '',
        data: payload.data || {},
        icon: '/images/notification-icon.png',
        badge: '/images/notification-badge.png'
    };

    event.waitUntil(self.registration.showNotification(title, options));
});

self.addEventListener('notificationclick', function (event) {
    event.notification.close();
    const urlToOpen = (event.notification.data && event.notification.data.url) || '/';

    event.waitUntil(clients.matchAll({ type: 'window', includeUncontrolled: true }).then(function (clientList) {
        for (let i = 0; i < clientList.length; i++) {
            const client = clientList[i];
            if (client.url === urlToOpen && 'focus' in client) {
                return client.focus();
            }
        }
        if (clients.openWindow) {
            return clients.openWindow(urlToOpen);
        }
    }));
});
