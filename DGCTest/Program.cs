using DGCTest.Helpers;
using DGCTest.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DGCTest
{
    class Program
    {
        private readonly static string PathToData = @"Data/";
        static async Task Main(string[] args)
        {
            /* --------- Create DGC ------------ */
            string data = await File.ReadAllTextAsync($"{PathToData}vac-stamo-min.json");
            DateTime validFrom = DateTime.Today;
            DateTime validTill = validFrom.AddYears(1);
            string dgc = DGCHelper.GenerateDGC(data, validFrom, validTill);
            DGCHelper.GenerateQR(dgc);

            /* --------- Validate DGC ------------ */
            DGCValidationModel validationResult = DGCHelper.Validate(dgc);

            Console.WriteLine(JsonConvert.SerializeObject(validationResult, Formatting.Indented));
        }
    }
}
