using System.Security.Cryptography;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class TokenCrypto
{
    public const string envFile = "/.env";
    public static EnvVariables encryptionConfig;

    //Encryption salt and password stored in environment variables: 
    private static string password;
    private static byte[] salt;

    /// <summary>
    /// Get the encryption key based on the password and salt.
    /// This key is used for AES encryption/decryption.
    /// </summary>
    /// <returns>256-bit key for AES encryption</returns>
    private static byte[] GetKey() // Used to encrypt and decrypt the token
    {
        if (string.IsNullOrEmpty(password) || salt == null)
        {
            throw new InvalidOperationException("Encryption configuration has not been loaded. Call LoadEncryptionConfig first.");
        }

        Debug.Log(GetMachineName());

        // Combine salt with machine name:
        byte[] machineSpecificSalt = CombineSaltAndMachineName(salt, GetMachineName());

        using (var kdf = new Rfc2898DeriveBytes(password, machineSpecificSalt, 100_000, HashAlgorithmName.SHA256))
        {
            return kdf.GetBytes(32); // 256-bit key
        }
    }

    /// <summary>
    /// Combine the salt with the machine name to create a unique key for this machine.
    /// This prevents the refresh token from being stolen and used on another machine.
    /// </summary>
    /// <param name="salt">The salt used for encryption</param>, <param name="machineName">The machine name</param>
    private static byte[] CombineSaltAndMachineName(byte[] salt, string machineName)
    {
        byte[] machineNameBytes = Encoding.UTF8.GetBytes(machineName);
        byte[] combined = new byte[salt.Length + machineNameBytes.Length];

        Buffer.BlockCopy(salt, 0, combined, 0, salt.Length);
        Buffer.BlockCopy(machineNameBytes, 0, combined, salt.Length, machineNameBytes.Length);

        return combined;
    }

    /// <summary>
    /// Encrypt the given plain text using AES encryption.
    /// The IV is generated randomly for each encryption.
    /// The encrypted data is returned as a base64 string.
    /// </summary>
    /// <param name="plainText">The text to encrypt</param>
    /// <returns>Base64 encoded encrypted text</returns>
    public static string Encrypt(string plainText)
    {
        using (AesManaged aes = new AesManaged())
        {
            aes.Key = GetKey();
            aes.GenerateIV(); // New IV for every encryption
            aes.Padding = PaddingMode.PKCS7;
            aes.Mode = CipherMode.CBC;

            ICryptoTransform encryptor = aes.CreateEncryptor();
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            // Concat IV + cipher text
            byte[] result = new byte[aes.IV.Length + cipherBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);

            return Convert.ToBase64String(result);
        }
    }

    /// <summary>
    /// Decrypt the given base64 encoded encrypted text using AES decryption.
    /// The IV is extracted from the encrypted data.
    /// </summary>
    /// <param name="encryptedBase64">Base64 encoded encrypted text</param>
    /// <returns>Decrypted plain text</returns>
    public static string Decrypt(string encryptedBase64)
    {
        byte[] fullData = Convert.FromBase64String(encryptedBase64);

        using (AesManaged aes = new AesManaged())
        {
            aes.Key = GetKey();
            aes.Padding = PaddingMode.PKCS7;
            aes.Mode = CipherMode.CBC;

            byte[] iv = new byte[16];
            Buffer.BlockCopy(fullData, 0, iv, 0, iv.Length);
            aes.IV = iv;

            byte[] cipherText = new byte[fullData.Length - iv.Length];
            Buffer.BlockCopy(fullData, iv.Length, cipherText, 0, cipherText.Length);

            ICryptoTransform decryptor = aes.CreateDecryptor();
            byte[] plainBytes = decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);

            return Encoding.UTF8.GetString(plainBytes);
        }
    }
    
    /// <summary>
    /// Load the encryption configuration from the `.env` file and store it in the variable `encryptionConfig`
    /// </summary>
    public static IEnumerator LoadEncryptionConfig(MonoBehaviour monoBehaviour)
    {

        #if UNITY_ANDROID || UNITY_IOS
            Debug.Log("Running on Android. Attempting to load .env in token crypto file using UnityWebRequest.");
            string path = System.IO.Path.Combine(Application.streamingAssetsPath, ".env");
            yield return monoBehaviour.StartCoroutine(LoadEncryptionConfigForAndroid(path));

        #else 
            string jsonEnv = File.ReadAllText(Application.streamingAssetsPath + envFile);
            encryptionConfig = JsonUtility.FromJson<EnvVariables>(jsonEnv);
        #endif

        Debug.Log("Encryption config:" + JsonUtility.ToJson(encryptionConfig));

        if (encryptionConfig == null || string.IsNullOrEmpty(encryptionConfig.ENCRYPTION_PASSWORD) || string.IsNullOrEmpty(encryptionConfig.ENCRYPTION_SALT))
        {
                throw new InvalidOperationException("Invalid encryption configuration. Ensure the `.env` file contains valid ENCRYPTION_PASSWORD and ENCRYPTION_SALT.");
        }

        password = encryptionConfig.ENCRYPTION_PASSWORD;
        salt = Convert.FromBase64String(encryptionConfig.ENCRYPTION_SALT);

        yield return null;
    }

    private static IEnumerator LoadEncryptionConfigForAndroid(string envFilePath)
    {
        UnityWebRequest request = UnityWebRequest.Get(envFilePath);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            string envContent = request.downloadHandler.text;
            encryptionConfig = JsonUtility.FromJson<EnvVariables>(envContent);
            Debug.Log("Encryption configuration successfully loaded on Android.");
        }
        else
        {
            Debug.LogError("Failed to load .env file on Android (token crypto): " + request.error);
        }
    }

    /// <summary>
    /// Get the machine name of the current environment to prevent steal of refresh token
    /// </summary>
    /// <returns>The machine name</returns>
    public static string GetMachineName()
    {
        return Environment.MachineName;
    }

}
