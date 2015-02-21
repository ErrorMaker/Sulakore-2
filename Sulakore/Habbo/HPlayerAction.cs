using System;
using System.Collections.Generic;

using Sulakore.Protocol;

namespace Sulakore.Habbo
{
    public class HPlayerAction : IHPlayerAction
    {
        public bool Empowered { get; private set; }
        public int PlayerIndex { get; private set; }

        public HPoint Tile { get; private set; }
        public HPoint MovingTo { get; private set; }

        public HSign Sign { get; private set; }
        public HStance Stance { get; private set; }

        public HDirection HeadDirection { get; private set; }
        public HDirection BodyDirection { get; private set; }

        public HAction LatestAction { get; private set; }

        public HPlayerAction(bool empowered, int playerIndex, HPoint tile, HPoint movingTo, HSign sign,
            HStance stance, HDirection headDirection, HDirection bodyDirection, HAction latestAction)
        {
            Empowered = empowered;
            PlayerIndex = playerIndex;

            Tile = tile;
            MovingTo = movingTo;

            Sign = sign;
            Stance = stance;

            HeadDirection = headDirection;
            BodyDirection = bodyDirection;

            LatestAction = latestAction;
        }

        public override string ToString()
        {
            return string.Format("Empowered: {0}, PlayerIndex: {1}, Tile: {2}, MovingTo: {3}, HeadDirection: {4}, BodyDirection: {5}, LatestAction: {6}",
                Empowered, PlayerIndex, Tile, MovingTo, HeadDirection, BodyDirection, LatestAction);
        }

        public static IList<IHPlayerAction> Extract(HMessage packet)
        {
            HAction action;
            HStance stance;
            bool empowered;
            string z, mZ, actionString;
            string[] actionData, actionValues, movingValues;
            int position = 0, playerIndex, x, mX, y, mY, hDir, bDir, sign;

            var playerActions = new List<IHPlayerAction>(packet.ReadInt(ref position));
            do
            {
                empowered = false;
                mZ = string.Empty;
                mX = mY = sign = 0;
                action = HAction.None;
                stance = HStance.Stand;

                playerIndex = packet.ReadInt(ref position);
                x = packet.ReadInt(ref position);
                y = packet.ReadInt(ref position);
                z = packet.ReadString(ref position);
                hDir = packet.ReadInt(ref position);
                bDir = packet.ReadInt(ref position);
                actionString = packet.ReadString(ref position);
                #region Grab Action Type(+Info)
                actionData = actionString.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string actionInfo in actionData)
                {
                    actionValues = actionInfo.Split(' ');
                    if (string.IsNullOrWhiteSpace(actionValues[0]) || actionValues.Length < 2) continue;

                    switch (actionValues[0])
                    {
                        default: action = HAction.None; break;
                        case "flatctrl":
                        {
                            empowered = true;
                            action = HAction.None;
                            break;
                        }
                        case "mv":
                        {
                            movingValues = actionValues[1].Split(',');
                            if (movingValues.Length >= 3)
                            {
                                mX = int.Parse(movingValues[0]);
                                mY = int.Parse(movingValues[1]);
                                mZ = movingValues[2];
                            }
                            action = HAction.Move;
                            break;
                        }
                        case "sit": action = HAction.Sit; stance = HStance.Sit; break;
                        case "lay": action = HAction.Lay; stance = HStance.Lay; break;
                        case "sign":
                        {
                            sign = int.Parse(actionValues[1]);
                            action = HAction.Sign;
                            break;
                        }
                    }
                }

                if (action != HAction.Move)
                {
                    mX = x;
                    mY = y;
                    mZ = z;
                }
                #endregion

                playerActions.Add(new HPlayerAction(empowered, playerIndex, new HPoint(x, y, z), new HPoint(mX, mY, mZ),
                    (HSign)sign, stance, (HDirection)hDir, (HDirection)bDir, action));
            }
            while (playerActions.Count < playerActions.Capacity);

            return playerActions;
        }
    }
}