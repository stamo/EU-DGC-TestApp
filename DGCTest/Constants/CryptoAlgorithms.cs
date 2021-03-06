namespace DGCTest.Constants
{
    /// <summary>
    /// Used cryptographic algorithms
    /// </summary>
    public static class CryptoAlgorithms
    {
        /// <summary>
        /// -7/-37 (ES256)
        /// </summary>
        public static int ES256 = -7;

        /// <summary>
        /// BouncyCastle signature algorithm ECDSA256
        /// </summary>
        public static string ECDSA256 = "SHA-256withECDSA";

        /// <summary>
        /// BouncyCastle signature algorithm RSA256
        /// </summary>
        public static string RSA256 = "SHA256withRSA/PSS";
    }
}
