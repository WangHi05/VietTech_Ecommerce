// push-subscribe.js - PHIÊN BẢN CHUẨN (ĐÃ FIX)
(function () {
    // Hàm hỗ trợ chuyển đổi Key
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

            // 1. Kiểm tra xem trình duyệt đã đăng ký chưa
            const existing = await reg.pushManager.getSubscription();
            let subscription = existing; 

            if (existing) {
                console.log('Already subscribed locally, updating server...');
            } else {
                // 2. Nếu chưa có thì đăng ký mới
                const vapidPublicKey = window.VAPID_PUBLIC_KEY || '';
                if (!vapidPublicKey) {
                    console.warn('No VAPID public key available -- cannot subscribe');
                    return;
                }
                
                subscription = await reg.pushManager.subscribe({
                    userVisibleOnly: true,
                    applicationServerKey: urlBase64ToUint8Array(vapidPublicKey)
                });
            }

            // 3. Luôn luôn gửi xuống Server để đồng bộ DB
            // Dù là đăng ký mới hay cũ, server đều cần biết để lưu vào DB
            const resp = await fetch('/api/push/subscribe', {
                method: 'POST',
                credentials: 'include', // Gửi kèm cookie đăng nhập
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(subscription)
            });

            if (!resp.ok) {
                const text = await resp.text();
                console.warn('Server rejected subscription:', resp.status, text);
            } else {
                console.log('Push subscription sent to server SUCCESS');
            }

        } catch (err) {
            console.error('Failed to subscribe to push', err);
        }
    }

    // Xử lý sự kiện click nút
    document.addEventListener('DOMContentLoaded', function () {
        const btn = document.getElementById('btnEnablePush');
        if (btn) {
            btn.addEventListener('click', async function (e) {
                e.preventDefault();
                // Xin quyền hiển thị thông báo
                const perm = await Notification.requestPermission();
                if (perm === 'granted') {
                    await subscribeUser();
                    // Thay đổi trạng thái nút sau khi thành công
                    btn.textContent = 'Đã bật thông báo';
                    btn.disabled = true;
                    btn.classList.remove('btn-primary');
                    btn.classList.add('btn-success');
                } else {
                    alert('Bạn cần cho phép thông báo để nhận thông báo đơn hàng.');
                }
            });
        }
    });
})();