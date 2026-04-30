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

// Cart Actions
async function addToCart(productId, btnElement) {
    try {
        const formData = new FormData();
        formData.append('productId', productId);
        formData.append('quantity', 1);

        const response = await fetch('/Cart/Add', {
            method: 'POST',
            body: formData
        });
        
        const data = await response.json();
        if (data.success) {
            updateCartBadge(data.cartCount);
            
            // Show visual feedback
            if (btnElement) {
                const originalText = btnElement.innerText;
                btnElement.innerText = "✓ Shtuar";
                btnElement.classList.add('btn-success');
                setTimeout(() => {
                    btnElement.innerText = originalText;
                    btnElement.classList.remove('btn-success');
                }, 2000);
            }
        } else {
            alert(data.message || 'Gabim gjatë shtimit në shportë.');
        }
    } catch (e) {
        console.error("Error adding to cart:", e);
        alert('Ndodhi një gabim gjatë shtimit në shportë.');
    }
}

function updateCartBadge(count) {
    const cartBadge = document.getElementById('cartBadge');
    if (cartBadge) {
        cartBadge.textContent = count;
        cartBadge.style.display = count > 0 ? 'flex' : 'none';
    }
}

// Cart Update Logic
async function updateCart() {
    try {
        const response = await fetch('/Cart/Count');
        if (!response.ok) return;
        
        const data = await response.json();
        updateCartBadge(data.count || 0);
    } catch (e) {
        console.error("Error loading cart count:", e);
    }
}

// Call updateCart when the page loads
document.addEventListener('DOMContentLoaded', updateCart);
