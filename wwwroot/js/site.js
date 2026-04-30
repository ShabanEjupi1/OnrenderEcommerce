/**
 * Enisi Center — site.js
 * Global interactions: mobile nav, cart badge, search suggestions, product loading.
 */

// =============================================================================
// Mobile hamburger menu — toggles cat-nav on small screens
// =============================================================================
document.addEventListener('DOMContentLoaded', () => {
    const hamburger = document.getElementById('mobileMenuBtn');
    const catNav    = document.getElementById('catNav');

    if (hamburger && catNav) {
        hamburger.addEventListener('click', () => {
            const isOpen = catNav.classList.toggle('mobile-open');
            hamburger.setAttribute('aria-expanded', isOpen);
        });

        // Close nav when a link inside it is clicked (SPA-like behaviour)
        catNav.querySelectorAll('.cat-nav-item').forEach(link => {
            link.addEventListener('click', () => {
                catNav.classList.remove('mobile-open');
                hamburger.setAttribute('aria-expanded', 'false');
            });
        });
    }

    // Highlight active cat-nav link
    const path = window.location.pathname;
    document.querySelectorAll('.cat-nav-item').forEach(link => {
        const href = link.getAttribute('href');
        if (href && href !== '/' && path.startsWith(href)) {
            link.classList.add('active');
        } else if (href === path) {
            link.classList.add('active');
        }
    });

    // Auto-hide subscribe toast after 5 seconds
    document.querySelectorAll('.subscribe-toast').forEach(el => {
        setTimeout(() => {
            el.style.transition = 'opacity 0.6s';
            el.style.opacity = '0';
            setTimeout(() => el.remove(), 700);
        }, 5000);
    });

    // Load cart count on page load
    updateCart();
});

// =============================================================================
// Cart helpers
// =============================================================================
async function addToCart(productId, btnElement) {
    try {
        const formData = new FormData();
        formData.append('productId', productId);
        formData.append('quantity', 1);

        const response = await fetch('/Cart/Add', { method: 'POST', body: formData });
        const data = await response.json();

        if (data.success) {
            updateCartBadge(data.cartCount);
            if (btnElement) {
                const orig = btnElement.innerText;
                btnElement.innerText = '✓ Shtuar!';
                btnElement.style.background = 'var(--accent-success, #10b981)';
                btnElement.style.color = 'white';
                setTimeout(() => {
                    btnElement.innerText = orig;
                    btnElement.style.background = '';
                    btnElement.style.color = '';
                }, 2000);
            }
        } else {
            alert(data.message || 'Gabim gjatë shtimit në shportë.');
        }
    } catch (e) {
        console.error('[Cart] Add error:', e);
    }
}

function updateCartBadge(count) {
    const badge = document.getElementById('cartBadge');
    if (!badge) return;
    badge.textContent = count;
    badge.style.display = count > 0 ? 'flex' : 'none';
}

async function updateCart() {
    try {
        const r = await fetch('/Cart/Count');
        if (!r.ok) return;
        const data = await r.json();
        updateCartBadge(data.count || 0);
    } catch (e) {
        // Silent fail — cart badge stays at whatever was server-rendered
    }
}
