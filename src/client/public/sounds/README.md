# Sound Effects Structure

This directory contains sound effects for the PoBabyTouch game.

## Color-Based Sound Effects

The game uses four distinct circle colors, each with 4 random sound effects:

- **Blue** - `/sounds/blue/sound1-4.mp3`
- **Green** - `/sounds/green/sound1-4.mp3`
- **Red** - `/sounds/red/sound1-4.mp3`
- **Purple** - `/sounds/purple/sound1-4.mp3`

## Downloading Custom Sound Effects

To download custom sound effects for each color, run the download script from the client directory:

```bash
cd src/client
./download-sounds.sh
```

This script uses `curl` to download sound effects from Freesound.org.

## Sound File Requirements

- Format: MP3
- Naming: `sound1.mp3`, `sound2.mp3`, `sound3.mp3`, `sound4.mp3`
- Location: `public/sounds/{color}/`

Each color folder must contain exactly 4 sound files.

## Custom Sounds

To use your own custom sounds:

1. Prepare 4 different MP3 files for each color (16 total files)
2. Name them `sound1.mp3` through `sound4.mp3`
3. Place them in the appropriate color folder:
   - `public/sounds/blue/`
   - `public/sounds/green/`
   - `public/sounds/red/`
   - `public/sounds/purple/`

## How It Works

When a player clicks/taps a colored circle, the game:
1. Identifies the circle's color
2. Randomly selects one of the 4 sound effects for that color
3. Plays the selected sound effect

This provides variety and makes the game more engaging.
