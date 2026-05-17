import os
import sys
import subprocess
import shutil
import platform
import re
import argparse
from pathlib import Path
from datetime import datetime

# Configuration
PROJECT_NAME = "Kuttab"
VERSION = "0.95"
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
ANDROID_PROJECT_DIR = os.path.join(SCRIPT_DIR, "Kuttab.Android")
CORE_PROJECT_DIR = os.path.join(SCRIPT_DIR, "Kuttab.Core")
INFO_ACTIVITY_PATH = os.path.join(ANDROID_PROJECT_DIR, "InfoActivity.cs")
DIST_DIR = os.path.join(SCRIPT_DIR, "dist")
APK_NAME = f"{PROJECT_NAME}-android-{VERSION}.apk"
TARGETS = [
    {"rid": "win-x64", "ext": "zip"},
    {"rid": "linux-x64", "ext": "tar.gz"},
    {"rid": "osx-x64", "ext": "tar.gz"},
    {"rid": "osx-arm64", "ext": "tar.gz"},
]

def run_command(cmd, cwd=None, env=None):
    """Run a shell command and return its output."""
    print(f"\n🚀 Running: {' '.join(cmd)}")
    try:
        result = subprocess.run(
            cmd,
            cwd=cwd or os.getcwd(),
            env=env or os.environ,
            check=True,
            text=True,
            capture_output=True
        )
        if result.stdout:
            print(result.stdout)
        if result.stderr:
            print(result.stderr)
        return True
    except subprocess.CalledProcessError as e:
        print(f"❌ Command failed with return code: {e.returncode}")
        if e.stdout:
            print(f"Standard output:\n{e.stdout}")
        if e.stderr:
            print(f"Error output:\n{e.stderr}")
        return False

def get_dotnet_command():
    """Find the best .NET SDK command (prefer .NET 9)."""
    dotnet9_path = str(Path.home() / ".dotnet-9" / "dotnet")
    if os.path.exists(dotnet9_path):
        return dotnet9_path
    return "dotnet"

def build_for_platform(target):
    """Build the project for a specific platform."""
    rid = target["rid"]
    print(f"\n{'='*80}")
    print(f"🔨 Building {PROJECT_NAME} for {rid}")
    print(f"{'='*80}")
    
    output_dir = f"./publish/{rid}"
    
    # Clean previous build
    if os.path.exists(output_dir):
        print(f"🧹 Cleaning previous build for {rid}...")
        shutil.rmtree(output_dir)
    
    dotnet_cmd = get_dotnet_command()
    print(f"ℹ️  Using dotnet: {dotnet_cmd}")
    
    # Build command
    cmd = [
        dotnet_cmd, "publish",
        "-c", "Release",
        "-r", rid,
        "--self-contained", "true",
        "-maxcpucount",
        "-p:PublishSingleFile=true",
        "-p:IncludeNativeLibrariesForSelfExtract=true",
        "-p:DebugType=None",
        "-p:DebugSymbols=false",
        "-p:EnableCompressionInSingleFile=true",
        "-o", output_dir,
        "Kuttab.csproj"
    ]
    
    if not run_command(cmd):
        print(f"❌ Failed to build for {rid}")
        return False
    
    print(f"✅ Successfully built for {rid}")
    return True

def update_build_date():
    """Update the build date in InfoActivity.cs with the current timestamp."""
    print(f"\n📅 Updating build date in InfoActivity.cs...")
    
    if not os.path.exists(INFO_ACTIVITY_PATH):
        print(f"⚠️  InfoActivity.cs not found at: {INFO_ACTIVITY_PATH}")
        return False
    
    with open(INFO_ACTIVITY_PATH, 'r', encoding='utf-8') as f:
        content = f.read()
    
    build_time = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
    # Replace the build date string
    new_content = re.sub(
        r'var buildTime = "[^"]*";(\s*//.*)?',
        f'var buildTime = "{build_time}"; // Auto-updated by build.py',
        content
    )
    
    if new_content == content:
        print("⚠️  Could not find build date pattern in InfoActivity.cs")
        return False
    
    with open(INFO_ACTIVITY_PATH, 'w', encoding='utf-8') as f:
        f.write(new_content)
    
    print(f"✅ Build date updated to: {build_time}")
    return True


def clean_build_artifacts():
    """Remove bin and obj directories to ensure a clean build."""
    print(f"\n🧹 Cleaning build artifacts...")
    
    dirs_to_clean = []
    for root, dirs, files in os.walk(SCRIPT_DIR):
        # Skip hidden directories and dist
        if any(part.startswith('.') for part in root.split(os.sep)):
            continue
        for d in dirs:
            if d in ('bin', 'obj'):
                full_path = os.path.join(root, d)
                dirs_to_clean.append(full_path)
    
    for d in dirs_to_clean:
        try:
            shutil.rmtree(d)
            print(f"   Removed: {os.path.relpath(d, SCRIPT_DIR)}")
        except Exception as e:
            print(f"   ⚠️  Failed to remove {d}: {e}")
    
    print(f"✅ Cleaned {len(dirs_to_clean)} directories")
    return True


def get_android_env():
    """Set up Android SDK environment variables."""
    env = os.environ.copy()
    android_home = env.get("ANDROID_HOME") or env.get("ANDROID_SDK_ROOT")
    if not android_home:
        android_home = str(Path.home() / "Android" / "Sdk")
    env["ANDROID_HOME"] = android_home
    if "ANDROID_SDK_ROOT" not in env:
        env["ANDROID_SDK_ROOT"] = android_home
    if not os.path.exists(android_home):
        print(f"❌ Android SDK not found at: {android_home}")
        print("   Set ANDROID_HOME or ANDROID_SDK_ROOT to your SDK path and retry.")
        return None
    return env


def get_connected_devices():
    """Get list of connected ADB devices."""
    try:
        result = subprocess.run(
            ["adb", "devices"],
            capture_output=True, text=True, check=True
        )
        devices = []
        for line in result.stdout.strip().split('\n')[1:]:
            parts = line.strip().split('\t')
            if len(parts) == 2 and parts[1] == 'device':
                device_id = parts[0]
                device_type = 'emulator' if device_id.startswith('emulator') else 'phone'
                devices.append({'id': device_id, 'type': device_type})
        return devices
    except Exception:
        return []


def install_apk(device_id, apk_path):
    """Install APK to a specific device."""
    print(f"\n📲 Installing APK to {device_id}...")
    
    # Uninstall first to ensure clean install
    subprocess.run(
        ["adb", "-s", device_id, "uninstall", "com.kuttab.app"],
        capture_output=True, text=True
    )
    
    result = subprocess.run(
        ["adb", "-s", device_id, "install", apk_path],
        capture_output=True, text=True
    )
    
    if result.returncode == 0:
        print(f"✅ Installed to {device_id}")
        # Launch the app
        subprocess.run(
            ["adb", "-s", device_id, "shell", "monkey", "-p", "com.kuttab.app",
             "-c", "android.intent.category.LAUNCHER", "1"],
            capture_output=True, text=True
        )
        print(f"🚀 Launched app on {device_id}")
        return True
    else:
        print(f"❌ Failed to install to {device_id}: {result.stderr}")
        return False


def build_android_apk(install=False):
    """Build the Android APK with a clean build."""
    print(f"\n{'='*80}")
    print(f"🤖 Building Android APK")
    print(f"{'='*80}")
    
    if not os.path.exists(ANDROID_PROJECT_DIR):
        print("⚠️  Android project not found, skipping Android build")
        return False
    
    # Step 1: Update build date
    update_build_date()
    
    # Step 2: Clean build artifacts
    clean_build_artifacts()
    
    # Step 3: Find dotnet command
    dotnet9_path = str(Path.home() / ".dotnet-9" / "dotnet")
    dotnet_android_path = str(Path.home() / ".dotnet-android" / "dotnet")
    android_dotnet_cmd = "dotnet"
    
    if os.path.exists(dotnet9_path):
        android_dotnet_cmd = dotnet9_path
        print(f"✅ Using .NET 9 SDK: {dotnet9_path}")
    elif os.path.exists(dotnet_android_path):
        android_dotnet_cmd = dotnet_android_path
        print(f"✅ Using .NET with Android workload: {dotnet_android_path}")
    else:
        result = subprocess.run(["which", "dotnet-android"], capture_output=True, text=True)
        if result.returncode == 0:
            android_dotnet_cmd = "dotnet-android"
            print("✅ Using dotnet-android command")
        else:
            print("ℹ️  Using dotnet (ensure Android workload is installed)")

    # Step 4: Set up Android environment
    env = get_android_env()
    if env is None:
        return False
    
    # Step 5: Build
    print(f"\n🔨 Publishing Android APK...")
    cmd = [
        android_dotnet_cmd, "publish",
        "-c", "Release",
        "-f", "net9.0-android",
        "-maxcpucount",
        "Kuttab.Android.csproj"
    ]
    
    if not run_command(cmd, cwd=ANDROID_PROJECT_DIR, env=env):
        print("❌ Failed to build Android APK")
        return False
    
    # Step 6: Find and copy APK
    apk_search_dir = os.path.join(ANDROID_PROJECT_DIR, "bin/Release/net9.0-android/publish")
    if not os.path.exists(apk_search_dir):
        apk_search_dir = os.path.join(ANDROID_PROJECT_DIR, "bin/Release/net9.0-android")
    
    if not os.path.exists(apk_search_dir):
        print("❌ APK output directory not found")
        return False
    
    apk_files = [f for f in os.listdir(apk_search_dir) if f.endswith("-Signed.apk")]
    if not apk_files:
        apk_files = [f for f in os.listdir(apk_search_dir) if f.endswith(".apk")]
    
    if not apk_files:
        print("❌ No APK file found in output directory")
        return False
    
    os.makedirs(DIST_DIR, exist_ok=True)
    src_apk = os.path.join(apk_search_dir, apk_files[0])
    dest_apk = os.path.join(DIST_DIR, APK_NAME)
    shutil.copy2(src_apk, dest_apk)
    os.utime(dest_apk, None)
    apk_size_mb = os.path.getsize(dest_apk) / (1024 * 1024)
    print(f"✅ APK ready: {dest_apk} ({apk_size_mb:.1f} MB)")
    
    # Step 7: Install if requested
    if install:
        devices = get_connected_devices()
        if not devices:
            print("⚠️  No connected devices found for installation")
        else:
            print(f"\n📱 Found {len(devices)} device(s):")
            for dev in devices:
                print(f"   - {dev['id']} ({dev['type']})")
                install_apk(dev['id'], dest_apk)
    
    return True

def cleanup_publish_folder():
    """Remove the publish folder after archives are created."""
    publish_dir = "./publish"
    if os.path.exists(publish_dir):
        print(f"\n🧹 Cleaning up publish folder...")
        shutil.rmtree(publish_dir)
        print(f"✅ Removed temporary publish folder")

def create_archive(rid, ext):
    """Create an archive for the built files."""
    print(f"\n📦 Creating {ext.upper()} archive for {rid}...")
    
    source_dir = f"./publish/{rid}"
    archive_name = f"{PROJECT_NAME}-{rid.replace('osx', 'macOS').replace('x64', 'x64' if 'arm' not in rid else 'AppleSilicon')}-{VERSION}.{ext}"
    
    try:
        if ext == "zip":
            shutil.make_archive(
                os.path.join("dist", archive_name.replace(f".{ext}", "")),
                'zip',
                source_dir
            )
        elif ext in ["tar.gz", "tgz"]:
            shutil.make_archive(
                os.path.join("dist", archive_name.replace(f".{ext}", "")),
                'gztar',
                source_dir
            )
        print(f"✅ Created archive: dist/{archive_name}")
        return True
    except Exception as e:
        print(f"❌ Failed to create archive: {e}")
        return False

def copy_installation_instructions():
    """Copy installation instructions to dist folder."""
    src_file = "utils/documentation/INSTALLATION_INSTRUCTIONS.md"
    dest_file = os.path.join("dist", "INSTALLATION_INSTRUCTIONS.md")
    
    if os.path.exists(src_file):
        shutil.copy2(src_file, dest_file)
        print(f"📄 Copied installation instructions to dist folder")
    else:
        print(f"⚠️  Installation instructions file not found: {src_file}")

def main():
    parser = argparse.ArgumentParser(description=f"{PROJECT_NAME} Build Script")
    parser.add_argument('--android', action='store_true',
                        help='Build only the Android APK (skip desktop targets)')
    parser.add_argument('--install', action='store_true',
                        help='Install APK to all connected devices after building')
    parser.add_argument('--all', action='store_true',
                        help='Build all targets (desktop + Android)')
    args = parser.parse_args()
    
    # Default to --android --install if no flags given
    if not args.android and not args.all:
        args.android = True
        args.install = True
    
    os.makedirs(DIST_DIR, exist_ok=True)
    
    if args.all:
        # Copy installation instructions
        copy_installation_instructions()
        
        # Build for all desktop targets
        success_count = 0
        for target in TARGETS:
            if build_for_platform(target):
                if create_archive(target["rid"], target["ext"]):
                    success_count += 1
        
        # Build Android APK
        android_success = build_android_apk(install=args.install)
        if android_success:
            success_count += 1
        
        total_targets = len(TARGETS) + 1
        
        print("\n" + "="*50)
        print(f"🚀 Build Summary")
        print("="*50)
        print(f"Total targets: {total_targets}")
        print(f"Successfully built: {success_count}")
        print(f"Failed: {total_targets - success_count}")
        
        if success_count > 0:
            print("\n📦 Distribution packages created in the 'dist' directory:")
            dist_files = os.listdir(DIST_DIR)
            for file in dist_files:
                size_mb = os.path.getsize(os.path.join(DIST_DIR, file)) / (1024 * 1024)
                print(f"- {file} ({size_mb:.2f} MB)")
        
        cleanup_publish_folder()
        
        print("\n✅ Build process completed!" if success_count > 0 else "\n❌ Build process completed with errors!")
    
    elif args.android:
        success = build_android_apk(install=args.install)
        if success:
            print("\n✅ Android build completed successfully!")
        else:
            print("\n❌ Android build failed!")
            sys.exit(1)


if __name__ == "__main__":
    main()
