Blackboard
====

## Summary

A blackboard is a temporary shared space with specific controllers to solve a problem. <br />
The goal is to group specific analyze result and have controllers deciding the next step to acheived to a solution.

## Parts

### Id

A blackboard instance is identified by two keys : 
- A Guid, generate at the first creation
- A pair Board Name and Blackboard Template Name

### Template

A template define all the data rules and controller to apply.
At blackboard creation, the template used is copyied.
Any modification will this way be ignored to prevent any data corruption.

```csharp
var blackboardTemplate = BlackboardTemplate.Build("MathBlackboard", fixUid: blackboardId)
                                           .SetupControllers(c =>
                                           {
                                               c.Storage.UseDefault()
                                                .Event.UseController<IAutoComputeEventController, AutoComputeBlackboardOptions>(new AutoComputeBlackboardOptions(sumSequenceId));
                                           })
                                           .LogicalTypeConfiguration("Val[a-zA-Z]+", cfg: r =>
                                           {
                                               r.Storage("NumberValues")
                                                .MaxRecord(5,
                                                           preferenceResolution: BlackboardProcessingResolutionLimitTypeEnum.KeepNewest,
                                                           removeResolution: BlackboardProcessingResolutionRemoveTypeEnum.Decommission)
                                                .Order(42)
                                                .AllowType<int>(c => c.Range(0, 50))
                                                .AllowType<double>(c => c.Range(0, 50.42));
                                           })
                                           .LogicalTypeConfiguration("Result", r =>
                                           {
                                               r.OnlyOne(replacePreviousOne: true)
                                                .AllowType<int>(c => c.Min(0));
                                           })
                                           .Build();
```

### Logical Types

All stored data are qualified by a logical type in string format.
All configuration and search by the logical type support regex expression.

This allow taxonomie rules to create a advanced data managment.

### Controller

A controller is in charge to react and control a blackboard.
Multiple type of controller exist:

- Storage: Controller in charge to solve any data conflict
- Event : Controller in charge to react to any blackboard event (data action, ...)
- state : Controller in charge to evaluate the blackboard status if the data solve the issue

### Storage

Each data qualified by a logical type will follow the storage configuration define in the template.

### Access

A blackboard could be access and manipulate by two way:
- IBlackboardProvider : Service in charge to provide a proxy instance used to interraction directly
- Dedicated VGrain : Some grain exist to pull and push data directly on sequence