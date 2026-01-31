/**
 * PageLeaf Preview Extensions
 * 
 * This script is injected into the preview HTML to provide:
 * 1. Mermaid initialization
 * 2. Manual fragment link handling (workaround for <base> tag issues)
 */

(function() {
    // 1. Initialize Mermaid
    if (typeof mermaid !== 'undefined') {
        mermaid.initialize({ startOnLoad: true, theme: 'default' });
        mermaid.contentLoaded();
    }

    // 2. Handle internal fragment links manually
    document.addEventListener('click', function(e) {
        const link = e.target.closest('a');
        if (link) {
            const href = link.getAttribute('href');
            if (href && href.startsWith('#')) {
                const id = decodeURIComponent(href.substring(1));
                const element = document.getElementById(id);
                if (element) {
                    element.scrollIntoView();
                    e.preventDefault();
                }
            }
        }
    });
})();
