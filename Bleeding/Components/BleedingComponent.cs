using System;
using System.Linq;
using TaleWorlds.Core;
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

			private readonly System.Timers.Timer _timer;
			private double _dropRate;

			private bool _enabled;

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

				_dropRate = 0.8f;
				_timer = new System.Timers.Timer() {
					Interval = _config.SecondsBetweenTicks * 1000,
					AutoReset = false
				};

				_enabled = true;

				DealBleedingDamage();
			}

			private void DealBleedingDamage() {
				if (_mission == null) return;
				if (_victim.Health <= 0) return;
				if (!_enabled) return;

				var victimEnemyOnInit = _victim.IsEnemyOf(_attacker);

				// slow the agent for the duration of bleeding
				if (_config.SlowOnBleed.Enabled) {
					_victim.SetMaximumSpeedLimit(_config.SlowOnBleed.Value, true);
				}

				// start bleeding
				_timer.Start();
				_timer.Elapsed += BleedingTick;

				// restore old speed
				if (_mission != null && _config.SlowOnBleed.Enabled)
					_victim.SetMaximumSpeedLimit(1f, true);

				if (_mission != null) _victim.RemoveComponent(this);
			}

			private void BleedingTick(object sender, System.Timers.ElapsedEventArgs e) {
				/*
				 * If you're a TW developer - I really wish there was support for DoT 
				 * effects, this component is constantly crashing the engine
				 * 
				 * As you can see, most of the stuff below is monumentally stupid. Prior to 1.5.4 (or even 1.5.3)
				 * this thing "sort of worked", obviously at a cost, since it keeps checking if the agent/mission exists
				 * all the time to avoid an access violation exception. Likely in vain, but I was desperate.
				 * 
				 * I think there's a need for this kind of thing because obviously the engine doesn't like something 
				 * that happens here, and DoT/HoT effects would definitely be a thing in fantasy mods.
				*/
				#region null checks
				if (_mission == null) {
					UnregisterBleeding();
					return;
				}
				if (_mission.Mode == MissionMode.Conversation) {
					UnregisterBleeding();
					return;
				}
				if (_bandaged) {
					UnregisterBleeding();
					return;
				}
				if (_tickDamage <= 1f) {
					UnregisterBleeding();
					return;
				}
				if (_mission.Agents?.Where(x => x == _victim)?.FirstOrDefault() == null) {
					UnregisterBleeding();
					return;
				}
				// don't process further if victim is dead
				if (_victim?.State == AgentState.Killed ||
					_victim?.State == AgentState.Deleted ||
					_victim?.Health <= 0) {
					UnregisterBleeding();
					return;
				}
				#endregion null checks

				// drop off the damage based on time passed and set bleed rate
				_tickDamage *= _dropRate;
				// reduce drop rate as per config
				_dropRate *= _config.BleedRate;
				// update timer interval
				_timer.Interval = _config.SecondsBetweenTicks * 1000 * (_dropRate + 1);

				// take damage
				if (_enabled && _mission != null) {
					if (_config.StaggerOnTick.Enabled && _tickDamage >= _config.StaggerOnTick.Value) {
						var tickBlow = _b;
						tickBlow.InflictedDamage = (int)_tickDamage;
						tickBlow.BlowFlag = BlowFlags.NoSound;
						if (_enabled && _mission.IsMissionEnding == false) _victim.RegisterBlow(tickBlow);
					} else _victim.Health -= (int)_tickDamage;
				}

				// finish off the target if necessary
				if (_enabled && _mission != null && _victim.Health <= 0) {
					var killBlow = _b;
					killBlow.DamageType = DamageTypes.Blunt;
					killBlow.BlowFlag = BlowFlags.NoSound;
					_victim.Die(killBlow);
				}

				// display player related bleeding ticks
				if (_enabled && _config.DisplayPlayerEffects) {
					if (_victim == Agent.Main)
						SayDarkRed("{=bleeding_damage}You took {DAMAGE} bleeding damage.".Replace("{DAMAGE}", _tickDamage.ToString("N2")));
					if (_attacker == Agent.Main && _hitFeed != null)
						// the try-catching is painful. Game crashes when adding bleeding damage to thie feed.
						try {
							_hitFeed.PersonalFeed.OnPersonalHit((int)Math.Ceiling(_tickDamage), _victim.Health <= 0, false, false, _victim.Name);
						} catch { }
				}
			}

			private void UnregisterBleeding() {
				_timer.Elapsed -= BleedingTick;
				_timer.Stop();
				_victim.RemoveComponent(this);
			}

			protected override void OnAgentRemoved() => _enabled = false;
		}
	}
}