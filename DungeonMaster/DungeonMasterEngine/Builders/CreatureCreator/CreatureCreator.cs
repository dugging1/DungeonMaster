using System.Collections.Generic;
using System.Linq;
using System.Xml;
using DungeonMasterEngine.DungeonContent.Entity;
using DungeonMasterEngine.DungeonContent.Tiles;
using DungeonMasterEngine.Helpers;
using DungeonMasterParser.Items;
using Microsoft.Xna.Framework;

namespace DungeonMasterEngine.Builders.CreatureCreator
{
    internal class CreatureCreator
    {
        private readonly LegacyMapBuilder builder;

        public CreatureCreator(LegacyMapBuilder builder)
        {
            this.builder = builder;
        }

        public virtual IEnumerable<Creature> AddCreature(CreatureItem creatureData, Tile tile, Point position)
        {
            var factory = builder.Factories.CreatureFactories[(int)creatureData.Type];

            var res = new List<Creature>();

            foreach (var tuple in creatureData.GetCreaturesInfo().Take(creatureData.CreaturesCount + 1))
            {
                var space = factory.Layout.AllSpaces.First(s => tuple.Item1.ToDirections().All(s.Sides.Contains));

                var creature = factory.Create(new CreatureInitializer
                {
                    HitPoints = tuple.Item2,
                    Location = factory.Layout.GetSpaceElement(space, tile),
                    RelationToken = builder.CreatureToken,
                    EnemiesTokens = builder.ChampionToken.ToEnumerable(),
                    PossessionItems = creatureData.PossessionItems.Select(x => builder.ItemCreator.CreateItem(x))
                });
                creature.Renderer = builder.RendererSource.GetCreatureRenderer(creature, factory.Texture);

                res.Add(creature);
            }

            return res;
        }


    }
}