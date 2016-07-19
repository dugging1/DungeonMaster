﻿using System;
using System.Collections.Generic;
using System.Linq;
using DungeonMasterEngine.Builders.ActuatorCreators;
using DungeonMasterEngine.DungeonContent;
using DungeonMasterEngine.DungeonContent.Actions;
using DungeonMasterEngine.DungeonContent.Actions.Factories;
using DungeonMasterEngine.DungeonContent.Entity;
using DungeonMasterEngine.DungeonContent.Entity.BodyInventory;
using DungeonMasterEngine.DungeonContent.Entity.GroupSupport;
using DungeonMasterEngine.DungeonContent.Entity.GroupSupport.Base;
using DungeonMasterEngine.DungeonContent.GrabableItems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using DungeonMasterEngine.DungeonContent.Tiles.Support;
using DungeonMasterEngine.Interfaces;
using Microsoft.Xna.Framework.Graphics;

namespace DungeonMasterEngine.Player
{
    public class LegacyLeader : PointOfViewCamera, ILeader
    {
        private readonly IFactories factorie;
        private MouseState prevMouse = Mouse.GetState();
        private KeyboardState prevKeyboard;

        public readonly List<Champion> partyGroup = new List<Champion>();

        public IReadOnlyList<Champion> PartyGroup => partyGroup;


        public object Interactor => Ray;

        IReadOnlyList<ILiveEntity> ILeader.PartyGroup => PartyGroup;

        public IGrabableItem Hand { get; set; }

        public ILiveEntity Leader => PartyGroup.FirstOrDefault();
        public bool Enabled { get; set; } = true;
        public ushort MagicalLightAmount { get; set; }

        public LegacyLeader(IFactories factorie)
        {
            this.factorie = factorie;
        }

        private void InitMocap()
        {
            var builder = new ChampionMocapCreator();
            var x = new[]
            {
                builder.GetChampion("CHANI|SAYYADINA SIHAYA||F|AACPACJOAABB|DJCFCPDJCFCPCF|BDACAAAAAAAADCDB"),
                builder.GetChampion("IAIDO|RUYITO CHIBURI||M|AADAACIKAAAL|CICLDHCICDCNDC|CDACAAAABBBCAAAA"),
                builder.GetChampion("HAWK|$CFEARLESS||M|AAEGADFCAAAK|CICNCDCGDHCDCD|CAACAAAAADADAAAA"),
                builder.GetChampion("ZED|DUKE OF BANVILLE||M|AADMACFIAAAK|DKCICICIDCCICI|CBBCCBCBBCBBBCBB"),
            };
            if (x.Any(champoin => !AddChampoinToGroup(champoin)))
            {
                throw new Exception();
            }
        }

        protected override bool CanMoveToTile(ITile tile) => base.CanMoveToTile(tile) && tile.LayoutManager.WholeTileEmpty;

        protected override void OnMapDirectionChanged(MapDirection oldDirection, MapDirection newDirection)
        {
            base.OnMapDirectionChanged(oldDirection, newDirection);
            //TODO $"Direction changed: {oldDirection} -> {newDirection}".Dump();

            if (oldDirection != newDirection.Opposite)
                RotateParty(oldDirection, newDirection);
            else
            {
                var midle = oldDirection.NextClockWise;
                RotateParty(oldDirection, midle);
                RotateParty(midle, newDirection);
            }

            foreach (var champion in PartyGroup)
                champion.MapDirection = MapDirection;
        }

        protected override void OnLocationChanging(ITile oldLocation, ITile newLocation)
        {
            base.OnLocationChanging(oldLocation, newLocation);

            oldLocation?.OnObjectLeaving(this);
            newLocation?.OnObjectEntering(this);

            MovePartyToRight(newLocation);
        }

        protected virtual void MovePartyToRight(ITile newLocation)
        {
            if (!newLocation.LayoutManager.WholeTileEmpty)
                throw new InvalidOperationException();

            foreach (var champion in PartyGroup)
            {
                var prevLocation = champion.Location;
                if (!newLocation.LayoutManager.TryGetSpace(champion, prevLocation.Space))
                    throw new InvalidOperationException("not expected");

                champion.Location = new FourthSpaceRouteElement(prevLocation.Space, newLocation);
                prevLocation.Tile.LayoutManager.FreeSpace(champion, prevLocation.Space);
            }
        }

        protected override void OnLocationChanged(ITile oldLocation, ITile newLocation)
        {
            base.OnLocationChanged(oldLocation, newLocation);

            //if (oldLocation == null)
            //    InitMocap();

            oldLocation?.OnObjectLeft(this);
            newLocation?.OnObjectEntered(this);
        }

        protected  virtual void RotateParty(MapDirection oldDirection, MapDirection newDirection)
        {
            var targetLocation = partyGroup.FirstOrDefault()?.Location?.Tile;

            if (targetLocation != null)
            {

                var counterClockWiseGridPoints = new[]
                {
                    Tuple.Create(new Point(0, 0), new Point(0, 1)),
                    Tuple.Create(new Point(0, 1), new Point(1, 1)),
                    Tuple.Create(new Point(1, 1), new Point(1, 0)),
                    Tuple.Create(new Point(1, 0), new Point(0, 0)),
                };

                Func<Point, Point> nextGridPoint = p =>
                {
                    if (oldDirection.NextCounterClockWise == newDirection)
                        return Array.Find(counterClockWiseGridPoints, t => t.Item1 == p).Item2;
                    else if (oldDirection.NextClockWise == newDirection)
                        return Array.Find(counterClockWiseGridPoints, t => t.Item2 == p).Item1;
                    else
                        throw new Exception();
                };

                foreach (var champoin in PartyGroup)
                    targetLocation.LayoutManager.FreeSpace(champoin, champoin.Location.Space);

                foreach (var champoin in PartyGroup)
                {
                    var newSpace = champoin.GroupLayout.AllSpaces.First(s => s.GridPosition == nextGridPoint(champoin.Location.Space.GridPosition));
                    champoin.Location = new FourthSpaceRouteElement(newSpace, targetLocation);
                    targetLocation.LayoutManager.TryGetSpace(champoin, champoin.Location.Space);
                }
            }
        }

        public virtual bool AddChampoinToGroup(ILiveEntity entity)
        {
            var champion = entity as Champion;
            if (champion == null || partyGroup.Count == 4)
                return false;

            var freeSpace = Small4GroupLayout.Instance.AllSpaces.Except(partyGroup.Select(ch => ch.Location?.Space).Where(x => x != null)).First();
            champion.Location = new FourthSpaceRouteElement(freeSpace, Location.Tile);
            partyGroup.Add(champion);
            champion.Died += (sender, deadChampion) => partyGroup.Remove(deadChampion);

            return true;
        }

        public override void Update(GameTime time)
        {
            if (!Enabled)
                return;

            base.Update(time);

            foreach (var champoin in PartyGroup)
            {
                champoin.Update(time);
            }

            if (IsActive && Mouse.GetState().LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released
                || Keyboard.GetState().IsKeyDown(Keys.Enter) && prevKeyboard.IsKeyUp(Keys.Enter))
            {
                var tiles = new[] { Location.Tile }.Concat(Location.Tile.Neighbors
                    .Select(x => x.Item1))
                    .ToArray();

                var matrix = Matrix.Identity;
                var anyTriggered = false;
                foreach (var tile in tiles)
                    if (tile.Renderer.Interact(this, ref matrix, null))
                    {
                        anyTriggered = true;
                        break;
                    }

                if (!anyTriggered && Hand != null)
                    ThrowItem();
            }

            prevMouse = Mouse.GetState();
            prevKeyboard = Keyboard.GetState();
        }

        protected void ThrowItem()
        {
            var storageType = ActionHandStorageType.Instance;
            var actionHand = Leader.Body.GetBodyStorage(storageType);
            var item = actionHand.TakeItemFrom(0);
            actionHand.AddItemTo(Hand, 0);
            Hand = null;

            var action = new ThrowAttack((ThrowActionFactory)factorie.FightActions[42], Leader, storageType);
            action.Apply(MapDirection);

            if(item != null)
                actionHand.AddItemTo(item, 0);
        }

        public bool IsActive => true;

        public virtual void Draw(BasicEffect effect)
        {
            foreach (var champoin in PartyGroup)
            {
                var mat = Matrix.Identity;
                champoin.Renderer.Render(ref mat, effect, null);
            }
        }

    }
}