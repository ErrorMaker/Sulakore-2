namespace Sulakore.Habbo
{
    public interface IHPlayerAction
    {
        bool Empowered { get; }
        int PlayerIndex { get; }

        HPoint Tile { get; }
        HPoint MovingTo { get; }

        HSign Sign { get; }
        HStance Stance { get; }

        HDirection HeadDirection { get; }
        HDirection BodyDirection { get; }

        HAction LatestAction { get; }
    }
}