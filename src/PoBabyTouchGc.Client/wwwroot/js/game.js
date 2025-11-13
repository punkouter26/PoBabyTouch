/**
 * PoBabyTouchGc Game JavaScript Functions
 * This file contains all the JS functionality required for the PoBabyTouchGc game
 */

// Audio management
let audioContext = null;
let soundBuffers = {};
let isMuted = false;
let volume = 0.7;

// Initialize Web Audio API
function initAudio() {
    if (!audioContext) {
        audioContext = new (window.AudioContext || window.webkitAudioContext)();
    }
}

// Function to get element dimensions (width, height)
function getElementDimensions(element) {
    return [element.clientWidth, element.clientHeight];
}

// Enhanced play sound with different sound types
function playSound(soundType) {
    if (isMuted) return;
    
    try {
        initAudio();
        
        // Map sound types to audio files
        const soundMap = {
            'tap': 'sounds/tap.mp3',
            'highscore': 'sounds/highscore.mp3',
            'gameover': 'sounds/gameover.mp3',
            'tick': 'sounds/tick.mp3',
            'matt': 'sounds/matt/Recording.m4a',
            'kim': 'sounds/kim/Recording.m4a',
            'nick': 'sounds/nick/Recording.m4a'
        };
        
        const soundFile = soundMap[soundType.toLowerCase()] || soundMap['tap'];
        
        const audio = new Audio(soundFile);
        audio.volume = volume;
        audio.play().catch(err => {
            console.log(`Audio play failed for ${soundType}:`, err);
        });
    } catch (e) {
        console.log('Audio error:', e);
    }
}

// Set volume (0.0 to 1.0)
function setVolume(newVolume) {
    volume = Math.max(0, Math.min(1, newVolume));
    localStorage.setItem('gameVolume', volume.toString());
}

// Get current volume
function getVolume() {
    const stored = localStorage.getItem('gameVolume');
    if (stored) {
        volume = parseFloat(stored);
    }
    return volume;
}

// Toggle mute
function toggleMute() {
    isMuted = !isMuted;
    localStorage.setItem('gameMuted', isMuted.toString());
    return isMuted;
}

// Get mute status
function getMuteStatus() {
    const stored = localStorage.getItem('gameMuted');
    if (stored) {
        isMuted = stored === 'true';
    }
    return isMuted;
}

// Vibration API for haptic feedback
function vibrate(duration = 50) {
    if ('vibrate' in navigator) {
        navigator.vibrate(duration);
    }
}

// Vibrate with pattern (array of durations)
function vibratePattern(pattern) {
    if ('vibrate' in navigator) {
        navigator.vibrate(pattern);
    }
}

// Function to preload all audio files
function preloadAudio() {
    const sounds = ['tap', 'highscore', 'gameover', 'tick'];
    const people = ['matt', 'kim', 'nick'];
    
    // Preload game sounds
    sounds.forEach(sound => {
        try {
            const audio = new Audio(`sounds/${sound}.mp3`);
            audio.volume = 0.01; // Very quiet for preload
            audio.play().then(() => audio.pause()).catch(() => {});
        } catch (err) {
            // Ignore preload errors
        }
    });
    
    // Preload person sounds
    people.forEach(person => {
        try {
            const audio = new Audio(`sounds/${person}/Recording.m4a`);
            audio.volume = 0.01;
            audio.play().then(() => audio.pause()).catch(() => {});
        } catch (err) {
            // Ignore preload errors
        }
    });
}

// Initialize on page load
document.addEventListener('DOMContentLoaded', () => {
    try {
        getVolume();
        getMuteStatus();
        preloadAudio();
    } catch (err) {
        console.log('Audio initialization skipped:', err.message);
    }
});

// Expose functions to Blazor
window.gameAudio = {
    playSound,
    setVolume,
    getVolume,
    toggleMute,
    getMuteStatus,
    vibrate,
    vibratePattern
};
