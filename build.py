import os
import sys
import subprocess
import shutil
import platform
from pathlib import Path

# Configuration
PROJECT_NAME = "Kuttab"
VERSION = "0.7.0"
TARGETS = [
    {"rid": "win-x64", "ext": "zip"},
    {"rid": "linux-x64", "ext": "tar.gz"},
    {"rid": "osx-x64", "ext": "tar.gz"},
    {"rid": "osx-arm64", "ext": "tar.gz"},
]

def run_command(cmd, cwd=None):
    """Run a shell command and return its output."""
    print(f"\n🚀 Running: {' '.join(cmd)}")
    try:
        result = subprocess.run(
            cmd,
            cwd=cwd or os.getcwd(),
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
    
    # Build command
    cmd = [
        "dotnet", "publish",
        "-c", "Release",
        "-r", rid,
        "--self-contained", "true",
        "-p:PublishSingleFile=true",
        "-p:IncludeNativeLibrariesForSelfExtract=true",
        "-p:DebugType=None",
        "-p:DebugSymbols=false",
        "-p:EnableCompressionInSingleFile=true",
        "-o", output_dir,
        "QuranSearchApp.csproj"
    ]
    
    if not run_command(cmd):
        print(f"❌ Failed to build for {rid}")
        return False
    
    print(f"✅ Successfully built for {rid}")
    return True

def build_android_apk():
    """Build the Android APK."""
    print(f"\n{'='*80}")
    print(f"🤖 Building Android APK")
    print(f"{'='*80}")
    
    android_project_dir = "./QuranSearch.Android"
    if not os.path.exists(android_project_dir):
        print("⚠️  Android project not found, skipping Android build")
        return False
    
    # Publish command for Android (creates signed APK)
    cmd = [
        "dotnet", "publish",
        "-c", "Release",
        "-f", "net8.0-android",
        "-r", "android-arm64",
        "QuranSearch.Android.csproj"
    ]
    
    if not run_command(cmd, cwd=android_project_dir):
        print("❌ Failed to build Android APK")
        return False
    
    # Find the generated APK (publish outputs to android-arm64 subfolder)
    apk_search_dir = os.path.join(android_project_dir, "bin/Release/net8.0-android/android-arm64")
    if not os.path.exists(apk_search_dir):
        # Fallback to non-RID path
        apk_search_dir = os.path.join(android_project_dir, "bin/Release/net8.0-android")
    
    if not os.path.exists(apk_search_dir):
        print("❌ APK output directory not found")
        return False
    
    # Find signed APK files
    apk_files = [f for f in os.listdir(apk_search_dir) if f.endswith("-Signed.apk")]
    if not apk_files:
        # Fallback to any APK
        apk_files = [f for f in os.listdir(apk_search_dir) if f.endswith(".apk")]
    
    if not apk_files:
        print("❌ No APK file found in output directory")
        return False
    
    # Copy APK to dist folder
    os.makedirs("dist", exist_ok=True)
    for apk_file in apk_files:
        src_apk = os.path.join(apk_search_dir, apk_file)
        # Rename to a cleaner name
        dest_apk = os.path.join("dist", f"{PROJECT_NAME}-android-{VERSION}.apk")
        shutil.copy2(src_apk, dest_apk)
        print(f"✅ Copied APK to: {dest_apk}")
        break  # Only copy the first (signed) APK
    
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
    src_file = "INSTALLATION_INSTRUCTIONS.md"
    dest_file = os.path.join("dist", "INSTALLATION_INSTRUCTIONS.md")
    
    if os.path.exists(src_file):
        shutil.copy2(src_file, dest_file)
        print(f"📄 Copied installation instructions to dist folder")
    else:
        print(f"⚠️  Installation instructions file not found: {src_file}")

def main():
    # Create necessary directories
    os.makedirs("dist", exist_ok=True)
    
    # Copy installation instructions
    copy_installation_instructions()
    
    # Build for all desktop targets
    success_count = 0
    for target in TARGETS:
        if build_for_platform(target):
            if create_archive(target["rid"], target["ext"]):
                success_count += 1
    
    # Build Android APK
    android_success = build_android_apk()
    if android_success:
        success_count += 1
    
    total_targets = len(TARGETS) + 1  # Desktop targets + Android
    
    # Print summary
    print("\n" + "="*50)
    print(f"🚀 Build Summary")
    print("="*50)
    print(f"Total targets: {total_targets}")
    print(f"Successfully built: {success_count}")
    print(f"Failed: {total_targets - success_count}")
    
    if success_count > 0:
        print("\n📦 Distribution packages created in the 'dist' directory:")
        dist_files = os.listdir("dist")
        for file in dist_files:
            size_mb = os.path.getsize(os.path.join("dist", file)) / (1024 * 1024)
            print(f"- {file} ({size_mb:.2f} MB)")
    
    # Clean up temporary publish folder
    cleanup_publish_folder()
    
    print("\n✅ Build process completed!" if success_count > 0 else "\n❌ Build process completed with errors!")

if __name__ == "__main__":
    main()
