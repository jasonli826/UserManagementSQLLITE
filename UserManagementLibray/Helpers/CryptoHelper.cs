using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
namespace UserManagementlibrary.Helpers
{
        public static class CryptoHelper
        {
            private static readonly string Password = "CSC";
            public static string Encrypt(string textToBeEncrypted)
            {
                using (TripleDESCryptoServiceProvider trip = new TripleDESCryptoServiceProvider())
                {
                    byte[] plainText = Encoding.Unicode.GetBytes(textToBeEncrypted);
                    byte[] salt = Encoding.ASCII.GetBytes(Password.Length.ToString());
                    PasswordDeriveBytes secretKey = new PasswordDeriveBytes(Password, salt);

                    ICryptoTransform encryptor = trip.CreateEncryptor(secretKey.GetBytes(16), secretKey.GetBytes(16));
                    using (MemoryStream memoryStream = new MemoryStream())
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(plainText, 0, plainText.Length);
                        cryptoStream.FlushFinalBlock();
                        return Convert.ToBase64String(memoryStream.ToArray());
                    }
                }
            }
            public static string Decrypt(string textToBeDecrypted)
            {
                try
                {
                    using (TripleDESCryptoServiceProvider trip = new TripleDESCryptoServiceProvider())
                    {
                        byte[] encryptedData = Convert.FromBase64String(textToBeDecrypted);
                        byte[] salt = Encoding.ASCII.GetBytes(Password.Length.ToString());
                        PasswordDeriveBytes secretKey = new PasswordDeriveBytes(Password, salt);

                        ICryptoTransform decryptor = trip.CreateDecryptor(secretKey.GetBytes(16), secretKey.GetBytes(16));
                        using (MemoryStream memoryStream = new MemoryStream(encryptedData))
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            byte[] plainText = new byte[encryptedData.Length];
                            int decryptedCount = cryptoStream.Read(plainText, 0, plainText.Length);
                            return Encoding.Unicode.GetString(plainText, 0, decryptedCount);
                        }
                    }
                }
                catch
                {
                    // Return original text if decryption fails
                    return textToBeDecrypted;
                }
            }
        }
    }


