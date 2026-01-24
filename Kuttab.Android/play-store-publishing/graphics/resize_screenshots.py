#!/usr/bin/env python3
"""
Screenshot Resizer for Google Play Store
Generates 7-inch and 10-inch tablet versions from phone screenshots
Also creates 512x512 app icon
"""

from PIL import Image
import os
import sys

# Target dimensions for tablets
TABLET_7_INCH = (1200, 1920)  # 7-inch tablet
TABLET_10_INCH = (1600, 2560)  # 10-inch tablet
APP_ICON_SIZE = (512, 512)  # Play Store icon

def resize_with_padding(image, target_size, bg_color=(255, 255, 255)):
    """
    Resize image to fit within target size while maintaining aspect ratio.
    Add padding if needed to match exact dimensions.
    """
    # Calculate aspect ratios
    img_ratio = image.width / image.height
    target_ratio = target_size[0] / target_size[1]
    
    # Determine new size maintaining aspect ratio
    if img_ratio > target_ratio:
        # Image is wider - fit to width
        new_width = target_size[0]
        new_height = int(new_width / img_ratio)
    else:
        # Image is taller - fit to height
        new_height = target_size[1]
        new_width = int(new_height * img_ratio)
    
    # Resize image
    resized = image.resize((new_width, new_height), Image.LANCZOS)
    
    # Create new image with target size and background
    new_image = Image.new('RGB', target_size, bg_color)
    
    # Calculate position to center the resized image
    x = (target_size[0] - new_width) // 2
    y = (target_size[1] - new_height) // 2
    
    # Paste resized image onto background
    new_image.paste(resized, (x, y))
    
    return new_image

def process_screenshot(input_path, output_dir):
    """Process a single screenshot and create tablet versions"""
    try:
        # Open image
        img = Image.open(input_path)
        print(f"Processing: {os.path.basename(input_path)}")
        print(f"  Original size: {img.width}x{img.height}")
        
        # Get base filename without extension
        base_name = os.path.splitext(os.path.basename(input_path))[0]
        
        # Create 7-inch version
        img_7inch = resize_with_padding(img, TABLET_7_INCH)
        output_7inch = os.path.join(output_dir, f"{base_name}_7inch.png")
        img_7inch.save(output_7inch, 'PNG', quality=95)
        print(f"  ✓ Created 7-inch: {output_7inch}")
        
        # Create 10-inch version
        img_10inch = resize_with_padding(img, TABLET_10_INCH)
        output_10inch = os.path.join(output_dir, f"{base_name}_10inch.png")
        img_10inch.save(output_10inch, 'PNG', quality=95)
        print(f"  ✓ Created 10-inch: {output_10inch}")
        
        return True
    except Exception as e:
        print(f"  ✗ Error processing {input_path}: {e}")
        return False

def create_app_icon(input_path, output_dir):
    """Create 512x512 app icon"""
    try:
        img = Image.open(input_path)
        print(f"\nProcessing app icon: {os.path.basename(input_path)}")
        print(f"  Original size: {img.width}x{img.height}")
        
        # Resize to 512x512
        icon = img.resize(APP_ICON_SIZE, Image.LANCZOS)
        
        output_path = os.path.join(output_dir, "app-icon-512x512.png")
        icon.save(output_path, 'PNG', quality=95)
        print(f"  ✓ Created 512x512 icon: {output_path}")
        
        return True
    except Exception as e:
        print(f"  ✗ Error creating icon: {e}")
        return False

def main():
    # Get script directory
    script_dir = os.path.dirname(os.path.abspath(__file__))
    
    # Input/output directories
    input_dir = script_dir
    output_dir = script_dir
    
    print("=" * 60)
    print("Google Play Store Screenshot Resizer")
    print("=" * 60)
    
    # Find all PNG screenshots (excluding icons)
    screenshots = [
        f for f in os.listdir(input_dir) 
        if f.endswith('.png') and 'Screenshot' in f
    ]
    
    if not screenshots:
        print("\n✗ No screenshots found!")
        print("  Looking for files with 'Screenshot' in the name")
        return 1
    
    print(f"\nFound {len(screenshots)} screenshot(s)")
    print("-" * 60)
    
    # Process each screenshot
    success_count = 0
    for screenshot in screenshots:
        input_path = os.path.join(input_dir, screenshot)
        if process_screenshot(input_path, output_dir):
            success_count += 1
        print()
    
    print("-" * 60)
    print(f"Processed {success_count}/{len(screenshots)} screenshots")
    
    # Create app icon
    print("\n" + "=" * 60)
    print("Creating App Icon")
    print("=" * 60)
    
    # Look for app icon
    icon_files = [
        'app-icon-original.png',
        'kuttab_playstore_icon.png',
        'kuttab_playstore_icon_transparent.png'
    ]
    
    icon_created = False
    for icon_file in icon_files:
        icon_path = os.path.join(input_dir, icon_file)
        if os.path.exists(icon_path):
            if create_app_icon(icon_path, output_dir):
                icon_created = True
                break
    
    if not icon_created:
        print("\n✗ No app icon found!")
        print("  Looking for: app-icon-original.png or kuttab_playstore_icon.png")
    
    print("\n" + "=" * 60)
    print("✓ Processing Complete!")
    print("=" * 60)
    print("\nGenerated files:")
    print("  - *_7inch.png  (1200x1920 - 7-inch tablets)")
    print("  - *_10inch.png (1600x2560 - 10-inch tablets)")
    if icon_created:
        print("  - app-icon-512x512.png (512x512 - Play Store icon)")
    print("\nReady to upload to Google Play Console!")
    
    return 0

if __name__ == "__main__":
    sys.exit(main())
