using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

public class CertLoader
{
    public X509Certificate2 LoadCertificate(string certificatePath, string privateKeyPath, string password)
    {
        // Charger le certificat public à partir du fichier PEM
        var certificatePem = File.ReadAllText(certificatePath);

        // Charger le certificat via X509Certificate2Loader
        var certLoader = new X509Certificate2Collection();
        certLoader.ImportFromPem(certificatePem);

        // Charger la clé privée à partir du fichier PEM
        var privateKeyPem = File.ReadAllText(privateKeyPath);

        // Supprimer les balises de la clé privée (si elles existent) 
        // ==> SANS CETTE PARTIE ON RENCONTRE UNE INCOMPATIBILITÉ AVEC BASE64
        privateKeyPem = privateKeyPem.Replace("-----BEGIN ENCRYPTED PRIVATE KEY-----", "").Replace("-----END ENCRYPTED PRIVATE KEY-----", "");

        // Convertir la clé privée en tableau de bytes
        var privateKeyBytes = Convert.FromBase64String(privateKeyPem);

        // Créer un RSA et importer la clé privée
        var rsaKey = RSA.Create();
        if (!string.IsNullOrEmpty(password))
        {
            // Si la clé est protégée par mot de passe, déchiffrer
            rsaKey.ImportEncryptedPkcs8PrivateKey(password, privateKeyBytes, out _);
        }
        else
        {
            // Si la clé n'est pas protégée, l'importer directement
            rsaKey.ImportPkcs8PrivateKey(privateKeyBytes, out _);
        }

        // Attacher la clé privée au certificat
        var certificate = certLoader[0];
        return certificate.CopyWithPrivateKey(rsaKey);
    }
}
