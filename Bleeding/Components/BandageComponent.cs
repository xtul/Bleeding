using System;
using System.Diagnostics.PerformanceData;
using System.Linq;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;
using static Bleeding.Helpers;

namespace Bleeding {
	public partial class BandageBehavior {
		/// <summary>
		/// For AI only. Player just uses BandageBehavior.
		/// </summary>
		internal class BandageComponent : AgentComponent {
			private readonly Agent agent;
			private bool canBandage;
			private readonly Mission mission;
			public int count;
			public BandageComponent(Agent agent, Config config, Mission mission) : base(agent) {
				this.agent = agent;
				count = config.Bandages.NpcCount;
				this.mission = mission;
				canBandage = true;
			}

			protected override void OnTickAsAI(float dt) {
				if (canBandage) DoBandaging();
			}

			private void DoBandaging() {
				var bleeding = agent.GetComponent<BleedingBehavior.BleedingComponent>();
				if (mission != null && bleeding != null && !bleeding._bandaged && count > 0 && bleeding._tickDamage * 3 > agent.Health) {
					canBandage = false;
					Announce($"{agent.Name} used {(agent.IsFemale ? "her" : "his")} bandage.");
					count--;
					agent.SetActionChannel(1, ActionIndexCache.Create("act_reload_crossbow"), true);
					if (!mission.MissionEnded())
						bleeding._bandaged = true;
				}
				canBandage = true;
			}
		}
	}
}