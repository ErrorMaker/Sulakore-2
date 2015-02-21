namespace Sulakore.Editor.Tags
{
    public interface IFlashTag
    {
        int Position { get; }
        RecordHeader Header { get; }
        byte[] ToBytes();
    }
}