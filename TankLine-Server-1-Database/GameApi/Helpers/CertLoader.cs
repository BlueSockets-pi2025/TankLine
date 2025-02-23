using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

public class CertLoader
{
    public X509Certificate2 LoadCertificate(string certificatePath, string privateKeyPath, string password)
    {
        // Load the public certificate from the PEM file
        var certificatePem = File.ReadAllText(certificatePath);

        // Load the certificate using X509Certificate2Loader
        var certLoader = new X509Certificate2Collection();
        certLoader.ImportFromPem(certificatePem);

        // Load the private key from the PEM file
        var privateKeyPem = File.ReadAllText(privateKeyPath);

        // Remove the tags from the private key (if they exist) 
        // ==> WITHOUT THIS PART WE ENCOUNTER A BASE64 INCOMPATIBILITY
        privateKeyPem = privateKeyPem.Replace("-----BEGIN ENCRYPTED PRIVATE KEY-----", "").Replace("-----END ENCRYPTED PRIVATE KEY-----", "");

        // Convert the private key to a byte array
        var privateKeyBytes = Convert.FromBase64String(privateKeyPem);

        // Create an RSA and import the private key
        var rsaKey = RSA.Create();
        if (!string.IsNullOrEmpty(password))
        {
            // If the key is password-protected, decrypt it
            rsaKey.ImportEncryptedPkcs8PrivateKey(password, privateKeyBytes, out _);
        }
        else
        {
            // If the key is not password-protected, import it directly
            rsaKey.ImportPkcs8PrivateKey(privateKeyBytes, out _);
        }

        // Attach the private key to the certificate
        var certificate = certLoader[0];
        return certificate.CopyWithPrivateKey(rsaKey);
    }
}
