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

			protected override async void OnTickAsAI(float dt) {
				base.OnTickAsAI(dt);
				if (canBandage) await DoBandaging();
			}

			private async Task DoBandaging() {
				var bleeding = agent.GetComponent<BleedingBehavior.BleedingComponent>();
				if (!mission.MissionEnded() && bleeding != null && !bleeding.bandaged && count > 0 && bleeding.tickDamage > 5) {
					canBandage = false;
					Announce("{=used_bandage}{AGENT} used bandage.".Replace("{AGENT}", agent.Name));
					count--;
					if (!mission.MissionEnded())
						bleeding.bandaged = true;
					var oldspeed = agent.GetCurrentSpeedLimit();
					if (!mission.MissionEnded())
						agent.SetMaximumSpeedLimit(oldspeed * 0.3f, false);
					await Task.Delay(3000);
					if (!mission.MissionEnded())
						agent.SetMaximumSpeedLimit(oldspeed, true);
				}
				await Task.Delay(3000); // cooldown
				canBandage = true;
			}
		}
	}
}