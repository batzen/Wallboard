namespace Batzendev.Wallboard.Models.CCTray
{
    using System.Xml.Serialization;

    [XmlType("Project")]
    public class CCTrayProject
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
}