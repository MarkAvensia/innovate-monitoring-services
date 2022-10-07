namespace NitroConnector.Models
{
    public class StatisticsQueueDetails
    {
        public int queuedEventCount { get; set; }
        public int errorEventCount { get; set; }
        public int currentSequenceNumber { get; set; }
    }
}
