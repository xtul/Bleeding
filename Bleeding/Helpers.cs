using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bleeding {
	public static class Helpers {
		public static void Say(string text) {
			InformationManager.DisplayMessage(new InformationMessage(text, new Color(0.5f, 0.5f, 0.5f)));
		}

		public static void Announce(string text) {
			InformationManager.AddQuickInformation(new TaleWorlds.Localization.TextObject(text));
		}

		public static void SayDarkRed(string text) {
			InformationManager.DisplayMessage(new InformationMessage(text, new Color((float)0.8, 0, 0)));
		}

		public static void SayLightRed(string text) {
			InformationManager.DisplayMessage(new InformationMessage(text, new Color(1, 0, 0)));
		}

		public static void SayGreen(string text) {
			InformationManager.DisplayMessage(new InformationMessage(text, new Color(0, 1, 0)));
		}

		// https://stackoverflow.com/a/2683487/
		/// <summary>
		/// Clamps a value between <paramref name="min"/> and <paramref name="max"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="val"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T> {
			if (val.CompareTo(min) < 0)
				return min;
			else if (val.CompareTo(max) > 0)
				return max;
			else
				return val;
		}

		/// <summary>
		/// Applies multipliers according to configuration file.
		/// </summary>
		/// <param name="tickDamage"></param>
		/// <param name="b"></param>
		/// <param name="collisionData"></param>
		/// <param name="config"></param>
		/// <returns></returns>
		public static decimal ApplyMultipliers(this decimal tickDamage, Blow b, AttackCollisionData collisionData, Config config) {
			if (b.DamageType == DamageTypes.Cut)
				tickDamage *= 1 + config.CutMultiplier;
			if (b.DamageType == DamageTypes.Blunt)
				tickDamage *= 1 + config.BluntMultiplier;
			if (b.DamageType == DamageTypes.Pierce)
				tickDamage *= 1 + config.PierceMultiplier;
			if (b.DamageType == DamageTypes.Invalid)
				tickDamage *= 1 + config.InvalidMultiplier;

			if (collisionData.VictimHitBodyPart == BoneBodyPartType.Neck)
				tickDamage *= 1 + config.BodyMultipliers.Neck.Mult;
			if (collisionData.VictimHitBodyPart == BoneBodyPartType.Head)
				tickDamage *= 1 + config.BodyMultipliers.Head.Mult;
			if (collisionData.VictimHitBodyPart == BoneBodyPartType.BipedalArmLeft || collisionData.VictimHitBodyPart == BoneBodyPartType.BipedalArmRight)
				tickDamage *= 1 + config.BodyMultipliers.Arms.Mult;
			if (collisionData.VictimHitBodyPart == BoneBodyPartType.BipedalLegs)
				tickDamage *= 1 + config.BodyMultipliers.Legs.Mult;
			if (collisionData.VictimHitBodyPart == BoneBodyPartType.ShoulderLeft || collisionData.VictimHitBodyPart == BoneBodyPartType.ShoulderRight)
				tickDamage *= 1 + config.BodyMultipliers.Shoulders.Mult;

			return tickDamage;
		}
	}
}
