namespace PRN231.TrialTest.API.Helper;

public static class StringHelper
{
    public static string GenerateRandomString()
    {
        Random random = new Random();
        char[] letters = new char[3];
        char[] digits = new char[4];

        // Generate 3 random uppercase letters
        for (int i = 0; i < letters.Length; i++)
        {
            letters[i] = (char)random.Next('A', 'Z' + 1);
        }

        // Generate 4 random digits
        for (int i = 0; i < digits.Length; i++)
        {
            digits[i] = (char)random.Next('0', '9' + 1);
        }

        // Combine letters and digits into a single string
        string result = new string(letters) + new string(digits);
        return result;
    }
}
