using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureMobileServiceLogViewer.Models
{
    public class Subscription
    {
        public Subscription()
        {
            MobileServices = new HashSet<MobileService>();
        } 


        public Guid Id { get; set; }
        public String Name { get; set; }

        public String Cert { get; set; }

        public virtual ICollection<MobileService> MobileServices { get; set; }
    }
    
   
}
