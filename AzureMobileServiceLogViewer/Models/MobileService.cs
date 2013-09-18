using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureMobileServiceLogViewer.Models
{
    public class MobileService
    {
        public int id { get; set; }
        public string Name { get; set; }

        public virtual Subscription subscription { get; set; }
    }

    public class MobileServiceManagement
    {
        public String Name { get; set; }
        public String State { get; set; }
    }
}
