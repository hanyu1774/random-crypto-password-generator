using System.Runtime.CompilerServices;
using NET.ConsoleApp.RandomPasswords.Interop;
internal sealed class PrintPasswords
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void run(ReadOnlySpan<char> buffer, byte number_of_passwords)
    {
        int count = buffer.Length / number_of_passwords;

        WindowsTerminal.Write("\n");
        WindowsTerminal.Write("\x1b[32mSUCCESS! ");
        WindowsTerminal.Write("\x1b[33mNew random passwords were made!\x1b[0m\n");
        WindowsTerminal.Write("\n");
        Span<char> numBuf = stackalloc char[3]; 
        for (int password_index = 0; password_index < count; password_index++)
        {
            ReadOnlySpan<char> slice = buffer.Slice(password_index * number_of_passwords, number_of_passwords);
            (password_index + 1).TryFormat(numBuf, out int written);
            WindowsTerminal.Write(numBuf[..written]);
            WindowsTerminal.Write(") ");
            WindowsTerminal.Write(slice);
            WindowsTerminal.Write("\n");
        }

        WindowsTerminal.Write("\n");
        WindowsTerminal.Write("\x1b[31m########################################################\n");
        WindowsTerminal.Write("## ATTENTION! THOSE PASSWORDS AREN'T STORED ANYWHERE! ##\n");
        WindowsTerminal.Write("## PLEASE WRITE DOWN YOUR CHOSEN PASSWORD(S) AND/OR   ##\n");
        WindowsTerminal.Write("## USE A SOFTWARE (E.G. KEEPASS) FOR STORAGE!         ##\n");
        WindowsTerminal.Write("########################################################\n\x1b[0m");
    }
}
