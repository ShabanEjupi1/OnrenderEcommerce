// ProjectTemplate JS

// Mobile Menu Toggle
const mobileMenuBtn = document.getElementById('mobileMenuBtn');
const mobileMenu = document.querySelector('.nav-links');
const overlay = document.getElementById('mobileOverlay');
const closeMenu = document.getElementById('closeMenuBtn');

function toggleMobileMenu() {
    mobileMenu.classList.toggle('active');
    overlay.classList.toggle('active');
    document.body.classList.toggle('no-scroll');
}

if (mobileMenuBtn) {
    mobileMenuBtn.addEventListener('click', toggleMobileMenu);
}

if (overlay) {
    overlay.addEventListener('click', toggleMobileMenu);
}

if (closeMenu) {
    closeMenu.addEventListener('click', toggleMobileMenu);
}

// Add 'active' class to the link that matches the current route
document.addEventListener('DOMContentLoaded', function () {
    const navLinks = document.querySelectorAll('.nav-link');
    const path = window.location.pathname;

    navLinks.forEach(link => {
        // Check if the link's href matches the current path
        // This works for absolute paths (e.g., "/shop/category")
        if (link.getAttribute('href') === path) {
            link.classList.add('active');
        }
        // Check for relative paths (e.g., "/shop") when on a subpage (e.g., "/shop/category")
        else if (path.startsWith(link.getAttribute('href'))) {
            link.classList.add('active');
        }
    });
});

// Cart Update Logic (Mini Cart)
async function updateCart() {
    try {
        const response = await fetch('/shop/mini-cart');
        if (!response.ok) {
            // Note: Endpoint /shop/mini-cart needs to be implemented in ShopController
            console.warn('Mini-cart endpoint not found or failed.');
            return;
        }
        const html = await response.text();

        // Update the mini-cart content
        const miniCartContainer = document.querySelector('#cart-mini .cart-container');
        if (miniCartContainer) {
            miniCartContainer.innerHTML = html;
        }

        // Update badge count based on a data attribute or element we can parse reliably
        const cartBadge = document.querySelector('#cartBadge');
        if (cartBadge) {
            // We can look for a specific element in the returned HTML that holds the count
            // Or extract it via a regex that looks for standard item counts.
            const tempDiv = document.createElement('div');
            tempDiv.innerHTML = html;
            // Assuming the partial view includes something like <span class="cart-total-items" data-count="X">
            const countElement = tempDiv.querySelector('.cart-total-items') || tempDiv.querySelector('[data-cart-count]');
            
            let totalItems = 0;
            if (countElement) {
                totalItems = parseInt(countElement.getAttribute('data-count') || countElement.getAttribute('data-cart-count') || countElement.innerText) || 0;
            } else {
                // Fallback: search for number of .cart-item elements
                const items = tempDiv.querySelectorAll('.cart-item');
                totalItems = items.length;
            }

            cartBadge.textContent = totalItems;
            cartBadge.style.display = totalItems > 0 ? 'flex' : 'none';
        }
    } catch (e) {
        console.error("Error loading cart:", e);
    }
}

// Call updateCart when the page loads
document.addEventListener('DOMContentLoaded', updateCart);
