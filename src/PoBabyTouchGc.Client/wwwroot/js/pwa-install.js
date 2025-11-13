// PWA Installation support
let deferredPrompt = null;

// Listen for the beforeinstallprompt event
window.addEventListener('beforeinstallprompt', (e) => {
    // Prevent the mini-infobar from appearing on mobile
    e.preventDefault();
    // Stash the event so it can be triggered later
    deferredPrompt = e;
    console.log('PWA install prompt available');
});

// Check if app can be installed
export function canInstall() {
    // Check if already installed
    if (window.matchMedia('(display-mode: standalone)').matches) {
        console.log('PWA already installed');
        return false;
    }
    
    // Check if prompt is available
    return deferredPrompt !== null;
}

// Show the install prompt
export async function promptInstall() {
    if (!deferredPrompt) {
        console.log('Install prompt not available');
        return false;
    }
    
    // Show the install prompt
    deferredPrompt.prompt();
    
    // Wait for the user to respond to the prompt
    const { outcome } = await deferredPrompt.userChoice;
    
    console.log(`User response to install prompt: ${outcome}`);
    
    // Clear the deferred prompt
    deferredPrompt = null;
    
    return outcome === 'accepted';
}

// Check if running as installed PWA
export function isInstalled() {
    return window.matchMedia('(display-mode: standalone)').matches ||
           window.navigator.standalone === true;
}

// iOS Add to Home Screen detection
export function isIOS() {
    return /iPhone|iPad|iPod/.test(navigator.userAgent);
}

// Show iOS install instructions
export function shouldShowIOSInstructions() {
    return isIOS() && !isInstalled();
}
