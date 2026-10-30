using System;

namespace TufControl;

public interface IThermal : IDisposable
{
	bool Available { get; }

	int Prepare();

	void Set(int celsius);

	int Read();

	void Restore(int celsius);
}
