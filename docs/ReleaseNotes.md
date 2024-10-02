Release Notes
===

[![Nuget Democrite](https://img.shields.io/nuget/dt/democrite.framework.core.svg?label=Nuget%20Democrite%20Framework%20downloads)](https://www.nuget.org/packages?q=democrite)

# Released

# Beta

## 0.5.0-prerelease

In this version we mainly:
- Optimize architecture around repository declaration and creation
- Fix bugs, improve testing
- New feature call [RefId](#050-prerelease/feature/refid)
- New feature call Yaml [Compilation](#050-prerelease/feature/yaml-compilation)

### Features
- **Artifact**
    - ***Arguments*** : 
        - You can now differenciate command line argument for the launcher than launched script
        - Choose the charactere that will be used in command line between the argument name and his value, by default is ':'. Example : --port:4242

- **RefId**
    - Using attribute and code genertion we tag each VGrain with a unique "SNI" (Simple Name Identifier) and a namespace through an URI used as user friend identifier
        - URI Example: ***ref://vgr@bag.text/text-toolbox*** is the unique reference to a vgrain (interface) in the namespace "bag.text" with a SNI "text-toolbox"
    - A ref id could target:
        - VGrain interface
        - VGrain Implementation
        - Type (class, struct)
        - Methods
        - Definitions (Sequence, Trigger, Signal, ...)
        - Blackboard controller

- **Yaml Compilation**
    - Compile Yaml into definitions
    - Schema definition used to help yaml creation [Schema](https://github.com/Nexai-net/democrite/docs/democrite.schema.json)
    - Support definitions
        - Sequence
        - Trigger (Cron, Signal)
        - Signal
        - StreamQueue

- **Auto-Configuration**
    - Schema definition to help configuration [Schema](https://github.com/Nexai-net/democrite/docs/configuration-node.schema.json)
    - Add logs to better understands

### Fix
-   **Repository**
    - ***Mongo***: Discriminator filter prevent some correct match 
-   **Artifact**: We are currently integrating other langugage like java and minor internal bug was revealed
    - ***Docker***: Fix image creation
-   **Auto-Configuration** : Fix configuration through json file, extension loading ...
-   **Portability**: Replaces Named Semaphore with Mutex: Suggests using a Mutex as a more portable alternative for synchronization.

### Breaking Changes
>[CAUTION]
> - **Repository** : Due to artitecture change some custom implementation risk to be impacted

## 0.4.4-prerelease

This update focuses on stability and bug fixes. <br/>
We've enhanced our development process with new testing and automation tools to prevent regressions and improve overall quality.<br/>

Please note that **Amexio** group is now supporting this projet.

## 0.4.3-prerelease

In this version we mainly:
- Enhance Blackboard usage
- Managed result after processing
- Stabilisation

### Features
- **Blackboard** : 
    - **Query** : Able to send query to blackboard, A type Request to have information or a type command to give order without response to the blackboard.
    - **Life Status** : Able to define a lifecycle status to request initialization or sealed with specific result when job is done.
    - **Signal** : Able to subscribe, received and unsubscribe to signals.

- **Sequence** :
    - **Extend Call** : Able to use property access in the call part, example : (grain, input, ctx) => grain.MethodAsync(input.Property.SubProperty, ctx).

- **Signal Hierarchy** : Able to create a hierarchy or signal, simply when a child fire it will automatically fire the parent.
- **Execution Handler** :
    - **Signal** : Add a signal information that need to be fire at the end of a execution.
    - **Deferred** : Able let process continue in background and use the deferred system let you know when the job is done
- **Meta Data** : Able to add description, tags, category information on descriptor like (Sequence, Trigger, ...), those meta data are used to "Understand" the goal
- **Artifact** : Able to setup an execution environment. Support Docker

### Breaking Changes
>[CAUTION]
> - **Artifact** : External execution had an issue during the message serialization. The protocol have been improved and change a bit.<br />
>                  Python democrite librairy MUST be updated

### Fix
- **node** :
    - **Deco/Reco** : With mongo db as cluster theire was a singleton issue
    - **Restore** : 

## 0.4.1-prerelease

In this version we mainly focus on dynamic feature :
- Definitions
- Redirections

### Features
- **Redirections** : Able to change the implementation used during a sequence resolution. In scope local and global.
- **Sequence Stages Call** : Extend sequence stages.
    - **Sequence** : Be able to call a sequence from another sequence.
    - **Select** : Be able to manipulate the input to select a sub element or provider one.
- **Dynamic Definitions** : Be able to inject definition as runtime

### Breaking Changes
>[CAUTION]
> - **ReturnNoMessage** : Renaming end stage method from "ReturnNoMessage" to "ReturnNoData".

### Fix
- **Trigger** : Fix trigger activation and desactivation based on definition.
- **Repository** : Auto-configuration default democrite repository


# Alpha

## 0.3-prerelease

In this version we mainly:
- Add blackboard extension
- Add Stream triggers to send and consumed
- Sequence feature Foreach/FireSignal
- Storage through repository

### Features
- **Blackboard** : Extension in charged to create a controlled shared memory.
- **Stream** : Trigger used to push to an orlean stream & trigger fire by message present in a stream
- **Sequence** 
    - **Foreach** : Be able to process through a foreach loop a sub-collectionn the model current data processed
    - **Fire Signal** : Be able to fire signal directly from a sequence
- **Trigger Cron** : Support second as minimal timer
- **Trigger Output** : Allow each trigger output to have a custom output value
- **Repository** : Simple storage space used to store information like state way but simpler without container

### Breaking Changes
>[CAUTION]
> - **rename(Bag)** : Rename debug tools and web tools VGRAINS to Bag terminologie <br />
> - **rename(Mongo)** : Rename extensions about storage in mongo from Democrite.Framework.Node.Mongo to   Democrite.Framework.Extensions.Mongo<br />
> - **Definition** : Force display name to be set <br />
> - **rename(SetInput)** : Rename SetInputValue to SetOutput<br />
> - **Move(TypeArgument)** : Move TypeArgument and EnumerableHelper in the abstractions assembly to allow more usage
> - **rename(Configuration)** : Changing configuration name from 'AddInMemoryMongoDefinitionProvider' to 'AddInMemoryDefinitionProvider' to remove mongo missplaced name

### Fix
- **ConcretType** : CollectionType was not tolerate as concret type

## 0.2.2-prerelease

In this version we mainly:
- Add Python VGrain support

### Features

- **Artifact** : Extended resource needed to democrite node to work (code, png, licences, pfx ...)
    - Create, build and add artifact
    - Define code artifact to be used as grain

- **Code Artifact** : Artifact use to configure the external code, for example python scripts
- **External Code Remote controller** : Controller using artifact code definition to launch, maintain and communicate to another program
- **Python Package** : Package deployed on [Pypi](https://pypi.org/project/democrite/) and [TestPypi](https://test.pypi.org/project/democrite/) (cf. [Python/README.md](/src/Extensions/Dist/Python/README.md))

## 0.2.1-prerelease

In this version we mainly:
- Fix serialization issues
- Add extension to use MongoDB as external storage
- Add new door mechanism
- Add samples

### Features
- **Configuration**: Allow DemocriteNode to be setup directly on existing IHostBuilder
- **Door** Create a door with condition based on source signal
    - Create builder RelayFilterDoor based on conditional expression serializableCreate builder RelayFilterDoor based on conditional expression serializable

- **Data Model**: Add IDefinition that introduction model validation

- **External Storage**: Add mongo extension to be used as storage for
    - **Cluster**: Using mongodb as meeting point, configuration could be done by config file, manually, auto
    - **Reminders**: Use mongodb as storage for reminders
    - **VGrain State** : Configuration to use MongoDB as VGrain state storage. Configure using mongo for alll storage
    - **Defintions** : Allow mongo to be a definitions provider. Could be configured inline or through config file.

- **Toolbox** : 
    - Add Expression serialization of math operator
    - Create AbstractType and AbstractMethod object to be able serialize type and method info.

- **Debug** : Extension used to provide debug sequences, vgrain ... to display information more easily  
- **Sequence** : In a sequence you can add some configuration to customize the context of the grain called (Often use to provide variable for grain id generation).
You can now setup dynamically the configuration based on the input data.

### Breaking Changes
>[CAUTION]
- **Data Model**: In sequence and other model that use the .net "Type". We have Change "Type" to "AbstractType" to easy serializations
- **Namespace**: Some configuration namespace have been changed to simplify it with two root "Democrite.Framework" and "Democrite.Framework.Configuration"
- **StartUntilEndAsync**: Now have a CancellationToken in third parameter

### Fix
- **Signal**: Change Signal input storage from only one not consumed to ordered linked list to support many
- **SequenceExecutor**: Sequence executor state change through surrogate

### Tests
- **Serialization** : Add lot of test to ensure surrogate or definition are serializable at least in json.
- **Door** : Add door unit testes to ensure signal handling is well performed.
- **Toolbox** : Many tool used in the framework core are now more unit tested.

### Others
- **Signal**:  Add serialization test on signal grain state to ensure save and restore
- **Door**: Test door behavior subscription and signal management
- **Configuration**:
    - Simplify "InMemory" definition configuration, register multiple definitions, register directly at root level
    - Add Debug tool injection though one line of configuration

## 0.2.0-prerelease

- **Definition Provider** : Architectured to allow multiple provider and in-memeory one build-in
- **Sequences** : Definition and execution.
- **Signal** : Definition, Subscribe and Fire
- **Door** : Definition, Subscribe and Fire
    - **Boolean logical Door** : Definition, Subscribe and Fire
- **Triggers** : Definition and fire
    - **Cron** : Definition and fire
    - **Signal** : Definition and fire
