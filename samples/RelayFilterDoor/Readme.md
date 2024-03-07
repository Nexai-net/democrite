RelayFilterDoor
====

## Goal

A signal can carry a data, the goal is to filter the signal and let pass only the one corresponding to filters.

**Example**:<br />
We analyze different type of data, text, image, ... <br/>
We could use create a **RelayFilterDoor** to filter the data block type and send to specific algorithm only the one that match.

## How to configure

Like other door we specify the signal source and pass a lambda as filer.

> [!Caution]
> Attention: the lamdba with be serialiazed it imply some limitation. Like no context call, like variable, method...

```csharp

var oodDoor = Door.Create("oodDoorFilter")
                  .Listen(signalAndTriggerPopulate)
                  .UseRelayFilter<int>((value, signal) => value % 2 == 1)
                  .Build()
```

## Sample

In this sample we create a loop every 10 sec using a simple Logic door. <br />
A trigger that will send number from 1 to 5 every time the loop tick. <br />
Base if number carry by the signal if ood or even dedicated door will fire or not. <br/>

In the output look at the follow values :

Signal -> From -> Data, this field give you the value carry by the source signal

Signal -> From -> SourceDefinitionName, this field give you the value door that fire the signal

```
warn: Democrite.Framework.VGrains.DebugTools.IDisplaySignalsInfoVGrain[0]
       IExecutionContext
      {
        "FlowUID": "18af9651-6286-4737-9d56-ef6bd48c8191",
        "CurrentExecutionId": "504438b6-cf18-44f8-960e-c26c8d2b0455",
        "ParentExecutionId": "6717262f-a1a5-4b01-9d61-b1f7fdcc7555"
      }


       signal
      {
        "Uid": "13f67f22-09fc-496c-835d-5f203944742a",
        "From": {
          "Data": 0,
          "SignalUid": "13f67f22-09fc-496c-835d-5f203944742a",
          "SourceDefinitionId": "643d93de-3dec-4ff1-8863-93d04bcb3b0a",
          "SourceDefinitionName": "evenDoorFilter",
```