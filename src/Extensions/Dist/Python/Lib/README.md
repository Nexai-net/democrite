Democrite
====

This library permit to create [VGrain](https://github.com/Nexai-net/democrite?tab=readme-ov-file#virtual-grains-vgrain) for using en [Democrite](https://github.com/Nexai-net/democrite) environment.

# Installation

```Shell
pip install democrite
```

# How to use

Your grain could be used in two mode :
- OneShot: Script mode, the script is execute at each request
- Deamon: The script is start and stay alive to perform request


## Create VGrain

To start you need to create a **vgrain** object with command line argument pass in parameters

```Python

import democrite

app = democrite.vgrain(sys.argv)

```

## Create function

You need to pass a function to execute each request

```Python

def execFunc(command: democrite.vgrainCommand, tools: democrite.vgrainTools) -> any:

```

## Choose Mode

### OneShot

The command data are pass in command line argument in format (Json -> Base64) <br/>
The command result is returned by the strandard output in format 'ExecutionId:(json -> Base64)'

```Python
app.execute(execFunc);
```

### Deamon

A connection is made in local using the port pass in argument '--port:4242'

```Python
app.run(execFunc);
```

### test

During you developement you need to test your code.<br />
To do so use the **test** method with data you need as input.<br/>

```Python
app.test("5+6", execFunc);
```

## vgrainCommand

The command is pass on an object exposing :

```Python

# uid unique of the flow executing the grain
vgrainCommand.get_flow_uid() -> uuid

# uid grain execution
vgrainCommand.get_execution_uid() -> uuid

# get command input
vgrainCommand.get_content() -> uuid

```

## vgrainTools

This object provide tools dedicated to request

```Python

# provide access to a logger that will send back the logs to democrite
vgrainTools.get_logger.logInformation("LOG")

# provide the remain argument not consumed by the library
vgrainTools.get_args() -> list[str]


```