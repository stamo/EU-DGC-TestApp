using Newtonsoft.Json.Converters;

namespace DGCTest.Converters
{
    /// <summary>
    /// Converting dates to specific format
    /// yyyy-MM-dd
    /// </summary>
    public class DateConverter : IsoDateTimeConverter
    {
        /// <summary>
        /// Setting base DateTimeFormat property
        /// </summary>
        public DateConverter()
        {
            base.DateTimeFormat = "yyyy-MM-dd";
        }
        
    }
}