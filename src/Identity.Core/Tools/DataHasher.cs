using System.Security.Cryptography;
using System.Text;

namespace Identity.Core.Tools;

public static class DataHasher
{
    public static (byte[] hash, byte[] salt) Hash(string data)
    {
        using var hmac = new HMACSHA512();
        var salt = hmac.Key;
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return (hash, salt);
    }

    public static (byte[] hash, byte[] salt) Hash(string data, string salt)
    {
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(salt));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return (hash, hmac.Key);
    }

    public static string Decrypt(byte[] data, byte[] key)
    {
        using var hmac = new HMACSHA512(key);
        var decrypted = hmac.ComputeHash(data);
        return Encoding.UTF8.GetString(decrypted);
    }

    public static bool ControlHash(string data, byte[] hash, byte[] salt)
    {
        using var hmac = new HMACSHA512(salt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        for (var i = 0; i < computedHash.Length; i++)
        {
            if (computedHash[i] != hash[i])
            {
                return false;
            }
        }

        return true;
    }

    public static string CreateRandomString(int size)
    {
        var random = new Random();
        var builder = new StringBuilder();
        char ch;
        for (var i = 0; i < size; i++)
        {
            ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
            builder.Append(ch);
        }

        return builder.ToString();
    }
}