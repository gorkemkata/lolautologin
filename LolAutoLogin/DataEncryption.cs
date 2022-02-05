using System;
using System.Security.Cryptography;
using System.Text;

namespace LolAutoLogin
{
    public static class DataEncryption
    {
        static String Entropy = "g3272q6t1h'^~5yı½9nubfmj|s£>#$4öaöç23ö4tacf94536ç#$]}½juı0qh98*ge";

        public static String Encrypt(String toEncrypt)
        {
            byte[] encryptedBytes = ProtectedData.Protect(
                Encoding.UTF8.GetBytes(toEncrypt),
                Encoding.UTF8.GetBytes(Entropy),
                DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedBytes);
        }

        public static String Decrypt(String toDecrypt)
        {
            byte[] decryptedBytes = ProtectedData.Unprotect(
                Convert.FromBase64String(toDecrypt),
                Encoding.UTF8.GetBytes(Entropy),
                DataProtectionScope.CurrentUser);

            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}
