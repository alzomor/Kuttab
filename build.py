import os
import sys
import subprocess
import shutil
import platform
from pathlib import Path

# Configuration
PROJECT_NAME = "Quraan"
VERSION = "0.2.0"
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
        return True
    except subprocess.CalledProcessError as e:
        print(f"❌ Command failed with error: {e}")
        if e.stderr:
            print(f"Error details:\n{e.stderr}")
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

def main():
    # Create necessary directories
    os.makedirs("dist", exist_ok=True)
    
    # Build for all targets
    success_count = 0
    for target in TARGETS:
        if build_for_platform(target):
            if create_archive(target["rid"], target["ext"]):
                success_count += 1
    
    # Print summary
    print("\n" + "="*50)
    print(f"🚀 Build Summary")
    print("="*50)
    print(f"Total targets: {len(TARGETS)}")
    print(f"Successfully built: {success_count}")
    print(f"Failed: {len(TARGETS) - success_count}")
    
    if success_count > 0:
        print("\n📦 Distribution packages created in the 'dist' directory:")
        dist_files = os.listdir("dist")
        for file in dist_files:
            size_mb = os.path.getsize(os.path.join("dist", file)) / (1024 * 1024)
            print(f"- {file} ({size_mb:.2f} MB)")
    
    print("\n✅ Build process completed!" if success_count > 0 else "\n❌ Build process completed with errors!")

if __name__ == "__main__":
    main()
