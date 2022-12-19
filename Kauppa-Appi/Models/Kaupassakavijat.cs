using System;
using System.Collections.Generic;
using System.Text;

namespace Kauppa_Appi.Models
{
    internal class Kaupassakavijat
{
        public int IdKavija { get; set; }
        public string Nimi { get; set; } /*= null!*/
        public DateTime? CreatedAt { get; set; }
        public bool Active { get; set; }

        public virtual ICollection<Timesheet> Timesheets { get; set; }
    }
}
