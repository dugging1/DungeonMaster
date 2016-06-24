﻿using DungeonMasterEngine.GameConsoleContent.Base;
using DungeonMasterEngine.Player;
using Microsoft.Xna.Framework;
using System.Threading.Tasks;
using DungeonMasterEngine.Builders;
using DungeonMasterEngine.Builders.Initializators;
using DungeonMasterEngine.DungeonContent.Entity.Attacks;
using DungeonMasterEngine.DungeonContent.Entity.BodyInventory;
using DungeonMasterEngine.DungeonContent.Items;
using DungeonMasterEngine.DungeonContent.Items.GrabableItems;
using DungeonMasterEngine.DungeonContent.Items.GrabableItems.Factories;
using DungeonMasterEngine.Helpers;

namespace DungeonMasterEngine.GameConsoleContent
{
    public class ItemCommand : Interpreter
    {
        private Theron theron;

        public override async Task Run()
        {
            theron = ConsoleContext.AppContext.Leader;

            if (Parameters.Length > 0)
            {
                switch (Parameters[0])
                {
                    case "create":
                        int identifer = -1;
                        if (Parameters.Length == 2 && int.TryParse(Parameters[1], out identifer))
                        {
                            if (theron.Hand != null)
                                Output.WriteLine("Hand is not empty.");
                            else
                            {
                                MiscItemFactory factory = null;// new MiscItemFactory("Fake item", 0, new IAttackFactory[0], BackPackStorageType.Instance.ToEnumerable(), TODO);
                                //TODO


                                theron.Hand = factory.Create(new MiscInitializator
                                {
                                    Attribute = 0
                                });
                                Output.Write($"Item: {theron.Hand} added to hand.");
                            }
                        }
                        else
                        {
                            Output.WriteLine("Invalid Parmeter");
                        }
                        break;
                }
            }
        }

    }
}
