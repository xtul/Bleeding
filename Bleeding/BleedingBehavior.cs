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

		public BleedingBehavior(Config config) {
			this.config = config;
		}

		public override MissionBehaviourType BehaviourType => MissionBehaviourType.Other;

		public override void OnRegisterBlow(Agent attacker, Agent victim, GameEntity realHitEntity, Blow b, ref AttackCollisionData collisionData) {
			try {
				if (collisionData.AttackBlockedWithShield) return;
				if (b.InflictedDamage < config.MinimumDamage) return;

				var tickms = config.SecondsBetweenTicks * 1000;

				decimal tickDamage = 0;
				if (b.DamageType == DamageTypes.Cut) tickDamage = b.InflictedDamage * config.PercentageBled * config.CutMultiplier;
				if (b.DamageType == DamageTypes.Blunt) tickDamage = b.InflictedDamage * config.PercentageBled * config.BluntMultiplier;
				if (b.DamageType == DamageTypes.Pierce) tickDamage = b.InflictedDamage * config.PercentageBled * config.PierceMultiplier;
				if (b.DamageType == DamageTypes.Invalid) tickDamage = b.InflictedDamage * config.PercentageBled * config.InvalidMultiplier;

				/* i can't believe i managet to make it work		 *
				 * when the butterlord finally takes me, please tell *
				 * my family i suffered greatly						 */
				victim.AddComponent(new BleedingComponent(victim, attacker, tickDamage, b, config));			

			} catch (Exception ex) { if (config.Debug) Say(ex.Message + "\n" + ex.StackTrace); }
			base.OnRegisterBlow(attacker, victim, realHitEntity, b, ref collisionData);
		}
	}
}