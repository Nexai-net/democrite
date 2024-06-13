import uuid
import sys
import json
import base64
import socket
import struct
import codecs
import concurrent.futures

from abc import ABC, abstractmethod 
# from asyncio.windows_events import NULL

class LogLevel:
    """
    Defines logging severity levels.
    """

    Trace = 0,
    """
    Logs that contain the most detailed messages. These messages may contain sensitive application data.
    These messages are disabled by default and should never be enabled in a production environment.
    """ 

    Debug = 1,
    """
    Logs that are used for interactive investigation during development.  These logs should primarily contain
    information useful for debugging and have no long-term value.
    """

    Information = 2,
    """
    Logs that track the general flow of the application. These logs should have long-term value.
    """

    Warning = 3,
    """
    Logs that highlight an abnormal or unexpected event in the application flow, but do not otherwise cause the
    application execution to stop.
    """

    Error = 4,
    """
    Logs that highlight when the current flow of execution is stopped due to a failure. These should indicate a
    failure in the current activity, not an application-wide failure.
    """

    Critical = 5,
    """
    Logs that describe an unrecoverable application or system crash, or a catastrophic failure that requires
    immediate attention.
    """

class _messageType:
    USER = 0
    SYSTEM = 1
    PING = 2
    PONG = 3
    ERROR = 255 # Max byte value

class VerboseLevel:
    SILENCE = 0
    MINIMAL = 1
    FULL = 2

def democrite_verbose_print(message:str, level:VerboseLevel, expectedLevel:VerboseLevel):
    if (level <= expectedLevel):
        print(message)

def democrite_any_to_base_64_string(data:any):
    data_str = data
    if (data is object):
        data_str = json.dumps(data)
    else:
        data_str = str(data)
    return base64.b64encode(data_str.encode("utf-8")).decode('utf-8')

class _remoteCommandServer:
    def __init__(self, port:int, serverIp:str, verboseLevel:VerboseLevel, callback):
        self._port = port
        self._serverIP = serverIp
        self._callback = callback
        self._verboseLevel = verboseLevel
        self._executor = concurrent.futures.ThreadPoolExecutor()

    def __private__sendResponse_with_message_type(self, id:str, result: str, messageType: _messageType):
        finalData = struct.pack("!B", messageType)
        finalData += codecs.utf_8_encode(id)[0] 

        if(result is not None):
            finalData += codecs.utf_8_encode(result)[0] 

        fullFinalData = struct.pack("H", len(finalData)) + finalData
        self._socket.send(fullFinalData)

    def __private__executeCommand_async(self, msgid:str, compressedCmd:str):
        try:
            result = self._callback(compressedCmd)
        except Exception as ex:
            democrite_verbose_print("[" + msgid + "] Error " + str(ex), VerboseLevel.MINIMAL, self._verboseLevel);
            self.__private__sendResponse_with_message_type(msgid, str(ex) + "\n" + str(sys.exc_info()), _messageType.ERROR)
            return
        
        democrite_verbose_print("[" + msgid + "] Message Processed", VerboseLevel.FULL, self._verboseLevel);
        self.__private__sendResponse_with_message_type(msgid, result, _messageType.USER)

    def send_direct_message_type(self, id:str, result: str, messageType: _messageType):
        self.__private__sendResponse_with_message_type(id, result, messageType)

    def run(self):
        if (hasattr(self, '_socket')):
            return

        self._socket = socket.socket(socket.AddressFamily.AF_INET, socket.SocketKind.SOCK_STREAM);
        self._socket.connect((self._serverIP, self._port));

        pkgSize = [] 
        while (True):
            
            try:

                pkgSizeArrayBytes = self._socket.recv(2)

                if (len(pkgSizeArrayBytes) < 2):
                    break;

                pkgSize = int(struct.unpack("H", pkgSizeArrayBytes)[0]);

                containerData = self._socket.recv(pkgSize)
                if (len(containerData) < pkgSize):
                    break;
            
                msgType = containerData[0]
                msgId = codecs.utf_8_decode(containerData[1:37])[0]

                msg = str()
                if (len(containerData) > 37):
                    msg = codecs.utf_8_decode(containerData[37:])[0]
                    # msg = base64.b64encode(msgDataStr)

                if (msgType == _messageType.PING):
                    democrite_verbose_print("[" + msgId + "] Ping Received", VerboseLevel.FULL, self._verboseLevel)
                    self.__private__sendResponse_with_message_type(msgId, None, _messageType.PONG)
                    continue

                if (msgType == _messageType.USER):
                    democrite_verbose_print("[" + msgId + "] Message Received", VerboseLevel.FULL, self._verboseLevel)
                    self._executor.submit(self.__private__executeCommand_async, msgId, msg)
                    continue
        
            except ConnectionResetError as ex:
                democrite_verbose_print("Server socket error " + str(ex) + "\n" + str(sys.exc_info()), VerboseLevel.MINIMAL, self._verboseLevel);
                break

            except ConnectionAbortedError as ex:
                democrite_verbose_print("Server socket error " + str(ex) + "\n" + str(sys.exc_info()), VerboseLevel.MINIMAL, self._verboseLevel);
                break

            except ConnectionRefusedError as ex:
                democrite_verbose_print("Server socket error " + str(ex) + "\n" + str(sys.exc_info()), VerboseLevel.MINIMAL, self._verboseLevel);
                break

            except ConnectionError as ex:
                democrite_verbose_print("Server socket error " + str(ex) + "\n" + str(sys.exc_info()), VerboseLevel.MINIMAL, self._verboseLevel);
                break

            except Exception as ex:
                democrite_verbose_print("Server socket error " + str(ex) + "\n" + str(sys.exc_info()), VerboseLevel.MINIMAL, self._verboseLevel);

        democrite_verbose_print("Server Stopped", VerboseLevel.MINIMAL, self._verboseLevel);

class vgrainCommand:
    def __init__(self, flow_uid, executionUid, inputData):
        self._flow_uid = flow_uid
        self._executionUid = executionUid
        self._content = inputData

    def get_flow_uid(self) -> str:
        """ Get the command unique id """
        return self._flow_uid;
    
    def get_execution_uid(self) -> str:
        """ Get the execution unique id """
        return self._executionUid;
    
    def get_content(self) -> any:
        """ Get the command content """
        return self._content;

    def Serialize(self) -> str:
        commandContainer = { 
            'FlowUid' : self.get_flow_uid(), 
            'ExecutionId' : self.get_execution_uid() ,
            'Content' : base64.b64encode(self.get_content())
        }
        
        jsonValue = json.dumps(commandContainer);
        byte_data = jsonValue.encode('utf-8')

        formatCommand = base64.b64encode(byte_data);
        return formatCommand
    
class loggerBase(ABC):
    # def __init__(self):
    #     super.__init__(self)

    def logInformation(self, message):
        self.log(LogLevel.Information, message)

    def logWarning(self, message):
        self.log(LogLevel.Warning, message)

    def logError(self, message):
        self.log(LogLevel.Error, message)

    def logDebug(self, message):
        self.log(LogLevel.Debug, message)

    def logCritical(self, message):
        self.log(LogLevel.Critical, message)

    @abstractmethod
    def log(self, level:LogLevel, message):
        pass

class _printLogger(loggerBase):
    # def __init__(self):
    #     super.__init__(self)

    def log(self, level:LogLevel, message):
        print("LOG:" + str(level[0]) + ": " + message)

class _remoteLogger(loggerBase):
    def __init__(self, comProxy: _remoteCommandServer, executionId: str):
        self._comProxy = comProxy
        self._executionId = executionId
        self._executor = concurrent.futures.ThreadPoolExecutor()

    def log(self, level:LogLevel, message):

        message_str = message
        if (message is object):
            message_str = json.dumps(message)

        message_str = base64.b64encode(message_str.encode("utf-8")).decode('utf-8')

        msg = {
            "Type" : "Log",
            "Level": level[0],
            "ExecutionId": self._executionId,
            "Message": message_str
        }

        msgId = str(uuid.uuid4())
        msgStr = json.dumps(msg);
        self._executor.submit(self._comProxy.send_direct_message_type, msgId, msgStr, _messageType.SYSTEM)

class vgrainTools:
    def __init__(self, args: list[str], logger: loggerBase, configs:dict) -> None:
        self._args = args
        self._logger = logger
        self._configs = configs
        pass

    @property
    def get_logger(self):
        return self._logger
    
    @property
    def get_config(self) -> dict:
        return self._configs;

    @property
    def get_args(self) -> list[str]:
        return self._args

def execFunc(command=vgrainCommand, tools=vgrainTools): ...

class vgrain:
    """ Base class of VGRain used execute democrite jobs"""
    
    def __init__(self, args: list[str]):

        self._serverIP = "127.0.0.1"
        self._remotePort = -1
        self._globalVerbose = VerboseLevel.MINIMAL
        self._configs = dict()

        # Search in arguments the command information "--cmd:'CMD_JSON_BASE64'"
        argCopy = args.copy();
        for arg in argCopy:

            if (arg.startswith("--cmd:'")):
                remainLen = len(arg) - 1
                self._command = arg[7:remainLen]
                args.remove(arg)

            if (arg.startswith("--port:")):
                remainLen = len(arg)
                self._remotePort = int(arg[7:remainLen])
                args.remove(arg)

            if (arg.startswith("--server:")):
                remainLen = len(arg)
                self._serverIP = arg[9:remainLen]
                args.remove(arg)

            if (arg.startswith("--verbose:")):
                remainLen = len(arg)
                verboseValue = arg[10:remainLen].strip().lower()
                if (verboseValue == "silence" or verboseValue == "0"):
                    self._globalVerbose = VerboseLevel.SILENCE
                elif (verboseValue == "minimal" or verboseValue == "1"):
                    self._globalVerbose = VerboseLevel.MINIMAL
                elif (verboseValue == "full" or verboseValue == "2"):
                    self._globalVerbose = VerboseLevel.FULL
                
                args.remove(arg)

            if (arg.startswith("--config")):
                remainLen = len(arg)
                remainConfig = arg[8:remainLen]
                encodeInBase64 = remainConfig.startswith("_b64:")
                if (encodeInBase64):
                    remainLen = remainLen - 5
                    remainConfig = remainConfig[5:remainLen]
                elif (remainConfig.startswith(":")):
                    remainLen = remainLen - 1
                    remainConfig = remainConfig[1:remainLen]
                else:
                    # don't process this arg
                    continue;

                splitArgCfg = remainConfig.index("=")
                if (splitArgCfg < 0):
                    continue;
                
                remainLen = len(remainConfig)
                configKey = remainConfig[0:splitArgCfg]
                
                splitArgCfg = splitArgCfg + 1
                configValue = remainConfig[splitArgCfg:remainLen]

                configValue = configValue.strip("'")

                if (encodeInBase64):
                    configValue = base64.b64decode(configValue)

                self._configs[configKey] = configValue

                args.remove(arg)

        self._arguments = args
        if (self._remotePort > -1):
            print("ServerIp: " + self._serverIP + ":" + str(self._remotePort))
        
    def test(self, data, func: execFunc):
        """ Execute the command pass by argument to test """
        
        testCommandContainer = { 
            'FlowUid' : uuid.uuid4().hex, 
            'ExecutionId' : uuid.uuid4().hex, 
            'Content' : democrite_any_to_base_64_string(data)
        }
        
        formatCommand = self.__private_format_command(testCommandContainer)
        result, _ = self.__private_execute_command(formatCommand, func)

        if (result is not None):
            print("Result : " + json.dumps(result))

        print("Original Data : " + json.dumps(data))

    def execute(self, func: execFunc) -> None:
        """ Execute the command pass by command line arguments in oneshot """

        if self._command is None:
            raise KeyError("Command not found, ensure the command is pass using --cmd:'CMD_JSON_BASE64'")

        response, cmd  = self.__private_execute_command(self._command, func)
        formatResponse = self.__private_format_command(response)
        print(cmd.get_execution_uid() + ":" + formatResponse)

    def run(self, func: execFunc):
        """ Run in background and process all command send """
        callback = lambda compressedMsg=str : self.__private_execute_command_and_format(compressedMsg, func)
        self._remoteServer = _remoteCommandServer(self._remotePort, self._serverIP, self._globalVerbose, callback)
        self._remoteServer.run()

    def __private_execute_command(self, compressCommand: str, func: execFunc) -> dict[str, any]:
        """ Execute  compressCommand (UTF-8 Json in base64 ) """

        democrite_verbose_print("New command arrived - try process", VerboseLevel.FULL, self._globalVerbose);

        try:
            bytes = base64.b64decode(compressCommand)
            cmd = json.loads(bytes)

            democrite_verbose_print("[" + cmd["ExecutionId"] + "] Start Command execution Verbose = " + str(self._globalVerbose), VerboseLevel.MINIMAL, self._globalVerbose);
            cmd_base_64_str = cmd["Content"];

            vgrainCmd = vgrainCommand(cmd["FlowUid"], cmd["ExecutionId"], base64.b64decode(cmd_base_64_str.encode("utf-8")).decode("utf-8"))
            
        except Exception as ex:
            democrite_verbose_print("Command json loading failed " + str(ex) + "\n" + str(sys.exc_info()), VerboseLevel.MINIMAL, self._globalVerbose);
            raise ex
        
        result = ""
        exceptionMessage = ""
        errorCode = "-1"
        success = False
        
        try:
            if (hasattr(self, "_remoteServer")):
                logger = _remoteLogger(self._remoteServer, vgrainCmd.get_execution_uid())
            else:
                logger = _printLogger()
            result = func(vgrainCmd, vgrainTools(self._arguments, logger, self._configs))
            success = True
            errorCode = "0"
        except Exception as ex:
            exceptionMessage = str(ex) + "\n" + str(sys.exc_info())
            democrite_verbose_print("[" + cmd["ExecutionId"] + "] Failed execution : " + exceptionMessage, VerboseLevel.FULL, self._globalVerbose);

        response = {
            'ExecutionId' : cmd["ExecutionId"],
            'Success' : success,
            'Message' : exceptionMessage,
            'ErrorCode' : errorCode
        }
        
        if (len(str(result)) > 0):
            result_str = democrite_any_to_base_64_string(result)
            response['Content'] = result_str

        democrite_verbose_print("[" + cmd["ExecutionId"] + "] Command execution end", VerboseLevel.FULL, self._globalVerbose);

        return response, vgrainCmd
    
    def __private_format_command(self, cmd) -> str:
        jsonValue = json.dumps(cmd);
        byte_data = jsonValue.encode('utf-8')
        formatCommand = base64.b64encode(byte_data);
        return codecs.utf_8_decode(formatCommand)[0]
    
    def __private_execute_command_and_format(self, compressCommand: str, func: execFunc) -> dict[str, any]:
        response, cmd  = self.__private_execute_command(compressCommand, func)
        return self.__private_format_command(response)