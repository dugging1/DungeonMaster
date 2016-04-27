﻿using System;
using DungeonMasterEngine.DungeonContent.Items;
using DungeonMasterEngine.DungeonContent.Tiles;
using Microsoft.Xna.Framework;

namespace DungeonMasterEngine.DungeonContent.Actuators.Wall
{
    public class SwitchActuator : RemoteActuator
    {
        public override ActionStateX TargetAction { get; }

        public SwitchActuator(Vector3 position, Tile targetTile, ActionStateX action ) : base(targetTile, position)
        {
            TargetAction = action;
        }

        public override GrabableItem ExchangeItems(GrabableItem item)
        {
            SendMessageAsync(); 
            return base.ExchangeItems(item);
        }
    }
}
