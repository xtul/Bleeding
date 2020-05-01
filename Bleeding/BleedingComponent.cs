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
				try {
					decimal ticks = 1;
					float oldSpeed = 0;
					if (config.SlowOnBleed.Enabled)
						oldSpeed = victim.GetCurrentSpeedLimit();
					while (true) {
						if (tickDamage < (decimal)0.1) {
							break;
						}

						tickDamage *= ticks;
						ticks *= config.BleedRate;
						if (config.SlowOnBleed.Enabled)
							victim.SetMaximumSpeedLimit(oldSpeed * config.SlowOnBleed.Value, false);

						await Task.Delay(config.SecondsBetweenTicks * 1000);

						if (victim.Health == 0)
							return;

						if (config.DisplayPlayerEffects) {
							if (victim == Agent.Main)
								SayRed($"You suffered {tickDamage:N2} bleeding damage.");
							if (attacker == Agent.Main)
								SayPink($"Your attacks caused {tickDamage:N2} bleeding damage.");
						}

						victim.Health -= (float)tickDamage;
						if (config.Debug)
							Say($"{victim.Name} took {tickDamage} tick damage. {victim.Health}/{victim.HealthLimit}");

						if (victim.Health <= 0) {
							victim.Die(new Blow() {
								OwnerId = attacker.Index,
								NoIgnore = true,
								BlowFlag = BlowFlags.NoSound,
								DamageType = b.DamageType
							});
							break;
						}
					}
					if (config.SlowOnBleed.Enabled)
						victim.SetMaximumSpeedLimit(oldSpeed, false);
				} catch (Exception ex) { if (config.Debug) Say(ex.Message); }
				victim.RemoveComponent(this);
			}
		}
	}
}