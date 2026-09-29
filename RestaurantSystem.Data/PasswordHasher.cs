using System;
using System.Security.Cryptography;
using System.Text;

namespace RestaurantSystem.Data;

/// <summary>
/// معالج كلمات المرور — يستخدم PBKDF2 مع Salt عشوائي لكل مستخدم (أمان عالي).
/// يدعم التوافق مع كلمات المرور القديمة (SHA-256) للانتقال السلس.
/// </summary>
public static class PasswordHasher
{
    private const int SaltSize    = 16;   // 128 بت
    private const int HashSize    = 32;   // 256 بت
    private const int Iterations  = 100_000;
    private const string Prefix   = "PBKDF2:";

    // --- الصيغة الجديدة الآمنة (PBKDF2 + Salt عشوائي) ---

    /// <summary>
    /// تشفير كلمة المرور بـ PBKDF2 مع Salt عشوائي (يختلف لكل مستخدم).
    /// </summary>
    public static string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password)) return string.Empty;

        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        byte[] hash = pbkdf2.GetBytes(HashSize);

        // تخزين: PBKDF2:base64(salt):base64(hash)
        return $"{Prefix}{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// التحقق من كلمة المرور — يدعم PBKDF2 (جديد) و SHA-256 Legacy (قديم).
    /// عند نجاح التحقق بالصيغة القديمة يُعلم المُستدعي لترقية التشفير.
    /// </summary>
    public static bool VerifyPassword(string inputPassword, string storedPassword)
    {
        if (string.IsNullOrEmpty(inputPassword) || string.IsNullOrEmpty(storedPassword)) return false;

        // الصيغة الجديدة PBKDF2
        if (storedPassword.StartsWith(Prefix))
        {
            return VerifyPbkdf2(inputPassword, storedPassword);
        }

        // الصيغة القديمة SHA-256 — للتوافق أثناء الانتقال فقط
        return VerifyLegacySha256(inputPassword, storedPassword);
    }

    /// <summary>
    /// هل كلمة المرور مشفرة بالصيغة الجديدة؟ (للكشف عن الحسابات التي تحتاج ترقية)
    /// </summary>
    public static bool IsModernHash(string storedPassword)
        => !string.IsNullOrEmpty(storedPassword) && storedPassword.StartsWith(Prefix);

    // ========================= Private Helpers =========================

    private static bool VerifyPbkdf2(string inputPassword, string storedHash)
    {
        try
        {
            // صيغة: PBKDF2:base64(salt):base64(hash)
            string withoutPrefix = storedHash.Substring(Prefix.Length);
            int colonIdx = withoutPrefix.IndexOf(':');
            if (colonIdx < 0) return false;

            byte[] salt    = Convert.FromBase64String(withoutPrefix.Substring(0, colonIdx));
            byte[] storedH = Convert.FromBase64String(withoutPrefix.Substring(colonIdx + 1));

            using var pbkdf2 = new Rfc2898DeriveBytes(inputPassword, salt, Iterations, HashAlgorithmName.SHA256);
            byte[] computedH = pbkdf2.GetBytes(HashSize);

            return CryptographicOperations.FixedTimeEquals(computedH, storedH);
        }
        catch
        {
            return false;
        }
    }

    private static bool VerifyLegacySha256(string inputPassword, string storedHash)
    {
        try
        {
            using var sha256 = SHA256.Create();
            byte[] salt     = Encoding.UTF8.GetBytes("AlMaqbooli_POS_Salt_2026");
            byte[] passBytes = Encoding.UTF8.GetBytes(inputPassword);
            byte[] combined = new byte[salt.Length + passBytes.Length];
            Buffer.BlockCopy(salt, 0, combined, 0, salt.Length);
            Buffer.BlockCopy(passBytes, 0, combined, salt.Length, passBytes.Length);
            byte[] hash = sha256.ComputeHash(combined);
            return Convert.ToBase64String(hash) == storedHash;
        }
        catch
        {
            return false;
        }
    }
}

