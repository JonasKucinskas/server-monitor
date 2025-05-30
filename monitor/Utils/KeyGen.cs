using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Crypto.Tls;
using System;
using System.IO;
using System.Text;

public class KeyGen
{
    public static void GenerateKeyPair()
    {
        var keyPairGenerator = new RsaKeyPairGenerator();
        keyPairGenerator.Init(new KeyGenerationParameters(new SecureRandom(), 2048));
        AsymmetricCipherKeyPair keyPair = keyPairGenerator.GenerateKeyPair();

        // convert to pem format
        var privateKeyPem = new StringBuilder();
        using (var stringWriter = new StringWriter(privateKeyPem))
        {
            var pemWriter = new PemWriter(stringWriter);
            pemWriter.WriteObject(keyPair.Private);
        }

        File.WriteAllText("privateKey.pem", privateKeyPem.ToString());

        // convert to open ssh
        var publicKey = (RsaKeyParameters)keyPair.Public;
        var publicKeyModulus = Convert.ToBase64String(publicKey.Modulus.ToByteArrayUnsigned());
        var publicExponent = Convert.ToBase64String(publicKey.Exponent.ToByteArrayUnsigned());
        var publicKeyString = $"ssh-rsa {publicKeyModulus} {publicExponent}";

        File.WriteAllText("publicKey.pub", publicKeyString);

        Console.WriteLine("RSA 2048-bit key pair (rsa-sha2-256) generated and saved.");
    }
}
