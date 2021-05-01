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
            CBORObject cbPayload = CBORObject.NewMap()
                .Set(ClaimConstants.iss, "BG")
                .Set(ClaimConstants.iat, oValidFrom.ToUnixTimeSeconds())
                .Set(ClaimConstants.exp, oValidTill.ToUnixTimeSeconds())
                .Set(ClaimConstants.hcert, cbHcert);

            CBORObject cosePayload = CBORObject.FromObject(cbPayload.EncodeToBytes());

            CBORObject protectedMap = CBORObject.NewMap()
                .Set(HeaderConstants.alg, CryptoAlgorithms.ES256)
                .Set(HeaderConstants.kid, CryptoHelper.GetKeyIdentifier());

            CBORObject coseProtected = CBORObject.FromObject(protectedMap.EncodeToBytes());

            CBORObject signature = CryptoHelper.GetSignature(coseProtected, cosePayload);

            CBORObject cose = CBORObject.NewArray()
                .Add(coseProtected)
                .Add(cosePayload)
                .Add(signature)
                .Add(new { });

            byte[] res = ZlibStream.CompressBuffer(cose.EncodeToBytes());

            GenerateTestData(data, cbPayload, cose, res);

            return $"{HCVersion.HC1}:{Base45Encoding.Encode(res)}";
        }

        private static void GenerateTestData(string data, CBORObject cbPayload, CBORObject cose, byte[] res)
        {
            string base45enc = Base45Encoding.Encode(res);
            JObject testData = new JObject();
            testData.Add("JSON", data);
            testData.Add("CBOR", Utils.ToHexString(cbPayload.EncodeToBytes()));
            testData.Add("COSE", Utils.ToHexString(cose.EncodeToBytes()));
            testData.Add("BASE45", base45enc);
            testData.Add("PREFIX", $"{HCVersion.HC1}:{base45enc}");
            testData.Add("EXPECTEDDECODE", true);
            testData.Add("EXPECTEDVERIFY", true);

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
            CBORObject header = CBORObject.DecodeFromBytes(cose[0].GetByteString());

            result.IsValid = CryptoHelper.ValidateSignature(cose);

            if (result.IsValid == false)
            {
                result.ErrorMessage = "Invalid Signature";
            }

            CBORObject payload = CBORObject.DecodeFromBytes(cose[1].GetByteString());

            result.Names = payload[ClaimConstants.hcert]["nam"].ToObject<Nam>();
            result.DateOfBirth = DateTime.Parse(payload[ClaimConstants.hcert]["dob"].AsString());

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
