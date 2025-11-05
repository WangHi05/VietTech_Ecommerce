// Checkout page JS: toggles payment UI and initializes Google Maps (if present)
(function(){
    // Toggle payment UI
    function setupPaymentToggle(){
        const paymentSelect = document.getElementById('PaymentMethod');
        const sections = Array.from(document.querySelectorAll('[data-payment-section]'));
        if (!paymentSelect) return;

        function updatePaymentUI() {
            const value = (paymentSelect.value || '').toLowerCase();
            sections.forEach(section => {
                const sectionType = (section.dataset.paymentSection || '').toLowerCase();
                const isVisible = sectionType === value;
                section.style.display = isVisible ? 'block' : 'none';
                section.toggleAttribute('hidden', !isVisible);
            });
        }

        paymentSelect.addEventListener('change', updatePaymentUI);
        updatePaymentUI(); // chạy 1 lần khi load

        const vnpayButton = document.getElementById('vnpayButton');
        const form = paymentSelect.form;
        if (vnpayButton && form) {
            const originalAction = form.getAttribute('action') || window.location.pathname;
            vnpayButton.addEventListener('click', function () {
                paymentSelect.value = 'vnpay';
                updatePaymentUI();
                form.setAttribute('action', originalAction.includes('?') ? originalAction.split('?')[0] + '?handler=VnPay' : originalAction + '?handler=VnPay');
                form.submit();
            });
        }
    }

    // Khởi tạo bản đồ Leaflet (OpenStreetMap)
    function initLeafletMap() {
        const mapEl = document.getElementById('map');
        if (!mapEl) return;

        const centerVN = [14.0583, 108.2772];
        const map = L.map('map').setView(centerVN, 6);

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19,
            attribution: '© OpenStreetMap'
        }).addTo(map);

        const marker = L.marker(centerVN, { draggable: true }).addTo(map);

        // Hàm cập nhật địa chỉ từ tọa độ
        function updateAddress(latlng) {
            marker.setLatLng(latlng);

            const url = `https://nominatim.openstreetmap.org/reverse?format=json&lat=${latlng.lat}&lon=${latlng.lng}`;

            fetch(url)
                .then(response => {
                    if (!response.ok) throw new Error('Network response was not ok');
                    return response.json();
                })
                .then(data => {
                    console.log("📍 Dữ liệu trả về từ Nominatim:", data); // để debug

                    if (data && data.display_name) {
                        const addr = data.display_name;
                        const sel = document.getElementById('selectedAddress');
                        if (sel) sel.value = addr;
                        const shipInput = document.querySelector('[name="ShippingAddress"]');
                        if (shipInput) shipInput.value = addr;

                        // 🔥 Tự động điền Quốc gia & Tỉnh/TP
                        const address = data.address || {};
                        const country = address.country || "";

                        // ✅ Ưu tiên city/state_district/state cho Việt Nam
                        const province =
                            address.city ||
                            address.state_district ||
                            address.state ||
                            address.province ||
                            address.region ||
                            address.county ||
                            "";

                        const countryInput = document.getElementById("Country");
                        const provinceInput = document.getElementById("Province");

                        if (countryInput) countryInput.value = country;
                        if (provinceInput) provinceInput.value = province;

                        console.log("✅ Lấy địa chỉ thành công:", { addr, country, province });
                    } else {
                        throw new Error('Không tìm thấy địa chỉ');
                    }
                })
                .catch(err => {
                    console.error('❌ Lỗi Nominatim Geocode: ', err);
                    const sel = document.getElementById('selectedAddress');
                    if (sel) sel.value = "Không thể lấy địa chỉ tự động.";
                });
        }

        // Sự kiện click trên bản đồ
        map.on('click', function (e) {
            updateAddress(e.latlng);
        });

        // Kéo thả marker
        marker.on('dragend', function(e) {
            updateAddress(e.target.getLatLng());
        });

        // Nút "Dùng vị trí của tôi"
        const btn = document.getElementById('btnUseMyLocation');
        if (btn) {
            btn.addEventListener('click', function () {
                if (navigator.geolocation) {
                    navigator.geolocation.getCurrentPosition(function (pos) {
                        const latlng = { lat: pos.coords.latitude, lng: pos.coords.longitude };
                        map.setView(latlng, 15);
                        updateAddress(latlng);
                    }, function (err) { alert('Không thể lấy vị trí: ' + err.message); });
                } else {
                    alert('Trình duyệt không hỗ trợ Geolocation');
                }
            });
        }
    }

    // DOM Ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function () {
            setupPaymentToggle();
            initLeafletMap();
        });
    } else {
        setupPaymentToggle();
        initLeafletMap();
    }
})();
