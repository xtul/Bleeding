﻿using System;
using System.Linq;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;
using static Bleeding.Helpers;

namespace Bleeding {
	public partial class BandageBehavior : MissionBehaviour {
		public override MissionBehaviourType BehaviourType => MissionBehaviourType.Other;

		public int count;
		private readonly Config config;

		public BandageBehavior(Config config) {
			count = config.Bandages.PlayerCount;
			this.config = config;
		}
		public override void OnAgentCreated(Agent agent) {
			if (agent == null) return;
			if (agent.Character == null) return;
			if (agent.IsPlayerControlled) return;

			if (agent != null && agent.IsHero) {
				SayGreen($"{agent.Name} received a bandage.");
				agent.AddComponent(new BandageComponent(agent, config));
			}
			base.OnAgentCreated(agent);
		}

		public override async void OnMissionTick(float dt) {
			if (Input.IsKeyReleased((InputKey)config.Bandages.KeyCode)) {
				var player = Agent.Main;
				var medicine = player.Character?.GetSkillValue(DefaultSkills.Medicine);
				if (medicine == null) medicine = 0;
				var applicationTime = 2000 - medicine * 2;
				var bleeding = player.GetComponent<BleedingBehavior.BleedingComponent>();
				if (count < 1) {
					SayGreen("You have no bandages left.");
					return;
				} else {
					SayGreen("You started bandaging.");
					var oldspeed = player.GetCurrentSpeedLimit();
					player.SetMaximumSpeedLimit(oldspeed * 0.1f, false);
					await Task.Delay(TimeSpan.FromMilliseconds((int)applicationTime));
					player.SetMaximumSpeedLimit(oldspeed, true);
					count--;
					if (bleeding != null) {
						bleeding.bandaged = true;
						SayGreen("Your bandage successfully stopped the bleeding.");
					}
					if (medicine > config.Bandages.MinimumMedicine) {
						var formula = ((int)medicine - config.Bandages.MinimumMedicine) * 0.15f;
						player.Health += formula;
						player.Health.Clamp(0, player.HealthLimit);
						SayGreen($"You healed {formula} damage.");
					}
					SayGreen($"You have {count} bandages left.");
				}
				//Agent.Main.AddComponent(new BandageComponent(Agent.Main));
			}
		}
	}
}