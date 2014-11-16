namespace Batzendev.Wallboard.Adapters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Serialization;
    using RestSharp;

    public class CCTrayAdapter : Adapter
    {
        protected RestClient RestClient;

        public override IEnumerable<Models.Project> GetProjects()
        {
            if (this.RestClient == null)
            {
                this.RestClient = this.CreateRestClient();
            }

            var cctrayEntries = this.GetCCTrayEntries();

            var projects = this.GetProjects(cctrayEntries);

            return projects;
        }

        protected virtual RestClient CreateRestClient()
        {
            if (string.IsNullOrWhiteSpace(this.Url))
            {
                return null;
            }

            var restClient = new RestClient(this.Url);

            if (string.IsNullOrWhiteSpace(this.Username) == false)
            {
                restClient.Authenticator = new HttpBasicAuthenticator(this.Username, this.Password);
            }

            return restClient;
        }

        private IReadOnlyList<Project> GetCCTrayEntries()
        {
            var rootObject = this.GetRootObject();
            var cctrayEntries = rootObject?.Project ?? new List<Project>();

            this.AdjustNameAndParent(cctrayEntries);

            return cctrayEntries;
        }

        protected virtual CCTrayRootObject GetRootObject()
        {
            var request = this.GetRequest();

            var response = this.RestClient.Execute<CCTrayRootObject>(request);

            return response.Data;
        }

        protected virtual RestRequest GetRequest()
        {
            var request = new RestRequest();

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
            return entry.lastBuildStatus.Equals("Success", StringComparison.InvariantCultureIgnoreCase);
        }

        protected virtual bool IsFailed(Project entry)
        {
            return entry.lastBuildStatus.Equals("Failure", StringComparison.InvariantCultureIgnoreCase)
                || entry.lastBuildStatus.Equals("Exception", StringComparison.InvariantCultureIgnoreCase);
        }

        protected virtual bool IsRunning(Project entry)
        {
            return entry.activity.Equals("Building", StringComparison.InvariantCultureIgnoreCase)
                || entry.activity.Equals("CheckingModifications", StringComparison.InvariantCultureIgnoreCase)
                || entry.lastBuildStatus.Equals("Pending", StringComparison.InvariantCultureIgnoreCase)
                || entry.lastBuildStatus.Equals("Unknown", StringComparison.InvariantCultureIgnoreCase);
        }

        [Serializable]
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

        [Serializable]
        [XmlRoot("Projects")]
        public class CCTrayRootObject
        {
            [XmlElement("Project", typeof(Project))]
            public List<Project> Project { get; set; }
        }
    }
}