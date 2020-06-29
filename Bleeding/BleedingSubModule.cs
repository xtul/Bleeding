using TaleWorlds.MountAndBlade;
using System.Xml.Serialization;
using System.IO;
using TaleWorlds.Library;
using static Bleeding.Helpers;

namespace Bleeding {
	public class BleedingSubModule : MBSubModuleBase {
		public Config config;
		public override void OnMissionBehaviourInitialize(Mission mission) {
			var serializer = new XmlSerializer(typeof(Config));
			var reader = new StreamReader(BasePath.Name + "Modules/Bleeding/bin/Win64_Shipping_Client/config.xml");
			config = (Config)serializer.Deserialize(reader);
			reader.Close();

			config.BleedRate.Clamp(0, 1);
			config.SlowOnBleed.Value.Clamp(0, 1);

			if (config.Debug) Say($"Bleeding mod activated.");

			if (config.Bandages.Enabled) mission.AddMissionBehaviour(new BandageBehavior(config, mission));
			mission.AddMissionBehaviour(new BleedingBehavior(config, mission));
			base.OnMissionBehaviourInitialize(mission);
		}
	}
}
