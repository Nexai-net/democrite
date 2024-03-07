Stream
====

## Summary

Use an EBS (Entreprise Bus Service) as storage an diffuseur of job to to.

Orlean can nativaly used different type of **ESB**. <br /> 
With Democrite we create connector through trigger to push and pull.

## How to configure trigger

Send to stream :
```csharp
// PUSH
var trigger = Trigger.Cron("*/25 * * * * *", "TGR: Push Every 25 sec")
                     
                     .AddTargetStream(streamDef) // <----- Add stream as target

                     .SetOutput(s => s.StaticCollection(Enumerable.Range(0, 50))
                                            .PullMode(PullModeEnum.Broadcast))
                     .Build();

```

Consume from stream :
```csharp
// PUSH
var fromStreamTrigger = Trigger.Stream(streamDef) // <----- Trigger that fire until theire is a message in the stream queue
                               .AddTargetSequence(consumeSeq.Uid)

                               // Limit the number of concurrent execution, Prevent consuming all messages without resources in the cluster to process them (CPU, RAM, ...)
                               .MaxConcurrentProcess(2)

                               .Build();

```