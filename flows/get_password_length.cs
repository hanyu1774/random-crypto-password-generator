using System.Runtime.CompilerServices;
using NET.ConsoleApp.RandomPasswords.Interop;

internal sealed class GetPasswordLength
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte run()
    {
        byte password_length;
        while (true)
        {
            WindowsTerminal.Clear();
            WindowsTerminal.Write("Enter the desired password length (8 - 255): ");

            ReadOnlySpan<char> user_input = WindowsTerminal.ReadLine().Trim();
            if (Byte.TryParse(user_input, out password_length) &&
                password_length > 7 && password_length <= Byte.MaxValue)
            {
                break;
            }

            WindowsTerminal.Write("\x1b[31m");
            WindowsTerminal.Write("ERROR: Invalid input! Please enter a positive number (8 - 255).\n");
            WindowsTerminal.Write("\x1b[0m");
        }
        return password_length;
    }
}
