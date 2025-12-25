# Debug Logging Guide

## Overview
The AudioService now includes comprehensive logging that works across all platforms (Windows, Linux, macOS).

## Where Debug Output Goes

### 1. **Log File (All Platforms)**
- **Location**: `audio_debug.log` in the same directory as the executable
- **Format**: Timestamped entries with detailed information
- **Usage**: Open this file with any text editor to see all debug messages
- **Example**:
  ```
  [2025-10-16 08:24:15.123] [AudioService] Looking for audio files in: C:\Program Files\Quraan\Alhusary
  [2025-10-16 08:24:15.125] [AudioService] Directory exists: True
  [2025-10-16 08:24:20.456] [AudioService] Using PowerShell MediaPlayer for Windows
  [2025-10-16 08:24:20.458] [AudioService] Starting playback: powershell -Command "..."
  ```

### 2. **Windows - DebugView Tool**
Since Windows GUI applications (`WinExe`) don't show console output, use Microsoft's DebugView:

#### Download DebugView:
- **URL**: https://learn.microsoft.com/en-us/sysinternals/downloads/debugview
- **Direct Download**: https://download.sysinternals.com/files/DebugView.zip

#### How to Use:
1. Extract DebugView.zip
2. Run `Dbgview.exe` as Administrator
3. In DebugView menu: **Capture** → Enable **Capture Global Win32**
4. Run your Quraan application
5. See real-time debug output in DebugView window

#### DebugView Features:
- Real-time output monitoring
- Filter messages by text
- Save output to file
- Highlight specific messages
- Clear output buffer

### 3. **Linux (Terminal Output)**
When running from terminal, debug messages appear directly:
```bash
./Quraan
# Output appears in terminal
```

### 4. **Visual Studio / Rider Debugger**
When debugging in an IDE:
- Debug output appears in the **Output** window (Visual Studio)
- Or **Debug Console** (Rider)
- Set breakpoints in `LogDebug()` method to inspect messages

## What Gets Logged

The AudioService logs:
- ✅ Audio file path discovery
- ✅ Directory existence checks
- ✅ Audio player selection (mpg123, paplay, PowerShell, etc.)
- ✅ Playback commands and arguments
- ✅ File existence verification
- ✅ Download progress (when using remote source)
- ✅ Cache operations
- ✅ Error messages and stack traces
- ✅ Player error output

## Troubleshooting

### No Log File Created?
- Check file permissions in the application directory
- On Windows, run as Administrator if installed in Program Files
- The log file is created in the same folder as the executable

### Log File Too Large?
- Delete `audio_debug.log` to start fresh
- The file appends continuously, so it grows over time

### Still No Output on Windows?
1. Verify the log file exists: `audio_debug.log`
2. Use DebugView tool (see above)
3. Check Windows Event Viewer for application errors

## Example Debug Session

### Successful Playback:
```
[2025-10-16 08:24:15.123] [AudioService] Looking for audio files in: C:\Quraan\Alhusary
[2025-10-16 08:24:15.125] [AudioService] Directory exists: True
[2025-10-16 08:24:20.456] [AudioService] Using PowerShell MediaPlayer for Windows
[2025-10-16 08:24:20.458] [AudioService] Starting playback: powershell -Command "..."
[2025-10-16 08:24:20.460] [AudioService] File path: C:\Quraan\Alhusary\001001.mp3
[2025-10-16 08:24:20.462] [AudioService] File exists: True
```

### File Not Found:
```
[2025-10-16 08:24:15.123] [AudioService] Looking for audio files in: C:\Quraan\Alhusary
[2025-10-16 08:24:15.125] [AudioService] Directory exists: False
[2025-10-16 08:24:20.456] [AudioService] Using PowerShell MediaPlayer for Windows
[2025-10-16 08:24:20.458] [AudioService] Starting playback: powershell -Command "..."
[2025-10-16 08:24:20.460] [AudioService] File path: C:\Quraan\Alhusary\001001.mp3
[2025-10-16 08:24:20.462] [AudioService] File exists: False
```

## Technical Details

### Logging Implementation:
The `LogDebug()` method writes to three outputs simultaneously:
1. **Debug.WriteLine()** - Visible in debuggers and DebugView
2. **Trace.WriteLine()** - Visible in trace listeners
3. **Console.WriteLine()** - Visible in terminal (Linux/macOS)
4. **File.AppendAllText()** - Persistent log file

### Thread Safety:
File writes are protected with a lock to prevent corruption when multiple threads log simultaneously.

### Performance:
- File write errors are silently ignored to prevent crashes
- Minimal overhead - logging happens asynchronously
- No impact on audio playback performance
