using System.Collections.Generic;

namespace NitroConnector.Models
{
    public class StatQueue
    {
        public string ID { get; set; }
        public string Extension { get; set; }
        public int QueuedEventCount { get; set; }
        public int ErrorEventCount { get; set; }
        public int CurrentSequenceNumber { get; set; }
        public string ExtentionUrl { get; set; }
    }

    public class StateQueue
    {
        public string Extension { get; set; }
        public int Queue { get; set; }
        public int Error { get; set; }
    }

    public class EpiQueue
    {
        public string Extension { get; set; }
        public int PendingEntities { get; set; }
        public int PendingLinks { get; set; }
    }
    public class StatQueueName
    {
        public string Name { get; set; }
        public IEnumerable<StatQueue> statDetails { get; set; }
    }
}
