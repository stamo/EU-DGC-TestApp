using Newtonsoft.Json;
using System;

namespace DGCTest.Models
{
    /// <summary>
    /// Digital Green Certificate Data Model
    /// </summary>
    public class DGCModel
    {
        /// <summary>
        /// Version of the schema, according to Semantic versioning 
        /// (ISO, https://semver.org/ version 2.0.0 or newer) 
        /// </summary>
        [JsonRequired]
        public string ver { get; set; }

        /// <summary>
        /// Surname(s), given name(s) - in that order
        /// </summary>
        [JsonRequired]
        public Nam nam { get; set; }

        /// <summary>
        /// Date of Birth of the person addressed in the DGC. 
        /// ISO 8601 date format restricted to range 1900-2099
        /// </summary>
        [JsonRequired]
        public DateTime dob { get; set; }

        /// <summary>
        /// Recovery Group
        /// </summary>
        public R[] r { get; set; }

        /// <summary>
        /// Test Group
        /// </summary>
        public T[] t { get; set; }

        /// <summary>
        /// Vaccination Group
        /// </summary>
        public V[] v { get; set; }
    }

    /// <summary>
    /// Person name: Surname(s), given name(s) - in that order
    /// </summary>
    public class Nam
    {
        /// <summary>
        /// The family or primary name(s) of the 
        /// person addressed in the certificate
        /// </summary>
        public string fn { get; set; }

        /// <summary>
        /// The given name(s) of the person 
        /// addressed in the certificate
        /// </summary>
        public string gn { get; set; }

        /// <summary>
        /// The family name(s) of the person transliterated
        /// Patern: ^[A-Z<]*$ 
        /// Example: DCERVENKOVA<PANKLOVA
        /// </summary>
        [JsonRequired]
        public string fnt { get; set; }

        /// <summary>
        /// The given name(s) of the person transliterated
        /// Patern: ^[A-Z<]*$
        /// Example: JIRINA<MARIA<ALENA
        /// </summary>
        public string gnt { get; set; }
    }
    public class R
    {
        /// <summary>
        /// disease or agent targeted
        /// </summary>
        [JsonRequired]
        public string tg { get; set; }

        /// <summary>
        /// ISO 8601 Date of First Positive Test Result
        /// </summary>
        [JsonRequired]
        public DateTime fr { get; set; }

        /// <summary>
        /// Country of Test
        /// </summary>
        [JsonRequired]
        public string co { get; set; }

        /// <summary>
        /// Certificate Issuer
        /// </summary>
        [JsonRequired]
        public string @is { get; set; }

        /// <summary>
        /// ISO 8601 Date: Certificate Valid From
        /// </summary>
        [JsonRequired]
        public DateTime df { get; set; }

        /// <summary>
        /// Certificate Valid Until
        /// </summary>
        [JsonRequired]
        public DateTime du { get; set; }

        /// <summary>
        /// Unique Certificate Identifier, UVCI
        /// </summary>
        [JsonRequired]
        public string ci { get; set; }
    }

    public class T
    {
        /// <summary>
        /// disease or agent targeted
        /// </summary>
        [JsonRequired]
        public string tg { get; set; }

        /// <summary>
        /// Type of Test
        /// </summary>
        [JsonRequired]
        public string tt { get; set; }

        /// <summary>
        /// NAA Test Name
        /// </summary>
        public string nm { get; set; }

        /// <summary>
        /// RAT Test name and manufacturer
        /// </summary>
        public string ma { get; set; }

        /// <summary>
        /// Test Result
        /// </summary>
        [JsonRequired]
        public string tr { get; set; }

        /// <summary>
        /// Date/Time of Sample Collection
        /// </summary>
        [JsonRequired]
        public DateTime sc { get; set; }

        /// <summary>
        /// Date/Time of Test Result
        /// </summary>
        public DateTime dr { get; set; }

        /// <summary>
        /// Testing Centre
        /// </summary>
        [JsonRequired]
        public string tc { get; set; }

        /// <summary>
        /// Country of Test
        /// </summary>
        [JsonRequired]
        public string co { get; set; }

        /// <summary>
        /// Certificate Issuer
        /// </summary>
        [JsonRequired]
        public string @is { get; set; }

        /// <summary>
        /// Unique Certificate Identifier, UVCI
        /// </summary>
        [JsonRequired]
        public string ci { get; set; }
    }

    /// <summary>
    /// Vaccination Entry
    /// </summary>
    public class V
    {
        /// <summary>
        /// disease or agent targeted
        /// </summary>
        [JsonRequired]
        public string tg { get; set; }

        /// <summary>
        /// vaccine or prophylaxis
        /// </summary>
        [JsonRequired]
        public string vp { get; set; }

        /// <summary>
        /// vaccine medicinal product
        /// </summary>
        [JsonRequired]
        public string mp { get; set; }

        /// <summary>
        /// Marketing Authorization Holder - 
        /// if no MAH present, then manufacturer
        /// </summary>
        [JsonRequired]
        public string ma { get; set; }

        /// <summary>
        /// Dose Number
        /// </summary>
        [JsonRequired]
        public int dn { get; set; }

        /// <summary>
        /// Total Series of Doses
        /// </summary>
        [JsonRequired]
        public int sd { get; set; }

        /// <summary>
        /// Date of Vaccination
        /// </summary>
        [JsonRequired]
        public DateTime dt { get; set; }

        /// <summary>
        /// Country of Vaccination
        /// </summary>
        [JsonRequired]
        public string co { get; set; }

        /// <summary>
        /// Certificate Issuer
        /// </summary>
        [JsonRequired]
        public string @is { get; set; }

        /// <summary>
        /// Unique Certificate Identifier: UVCI
        /// </summary>
        [JsonRequired]
        public string ci { get; set; }
    }
}