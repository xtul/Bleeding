using System;
using System.Linq;
using System.Threading;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using static Bleeding.Helpers;

namespace Bleeding {
	internal partial class BleedingBehavior : MissionBehaviour {
		private readonly Config config;
		private readonly Mission mission;

		public BleedingBehavior(Config config, Mission mission) {
			this.config = config;
			this.mission = mission;
		}

		public override MissionBehaviourType BehaviourType => MissionBehaviourType.Other;

		public override void OnRegisterBlow(Agent attacker, Agent victim, GameEntity realHitEntity, Blow b, ref AttackCollisionData collisionData) {
			// filter out unwanted blows
			if (victim == null) return;
			if (attacker == null) return;
			// don't you love it when you talk to your spouse on the field and she faints and dies after a while
			if (mission.Mode == MissionMode.Conversation) return; 
			if (config.DisabledForPlayer && victim == Agent.Main) return;
			if (collisionData.AttackBlockedWithShield) return;
			if (b.InflictedDamage < config.MinimumDamage) return;

			try {

				decimal tickDamage = b.InflictedDamage * config.PercentageBled;

				tickDamage = tickDamage.ApplyMultipliers(b, collisionData, config);
				if (config.ReducedForNPCs.Enabled && victim != null && !victim.IsHero) tickDamage *= config.ReducedForNPCs.Value;

				var bleeding = victim.GetComponent<BleedingComponent>();

				if (tickDamage != 0) {
					if (bleeding != null) {
						bleeding.tickDamage += tickDamage * 0.35m;
						if (victim == Agent.Main) SayDarkRed("{=bleeding_worsened}Your bleeding got worse!");
					}
					if (victim == Agent.Main && bleeding == null) SayDarkRed("{=bleeding_started}You started bleeding.");
					victim.AddComponent(new BleedingComponent(victim, attacker, tickDamage, b, config, mission));
				}

			} catch (Exception ex) { if (config.Debug) Say(ex.Message + "\n" + ex.StackTrace); }
			base.OnRegisterBlow(attacker, victim, realHitEntity, b, ref collisionData);
		}
	}
}