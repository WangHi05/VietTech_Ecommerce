// push-subscribe.js
(function () {
    function urlBase64ToUint8Array(base64String) {
        const padding = '='.repeat((4 - base64String.length % 4) % 4);
        const base64 = (base64String + padding).replace(/-/g, '+').replace(/_/g, '/');
        const rawData = window.atob(base64);
        const outputArray = new Uint8Array(rawData.length);
        for (let i = 0; i < rawData.length; ++i) {
            outputArray[i] = rawData.charCodeAt(i);
        }
        return outputArray;
    }

    async function subscribeUser() {
        if (!('serviceWorker' in navigator)) return;
        if (!('PushManager' in window)) return;

        try {
            const reg = await navigator.serviceWorker.register('/sw.js');
            console.log('Service worker registered', reg);

            const existing = await reg.pushManager.getSubscription();
            if (existing) {
                console.log('Already subscribed to push');
                return;
            }

            const vapidPublicKey = window.VAPID_PUBLIC_KEY || '';
            if (!vapidPublicKey) {
                console.warn('No VAPID public key available -- cannot subscribe');
                return;
            }

            const subscription = await reg.pushManager.subscribe({
                userVisibleOnly: true,
                applicationServerKey: urlBase64ToUint8Array(vapidPublicKey)
            });

            // Send subscription to server
            const resp = await fetch('/api/push/subscribe', {
                method: 'POST',
                credentials: 'same-origin',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(subscription)
            });

            if (!resp.ok) {
                const text = await resp.text();
                console.warn('Server rejected subscription:', resp.status, text);
            }

            console.log('Push subscription sent to server');
        } catch (err) {
            console.error('Failed to subscribe to push', err);
        }
    }

    document.addEventListener('DOMContentLoaded', function () {
        const btn = document.getElementById('btnEnablePush');
        if (btn) {
            btn.addEventListener('click', async function (e) {
                e.preventDefault();
                // ask permission first
                const perm = await Notification.requestPermission();
                if (perm === 'granted') {
                    await subscribeUser();
                    btn.textContent = 'Đã bật thông báo';
                    btn.disabled = true;
                } else {
                    alert('Bạn cần cho phép thông báo để nhận thông báo đơn hàng.');
                }
            });
        }
    });
})();
