namespace RealmRpgBot.JobScheduler
{
	using System;
	using FluentScheduler;

	public class Scheduler
	{
		static Serilog.ILogger log;

		public static void Initialize()
		{
			log = Serilog.Log.ForContext<Scheduler>();

			JobManager.JobException += msg => log.Error(msg.Exception, "Scheduling error with job {jobname}:", msg.Name);
			JobManager.JobStart += info => log.Debug("Started job: {jobname}", info.Name);
			JobManager.JobEnd += info => log.Debug("Ended job: {jobname}", info.Name);

			JobManager.Initialize(new RealmJobRegistry());
		}
	}
}
