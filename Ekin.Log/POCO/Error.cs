using Ekin.Log.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekin.Log
{
    public class Error : ILogItem
    {
        public string Id { get; set; }
        public DateTime Time { get; set; }
        public string Class { get; set; }
        public string Function { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public object Data { get; set; }

        public Error() { }
    }
}
