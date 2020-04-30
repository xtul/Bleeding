using System;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;
using static Bleeding.Helpers;

namespace Bleeding {
	internal partial class BleedingBehavior {
		private class BleedingComponent : AgentComponent {
			private readonly Agent victim;
			private readonly Agent attacker;
			private readonly Blow b;
			private readonly Config config;
			private decimal tickDamage;

			public BleedingComponent(Agent victim, Agent attacker, decimal tickDamage, Blow b, Config config) : base(victim) {
				this.victim = victim;
				this.attacker = attacker;
				this.b = b;
				this.config = config;
				this.tickDamage = tickDamage;
				Initialize();
			}

			protected override async void Initialize() {
				base.Initialize();
				await DealBleedingDamage();
			}

			private async Task DealBleedingDamage() {
				decimal ticks = 1;
				while (true) {
					tickDamage *= ticks;
					ticks *= config.BleedRate;
					await Task.Delay(config.SecondsBetweenTicks * 1000);
					if (victim.Health == 0)
						return;
					if (victim == Agent.Main) SayRed($"You suffered {tickDamage:N2} bleeding damage.");

					victim.Health -= (float)tickDamage;
					if (config.Debug) Say($"{victim.Name} took {tickDamage} tick damage. {victim.Health}/{victim.HealthLimit}");

					if (victim.Health <= 0) {
						victim.Die(new Blow() {
							OwnerId = attacker.Index,
							NoIgnore = true,
							BlowFlag = BlowFlags.NoSound,
							DamageType = b.DamageType
						});
						break;
					}
					if (tickDamage < (decimal)0.1) {
						break;
					}
				}
				victim.RemoveComponent(this);
			}
		}
	}
}