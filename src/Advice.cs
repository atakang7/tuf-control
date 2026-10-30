namespace TufControl;

public record Advice(string Id, AdviceKind Kind, string Title, string Why, string Fix, object[] Args, AdviceAction? Action = null);
