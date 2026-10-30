namespace TufControl;

// Match: true = read back and equal, false = read back and different, null = firmware offers no readback.
public record VerifyItem(string Name, string Expected, string Actual, bool? Match);
