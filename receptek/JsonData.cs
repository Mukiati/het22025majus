using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace receptek
{
    public class JsonData
    {
        public int id { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public DateTime createdAt { get; set; }
        public string message { get; set; }
        public string token { get; set; }
    }
}
