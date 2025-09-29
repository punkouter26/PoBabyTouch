/**
 * PoBabyTouchGc Game JavaScript Functions
 * This file contains all the JS functionality required for the PoBabyTouchGc game
 */

// Function to get element dimensions (width, height)
function getElementDimensions(element) {
    return [element.clientWidth, element.clientHeight];
}

// Function to play sound effect based on person
function playSound(person) {
    try {
        const audio = new Audio(`sounds/${person}/Recording.m4a`);
        audio.play().catch(err => {
            // Silent fail for audio issues
            console.log(`Audio play failed for ${person}`);
        });
    } catch (e) {
        // Silent fail
    }
}

// Function to preload all audio files (simplified)
function preloadAudio() {
    const people = ["matt", "kim", "nick"];
    people.forEach(person => {
        try {
            playSound(person);
        } catch (err) {
            // Ignore preload errors
        }
    });
}

// Call preload when the page loads, but make it optional
document.addEventListener('DOMContentLoaded', () => {
    try {
        preloadAudio();
    } catch (err) {
        console.log('Audio preload skipped:', err.message);
    }
});
