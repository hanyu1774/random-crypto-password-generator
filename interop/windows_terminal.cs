using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NET.ConsoleApp.RandomPasswords.Interop;

public static partial class WindowsTerminal
{
    [LibraryImport("kernel32")]
    private static partial IntPtr GetStdHandle(int nStdHandle);

    [LibraryImport("kernel32")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [LibraryImport("kernel32")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    [LibraryImport("kernel32", EntryPoint = "WriteConsoleW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool WriteConsoleW(IntPtr hConsoleOutput, ReadOnlySpan<char> lpBuffer, uint nNumberOfCharsToWrite, out uint lpNumberOfCharsWritten, IntPtr lpReserved);

    [LibraryImport("kernel32")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetConsoleScreenBufferInfo(IntPtr hConsoleOutput, out CONSOLE_SCREEN_BUFFER_INFO lpConsoleScreenBufferInfo);

    [LibraryImport("kernel32", EntryPoint = "FillConsoleOutputCharacterW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool FillConsoleOutputCharacter(IntPtr hConsoleOutput, char cCharacter, uint nLength, COORD dwWriteCoord, out uint lpNumberOfCharsWritten);

    [LibraryImport("kernel32")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetConsoleCursorPosition(IntPtr hConsoleOutput, COORD dwCursorPosition);

    [LibraryImport("kernel32", EntryPoint = "ReadConsoleW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool ReadConsoleW(IntPtr hConsoleInput, Span<char> lpBuffer, uint nNumberOfCharsToRead, out uint lpNumberOfCharsRead, IntPtr lpReserved);

    static WindowsTerminal()
    {
        IntPtr consoleHandle = GetStdHandle(-11);

        GetConsoleMode(consoleHandle, out uint consoleMode);
        consoleMode |= 0x0004; // ENABLE_VIRTUAL_TERMINAL_PROCESSING
        SetConsoleMode(consoleHandle, consoleMode);
    }

    public static void Write(ReadOnlySpan<char> value)
    {
        IntPtr consoleHandle = GetStdHandle(-11);
        WriteConsoleW(consoleHandle, value, (uint)value.Length, out _, IntPtr.Zero);
    }


    public static void Write(char value)
    {
        ReadOnlySpan<char> span_value = stackalloc char[] {value};
        IntPtr consoleHandle = GetStdHandle(-11);
        WriteConsoleW(consoleHandle, span_value, 1, out _, IntPtr.Zero);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Clear()
    {
        IntPtr hConsole = GetStdHandle(-11);
        GetConsoleScreenBufferInfo(hConsole, out CONSOLE_SCREEN_BUFFER_INFO csbi);

        COORD dwTopLeft = new COORD();

        FillConsoleOutputCharacter(hConsole, ' ', (uint)(csbi.dwSize.X * csbi.dwSize.Y), dwTopLeft, out _);
        SetConsoleCursorPosition(hConsole, dwTopLeft);
    }

    public static ReadOnlySpan<char> ReadLine()
    {
        IntPtr hConsoleInput = GetStdHandle(-10);
        Span<char> buffer = stackalloc char[1024];

        ReadConsoleW(hConsoleInput, buffer, (uint)buffer.Length, out uint charsRead, IntPtr.Zero);

        int len = (int)charsRead;
        // Trim trailing newline, mirroring ReadConsole's line-mode behavior
        if (len > 0 && buffer[len - 1] == '\n') len--;
        if (len > 0 && buffer[len - 1] == '\r') len--;

        char[] result = new char[len];
        buffer[..len].CopyTo(result);
        return result;
    }

    struct COORD
    {
        public short X;
        public short Y;
    }

    struct CONSOLE_SCREEN_BUFFER_INFO
    {
        public COORD dwSize;
        public COORD dwCursorPosition;
        public short wAttributes;
        public SMALL_RECT srWindow;
        public COORD dwMaximumWindowSize;
    }

    struct SMALL_RECT
    {
        public short Left;
        public short Top;
        public short Right;
        public short Bottom;
    }
}
