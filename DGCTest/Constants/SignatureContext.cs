namespace DGCTest.Constants
{
    /// <summary>
    /// A text string identifying the context of the signature
    /// </summary>
    public static class SignatureContext
    {
        /// <summary>
        /// signatures using the COSE_Signature structure
        /// </summary>
        public static string Signature = "Signature";

        /// <summary>
        /// signatures using the COSE_Sign1 structure
        /// </summary>
        public static string Signature1 = "Signature1";

        /// <summary>
        /// signatures used as counter signature attributes
        /// </summary>
        public static string CounterSignature = "CounterSignature";
    }
}
