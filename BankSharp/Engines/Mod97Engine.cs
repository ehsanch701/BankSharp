using System.Runtime.CompilerServices;

namespace BankSharp.Engines;



/// <summary>
/// High-performance, zero-allocation ISO 7064 Mod 97-10 calculation engine.
/// </summary>
internal static class Mod97Engine
{
    /// <summary>
    /// Calculates the ISO 7064 Mod 97 remainder for an IBAN without heap allocations.
    /// Returns 1 if the IBAN checksum is valid; otherwise, returns the computed remainder or -1 on invalid character.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CalculateIbanMod97(ReadOnlySpan<char> iban)
    {
        if (iban.Length < 4)
        {
            return -1;
        }

        int remainder = 0;

        // Step 1: Process characters starting from index 4 to end (BBAN portion)
        for (int i = 4; i < iban.Length; i++)
        {
            if (!ProcessChar(iban[i], ref remainder))
            {
                return -1;
            }
        }

        // Step 2: Append the first 4 characters (country code + checksum digits) to the calculation
        for (int i = 0; i < 4; i++)
        {
            if (!ProcessChar(iban[i], ref remainder))
            {
                return -1;
            }
        }

        return remainder;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ProcessChar(char c, ref int remainder)
    {
        // Numeric digit: '0' - '9'
        if ((uint)(c - '0') <= 9)
        {
            remainder = (remainder * 10 + (c - '0')) % 97;
            return true;
        }

        // Normalize lowercase letters to uppercase
        if ((uint)(c - 'a') <= 25)
        {
            c = (char)(c - 32);
        }

        // Alphabet letter: 'A' - 'Z' (maps to values 10 through 35)
        if ((uint)(c - 'A') <= 25)
        {
            int value = c - 'A' + 10;
            remainder = (remainder * 100 + value) % 97;
            return true;
        }

        // Invalid character encountered (e.g. whitespace, hyphen, special symbols)
        return false;
    }
}
