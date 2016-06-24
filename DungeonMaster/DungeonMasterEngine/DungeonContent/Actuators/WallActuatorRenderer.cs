using System.Linq;
using DungeonMasterEngine.DungeonContent.Tiles.Support;
using Microsoft.Xna.Framework;

namespace DungeonMasterEngine.DungeonContent.Actuators
{

    public class WallActuatorRenderer : ActuatorRenderer<WallActuator>
    {


        public override bool Interact(ILeader leader, ref Matrix currentTransformation, object param)
        {
            if (Actuator.SensorsEnumeration.LastOrDefault()?.GraphicsBase.Renderer.Interact(leader, ref currentTransformation, param) ?? false)
            {
                return Actuator.Trigger(leader);
            }
            return false;
        }

        public WallActuatorRenderer(WallActuator actuator) : base(actuator) {}
    }
}