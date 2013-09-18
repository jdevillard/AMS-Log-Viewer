using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AzureMobileServiceLogViewer.Helper
{
    public static class UpdateChecker
    {
        public static OnlineVersion CheckForUpdates(string feedUrl, UpdateFilter filter)
        {
            // NOTE: Requires a reference to System.ServiceModel.dll      
            var formatter = new Atom10FeedFormatter();
            try
            {
                // Read the feed         
                var reader = XmlReader.Create(feedUrl);
                formatter.ReadFrom(reader);
            }
            catch (Exception e)
            {
                Trace.WriteLine(String.Format("Error while trying to check updade : {0} ", e.Message));
                return null;
            }

            var latestUpdate = formatter.Feed.Items
                .OrderByDescending(u => u.LastUpdatedTime)
                .FirstOrDefault();

            if (latestUpdate != null)
            {
                return new OnlineVersion()
                {
                    downloadurl = latestUpdate.Links.Single().Uri.ToString(),
                    newversion = latestUpdate.Title.Text,
                    content = latestUpdate.Summary.Text
                };

            }
            else
                return null;
        }
    }

    public enum ReleaseStatus
    {
        Stable = 1,
        Beta = 2,
        Alpha = 4
    }
    public enum UpdateFilter
    {
        Stable = ReleaseStatus.Stable,
        Beta = Stable | ReleaseStatus.Beta,
        Alpha = Beta | ReleaseStatus.Alpha
    }

    public class OnlineVersion
    {
        public string newversion { get; set; }
        public string downloadurl { get; set; }
        public string content { get; set; }
    }
}
