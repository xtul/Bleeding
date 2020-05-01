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
				var bleeding = agent.GetComponent<BleedingBehavior.BleedingComponent>();
				if (!mission.MissionEnded() && bleeding != null && !bleeding.bandaged && count > 0 && canBandage && agent.Health < agent.HealthLimit / 2) {
					canBandage = false;
					Announce($"{agent.Name} used a bandage.");
					var oldspeed = agent.GetCurrentSpeedLimit();
					agent.SetMaximumSpeedLimit(oldspeed * 0.3f, false);
					await Task.Delay(3000);
					agent.SetMaximumSpeedLimit(oldspeed, true);
					count--;
					bleeding.bandaged = true;
				}
				await Task.Delay(3000); // cooldown
				canBandage = true;
			}
		}
	}
}