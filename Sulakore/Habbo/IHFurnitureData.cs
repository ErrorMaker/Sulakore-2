namespace Sulakore.Habbo
{
    public interface IHFurnitureData
    {
        int FurnitureOwnerId { get; }
        string FurnitureOwnerName { get; }

        int FurnitureId { get; }
        int FurnitureIndex { get; }
        int FurnitureTypeId { get; }

        int State { get; }
        HPoint Tile { get; }
        HDirection Direction { get; }
    }
}