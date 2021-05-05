namespace DGCTest.Constants
{
    /// <summary>
    /// CBOR claims
    /// </summary>
    public static class ClaimConstants
    {
        /// <summary>
        /// Issuer of the DGC
        /// </summary>
        public static int iss = 1;

        /// <summary>
        /// Expiring Date of the DGC
        /// </summary>
        public static int exp = 4;

        /// <summary>
        /// Issuing Date of the DGC
        /// </summary>
        public static int iat = 6;

        /// <summary>
        /// Payload of the DGC
        /// </summary>
        public static int hcert = -260;

        /// <summary>
        /// DGC data must be in MAP with key 1
        /// </summary>
        public static int hcertData = 1;
    }
}
