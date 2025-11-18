// Checkout page JS: toggles payment UI and initializes Google Maps (if present)
(function(){
    // Toggle payment UI
    function setupPaymentToggle(){
        const paymentSelect = document.getElementById('PaymentMethod');
        const sections = Array.from(document.querySelectorAll('[data-payment-section]'));
        if (!paymentSelect) return;

        const form = paymentSelect.form;
        const originalAction = form ? (form.getAttribute('action') || window.location.pathname) : window.location.pathname;

        function updatePaymentUI() {
            const value = (paymentSelect.value || '').toLowerCase();
            sections.forEach(section => {
                const sectionType = (section.dataset.paymentSection || '').toLowerCase();
                const isVisible = sectionType === value;
                section.style.display = isVisible ? 'block' : 'none';
                section.toggleAttribute('hidden', !isVisible);
            });
            // Nếu không phải VNPay thì reset lại action
            if (form && value !== 'vnpay') {
                form.setAttribute('action', originalAction);
            }
        }

        paymentSelect.addEventListener('change', updatePaymentUI);
        updatePaymentUI(); // chạy 1 lần khi load

        const vnpayButton = document.getElementById('vnpayButton');
        if (vnpayButton && form) {
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

    // Update shipping fee and total on the client when user changes shipping method
    function setupShippingMethodRealtimeUpdate() {
        const radios = Array.from(document.querySelectorAll('input[name="ShippingMethod"]'));
        if (!radios.length) return;

        // Helper to parse numbers like "1,234,567" or "- 1,234" into decimal
        function parseMoney(text) {
            if (!text) return 0;
            // remove non-digit, non-minus characters
            const cleaned = text.replace(/[^0-9\-]/g, '');
            if (!cleaned) return 0;
            return parseInt(cleaned, 10) || 0;
        }

        function formatMoney(n) {
            return n.toLocaleString('vi-VN');
        }

        const subTotalEl = Array.from(document.querySelectorAll('.summary-row')).find(r => r.textContent && r.textContent.includes('Tạm tính'))?.querySelector('strong');
        const discountEl = Array.from(document.querySelectorAll('.summary-row')).find(r => r.textContent && r.textContent.includes('Giảm'))?.querySelector('strong');
        const shippingEl = Array.from(document.querySelectorAll('.summary-row')).find(r => r.textContent && r.textContent.includes('Phí vận chuyển'))?.querySelector('strong');
        const totalEl = document.querySelector('.summary-row.total strong');

        function recalc(e) {
            const sub = parseMoney(subTotalEl ? subTotalEl.textContent : '0');
            // Discount is rendered like "- 10,000 ₫" so allow minus
            const disc = parseMoney(discountEl ? discountEl.textContent : '0');

            const checked = radios.find(r => r.checked);
            let ship = 0;
            if (checked) {
                const v = (checked.value || '').toLowerCase();
                if (v === 'express') ship = 50000;
                else if (v === 'pickup') ship = 0;
                else ship = 0; // standard fallback: 0 here because server may have a different shipping calc
            }

            if (shippingEl) shippingEl.textContent = formatMoney(ship) + ' ₫';

            const total = sub - disc + ship;
            if (totalEl) totalEl.textContent = formatMoney(total) + ' ₫';
        }

        radios.forEach(r => r.addEventListener('change', recalc));
        // run initially in case default selected isn't standard
        recalc();
    }

    // DOM Ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function () {
            setupPaymentToggle();
            initLeafletMap();
            setupShippingMethodRealtimeUpdate();
        });
    } else {
        setupPaymentToggle();
        initLeafletMap();
        setupShippingMethodRealtimeUpdate();
    }
})();
