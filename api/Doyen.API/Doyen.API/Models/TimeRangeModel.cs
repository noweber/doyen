using System.ComponentModel.DataAnnotations;

namespace Doyen.API.Models
{
    public class TimeRangeModel
    {
        public DateTime GreaterThan { get; set; } = DateTime.Now.AddYears(-5);

        public DateTime LessThan { get; set; } = DateTime.Now;
    }
}
