namespace CluedIn.Contrib.Submitter.Mapping;

public readonly record struct CodeConfiguration(string Origin, string Id);

public readonly record struct EdgeConfiguration(string EdgeType, string EntityType, string Origin, string Id);
