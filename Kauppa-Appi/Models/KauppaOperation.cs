using System;
using System.Collections.Generic;
using System.Text;

namespace Kauppa_Appi.Models
{
    internal class KauppaOperation
{
    public int KavijaID { get; set; }
    public int KauppaOstosID { get; set; }
    public string OperationType { get; set; }
    public string Comment { get; set; }
    public string Latitude { get; set; }
    public string Longitude { get; set; }
}
}
