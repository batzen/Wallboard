namespace Batzendev.Wallboard.Adapters
{
    using System;
    using System.IO;
    using Newtonsoft.Json;
    using Microsoft.Framework.Runtime;
    using System.Threading.Tasks;
    using System.Net.Http;

    public class SampleCCTrayAdapter : CCTrayAdapter
    {
        private readonly IApplicationEnvironment appEnvironment;

        public SampleCCTrayAdapter(IApplicationEnvironment appEnvironment)
        {
            this.appEnvironment = appEnvironment;
        }

        protected override HttpClient CreateRestClient()
        {
            return null;
        }

        private string GetUrl()
        {
            if (this.Url.EndsWith("json", StringComparison.OrdinalIgnoreCase))
            {
                return Path.Combine(appEnvironment.ApplicationBasePath, "SampleData/cctray.json");
            }

            if (this.Url.EndsWith("xml", StringComparison.OrdinalIgnoreCase))
            {
                return Path.Combine(appEnvironment.ApplicationBasePath, "SampleData/cctray.xml");
            }

            throw new Exception(string.Format("{0} is not a valid sample file type. json and xml are allowed.", this.Url));
        }

        protected override Task<CCTrayRootObject> GetRootObject()
        {
            if (this.Url.EndsWith("json", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(JsonConvert.DeserializeObject<CCTrayRootObject>(File.ReadAllText(Path.Combine(appEnvironment.ApplicationBasePath, "SampleData/cctray.json"))));
            }

            if (this.Url.EndsWith("xml", StringComparison.OrdinalIgnoreCase))
            {
                var content = File.ReadAllText(Path.Combine(appEnvironment.ApplicationBasePath, "SampleData/cctray.xml"));

                var ser = new System.Xml.Serialization.XmlSerializer(typeof(CCTrayRootObject));

                using (var sr = new StringReader(content))
                {
                    var root = (CCTrayRootObject)ser.Deserialize(sr);
                    return Task.FromResult(root);
                }
            }

            throw new Exception(string.Format("{0} is not a valid sample file type. json and xml are allowed.", this.Url));
        }
    }
}