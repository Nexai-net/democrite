namespace Democrite.Sample.Blackboard.Memory.Models
{
    using System.ComponentModel;
    using System.Runtime.Serialization;

    [Immutable]
    [DataContract]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record class RecordDisplay(double Value, string LogicalType, string RecordStatus);
}
