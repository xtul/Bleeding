using SandBox.View.Menu;
using System;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Library;
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
			public decimal tickDamage;
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

			[HandleProcessCorruptedStateExceptions]
			private async Task DealBleedingDamage() {
				try {
					decimal ticks = 0.8m;
					float oldSpeed = 0;
					var victimEnemyOnInit = victim.IsEnemyOf(attacker);

					// slow the agent for the duration of bleeding
					if (config.SlowOnBleed.Enabled) { 
						oldSpeed = victim.GetCurrentSpeedLimit();
						victim.SetMaximumSpeedLimit(oldSpeed * config.SlowOnBleed.Value, false);					
					}

					while (true) {
						await Task.Delay(config.SecondsBetweenTicks * 1000);
						if (mission.Mode == MissionMode.Conversation) break;
						if (bandaged) break;
						if (tickDamage < 1m) break;
						// don't process further if victim died due to other means
						if (victim.State == AgentState.Killed 
						|| victim.State == AgentState.Deleted 
						|| victim.Health == 0) {
							break;
						}
						// if enemy/friend status change since the bleeding started, cancel bleeding
						if (!mission.MissionEnded() && victimEnemyOnInit == true && victim.IsFriendOf(attacker)) break;

						// drop off the damage based on time passed and set bleed rate
						tickDamage *= ticks;
						ticks *= config.BleedRate;

						// finally, reduce health
						victim.Health -= (float)tickDamage;

						// display player related bleedings
						if (config.DisplayPlayerEffects) {
							if (victim == Agent.Main)
								SayDarkRed("{=bleeding_damage}You took {DAMAGE} bleeding damage.".Replace("{DAMAGE}", tickDamage.ToString("N2")));
							if (attacker == Agent.Main)
								SayLightRed("{=bleeding_dealt}Your attacks caused {DAMAGE} bleeding damage.".Replace("{DAMAGE}", tickDamage.ToString("N2")));
						}
						if (config.Debug && !mission.MissionEnded())
							Say($"{victim.Name} took {tickDamage} tick damage. {victim.Health}/{victim.HealthLimit}");

						// finish off the target. otherwise health will reach negative numbers and victim will still live
						// IMPORTANT NOTE EOEOEOEOE DONT SKIP
						// checking if mission is ended is very fucking important because doing anything with 
						// agents after mission is closed will fuck everything up (also known as access violation exception)
						if (!mission.MissionEnded() && victim.Health <= 0) {
							if (config.Debug) Say($"{victim.Name} should die");
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

					// restore old speed
					if (!mission.MissionEnded() && config.SlowOnBleed.Enabled)
						victim.SetMaximumSpeedLimit(oldSpeed, false);
				
				// this is probably bad practice but it's better than crashing out of nowhere...
				} catch (Exception ex) { if (config.Debug) Say(ex.Message); }
				if (!mission.MissionEnded()) victim.RemoveComponent(this);
			}
		}
	}
}