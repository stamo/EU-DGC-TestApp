using DGCTest.Converters;
using Newtonsoft.Json;
using System;

namespace DGCTest.Models
{
    /// <summary>
    /// Digital Green Certificate Validation model
    /// </summary>
    public class DGCValidationModel
    {
        /// <summary>
        /// Is DGC valid
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Person Names
        /// </summary>
        public Nam Names { get; set; }

        /// <summary>
        /// Person Date of birth
        /// </summary>
        [JsonConverter(typeof(DateConverter))]
        public DateTime DateOfBirth { get; set; }

        /// <summary>
        /// If DGC is invalid will contain the reason
        /// else null
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
