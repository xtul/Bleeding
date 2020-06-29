using SandBox.View.Menu;
using System;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection.HUD.KillFeed;
using static Bleeding.Helpers;

namespace Bleeding {
	internal partial class BleedingBehavior {
		public class BleedingComponent : AgentComponent {
			private readonly Agent _victim;
			private readonly Agent _attacker;
			private readonly Blow _b;
			private readonly Config _config;
			private readonly Mission _mission;
			private readonly SPKillFeedVM _hitFeed;

			public double _tickDamage;
			public bool _bandaged;

			public BleedingComponent(Agent victim, Agent attacker, double tickDamage, Blow b, Config config, Mission mission, SPKillFeedVM hitFeed) : base(victim) {
				_victim = victim;
				_attacker = attacker;
				_b = b;
				_config = config;
				_mission = mission;
				_tickDamage = tickDamage;
				_bandaged = false;
				_hitFeed = hitFeed;
				Initialize();
			}

			protected override async void Initialize() {
				base.Initialize();
				await DealBleedingDamage();
			}

			private async Task DealBleedingDamage() {
				try {
					double ticks = 0.8;
					var victimEnemyOnInit = _victim.IsEnemyOf(_attacker);

					// slow the agent for the duration of bleeding
					if (_config.SlowOnBleed.Enabled) { 
						_victim.SetMaximumSpeedLimit(_config.SlowOnBleed.Value, true);					
					}

					while (true) {
						await Task.Delay(_config.SecondsBetweenTicks * 1000);
						if (_mission == null) break;
						if (_mission.Mode == MissionMode.Conversation) break;
						if (_bandaged) break;
						if (_tickDamage <= 1) break;
						// don't process further if victim died due to other means
						if (_victim?.State == AgentState.Killed ||
							_victim?.State == AgentState.Deleted ||
							_victim?.Health <= 0) {
							break;
						}

						// drop off the damage based on time passed and set bleed rate
						_tickDamage *= ticks;
						ticks *= _config.BleedRate;

						// finally, take damage
						if (_mission != null) {
							if (_config.StaggerOnTick.Enabled && _tickDamage >= _config.StaggerOnTick.Value) {
								var tickB = _b;
								tickB.InflictedDamage = (int)_tickDamage;
								tickB.BlowFlag = BlowFlags.NoSound;
								if (_mission != null) _victim.RegisterBlow(tickB);
							} else _victim.Health -= (int)_tickDamage;
						}
						
						// finish off the target if necessary
						if (_mission != null && _victim.Health <= 0) {
							var killB = _b;
							killB.DamageType = DamageTypes.Blunt;
							killB.BlowFlag = BlowFlags.NoSound;
							if (_mission != null) _victim.Die(killB);
						}

						// display player related bleedings
						if (_config.DisplayPlayerEffects) {
							if (_victim == Agent.Main)
								SayDarkRed("{=bleeding_damage}You took {DAMAGE} bleeding damage.".Replace("{DAMAGE}", _tickDamage.ToString("N2")));
							if (_attacker == Agent.Main)
								_hitFeed.PersonalFeed.OnPersonalHit((int)Math.Ceiling(_tickDamage), _victim.Health <= 0, false, false, _victim.Name);
						}	
					}

					// restore old speed
					if (_mission != null && _config.SlowOnBleed.Enabled)
						_victim.SetMaximumSpeedLimit(1f, true);
				
				// this is probably bad practice but it's better than crashing out of nowhere...
				} catch (Exception ex) {
					if (_config.Debug) {
						Say(ex.Message);
						System.Windows.Forms.Clipboard.SetText(ex.StackTrace);
					}
				}
				if (_mission != null) _victim.RemoveComponent(this);
			}
		}
	}
}