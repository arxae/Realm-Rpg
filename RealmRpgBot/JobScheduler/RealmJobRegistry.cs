﻿namespace RealmRpgBot.JobScheduler
{
	using System;

	using FluentScheduler;

	using Jobs;

	public class RealmJobRegistry : Registry
	{
		public RealmJobRegistry()
		{
			Schedule<ServerTick>().ToRunOnceAt(DateTime.Now.AddSeconds(60 - DateTime.Now.Second)).AndEvery(1).Minutes();
			Schedule<LocationAttackTick>().ToRunOnceAt(DateTime.Now.AddSeconds(15 - DateTime.Now.Second)).AndEvery(5).Minutes();
			Schedule<LocationInventoryDecay>().ToRunOnceAt(DateTime.Now.AddSeconds(60 - DateTime.Now.Second)).AndEvery(1).Minutes();
			Schedule<InvalidateSettingsCache>().ToRunOnceAt(DateTime.Now.AddMinutes(60 + (60 - DateTime.Now.Minute))).AndEvery(3).Hours();
		}
	}
}