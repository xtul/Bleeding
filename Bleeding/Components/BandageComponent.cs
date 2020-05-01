using System.Diagnostics.PerformanceData;
using System.Linq;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;
using static Bleeding.Helpers;

namespace Bleeding {
	public partial class BandageBehavior {
		internal class BandageComponent : AgentComponent {
			private readonly Agent agent;
			public int count;
			public BandageComponent(Agent agent, Config config) : base(agent) {
				this.agent = agent;
				count = config.Bandages.NpcCount;
			}

			protected override async void OnTickAsAI(float dt) {
				base.OnTickAsAI(dt);
				var bleeding = agent.GetComponent<BleedingBehavior.BleedingComponent>();
				if (bleeding != null && count > 0) {
					if (agent.Health < agent.HealthLimit / 2) {
						Announce($"{agent.Name} used a bandage.");
						var oldspeed = agent.GetCurrentSpeedLimit();
						agent.SetMaximumSpeedLimit(oldspeed * 0.3f, false);
						await Task.Delay(3000);
						agent.SetMaximumSpeedLimit(oldspeed, true);
						count--;
						bleeding.bandaged = true;
					}
				}
				await Task.Delay(3000); // cooldown
			}
		}
	}
}