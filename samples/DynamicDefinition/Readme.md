Dynamic Definitions
====

## Goal

Democrite serves as an orchestrator for vgrains, utilizing serializable definitions as work orders. <br />
These definitions are primarily formulated and stored in an external system such as databases. <br/><br/>

The objective of the "Dynamic Definitions" feature is to generate definitions (sequences, triggers, etc.) at runtime, with a lifecycle linked to storage.<br/>
If no specific storage is provided, the definition will exist only for the duration of the cluster's lifetime.<br/><br/>

This capability enables dynamic testing of new sequences, creation of temporary triggers, and activation of temporary debugging features, among other functionalities.

## How to configure

The service "**IDynamicDefinitionHandler**" will enable all the dynamic features

```csharp

// Push new or updtae definition 
await this._dynamicDefinitionHandler.PushDefinitionAsync(seqDefinition, true, this._identityCard, default);

// Enable or disable a dynamic feature
await this._dynamicDefinitionHandler.ChangeStatus(definitionId, true/false, this._identityCard_, token);

// Get all the dynamic definition meta data
await this._dynamicDefinitionHandler.GetDynamicDefinitionMetaDatasAsync(token: token);

```

> [!CAUTION]
> Attention: **IDynamicDefinitionHandler** will have an impact above all the cluster.
> Sensible method require an IIdentityCard.
> This card will be used to managed the security and right.
> <br/>
> :warning: During this beta version, security is still in place but we start puting in place elements to prevent signature breaking change in the future.

## Sample

** Create **
- <font color="orange">"/create/sequence"</font> : Create a simple sequence that will take the sentence, split words and organize them in function of the enum value.
- <font color="orange">"/create/trigger/cron"</font> : Create a cron trigger (second format) that will call a sequence

** Execute **
- <font color="orange">"/execute/sequence"</font> : Execute a sequence by its Uid

** Inspect **
- <font color="orange">"/definition/sequences"</font> : Look through the classic **ISequenceDefinitionProvider** all the sequences available (You will see the dynamic appeard here)
- <font color="orange">"/definition/dynamics"</font> : Look through the **IDynamicDefinitionHandler** all the dynamic definition available.

** Change dynamic definitions **
- <font color="orange">"/definition/dynamics/ChangeStatus"</font> : Allow to enable/disable a dynamic definition.
- <font color="orange">"/definition/dynamics/Remove"</font> : Remove a specific definition.
- <font color="orange">"/definition/dynamics/Clear"</font> : Clear all the dynamic definitions.