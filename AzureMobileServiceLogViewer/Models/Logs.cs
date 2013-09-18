using AzureMobileServiceLogViewer.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace AzureMobileServiceLogViewer.Models
{

    public class Logs
    {
        public List<Result> results { get; set; }
        public string continuationToken { get; set; }
    }

    public class Result
    {
        public int Id { get; set; }
        public DateTime timeCreated { get; set; }
        public string type { get; set; }
        public string source { get; set; }
        public string message { get; set; }
    }


    
}
