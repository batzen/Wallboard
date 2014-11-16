namespace Batzendev.Wallboard.Adapters
{
    using System;
    using System.IO;
    using Newtonsoft.Json;
    using Microsoft.Framework.Runtime;

    public class SampleCCTrayAdapter : CCTrayAdapter
    {
        private readonly IApplicationEnvironment appEnvironment;

        public SampleCCTrayAdapter(IApplicationEnvironment appEnvironment)
        {
            this.appEnvironment = appEnvironment;
        }

        //protected override RestClient CreateRestClient()
        //{
        //    var client = base.CreateRestClient();
        //    client.BaseUrl = this.GetUrl();

        //    return client;
        //}

        private string GetUrl()
        {
            if (this.Url.EndsWith("json", StringComparison.InvariantCultureIgnoreCase))
            {
                return Path.Combine(appEnvironment.ApplicationBasePath, "SampleData/cctray.json");
            }

            if (this.Url.EndsWith("xml", StringComparison.InvariantCultureIgnoreCase))
            {
                return Path.Combine(appEnvironment.ApplicationBasePath, "SampleData/cctray.xml");
            }

            throw new Exception(string.Format("{0} is not a valid sample file type. json and xml are allowed.", this.Url));
        }

        protected override CCTrayRootObject GetRootObject()
        {
            if (this.Url.EndsWith("json", StringComparison.InvariantCultureIgnoreCase))
            {
                return JsonConvert.DeserializeObject<CCTrayRootObject>(File.ReadAllText(Path.Combine(appEnvironment.ApplicationBasePath, "SampleData/cctray.json")));
            }

            if (this.Url.EndsWith("xml", StringComparison.InvariantCultureIgnoreCase))
            {
                var content = File.ReadAllText(Path.Combine(appEnvironment.ApplicationBasePath, "SampleData/cctray.xml"));

                var ser = new System.Xml.Serialization.XmlSerializer(typeof(CCTrayRootObject));

                using (var sr = new StringReader(File.ReadAllText(Path.Combine(appEnvironment.ApplicationBasePath, "SampleData/cctray.xml"))))
                {
                    var root = (CCTrayRootObject)ser.Deserialize(sr);
                    return root;
                }
            }

            throw new Exception(string.Format("{0} is not a valid sample file type. json and xml are allowed.", this.Url));
        }

        //protected override RestRequest GetRequest()
        //{
        //    var request = base.GetRequest();

        //    if (this.Url.EndsWith("json", StringComparison.InvariantCultureIgnoreCase))
        //    {
        //        request.RequestFormat = DataFormat.Xml;
        //    }

        //    return request;
        //}
    }
}