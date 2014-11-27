﻿namespace Batzendev.Wallboard.Adapters
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Serialization;
    using Newtonsoft.Json;

    public class CCTrayAdapter : Adapter
    {
        protected HttpClient RestClient;

        public override async Task<IEnumerable<Models.Project>> GetProjectsForCache()
        {            
            if (this.RestClient == null)
            {
                this.RestClient = this.CreateRestClient();
            }

            var cctrayEntries = await this.GetCCTrayEntries();

            var projects = this.GetProjects(cctrayEntries);

            return projects;
        }

        protected virtual HttpClient CreateRestClient()
        {
            if (string.IsNullOrWhiteSpace(this.Url))
            {
                return null;
            }

            var restClient = new HttpClient();
            restClient.BaseAddress = new Uri(this.Url);
            
            return restClient;
        }

        private async Task<IReadOnlyList<Project>> GetCCTrayEntries()
        {
            var rootObject = await this.GetRootObject();
            var cctrayEntries = rootObject?.Project ?? new List<Project>();

            this.AdjustNameAndParent(cctrayEntries);

            return cctrayEntries;
        }

        protected virtual async Task<CCTrayRootObject> GetRootObject()
        {
            var request = this.GetRequest();

            var response = await this.RestClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);            

            return await this.GetRootObject(response);
        }

        protected virtual async Task<CCTrayRootObject> GetRootObject(HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var contentType = response.Content.Headers.ContentType;

            switch (contentType.MediaType)
            {
                case "application/xml":
                case "text/javascript":
                case "text/xml":
                    return DeserializeXml(content);

                case "application/json":
                case "text/json":
                case "text/x-json":
                    return JsonConvert.DeserializeObject<CCTrayRootObject>(content);

                default:
                    return JsonConvert.DeserializeObject<CCTrayRootObject>(content);
            }
        }

        private static CCTrayRootObject DeserializeXml(string content)
        {
            var ser = new XmlSerializer(typeof(CCTrayRootObject));

            using (var sr = new StringReader(content))
            {
                var root = (CCTrayRootObject)ser.Deserialize(sr);
                return root;
            }
        }

        protected virtual HttpRequestMessage GetRequest()
        {
            var request = new HttpRequestMessage();

            if (string.IsNullOrWhiteSpace(this.Username) == false)
            {
                // only add the Authorization parameter if it hasn't been added by a previous Execute 
                if (!request.Headers.Any(p => p.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase)))
                {
                    var token = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format("{0}:{1}", this.Username, this.Password)));
                    var authHeader = string.Format("Basic {0}", token);

                    request.Headers.Add("Authorization", authHeader);
                }
            }

            return request;
        }

        protected void AdjustNameAndParent(IReadOnlyList<Project> cctrayEntries)
        {
            foreach (var cctrayEntry in cctrayEntries)
            {
                var splittedName = cctrayEntry.name.Split(new[] { " :: " }, StringSplitOptions.RemoveEmptyEntries);

                cctrayEntry.name = splittedName.Last();

                if (splittedName.Count() > 1)
                {
                    cctrayEntry.parent = string.Join(" :: ", splittedName.Take(splittedName.Count() - 1));
                }
            }
        }

        protected virtual IEnumerable<Models.Project> GetProjects(IReadOnlyList<Project> cctrayEntries)
        {
            var entriesWithParent = cctrayEntries.Where(x => string.IsNullOrWhiteSpace(x.parent) == false)
                .GroupBy(x => x.parent);

            foreach (var group in entriesWithParent)
            {
                var project = this.GetProject(group.Key, @group.ToArray());

                yield return project;
            }

            var entriesWithoutParent = cctrayEntries.Where(x => string.IsNullOrWhiteSpace(x.parent));

            foreach (var cctrayEntry in entriesWithoutParent)
            {
                var project = this.GetProject(cctrayEntry.name, new[] { cctrayEntry });

                yield return project;
            }
        }

        private Models.Project GetProject(string name, IList<Project> cctrayEntries)
        {
            var project = new Models.Project
            {
                Id = this.GetProjectId(name),
                Name = name,
                Successful = cctrayEntries.Count(x => this.IsRunning(x) == false && this.IsSuccessful(x)),
                Failed = cctrayEntries.Count(x => this.IsRunning(x) == false && this.IsFailed(x)),
                Running = cctrayEntries.Count(this.IsRunning)
            };

            return project;
        }

        protected virtual string GetProjectId(string key)
        {
            return string.Format("{0}.{1}", this.Id, key);
        }

        protected virtual bool IsSuccessful(Project entry)
        {
            return entry.lastBuildStatus.Equals("Success", StringComparison.OrdinalIgnoreCase);
        }

        protected virtual bool IsFailed(Project entry)
        {
            return entry.lastBuildStatus.Equals("Failure", StringComparison.OrdinalIgnoreCase)
                || entry.lastBuildStatus.Equals("Exception", StringComparison.OrdinalIgnoreCase);
        }

        protected virtual bool IsRunning(Project entry)
        {
            return entry.activity.Equals("Building", StringComparison.OrdinalIgnoreCase)
                || entry.activity.Equals("CheckingModifications", StringComparison.OrdinalIgnoreCase)
                || entry.lastBuildStatus.Equals("Pending", StringComparison.OrdinalIgnoreCase)
                || entry.lastBuildStatus.Equals("Unknown", StringComparison.OrdinalIgnoreCase);
        }

        [XmlType("Project")]
        public class Project
        {
            [XmlAttribute("webUrl")]
            public string webUrl { get; set; }

            [XmlAttribute("lastBuildLabel")]
            public string lastBuildLabel { get; set; }

            [XmlAttribute("lastBuildTime")]
            public string lastBuildTime { get; set; }

            [XmlAttribute("lastBuildStatus")]
            public string lastBuildStatus { get; set; }

            [XmlAttribute("activity")]
            public string activity { get; set; }

            [XmlAttribute("name")]
            public string name { get; set; }

            [XmlIgnore]
            public string parent { get; set; }
        }

        [XmlRoot("Projects")]
        public class CCTrayRootObject
        {
            [XmlElement("Project", typeof(Project))]
            public List<Project> Project { get; set; }
        }
    }
}