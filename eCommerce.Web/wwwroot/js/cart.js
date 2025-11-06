document.addEventListener('DOMContentLoaded', function () {
    // No toast UI: we use fly-to-cart animation and a subtle header pulse as feedback.
    function pulseCartCount() {
        const el = document.querySelector('.cart-item-count[data-test="header-cart-count"]') || document.querySelector('.cart-item-count');
        if (!el) return;
        el.classList.add('pulse');
        setTimeout(() => el.classList.remove('pulse'), 350);
    }

    async function handleInlineFormSubmit(e) {
        e.preventDefault();
        const form = e.currentTarget;
        try {
            const url = form.getAttribute('action') || '/Cart?handler=Add';
            const formData = new FormData(form);
            const resp = await fetch(url, {
                method: 'POST',
                headers: { 'X-Requested-With': 'XMLHttpRequest' },
                body: formData,
                credentials: 'same-origin'
            });
            if (!resp.ok) {
                // error — do not show toast, just log
                try { const err = await resp.json(); console.error('Add failed:', err); } catch (e) { console.error('Add failed', resp.status); }
                return;
            }
            // success path
            const json = await resp.json().catch(() => null);
            if (json && json.count !== undefined) {
                const el = document.querySelector('.cart-item-count[data-test="header-cart-count"]') || document.querySelector('.cart-item-count');
                if (el) el.textContent = json.count;
            }
            // perform fly-to-cart animation using the product image if available
            const animated = animateImageToCart(form);
            if (!animated) {
                // if we couldn't animate (no image), give subtle feedback by pulsing the header count
                pulseCartCount();
            }
        } catch (err) {
            console.error('Add to cart failed', err);
            // do not show toast; leave error in console
        }
    }

    function animateImageToCart(form) {
        try {
            // --- SỬA LỖI TẠI ĐÂY ---
            let img = null;

            // 1. Thử tìm ảnh theo cấu trúc trang Products (.product-card)
            const productCard = form.closest('.product-card');
            if (productCard) {
                img = productCard.querySelector('img');
            }

            // 2. Nếu không thấy (đang ở trang Detail), thử tìm theo cấu trúc trang ProductDetail
            if (!img) {
                const detailLayout = form.closest('.product-detail-layout');
                if (detailLayout) {
                    // Tìm ảnh trong class .product-detail-image
                    img = detailLayout.querySelector('.product-detail-image img'); 
                }
            }
            // --- KẾT THÚC SỬA LỖI ---

            const cartButton = document.querySelector('.cart-button');

            // Nếu cả 2 cách đều không tìm thấy ảnh, hoặc không thấy nút giỏ hàng -> thoát
            if (!img || !cartButton) {
                console.log('Không tìm thấy ảnh hoặc nút giỏ hàng để tạo hiệu ứng.');
                return false; // Thoát và chỉ pulse giỏ hàng
            }

            const imgRect = img.getBoundingClientRect();
            const cartRect = cartButton.getBoundingClientRect();

            const clone = img.cloneNode(true);
            clone.style.position = 'fixed';
            clone.style.left = imgRect.left + 'px';
            clone.style.top = imgRect.top + 'px';
            clone.style.width = imgRect.width + 'px';
            clone.style.height = imgRect.height + 'px';
            clone.style.transition = 'transform 1000ms cubic-bezier(.2,.9,.2,1), opacity 1000ms ease';
            clone.style.zIndex = 9999;
            clone.style.pointerEvents = 'none';
            document.body.appendChild(clone);

            // force reflow
            clone.getBoundingClientRect();

            const targetX = cartRect.left + (cartRect.width / 2) - (imgRect.width / 4);
            const targetY = cartRect.top + (cartRect.height / 2) - (imgRect.height / 4);
            const scale = 0.4;

            const translateX = targetX - imgRect.left;
            const translateY = targetY - imgRect.top;

            clone.style.transform = `translate(${translateX}px, ${translateY}px) scale(${scale}) rotate(10deg)`;
            clone.style.opacity = '0.9';

            // remove clone after animation
            setTimeout(() => {
                if (clone && clone.parentNode) clone.parentNode.removeChild(clone);
            }, 750);

            return true;
        } catch (e) {
            console.error('animateImageToCart failed', e);
            return false;
        }
    }

    // attach to existing inline forms
    document.querySelectorAll('.inline-form').forEach(function (form) {
        form.addEventListener('submit', handleInlineFormSubmit);
    });

    // support future dynamically inserted forms via event delegation (simple approach)
    document.body.addEventListener('submit', function (e) {
        const target = e.target;
        if (target && target.matches && target.matches('.inline-form')) {
            handleInlineFormSubmit(e);
        }
    });

    // Helper to update summary totals in the cart from server JSON
    function updateTotalsFromJson(json) {
        try {
            if (!json) return;
            const setIf = (selector, value) => {
                const el = document.querySelector(selector);
                if (el) el.querySelector('.value') ? el.querySelector('.value').textContent = value : el.textContent = value;
            };
            setIf('[data-test="subtotal"]', json.subtotal);
            setIf('[data-test="discount"]', (json.discount && json.discount.startsWith('-')) ? json.discount : ('- ' + json.discount));
            setIf('[data-test="shipping"]', json.shipping);
            setIf('[data-test="total"]', json.total);
            // header count
            if (json.count !== undefined) {
                const el = document.querySelector('.cart-item-count[data-test="header-cart-count"]') || document.querySelector('.cart-item-count');
                if (el) el.textContent = json.count;
            }
        } catch (e) { console.error('updateTotalsFromJson failed', e); }
    }

    // Intercept update quantity forms
    document.querySelectorAll('.update-form').forEach(function (form) {
        form.addEventListener('submit', async function (e) {
            e.preventDefault();
            try {
                const url = form.getAttribute('action') || window.location.pathname + '?handler=Update';
                const fd = new FormData(form);
                const resp = await fetch(url, { method: 'POST', headers: { 'X-Requested-With': 'XMLHttpRequest' }, body: fd, credentials: 'same-origin' });
                if (!resp.ok) return;
                const json = await resp.json().catch(() => null);
                if (json) {
                    // update the line total for product
                    if (json.productId && json.lineTotal) {
                        const line = document.querySelector('.line-total[data-product-id="' + json.productId + '"]');
                        if (line) {
                            // animate highlight
                            line.style.transition = 'background-color 220ms';
                            const prev = line.textContent;
                            line.textContent = json.lineTotal;
                            line.style.backgroundColor = '#fff7e6';
                            setTimeout(() => line.style.backgroundColor = '', 240);
                        }
                    }
                    updateTotalsFromJson(json);
                }
            } catch (err) { console.error('Update form failed', err); }
        });
    });

    // Intercept voucher form
    document.querySelectorAll('.voucher-form').forEach(function (form) {
        form.addEventListener('submit', async function (e) {
            e.preventDefault();
            try {
                const url = form.getAttribute('action') || window.location.pathname + '?handler=ApplyVoucher';
                const fd = new FormData(form);
                const resp = await fetch(url, { method: 'POST', headers: { 'X-Requested-With': 'XMLHttpRequest' }, body: fd, credentials: 'same-origin' });
                if (!resp.ok) return;
                const json = await resp.json().catch(() => null);
                if (json) updateTotalsFromJson(json);
            } catch (err) { console.error('Apply voucher failed', err); }
        });
    });

    // Intercept shipping form
    document.querySelectorAll('.shipping-form').forEach(function (form) {
        form.addEventListener('submit', async function (e) {
            e.preventDefault();
            try {
                const url = form.getAttribute('action') || window.location.pathname + '?handler=CalculateShipping';
                const fd = new FormData(form);
                const resp = await fetch(url, { method: 'POST', headers: { 'X-Requested-With': 'XMLHttpRequest' }, body: fd, credentials: 'same-origin' });
                if (!resp.ok) return;
                const json = await resp.json().catch(() => null);
                if (json) updateTotalsFromJson(json);
            } catch (err) { console.error('Calculate shipping failed', err); }
        });
    });
});
