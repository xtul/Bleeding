﻿using System;
using System.Linq;
using System.Windows.Forms;
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

		private SPKillFeedVM _hitFeed;

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
			var killFeedHandler = Mission.Current.GetMissionBehaviour<MissionGauntletKillNotificationSingleplayerUIHandler>();
			foreach (var screenLayer in killFeedHandler.MissionScreen.Layers) {
				if (screenLayer.Name != "ScreenLayer") {
					continue;
				}
				var gauntletLayer = (GauntletLayer)screenLayer;
				var queryResult = (SPKillFeedVM)gauntletLayer._moviesAndDatasources?
															.Where(x => x.Item1.MovieName == "SingleplayerKillfeed")?
															.FirstOrDefault()?
															.Item2;
				if (queryResult != null) {
					_hitFeed = queryResult;
					return;
				}
			}
		}

		public override void OnRegisterBlow(Agent attacker, Agent victim, GameEntity realHitEntity, Blow b, ref AttackCollisionData collisionData) {
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
						bleeding._tickDamage += (int)Math.Round(tickDamage * 0.35);
						if (victim == Agent.Main) SayDarkRed("{=bleeding_worsened}Your bleeding got worse!");
					} else {
						if (victim == Agent.Main && bleeding == null) SayDarkRed("{=bleeding_started}You started bleeding.");
						victim.AddComponent(new BleedingComponent(victim, attacker, tickDamage, b, _config, _mission, _hitFeed));
					}
				}

			} catch (Exception ex) { if (_config.Debug) Say(ex.Message + "\n" + ex.StackTrace); }
		}

		public override void OnClearScene() {
			var agentList = _mission.Agents
									.Where(x => x != null)
									.Where(x => x.IsHuman);

			foreach (var agent in agentList) {
				var bleeding = agent.GetComponent<BleedingComponent>();
				if (bleeding != null) {
					agent.RemoveComponent(bleeding);
				}
			}
		}
	}
}