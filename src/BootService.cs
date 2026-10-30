using System.ServiceProcess;
using System.Threading;

namespace TufControl;

public sealed class BootService(string[] args) : ServiceBase
{
	public const string Name = "TufControlBoot";

	private Thread? worker;

	protected override void OnStart(string[] startArgs)
	{
		worker = new Thread(() =>
		{
			try
			{
				BootRunner.Run(args);
			}
			finally
			{
				try
				{
					Stop();
				}
				catch
				{
				}
			}
		})
		{
			IsBackground = true,
			Name = "TufControl boot apply"
		};
		worker.Start();
	}

	public static int Run(string[] args)
	{
		ServiceBase.Run(new BootService(args)
		{
			ServiceName = "TufControlBoot",
			CanStop = true,
			AutoLog = false
		});
		return 0;
	}
}
