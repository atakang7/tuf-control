using System;

namespace TufControl;

public interface IHardware : IDisposable
{
	int? Read(uint id);

	byte[] ReadCurve(uint id, int mode);

	void Write(uint id, byte[] payload);
}
