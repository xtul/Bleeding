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
			if (victim == null) return;
			if (attacker == null) return;
			try {
				if (collisionData.AttackBlockedWithShield) return;
				if (b.InflictedDamage < config.MinimumDamage) return;

				decimal tickDamage = b.InflictedDamage * config.PercentageBled;
				if (b.DamageType == DamageTypes.Cut) tickDamage *= 1+config.CutMultiplier;
				if (b.DamageType == DamageTypes.Blunt) tickDamage *= 1+config.BluntMultiplier;
				if (b.DamageType == DamageTypes.Pierce) tickDamage *= 1+config.PierceMultiplier;
				if (b.DamageType == DamageTypes.Invalid) tickDamage *= 1+config.InvalidMultiplier;

				if (victim == Agent.Main) SayRed("You started bleeding.");

				if (collisionData.VictimHitBodyPart == BoneBodyPartType.Neck || collisionData.VictimHitBodyPart == BoneBodyPartType.BipedalLegs) {
					
				}
				if (tickDamage != 0) 
					victim.AddComponent(new BleedingComponent(victim, attacker, tickDamage, b, config));			

			} catch (Exception ex) { if (config.Debug) Say(ex.Message + "\n" + ex.StackTrace); }
			base.OnRegisterBlow(attacker, victim, realHitEntity, b, ref collisionData);
		}
	}
}