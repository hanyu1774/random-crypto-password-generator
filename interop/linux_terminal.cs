using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace NET.ConsoleApp.RandomPasswords.Interop;

    public static partial class LinuxTerminal
    {
        // ---- file descriptors (POSIX equivalent of Windows STD_*_HANDLE) ----
        const int STDIN_FD = 0;
        const int STDOUT_FD = 1;

        // ---- termios c_lflag bits we care about ----
        const uint ICANON = 0x0002; // canonical (line-buffered) mode
        const uint ECHO = 0x0008;   // echo input characters

        const int TCSANOW = 0;

        // ---- libc P/Invokes (source-generated, AOT-safe. Replaces kernel32 imports) ----

        [LibraryImport("libc", SetLastError = true)]
        private static partial int write(int fd, ReadOnlySpan<byte> buf, nuint count);

        [LibraryImport("libc", SetLastError = true)]
        private static partial int read(int fd, Span<byte> buf, nuint count);

        [LibraryImport("libc", SetLastError = true)]
        private static partial int tcgetattr(int fd, out Termios termios);
        
        [LibraryImport("libc", SetLastError = true)]
        private static partial int tcsetattr(int fd, int optionalActions, in Termios termios);

        [LibraryImport("libc", SetLastError = true)]
        private static partial int ioctl(int fd, ulong request, out WinSize ws);

        // TIOCGWINSZ differs per architecture in theory, but is 0x5413 on Linux/x86_64 and arm64
        const ulong TIOCGWINSZ = 0x5413;

        static LinuxTerminal()
        {
            // Windows dll imports for the native Windows console enable ANSI/VT escape
            // processing via SetConsoleMode.
            // Linux terminals (including Arch's default alacritty/kitty/gnome-terminal/etc.)
            // already interpret ANSI/VT100 escapes natively. There is no equivalent mode
            // flag to set. Nothing to do here
        }

        public static void Write(string value)
        {
            // UTF-8 encode and write directly to fd 1, bypassing System.Console/Stream entirely.
            int byteCount = Encoding.UTF8.GetByteCount(value);
            Span<byte> bytes = byteCount <= 256 ? stackalloc byte[byteCount] : new byte[byteCount];
            Encoding.UTF8.GetBytes(value, bytes);
            write(STDOUT_FD, bytes, (nuint)byteCount);
        }

        public static void Write(ReadOnlySpan<char> value)
        {
            int byteCount = Encoding.UTF8.GetByteCount(value);
            Span<byte> bytes = byteCount <= 256 ? stackalloc byte[byteCount] : new byte[byteCount];
            Encoding.UTF8.GetBytes(value, bytes);
            write(STDOUT_FD, bytes, (nuint)byteCount);
        }

        public static void Write(char value)
        {
            if (value < 128)
            {
                Span<byte> single = stackalloc byte[1] { (byte)value };
                write(STDOUT_FD, single, 1);
            }
            else
            {
            Span<char> charSpan = stackalloc char[1] { value };
            Write((ReadOnlySpan<char>)charSpan);
        }
}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clear()
        {
            // No FillConsoleOutputCharacter/SetConsoleCursorPosition equivalent exists on
            // Linux terminals. Clearing is done via ANSI escape sequences instead:
            //   \x1b[2J  -> clear entire screen
            //   \x1b[H   -> move cursor to row 1, col 1 (home)
            Write("\x1b[2J\x1b[H");
        }

        public static ReadOnlySpan<char> ReadLine()
        {
            Span<byte> buffer = stackalloc byte[1024];
            int n = read(STDIN_FD, buffer, (nuint)buffer.Length);

            if (n <= 0)
            {
                return ReadOnlySpan<char>.Empty;
            }

            // Trim trailing newline like Windows ReadConsole's line-mode behavior
            int len = n;
            if (len > 0 && buffer[len - 1] == (byte)'\n') len--;
            if (len > 0 && buffer[len - 1] == (byte)'\r') len--;

            ReadOnlySpan<byte> trimmed = buffer[..len];

            // UTF-8 decoding never produces more chars than input bytes
            // (every UTF-16 char needs ≥1 byte to encode, surrogate pairs need 2
            // chars but 4 bytes), so `len` is always a safe upper bound. No need
            // for a separate GetCharCount pass.
            char[] chars = new char[len];
            int charsWritten = Encoding.UTF8.GetChars(trimmed, chars);

            return chars.AsSpan(0, charsWritten);
        }

        /// <summary>
        /// Attempts to switch the terminal out of canonical mode (and optionally disable
        /// echo). Returns false if stdin isn't a real terminal (e.g. redirected from a
        /// pipe or /dev/null), in which case the underlying tcgetattr/tcsetattr calls
        /// fail with ENOTTY and Original is left zeroed — check the return value rather
        /// than assuming success.
        /// </summary>
        public static bool TryEnterRawMode(bool echo, out RawModeScope scope)
        {
            scope = default;

            if (tcgetattr(STDIN_FD, out Termios original) != 0)
                return false;

            var raw = original;
            raw.c_lflag &= ~ICANON;
            if (!echo) raw.c_lflag &= ~ECHO;

            if (tcsetattr(STDIN_FD, TCSANOW, in raw) != 0)
                return false;

            scope.Original = original;
            return true;
        }

        /// <summary>
        /// Holds the terminal's prior termios settings so they can be restored later.
        /// </summary>
        public struct RawModeScope
        {
            public Termios Original;

            /// <returns>false if tcsetattr failed (e.g. stdin is not a terminal).</returns>
            public readonly bool Restore() => tcsetattr(STDIN_FD, TCSANOW, in Original) == 0;
        }

        // ---- structs ----
        // Windows COORD/CONSOLE_SCREEN_BUFFER_INFO/SMALL_RECT kept (renamed-friendly) for
        // call-site compatibility where width/height info is still useful, now populated
        // via ioctl(TIOCGWINSZ) instead of GetConsoleScreenBufferInfo.

        public struct COORD
        {
            public short X;
            public short Y;
        }

        public struct CONSOLE_SCREEN_BUFFER_INFO
        {
            public COORD dwSize;
            public COORD dwCursorPosition;
            public short wAttributes;
            public SMALL_RECT srWindow;
            public COORD dwMaximumWindowSize;
        }

        public struct SMALL_RECT
        {
            public short Left;
            public short Top;
            public short Right;
            public short Bottom;
        }

        // Linux ioctl(TIOCGWINSZ) result. The actual source of terminal size on Linux
        [StructLayout(LayoutKind.Sequential)]
        public struct WinSize
        {
            public ushort ws_row;
            public ushort ws_col;
            public ushort ws_xpixel;
            public ushort ws_ypixel;
        }

        // POSIX termios struct (Linux x86_64/arm64 layout; NCCS = 32).
        // c_cc must be a blittable fixed-size buffer (not a managed byte[]) for the
        // LibraryImport source generator to treat Termios as blittable.
        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct Termios
        {
            public uint c_iflag;
            public uint c_oflag;
            public uint c_cflag;
            public uint c_lflag;
            public byte c_line;
            public fixed byte c_cc[32];
            public uint c_ispeed;
            public uint c_ospeed;
        }

        /// <summary>
        /// Reads current terminal size via ioctl(TIOCGWINSZ), the Linux equivalent of
        /// GetConsoleScreenBufferInfo's dwSize.
        /// </summary>
        public static CONSOLE_SCREEN_BUFFER_INFO GetScreenBufferInfo()
        {
            ioctl(STDOUT_FD, TIOCGWINSZ, out WinSize ws);

            return new CONSOLE_SCREEN_BUFFER_INFO
            {
                dwSize = new COORD { X = (short)ws.ws_col, Y = (short)ws.ws_row },
                dwCursorPosition = new COORD { X = 0, Y = 0 },
                dwMaximumWindowSize = new COORD { X = (short)ws.ws_col, Y = (short)ws.ws_row }
            };
        }
    }

