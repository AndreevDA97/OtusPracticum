namespace OtusPracticum.Models
{
    public class RequestLog
    {
        public string Id { get; set; } = null!;
        public DateTime DateTime { get; set; } = DateTime.UtcNow;
        public string Method { get; set; } = null!;
        public string Host { get; set; } = null!;
        public string Path { get; set; } = null!;
        public string? QueryParams { get; set; }
        public string Type { get; set; } = null!;
        public string IPAddress { get; set; } = null!;
        public double ExecutionTimeMs { get; set; }
        public string? RequestHeaders { get; set; }
        public string? ResponseHeaders { get; set; }
        public int ResponseStatus { get; set; }
    }
}
