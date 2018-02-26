namespace RealmRpgBot.JobScheduler
{
	using System;

	using FluentScheduler;

	using Jobs;

	public class RealmJobRegistry : Registry
	{
		public RealmJobRegistry()
		{
			Schedule<LocationInventoryDecayJob>().ToRunOnceAt(DateTime.Now.AddSeconds(60 - DateTime.Now.Second)).AndEvery(1).Minutes();
			Schedule<InvalidateSettingsCache>().ToRunOnceAt(DateTime.Now.AddMinutes(60 + (60 - DateTime.Now.Minute))).AndEvery(2).Hours();
		}
	}
}