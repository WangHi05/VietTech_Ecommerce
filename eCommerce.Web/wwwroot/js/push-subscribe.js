// push-subscribe.js - PHIÊN BẢN BẬT/TẮT (TOGGLE)
(function () {
    let isSubscribed = false; // Biến theo dõi trạng thái hiện tại
    let swRegistration = null; // Biến lưu Service Worker Registration

    // 1. Hàm chuyển đổi key
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

    // 2. Cập nhật giao diện nút bấm
    function updateButtonUI() {
        const btn = document.getElementById('btnEnablePush');
        if (!btn) return;

        if (isSubscribed) {
            // Trạng thái: Đang bật -> Hiển thị nút Tắt
            btn.innerHTML = '<i class="bi bi-bell-slash-fill"></i> Tắt thông báo';
            btn.classList.remove('btn-primary');
            btn.classList.add('btn-danger'); // Màu đỏ để báo hiệu tắt
            btn.disabled = false; // Quan trọng: Phải cho phép bấm để tắt
        } else {
            // Trạng thái: Đang tắt -> Hiển thị nút Bật
            btn.textContent = 'Bật thông báo';
            btn.classList.remove('btn-danger');
            btn.classList.remove('btn-success');
            btn.classList.add('btn-primary');
            btn.disabled = false;
        }
    }

    // 3. Hàm Đăng ký (Bật)
    async function subscribeUser() {
        try {
            const vapidPublicKey = window.VAPID_PUBLIC_KEY || '';
            if (!vapidPublicKey) return;

            const subscription = await swRegistration.pushManager.subscribe({
                userVisibleOnly: true,
                applicationServerKey: urlBase64ToUint8Array(vapidPublicKey)
            });

            // Gửi lên server
            await fetch('/api/push/subscribe', {
                method: 'POST',
                credentials: 'include',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(subscription)
            });

            isSubscribed = true;
            updateButtonUI();
            console.log('Đã bật thông báo');
        } catch (err) {
            console.error('Lỗi bật:', err);
            alert('Không thể bật thông báo: ' + err.message);
        }
    }

    // 4. Hàm Hủy đăng ký (Tắt)
    async function unsubscribeUser() {
        try {
            const subscription = await swRegistration.pushManager.getSubscription();
            if (subscription) {
                // 1. Gửi yêu cầu xóa khỏi DB Server trước
                await fetch('/api/push/unsubscribe', {
                    method: 'POST',
                    credentials: 'include',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(subscription)
                });

                // 2. Hủy đăng ký ở trình duyệt
                await subscription.unsubscribe();
            }

            isSubscribed = false;
            updateButtonUI();
            console.log('Đã tắt thông báo');
        } catch (err) {
            console.error('Lỗi tắt:', err);
        }
    }

    // 5. Khởi tạo
    async function initializeState() {
        if ('serviceWorker' in navigator && 'PushManager' in window) {
            try {
                // Lưu registration vào biến toàn cục để dùng lại
                swRegistration = await navigator.serviceWorker.register('/sw.js');
                await navigator.serviceWorker.ready;

                const subscription = await swRegistration.pushManager.getSubscription();
                isSubscribed = !!subscription; // Chuyển thành boolean
                
                updateButtonUI();
            } catch (error) {
                console.error('SW Error:', error);
            }
        }
    }

    // 6. Gắn sự kiện Click
    document.addEventListener('DOMContentLoaded', function () {
        initializeState();

        const btn = document.getElementById('btnEnablePush');
        if (btn) {
            btn.addEventListener('click', async function (e) {
                e.preventDefault();
                btn.disabled = true; // Khóa nút tạm thời khi đang xử lý

                if (isSubscribed) {
                    // Nếu đang bật -> Gọi hàm Tắt
                    await unsubscribeUser();
                } else {
                    // Nếu đang tắt -> Gọi hàm Bật
                    const perm = await Notification.requestPermission();
                    if (perm === 'granted') {
                        await subscribeUser();
                    } else {
                        alert('Bạn đã chặn thông báo. Hãy vào cài đặt trình duyệt để mở lại.');
                        updateButtonUI(); // Reset lại nút
                    }
                }
            });
        }
    });
})();