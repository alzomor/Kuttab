using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace QuranSearchApp.Services
{
    /// <summary>
    /// Cross-platform service to prevent screen dimming and system sleep during audio playback
    /// </summary>
    public class ScreenKeepAwakeService : IDisposable
    {
        private bool _isKeepingAwake = false;
        private Process? _caffeinateProcess; // For macOS
        private uint _previousExecutionState; // For Windows

        // Windows API for preventing sleep
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint SetThreadExecutionState(uint esFlags);

        // Execution state flags for Windows
        private const uint ES_CONTINUOUS = 0x80000000;
        private const uint ES_SYSTEM_REQUIRED = 0x00000001;
        private const uint ES_DISPLAY_REQUIRED = 0x00000002;

        /// <summary>
        /// Prevents the screen from dimming and the system from sleeping
        /// </summary>
        public void PreventSleep()
        {
            if (_isKeepingAwake)
                return;

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    PreventSleepWindows();
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    PreventSleepMacOS();
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    PreventSleepLinux();
                }

                _isKeepingAwake = true;
                Debug.WriteLine("[ScreenKeepAwake] Sleep prevention activated");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ScreenKeepAwake] Failed to prevent sleep: {ex.Message}");
            }
        }

        /// <summary>
        /// Allows the screen to dim and the system to sleep again
        /// </summary>
        public void AllowSleep()
        {
            if (!_isKeepingAwake)
                return;

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    AllowSleepWindows();
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    AllowSleepMacOS();
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    AllowSleepLinux();
                }

                _isKeepingAwake = false;
                Debug.WriteLine("[ScreenKeepAwake] Sleep prevention deactivated");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ScreenKeepAwake] Failed to allow sleep: {ex.Message}");
            }
        }

        private void PreventSleepWindows()
        {
            // Prevent display and system from sleeping
            _previousExecutionState = SetThreadExecutionState(
                ES_CONTINUOUS | ES_SYSTEM_REQUIRED | ES_DISPLAY_REQUIRED);
        }

        private void AllowSleepWindows()
        {
            // Restore previous execution state (allow sleep)
            SetThreadExecutionState(ES_CONTINUOUS);
        }

        private void PreventSleepMacOS()
        {
            // Use caffeinate command to prevent sleep
            try
            {
                _caffeinateProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "caffeinate",
                        Arguments = "-d", // Prevent display from sleeping
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };
                _caffeinateProcess.Start();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ScreenKeepAwake] caffeinate failed: {ex.Message}");
            }
        }

        private void AllowSleepMacOS()
        {
            try
            {
                if (_caffeinateProcess != null && !_caffeinateProcess.HasExited)
                {
                    _caffeinateProcess.Kill();
                    _caffeinateProcess.WaitForExit(1000);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ScreenKeepAwake] Failed to stop caffeinate: {ex.Message}");
            }
            finally
            {
                _caffeinateProcess?.Dispose();
                _caffeinateProcess = null;
            }
        }

        private void PreventSleepLinux()
        {
            // Try using systemd-inhibit or xdg-screensaver
            try
            {
                // First try xdg-screensaver suspend (works on most desktop environments)
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "xdg-screensaver",
                        Arguments = "suspend $$",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };
                process.Start();
                process.WaitForExit(1000);
            }
            catch
            {
                // xdg-screensaver not available, try alternative methods
                try
                {
                    // Try using xset to disable screen saver
                    var xsetProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "xset",
                            Arguments = "s off -dpms",
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };
                    xsetProcess.Start();
                    xsetProcess.WaitForExit(1000);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ScreenKeepAwake] Linux sleep prevention failed: {ex.Message}");
                }
            }
        }

        private void AllowSleepLinux()
        {
            try
            {
                // Resume xdg-screensaver
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "xdg-screensaver",
                        Arguments = "resume $$",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };
                process.Start();
                process.WaitForExit(1000);
            }
            catch
            {
                // Try restoring xset settings
                try
                {
                    var xsetProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "xset",
                            Arguments = "s on +dpms",
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };
                    xsetProcess.Start();
                    xsetProcess.WaitForExit(1000);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ScreenKeepAwake] Linux sleep restore failed: {ex.Message}");
                }
            }
        }

        public void Dispose()
        {
            AllowSleep();
            _caffeinateProcess?.Dispose();
        }
    }
}
