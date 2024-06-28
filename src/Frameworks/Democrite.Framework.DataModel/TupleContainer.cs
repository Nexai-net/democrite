using Orleans;

using System.ComponentModel;

[Immutable]
[Serializable]
[GenerateSerializer]
[ImmutableObject(true)]
public record struct TupleContainer<TArg0>(TArg0 Item0);

[Immutable]
[Serializable]
[GenerateSerializer]
[ImmutableObject(true)]
public record struct TupleContainer<TArg0, TArg1>(TArg0 Item0, TArg1 Item1);

[Immutable]
[Serializable]
[GenerateSerializer]
[ImmutableObject(true)]
public record struct TupleContainer<TArg0, TArg1, TArg2>(TArg0 Item0, TArg1 Item1, TArg2 Item2);

[Immutable]
[Serializable]
[GenerateSerializer]
[ImmutableObject(true)]
public record struct TupleContainer<TArg0, TArg1, TArg2, TArg3>(TArg0 Item0, TArg1 Item1, TArg2 Item2, TArg3 Item3);

[Immutable]
[Serializable]
[GenerateSerializer]
[ImmutableObject(true)]
public record struct TupleContainer<TArg0, TArg1, TArg2, TArg3, TArg4>(TArg0 Item0, TArg1 Item1, TArg2 Item2, TArg3 Item3, TArg4 Item4);

[Immutable]
[Serializable]
[GenerateSerializer]
[ImmutableObject(true)]
public record struct TupleContainer<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5>(TArg0 Item0, TArg1 Item1, TArg2 Item2, TArg3 Item3, TArg4 Item4, TArg5 Item5);

[Immutable]
[Serializable]
[GenerateSerializer]
[ImmutableObject(true)]
public record struct TupleContainer<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(TArg0 Item0, TArg1 Item1, TArg2 Item2, TArg3 Item3, TArg4 Item4, TArg5 Item5, TArg6 Item6);

[Immutable]
[Serializable]
[GenerateSerializer]
[ImmutableObject(true)]
public record struct TupleContainer<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(TArg0 Item0, TArg1 Item1, TArg2 Item2, TArg3 Item3, TArg4 Item4, TArg5 Item5, TArg6 Item6, TArg7 Item7);

[Immutable]
[Serializable]
[GenerateSerializer]
[ImmutableObject(true)]
public record struct TupleContainer<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>(TArg0 Item0, TArg1 Item1, TArg2 Item2, TArg3 Item3, TArg4 Item4, TArg5 Item5, TArg6 Item6, TArg7 Item7, TArg8 Item8);

