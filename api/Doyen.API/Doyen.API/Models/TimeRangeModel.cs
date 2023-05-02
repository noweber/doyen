using System.ComponentModel.DataAnnotations;

namespace Doyen.API.Models
{
    public class TimeRangeModel
    {
        public static string DATE_TIME_FORMAT { get; private set; } = "yyyy-MM-dd";

        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:DATE_TIME_FORMAT}")]
        public DateTime GreaterThan { get; set; } = DateTime.Now.AddYears(-5);


        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:DATE_TIME_FORMAT}")]
        public DateTime LessThan { get; set; } = DateTime.Now;
    }
}
