using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureMobileServiceLogViewer.Models
{
    //public class PublishData
    //{

    //}
    //https://windows.azure.com/download/publishprofile.aspx
    public class PublishProfile
    {
        public String PublishMethod { get; set; }
        public String Url { get; set; }
        public IList<Subscription> Subscriptions { get; set; }
    }

}
