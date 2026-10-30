namespace TufControl;

public sealed class AppRule
{
	public string Process { get; set; } = "";

	public string Profile { get; set; } = "";

	public AppRule Clone()
	{
		return new AppRule
		{
			Process = Process,
			Profile = Profile
		};
	}
}
