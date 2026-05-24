/* ===== Navbar Scroll Effect ===== */
document.addEventListener('DOMContentLoaded', () => {
    const navbar = document.querySelector('.navbar');

    window.addEventListener('scroll', () => {
        if (window.scrollY > 50) {
            navbar.classList.add('scrolled');
        } else {
            navbar.classList.remove('scrolled');
        }
    });
});

/* ===== Counter Animation ===== */
function animateCounters() {
    const counters = document.querySelectorAll('.stat-number');

    counters.forEach(counter => {
        const target = parseInt(counter.getAttribute('data-target'));
        const increment = Math.ceil(target / 40);
        let current = 0;

        const update = () => {
            current += increment;
            if (current >= target) {
                counter.textContent = target + (target === 100 ? '%' : '+');
                return;
            }
            counter.textContent = current + '+';
            requestAnimationFrame(update);
        };

        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    update();
                    observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.5 });

        observer.observe(counter);
    });
}

/* ===== Smooth Reveal on Scroll ===== */
function initReveal() {
    const cards = document.querySelectorAll('.feature-card, .tech-card, .screenshot-card, .download-card');

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.opacity = '1';
                entry.target.style.transform = 'translateY(0)';
            }
        });
    }, { threshold: 0.1 });

    cards.forEach(card => {
        card.style.opacity = '0';
        card.style.transform = 'translateY(30px)';
        card.style.transition = 'opacity 0.6s ease, transform 0.6s ease';
        observer.observe(card);
    });
}

/* ===== Parallax Hero Glow ===== */
function initHeroParallax() {
    const hero = document.querySelector('.hero-section');
    if (!hero) return;

    hero.addEventListener('mousemove', (e) => {
        const x = (e.clientX / window.innerWidth - 0.5) * 20;
        const y = (e.clientY / window.innerHeight - 0.5) * 20;
        hero.style.setProperty('--mouse-x', `${x}px`);
        hero.style.setProperty('--mouse-y', `${y}px`);
    });
}

/* ===== Typing Effect for Hero Subtitle ===== */
function initTyping() {
    const el = document.querySelector('.hero-sub');
    if (!el) return;

    const text = el.textContent;
    el.textContent = '';
    el.style.display = 'inline-block';
    el.style.borderRight = '2px solid var(--accent)';
    el.style.overflow = 'hidden';
    el.style.whiteSpace = 'nowrap';
    el.style.animation = 'blink 0.8s step-end infinite';

    let i = 0;
    const type = () => {
        if (i < text.length) {
            el.textContent += text.charAt(i);
            i++;
            setTimeout(type, 40);
        }
    };
    setTimeout(type, 500);
}

/* ===== Init ===== */
document.addEventListener('DOMContentLoaded', () => {
    animateCounters();
    initReveal();
    initHeroParallax();
    initTyping();
});
