"""Create animated GIF from screenshots"""
import os
from PIL import Image, ImageDraw, ImageFont

OUTPUT_DIR = os.path.dirname(os.path.abspath(__file__))
Screenshot_DIR = OUTPUT_DIR

# Key frames: ZV import -> ZQ import -> Flood import -> Results
frames = [
    ("02_zv_imported.png", "1. Import ZV curve"),
    ("03_zq_imported.png", "2. Import ZQ curve"),
    ("04_flood_imported.png", "3. Import Flood data"),
    ("05_results.png", "4. Calculate & Results"),
]

images = []
for fname, label in frames:
    path = os.path.join(Screenshot_DIR, fname)
    if os.path.exists(path):
        img = Image.open(path)
        # Resize to 800px width for article-friendly size
        w, h = img.size
        new_w = 800
        new_h = int(h * new_w / w)
        img = img.resize((new_w, new_h), Image.LANCZOS)

        # Add label at bottom
        draw = ImageDraw.Draw(img)
        try:
            font = ImageFont.truetype("arial.ttf", 20)
        except:
            font = ImageFont.load_default()
        # Draw semi-transparent label bar
        label_h = 40
        draw.rectangle([(0, new_h - label_h), (new_w, new_h)], fill=(0, 0, 0, 180))
        bbox = draw.textbbox((0, 0), label, font=font)
        tw = bbox[2] - bbox[0]
        draw.text(((new_w - tw) // 2, new_h - label_h + 8), label, fill=(255, 255, 255), font=font)

        images.append(img)
        print(f"  Added frame: {fname} ({new_w}x{new_h})")
    else:
        print(f"  MISSING: {fname}")

if len(images) >= 2:
    gif_path = os.path.join(OUTPUT_DIR, "aquawater_demo.gif")
    images[0].save(
        gif_path,
        save_all=True,
        append_images=images[1:],
        duration=2000,  # 2 seconds per frame
        loop=0,
        optimize=True,
    )
    print(f"\n[OK] GIF saved: {gif_path}")
    print(f"  Frames: {len(images)}, Size: {os.path.getsize(gif_path) / 1024:.0f} KB")
else:
    print("\n[FAIL] Not enough frames to create GIF")
