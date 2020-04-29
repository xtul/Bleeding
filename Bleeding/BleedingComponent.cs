using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;
using static Bleeding.Helpers;

namespace Bleeding {
	internal partial class BleedingBehavior {
		private class BleedingComponent : AgentComponent {
			private readonly Agent victim;
			private readonly Agent attacker;
			private readonly decimal tickDamage;
			private readonly Blow b;
			private readonly Config config;

			public BleedingComponent(Agent victim, Agent attacker, decimal tickDamage, Blow b, Config config) : base(victim) {
				this.victim = victim;
				this.attacker = attacker;
				this.tickDamage = tickDamage;
				this.b = b;
				this.config = config;
				Initialize();
			}

			protected override async void Initialize() {
				base.Initialize();
				await DealBleedingDamage();
			}

			private async Task DealBleedingDamage() {
				for (int tick = 0; tick < config.TickAmount; tick++) {
					await Task.Delay(config.SecondsBetweenTicks * 1000);
					if (victim.Health == 0)
						return;

					if (config.Debug) Say($"{victim.Name} took {tickDamage} tick damage. {victim.Health}/{victim.HealthLimit}");
					victim.Health -= (float)tickDamage;

					if (victim.Health <= 0) {
						victim.Die(new Blow() {
							OwnerId = attacker.Index,
							NoIgnore = true,
							BlowFlag = BlowFlags.ShrugOff,
							DamageType = b.DamageType
						});
					}
				}
				victim.RemoveComponent(this);
			}
		}
	}
}