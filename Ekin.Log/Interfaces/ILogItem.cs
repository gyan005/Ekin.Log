using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekin.Log.Interfaces
{
    public interface ILogItem
    {
        string Id { get; set; }
        DateTime Time { get; set; }
        string Class { get; set; }
        string Function { get; set; }
        string Message { get; set; }
        string StackTrace { get; set; }
        string FullDetails { get; }
        object Data { get; set; }
    }
}
