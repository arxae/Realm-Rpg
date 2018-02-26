namespace RealmRpgBot.JobScheduler.Jobs
{
	public class InvalidateSettingsCache : FluentScheduler.IJob
	{
		public void Execute()
		{
			Realm.ClearSettingsCache();
		}
	}
}
