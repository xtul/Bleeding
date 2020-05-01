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
			if (victim == null) return;
			if (attacker == null) return;
			if (mission.Mode == MissionMode.Conversation) return;
			if (config.DisabledForPlayer && victim == Agent.Main) return;
			try {
				if (collisionData.AttackBlockedWithShield) return;
				if (b.InflictedDamage < config.MinimumDamage) return;

				decimal tickDamage = b.InflictedDamage * config.PercentageBled;

				tickDamage = tickDamage.ApplyMultipliers(b, collisionData, config);

				if (tickDamage != 0) {
					if (victim == Agent.Main) SayRed("You started bleeding.");
					victim.AddComponent(new BleedingComponent(victim, attacker, tickDamage, b, config, mission));
				}

			} catch (Exception ex) { if (config.Debug) Say(ex.Message + "\n" + ex.StackTrace); }
			base.OnRegisterBlow(attacker, victim, realHitEntity, b, ref collisionData);
		}
	}
}