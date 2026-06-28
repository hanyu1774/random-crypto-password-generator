using System.Security.Cryptography;
using System.Runtime.CompilerServices;

namespace NET.ConsoleApp.RandomPasswords.Flows;


internal readonly struct CharacterSets
{
    public static ReadOnlySpan<char> all_characters => new char[94] 
    {
        '!', '"', '#', '$', '%', '&', '\'', '(', ')', '*', '+', ',', '-', '.', '/',
        ':', ';', '<', '=', '>', '?', '@',
        '[', '\\', ']', '^', '_', '`',
        '{', '|', '}', '~',
        '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
        'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
        'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
        'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M',
        'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
    };
}

internal sealed class MakeRandomPasswords
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public ReadOnlySpan<char> run
        (
            byte password_length,
            byte number_of_passwords
        )
    {
        char[] buffer = new char[password_length * number_of_passwords];
        Span<char> buffer_span = buffer;
;
        for (int password_index = 0; password_index < number_of_passwords; password_index++)
        {
            Span<char> slice = buffer_span.Slice(password_index * password_length, password_length);
            RandomNumberGenerator.GetItems(CharacterSets.all_characters, slice); 
            
        }
        return buffer;
    }
}
