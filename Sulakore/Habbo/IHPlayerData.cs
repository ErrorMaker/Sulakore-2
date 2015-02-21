using Sulakore.Habbo;

namespace Sulakore.Habbo
{
    public interface IHPlayerData
    {
        int PlayerId { get; }
        int PlayerIndex { get; }
        string PlayerName { get; }

        HPoint Tile { get; }
        HGender Gender { get; }

        string Motto { get; }
        string FigureId { get; }
        string FavoriteGroup { get; }
    }
}