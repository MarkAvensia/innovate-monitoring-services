namespace NitroConnector.Models
{
    public class ExtensionModel
    {
        public string extensionId { get; set; }
        public string extensionType { get; set; }
        public string assemblyName { get; set; }
        public string assemblyType { get; set; }
        public Status status { get; set; }
    }

    public class Status
    {
        public bool isEnabled { get; set; }
        public bool isPaused { get; set; }
    }
}
