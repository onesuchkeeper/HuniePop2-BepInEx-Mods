public interface IExpandedStyleDefinition
{
    public string Name { get; }
    public bool IsNSFW { get; }
    public bool IsCodeUnlocked { get; }
    public bool IsPurchased { get; }
}