using DGCTest.Constants;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using PeterO.Cbor;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace DGCTest.Helpers
{
    /// <summary>
    /// Cryptographic operations over DGC
    /// </summary>
    public static class CryptoHelper
    {
        /// <summary>
        /// Path to test certificates
        /// </summary>
        private readonly static string PathToCertificates = @"Certificates/";

        /// <summary>
        /// Calculates COSE signature
        /// </summary>
        /// <param name="coseProtected">COSE Protected header</param>
        /// <param name="cosePayload">COSE Payload</param>
        /// <returns></returns>
        public static CBORObject GetSignature(CBORObject coseProtected, CBORObject cosePayload)
        {
            CBORObject signObj = CBORObject.NewArray()
                .Add(SignatureContext.Signature1)
                .Add(coseProtected)
                .Add(new byte[0])
                .Add(cosePayload);

            AsymmetricCipherKeyPair keyPair = GetKeyPair();
            byte[] signature = SignData(signObj.EncodeToBytes(), keyPair.Private);
            signature = Utils.ConvertDerToConcat(signature, 32);

            return CBORObject.FromObject(signature);
        }

        /// <summary>
        /// Sign data with ECDSA256 algorithm
        /// </summary>
        /// <param name="data">Data to be signed</param>
        /// <param name="privateKey">Signer certificate Private key</param>
        /// <returns></returns>
        public static byte[] SignData(byte[] data, AsymmetricKeyParameter privateKey)
        {
            var signer = SignerUtilities.GetSigner(CryptoAlgorithms.ECDSA256);

            signer.Init(true, privateKey);

            signer.BlockUpdate(data, 0, data.Length);

            return signer.GenerateSignature();
        }

        /// <summary>
        /// Gets the Public key Identifier 
        /// Needed to identify public key in trusted keys list
        /// Used only in production
        /// </summary>
        /// <returns>First 8 bytes of certificate hash</returns>
        public static byte[] GetKeyIdentifier()
        {
            byte[] kid = null;

            var certificateString = File.ReadAllText($"{PathToCertificates}dsc-worker.pem");
            using (var textReader = new StringReader(certificateString))
            {
                X509Certificate bcCertificate = (X509Certificate)new PemReader(textReader).ReadObject();

                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] digest = sha256.ComputeHash(bcCertificate.GetEncoded());
                    kid = digest
                        .Take(8)
                        .ToArray();
                }
            }

            return kid;
        }

        /// <summary>
        /// Validates COSE signature
        /// </summary>
        /// <param name="cose">COSE object</param>
        /// <returns></returns>
        public static bool ValidateSignature(CBORObject cose, byte[]  cert)
        {
            CBORObject signData = CBORObject.NewArray()
                .Add(SignatureContext.Signature1)
                .Add(cose[0])
                .Add(new byte[0])
                .Add(cose[2]);

            byte[] data = signData.EncodeToBytes();
            byte[] signature = cose[3].GetByteString();

            CBORObject header = CBORObject.DecodeFromBytes(cose[0].GetByteString());
            string algorithm = CryptoAlgorithms.RSA256;

            if (header[HeaderConstants.alg].AsInt32() == -7)
            {
                algorithm = CryptoAlgorithms.ECDSA256;
                signature = Utils.ConvertConcatToDer(signature);
            }
            
            AsymmetricKeyParameter publicKey;

            if (cert == null)
            {
                AsymmetricCipherKeyPair keyPair = GetKeyPair();
                publicKey = keyPair.Public;
            }
            else
            {
                publicKey = GetPublicKey(cert);
            }

            

            ISigner signer = SignerUtilities.GetSigner(algorithm);
            signer.Init(false, publicKey);
            signer.BlockUpdate(data, 0, data.Length);
            return signer.VerifySignature(signature);
        }

        /// <summary>
        /// Gets Signer's certificate key pair
        /// </summary>
        /// <returns></returns>
        public static AsymmetricCipherKeyPair GetKeyPair()
        {

            AsymmetricKeyParameter privateKey, publicKey;

            var privateKeyString = File.ReadAllText($"{PathToCertificates}dsc-worker.p8");
            using (var textReader = new StringReader(privateKeyString))
            {
                // Only a private key
                privateKey = (AsymmetricKeyParameter)new PemReader(textReader).ReadObject();
            }

            var certificateString = File.ReadAllText($"{PathToCertificates}dsc-worker.pem");
            using (var textReader = new StringReader(certificateString))
            {
                // Only a private key
                Org.BouncyCastle.X509.X509Certificate bcCertificate = (X509Certificate)new PemReader(textReader).ReadObject();
                publicKey = bcCertificate.GetPublicKey();
            }

            return new AsymmetricCipherKeyPair(publicKey, privateKey);

        }

        /// <summary>
        /// Gets CBOR Digest for External signing
        /// To be used with HSM in production
        /// </summary>
        /// <param name="payload">Object to be signed</param>
        /// <param name="keyIdentifier">Public key Identifier</param>
        /// <returns></returns>
        public static byte[] GetDigest(byte[] payload, byte[] keyIdentifier)
        {
            byte[] bytesToBeSigned = DGCHelper.GetDataToSign(payload, keyIdentifier)
                .EncodeToBytes();

            IDigest digest = new Sha256Digest();

            digest.BlockUpdate(bytesToBeSigned, 0, bytesToBeSigned.Length);
            byte[] digestedMessage = new byte[digest.GetDigestSize()];
            digest.DoFinal(digestedMessage, 0);

            return digestedMessage;
        }

        /// <summary>
        /// To be implemented for External signature
        /// </summary>
        /// <param name="digest">Digest to be signed</param>
        /// <returns>Signature bytes</returns>
        static byte[] SignDigest(byte[] digest)
        {
            throw new NotImplementedException();
        }

        static AsymmetricKeyParameter GetPublicKey(byte[] cert)
        {
            X509CertificateParser certParser = new X509CertificateParser();
            Org.BouncyCastle.X509.X509Certificate certificate = certParser.ReadCertificate(cert);

            return certificate.GetPublicKey();
        }
    }
}
