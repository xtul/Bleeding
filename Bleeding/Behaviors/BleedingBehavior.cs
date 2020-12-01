using System;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.GauntletUI.Mission.Singleplayer;
using TaleWorlds.MountAndBlade.ViewModelCollection.HUD.KillFeed;
using static Bleeding.Helpers;

namespace Bleeding {
	internal partial class BleedingBehavior : MissionBehaviour {
		private readonly Config _config;
		private readonly Mission _mission;

		// private SPKillFeedVM _hitFeed;

		public BleedingBehavior(Config config, Mission mission) {
			_config = config;
			_mission = mission;		
		}

		public override MissionBehaviourType BehaviourType => MissionBehaviourType.Other;

		public override void OnRenderingStarted() {
			/*	
			 *	I'm trying to get into the pre-made instance of player damage widget.
			 *	It took some time because it's name is pretty bad and
			 *	easily confused with agent death on the top right part of the screen.
			 *	Anyway, there's a method to find a layer by name and type, Layers.FindLayer<T>("name"). Useful.
			 *	However, since ALMOST ALL LAYERS ARE NAMED "GAUNTLETLAYER" IT'S COMPLETELY USELESS.
			 *	So we iterate over all layers on the screen. Also one of the layers is SceneLayer,
			 *	not ScreenLayer, which you can not cast to a GauntletLayer.
			 *	Now I recall a retired developer/closed beta player also having a meltdown over something similar.
			 *	Hours later: I realized I just need something that derives from MissionView. My frustration discovered a workaround. 
			*/

			/*
			 * 01.12.2020:
			 * Looks like they've moved things around. MissionGauntletGonnaKillMyselfLookingAtThisNameHandler is now in Native
			 * mod. I've been looking for this too long, man. I started to think they decided to disallow altering default UI.
			 * 
			 * It also started to crash and throw NullReferenceException when I call SPKillFeedVM.PersonalFeed.OnPersonalHit().
			 * Confusing name yet again. Disabled for now...
			*/

			// var killFeedHandler = Mission.Current.GetMissionBehaviour<MissionGauntletKillNotificationSingleplayerUIHandler>();
			// if (killFeedHandler != null) {
			// 	foreach (var screenLayer in killFeedHandler.MissionScreen.Layers) {
			// 		if (screenLayer.Name != "ScreenLayer") {
			// 			continue;
			// 		}
			// 		var gauntletLayer = (GauntletLayer)screenLayer;
			// 		var queryResult = (SPKillFeedVM)gauntletLayer._moviesAndDatasources?
			// 													.Where(x => x.Item1.MovieName == "SingleplayerKillfeed")?
			// 													.FirstOrDefault()?
			// 													.Item2;
			// 		if (queryResult != null) {
			// 			_hitFeed = queryResult;
			// 			return;
			// 		}
			// 	}
			// }
		}


		public override void OnRegisterBlow(Agent attacker, Agent victim, GameEntity realHitEntity, Blow b, ref AttackCollisionData collisionData, in MissionWeapon attackerWeapon) {
			// filter out unwanted blows
			if (victim == null) return;
			if (attacker == null) return;
			if (victim.IsMount || attacker.IsMount) return;
			// don't apply bleeding when player is in conversation
			if (_mission.Mode == MissionMode.Conversation) return; 
			if (_config.DisabledForPlayer && victim == Agent.Main) return;
			if (collisionData.AttackBlockedWithShield) return;
			if (b.InflictedDamage < _config.MinimumDamage) return;

			try {
				double tickDamage = (int)(b.InflictedDamage * _config.PercentageBled);

				tickDamage = tickDamage.ApplyMultipliers(b, collisionData, _config);
				if (_config.ReducedForNPCs.Enabled && victim != null && victim.Character != null && !victim.IsHero) tickDamage *= _config.ReducedForNPCs.Value;

				var bleeding = victim.GetComponent<BleedingComponent>();

				if (tickDamage != 0) {
					if (bleeding != null) {
						if (victim == Agent.Main) SayDarkRed("{=bleeding_worsened}Your bleeding got worse!");
						bleeding._tickDamage += (int)Math.Round(tickDamage * 0.35);
					} else {
						if (victim == Agent.Main) SayDarkRed("{=bleeding_started}You started bleeding.");
						victim.AddComponent(new BleedingComponent(victim, attacker, tickDamage, b, _config, _mission, null));

					}
				}

			} catch (Exception ex) { if (_config.Debug) Say(ex.Message + "\n" + ex.StackTrace); }
		}		
	}
}