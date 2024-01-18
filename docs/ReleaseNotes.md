Release Notes
===

[![Last Release](https://img.shields.io/github/v/release/nexai-net/democrite)](https://github.com/nexai-net/democrite/releases)

# Released

# Beta

# Alpha

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
