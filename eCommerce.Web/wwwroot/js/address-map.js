// Khởi tạo bản đồ Leaflet (OpenStreetMap)
(function initLeafletMap() {
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

                    // SỬA LỖI 2: Dùng querySelector để tìm theo 'name'
                    const countryInput = document.querySelector('[name="Country"]');
                    const provinceInput = document.querySelector('[name="Province"]');

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
})();