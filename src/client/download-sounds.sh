#!/bin/bash
# Script to download custom sound effects for the four circle colors
# Run this script locally with internet access to download unique sound effects

SOUNDS_DIR="public/sounds"

echo "Downloading custom sound effects for circle colors..."

# Blue sounds - higher pitched, water-like sounds
echo "Downloading blue sounds..."
curl -L -o "$SOUNDS_DIR/blue/sound1.mp3" "https://freesound.org/data/previews/320/320655_5260872-lq.mp3"
curl -L -o "$SOUNDS_DIR/blue/sound2.mp3" "https://freesound.org/data/previews/415/415564_5121236-lq.mp3"
curl -L -o "$SOUNDS_DIR/blue/sound3.mp3" "https://freesound.org/data/previews/456/456966_5674468-lq.mp3"
curl -L -o "$SOUNDS_DIR/blue/sound4.mp3" "https://freesound.org/data/previews/531/531622_11120538-lq.mp3"

# Green sounds - nature/organic sounds
echo "Downloading green sounds..."
curl -L -o "$SOUNDS_DIR/green/sound1.mp3" "https://freesound.org/data/previews/458/458272_7661587-lq.mp3"
curl -L -o "$SOUNDS_DIR/green/sound2.mp3" "https://freesound.org/data/previews/446/446111_7255534-lq.mp3"
curl -L -o "$SOUNDS_DIR/green/sound3.mp3" "https://freesound.org/data/previews/448/448274_7255534-lq.mp3"
curl -L -o "$SOUNDS_DIR/green/sound4.mp3" "https://freesound.org/data/previews/512/512137_10202813-lq.mp3"

# Red sounds - energetic/vibrant sounds  
echo "Downloading red sounds..."
curl -L -o "$SOUNDS_DIR/red/sound1.mp3" "https://freesound.org/data/previews/456/456965_5674468-lq.mp3"
curl -L -o "$SOUNDS_DIR/red/sound2.mp3" "https://freesound.org/data/previews/521/521623_10806732-lq.mp3"
curl -L -o "$SOUNDS_DIR/red/sound3.mp3" "https://freesound.org/data/previews/541/541445_11120538-lq.mp3"
curl -L -o "$SOUNDS_DIR/red/sound4.mp3" "https://freesound.org/data/previews/551/551695_11120538-lq.mp3"

# Purple sounds - magical/mystical sounds
echo "Downloading purple sounds..."
curl -L -o "$SOUNDS_DIR/purple/sound1.mp3" "https://freesound.org/data/previews/456/456967_5674468-lq.mp3"
curl -L -o "$SOUNDS_DIR/purple/sound2.mp3" "https://freesound.org/data/previews/433/433639_7255534-lq.mp3"
curl -L -o "$SOUNDS_DIR/purple/sound3.mp3" "https://freesound.org/data/previews/521/521624_10806732-lq.mp3"
curl -L -o "$SOUNDS_DIR/purple/sound4.mp3" "https://freesound.org/data/previews/531/531621_11120538-lq.mp3"

echo "Sound effects download complete!"
echo "Note: These are preview-quality sounds from Freesound.org"
echo "For production use, consider downloading full-quality sounds or using licensed sound effects"
