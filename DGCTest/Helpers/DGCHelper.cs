using DGCTest.Constants;
using DGCTest.Models;
using Ionic.Zlib;
using Newtonsoft.Json.Linq;
using PeterO.Cbor;
using System;
using System.IO;
using System.Text;
using ZXing.QrCode.Internal;

namespace DGCTest.Helpers
{
    /// <summary>
    /// Operations over Digital Green Certificate 
    /// </summary>
    public static class DGCHelper
    {
        /// <summary>
        /// Generate Digital Green Certificate
        /// </summary>
        /// <param name="data">Certificate data</param>
        /// <param name="validFrom">Certificate issue date</param>
        /// <param name="validTill">Certificate expire date</param>
        /// <returns>Generated DGC to be encoded in 2D code</returns>
        public static string GenerateDGC(string data, DateTime validFrom, DateTime validTill)
        {
            DateTimeOffset oValidFrom = new DateTimeOffset(validFrom);
            DateTimeOffset oValidTill = new DateTimeOffset(validTill);


            CBORObject cbHcert = CBORObject.FromJSONString(data);
            CBORObject cbHcertMap = CBORObject.NewMap()
                .Set(ClaimConstants.hcertData, cbHcert);
            CBORObject cbPayload = CBORObject.NewMap()
                .Set(ClaimConstants.iss, "BG")
                .Set(ClaimConstants.iat, oValidFrom.ToUnixTimeSeconds())
                .Set(ClaimConstants.exp, oValidTill.ToUnixTimeSeconds())
                .Set(ClaimConstants.hcert, cbHcertMap);

            CBORObject cosePayload = CBORObject.FromObject(cbPayload.EncodeToBytes());

            CBORObject protectedMap = CBORObject.NewMap()
                .Set(HeaderConstants.alg, CryptoAlgorithms.ES256);

            CBORObject unprotectedMap = CBORObject.NewMap()
                .Set(HeaderConstants.kid, CryptoHelper.GetKeyIdentifier());

            CBORObject coseProtected = CBORObject.FromObject(protectedMap.EncodeToBytes());

            CBORObject signature = CryptoHelper.GetSignature(coseProtected, cosePayload);

            CBORObject cose = CBORObject.NewArray()
                .Add(coseProtected)
                .Add(unprotectedMap)
                .Add(cosePayload)
                .Add(signature)
                .WithTag(TagConstants.COSE_Sign1);

            byte[] res = ZlibStream.CompressBuffer(cose.EncodeToBytes());

            GenerateTestData(data, cbHcert, cose, res);

            return $"{HCVersion.HC1}:{Base45Encoding.Encode(res)}";
        }

        public static void TestMSData()
        {
            string stCbor = "a4041a608fa985061a608d068501624154390103a101a4617682aa62646e01626d616d4f52472d313030303330323135627670674a3037425830336264746a323032312d30322d313662636f624247626369782175726e3a757663693a31303a42473a3350334b364635474c5734364c525a542348626d706c45552f312f32302f31353238626973724d696e6973747279206f66204865616c74686273640262746769383430353339303036aa62646e02626d616d4f52472d313030303330323135627670674a3037425830336264746a323032312d30332d303962636f624247626369782175726e3a757663693a31303a42473a3350334b364635474c5734364c525a542348626d706c45552f312f32302f31353238626973724d696e6973747279206f66204865616c74686273640262746769383430353339303036636e616da463666e74665045544b4f5662666e6cd09fd095d0a2d09ad09ed09263676e746e5354414d4f3c47454f524749455662676e781bd0a1d0a2d090d09cd09e20d093d095d09ed0a0d093d098d095d0926376657265312e302e3063646f626a313937382d30312d3236";
            byte[] baCbor = Utils.StringToByteArray(stCbor);
            CBORObject obCbor = CBORObject.DecodeFromBytes(baCbor);

            string stCose = "d2844da2044859339762b612e3ba0126a05901aaa4041a608fa985061a608d068501624154390103a101a4617682aa62646e01626d616d4f52472d313030303330323135627670674a3037425830336264746a323032312d30322d313662636f624247626369782175726e3a757663693a31303a42473a3350334b364635474c5734364c525a542348626d706c45552f312f32302f31353238626973724d696e6973747279206f66204865616c74686273640262746769383430353339303036aa62646e02626d616d4f52472d313030303330323135627670674a3037425830336264746a323032312d30332d303962636f624247626369782175726e3a757663693a31303a42473a3350334b364635474c5734364c525a542348626d706c45552f312f32302f31353238626973724d696e6973747279206f66204865616c74686273640262746769383430353339303036636e616da463666e74665045544b4f5662666e6cd09fd095d0a2d09ad09ed09263676e746e5354414d4f3c47454f524749455662676e781bd0a1d0a2d090d09cd09e20d093d095d09ed0a0d093d098d095d0926376657265312e302e3063646f626a313937382d30312d3236584052e086c4f600592419795b7eca6404d20b5c41e787fabed2f86cdf7f74abb62574f496f6580afafe62e4ad1c8fbfd4e9c146dafb28c372530c5f9f736ade2707";
            byte[] baCose = Utils.StringToByteArray(stCose);
            CBORObject obCose = CBORObject.DecodeFromBytes(baCose);

            CBORObject obProtected = CBORObject.DecodeFromBytes(obCose[0].GetByteString());
            CBORObject obPayload = CBORObject.DecodeFromBytes(obCose[2].GetByteString());
        }

        private static void GenerateTestData(string data, CBORObject cbHcert, CBORObject cose, byte[] res)
        {
            string base45enc = Base45Encoding.Encode(res);
            JObject testData = new JObject();
            testData.Add("JSON", data);
            testData.Add("CBOR", Utils.ToHexString(cbHcert.EncodeToBytes()));
            testData.Add("COSE", Utils.ToHexString(cose.EncodeToBytes()));
            testData.Add("BASE45", base45enc);
            testData.Add("PREFIX", $"{HCVersion.HC1}:{base45enc}");
            testData.Add("EXPECTEDDECODE", true);
            testData.Add("EXPECTEDVERIFY", true);
            testData.Add("VALIDATIONCLOCK", DateTime.UtcNow);

            string dt = testData.ToString();

            File.WriteAllText("test_vac.json", dt, Encoding.UTF8);
        }

        /// <summary>
        /// Gets CBOR to be signed
        /// </summary>
        /// <param name="payload">CBOR Payload</param>
        /// <param name="keyIdentifier">Public key identifier</param>
        /// <returns></returns>
        public static CBORObject GetDataToSign(byte[] payload, byte[] keyIdentifier)
        {
            CBORObject protectedMap = CBORObject.NewMap();
            protectedMap.Set(HeaderConstants.alg, CryptoAlgorithms.ES256);
            protectedMap.Set(HeaderConstants.kid, keyIdentifier);

            CBORObject cborProtected = CBORObject.FromObject(protectedMap.EncodeToBytes());
            CBORObject signObj = CBORObject.NewArray();
            signObj.Add(cborProtected);
            signObj.Add(payload);

            return signObj;
        }

        /// <summary>
        /// Validates Digital Green Certificate
        /// </summary>
        /// <param name="dgc">Digital Green Certificate to be validated</param>
        /// <returns>Validated data</returns>
        public static DGCValidationModel Validate(string dgc)
        {
            DGCValidationModel result = new DGCValidationModel() { IsValid = false };
            string prefix = $"{HCVersion.HC1}:";

            if (dgc.StartsWith(prefix) == false)
            {
                result.ErrorMessage = "Invalid QR code context prefix";

                return result;
            }

            byte[] data = Base45Encoding.Decode(dgc.Substring(prefix.Length));
            byte[] unzipped = ZlibStream.UncompressBuffer(data);

            CBORObject cose = CBORObject.DecodeFromBytes(unzipped);

            if (cose.MostOuterTag.ToInt32Checked() != TagConstants.COSE_Sign1)
            {
                result.ErrorMessage = $"Invalid Tag. Only COSE {SignatureContext.Signature1} structures are supported";

                return result;
            }

            CBORObject header = CBORObject.DecodeFromBytes(cose[0].GetByteString());
            CBORObject payload = CBORObject.DecodeFromBytes(cose[2].GetByteString());
            long today = new DateTimeOffset(DateTime.Today).ToUnixTimeSeconds();

            if (today < payload[ClaimConstants.iat].AsInt64Value() ||
                today > payload[ClaimConstants.exp].AsInt64Value())
            {
                result.ErrorMessage = "Certificate has expired or not yet active.";

                return result;
            }

            result.IsValid = CryptoHelper.ValidateSignature(cose);

            if (result.IsValid == false)
            {
                result.ErrorMessage = "Invalid Signature";

                return result;
            }

            result.Names = payload[ClaimConstants.hcert][ClaimConstants.hcertData]["nam"].ToObject<Nam>();
            result.DateOfBirth = DateTime.Parse(payload[ClaimConstants.hcert][ClaimConstants.hcertData]["dob"].AsString());

            return result;
        }

        /// <summary>
        /// Generates 2D code containing DGC
        /// </summary>
        /// <param name="text">DGC to be encoded</param>
        public static void GenerateQR(string text)
        {
            var qrCodeWriter = new ZXing.BarcodeWriterPixelData
            {
                Format = ZXing.BarcodeFormat.QR_CODE,
                Options = new ZXing.QrCode.QrCodeEncodingOptions
                {
                    Height = 250,
                    Width = 250,
                    
                    // 25% error correction
                    ErrorCorrection = ErrorCorrectionLevel.Q
                }
            };
            var pixelData = qrCodeWriter.Write(text);
            // creating a bitmap from the raw pixel data; if only black and white colors are used it makes no difference    
            // that the pixel data ist BGRA oriented and the bitmap is initialized with RGB    
            using (var bitmap = new System.Drawing.Bitmap(pixelData.Width, pixelData.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb))
            using (var ms = new MemoryStream())
            {
                var bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, pixelData.Width, pixelData.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                try
                {
                    // we assume that the row stride of the bitmap is aligned to 4 byte multiplied by the width of the image    
                    System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0, pixelData.Pixels.Length);
                }
                finally
                {
                    bitmap.UnlockBits(bitmapData);
                }
                
                // save to stream as PNG    
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

                File.WriteAllBytes(@"qr.png", ms.ToArray());
            }
        }
    }
}
