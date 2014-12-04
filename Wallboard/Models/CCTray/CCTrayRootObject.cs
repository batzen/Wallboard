namespace Batzendev.Wallboard.Models.CCTray
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [XmlRoot("Projects")]
    public class CCTrayRootObject
    {
        [XmlElement("Project", typeof(CCTrayProject))]
        public List<CCTrayProject> Project { get; set; }
    }
}