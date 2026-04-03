namespace Company.Template.Api.Options;

public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public int PermitLimit { get; set; } = 100;

    public int WindowInSeconds { get; set; } = 60;

    public string QueueProcessingOrder { get; set; } = "OldestFirst";

    public int QueueLimit { get; set; } = 0;
}
