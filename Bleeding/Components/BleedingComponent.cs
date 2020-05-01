using SandBox.View.Menu;
using System;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using static Bleeding.Helpers;

namespace Bleeding {
	internal partial class BleedingBehavior {
		public class BleedingComponent : AgentComponent {
			private readonly Agent victim;
			private readonly Agent attacker;
			private readonly Blow b;
			private readonly Config config;
			private readonly Mission mission;
			private decimal tickDamage;
			public bool bandaged;

			public BleedingComponent(Agent victim, Agent attacker, decimal tickDamage, Blow b, Config config, Mission mission) : base(victim) {
				this.victim = victim;
				this.attacker = attacker;
				this.b = b;
				this.config = config;
				this.mission = mission;
				this.tickDamage = tickDamage;
				this.bandaged = false;
				Initialize();
			}

			protected override async void Initialize() {
				base.Initialize();
				await DealBleedingDamage();
			}

			protected override void OnStopUsingGameObject() {
				base.OnStopUsingGameObject();
				Say("lol");
			}

			[HandleProcessCorruptedStateExceptions]
			private async Task DealBleedingDamage() {
				try {
					decimal ticks = 1;
					float oldSpeed = 0;
					if (config.SlowOnBleed.Enabled)
						oldSpeed = victim.GetCurrentSpeedLimit();
					while (true) {
						if (!bandaged) await Task.Delay(config.SecondsBetweenTicks * 1000);
						if (mission.Mode == MissionMode.Conversation) { 
							break;
						}
						if (bandaged) {
							break;
						}
						if (tickDamage < 1) {
							break;
						}
						// don't process further if victim died due to other means
						if ((victim.State == AgentState.Killed || victim.State == AgentState.Deleted) || victim.Health == 0) {
							break;
						}

						tickDamage *= ticks;
						ticks *= config.BleedRate;
						if (config.SlowOnBleed.Enabled)
							victim.SetMaximumSpeedLimit(oldSpeed * config.SlowOnBleed.Value, false);


						if (config.DisplayPlayerEffects) {
							if (victim == Agent.Main)
								SayDarkRed($"You suffered {tickDamage:N2} bleeding damage.");
							if (attacker == Agent.Main)
								SayLightRed($"Your attacks caused {tickDamage:N2} bleeding damage @{b.VictimBodyPart}.");
						}

						victim.Health -= (float)tickDamage;
						if (config.Debug)
							Say($"{victim.Name} took {tickDamage} tick damage. {victim.Health}/{victim.HealthLimit}");

						// finish off the target. otherwise health will reach negative numbers and victim will still live
						// IMPORTANT NOTE EOEOEOEOE DONT SKIP
						// checking if mission is ended is very fucking important because doing anything with 
						// agents after mission is closed will fuck everything up (also known as access violation exception)
						if (!mission.MissionEnded() && victim.Health <= 0) {
							Say($"{victim.Name} should die");
							victim.Die(new Blow {
								OwnerId = attacker.Index,
								NoIgnore = true,
								BaseMagnitude = 0,
								VictimBodyPart = b.VictimBodyPart,
								DamageType = DamageTypes.Blunt
							});
							victim.RemoveComponent(this);
							break;
						}
					}
					if (!mission.MissionEnded() && config.SlowOnBleed.Enabled)
						victim.SetMaximumSpeedLimit(oldSpeed, false);
				} catch (Exception ex) { if (config.Debug) Say(ex.Message); }
				victim.RemoveComponent(this);
			}
		}
	}
}