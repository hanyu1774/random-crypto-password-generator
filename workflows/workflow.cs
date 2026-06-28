using NET.ConsoleApp.RandomPasswords.Flows;
using NET.ConsoleApp.RandomPasswords.Interop;
using System.Runtime.CompilerServices;
namespace NET.ConsoleApp.RandomPasswords.Workflows;

public sealed class Workflow
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void run()
    {
        CreateHeader create_header = new();
        GetPasswordLength get_password_length = new();
        MakeRandomPasswords make_random_passwords = new();
        PrintPasswords print_passwords = new();
        
        byte length = get_password_length.run();
        const byte number_of_passwords = 12;
        ReadOnlySpan<char> made_passwords = make_random_passwords.run(length, number_of_passwords);
        
        ReadOnlySpan<char> header = create_header.run("RANDOM PASSWORDS");
        WindowsTerminal.Clear();
        WindowsTerminal.Write(header);
        WindowsTerminal.Write('\n');
        print_passwords.run(made_passwords,  length);
    }
}
