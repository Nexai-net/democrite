import uuid
import json
import base64
import socket
import struct
import codecs
import concurrent.futures

from abc import ABC, abstractmethod 
from asyncio.windows_events import NULL

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

class _remoteCommandServer:
    def __init__(self, port:int, callback):
        self._port = port
        self._callback = callback
        self._executor = concurrent.futures.ThreadPoolExecutor()

    def __private__sendResponse_with_message_type(self, id:str, result: str, messageType: _messageType):
        finalData = struct.pack("!B", messageType)
        finalData += codecs.utf_8_encode(id)[0] 

        if(result != NULL):
            finalData += codecs.utf_8_encode(result)[0] 

        fullFinalData = struct.pack("H", len(finalData)) + finalData
        self._socket.send(fullFinalData)

    def __private__executeCommand_async(self, msgid:str, compressedCmd:str):
        result = self._callback(compressedCmd)
        self.__private__sendResponse_with_message_type(msgid, result, _messageType.USER)

    def send_direct_message_type(self, id:str, result: str, messageType: _messageType):
        self.__private__sendResponse_with_message_type(id, result, messageType)

    def run(self):
        if (hasattr(self, '_socket')):
            return

        self._socket = socket.socket(socket.AddressFamily.AF_INET, socket.SocketKind.SOCK_STREAM);
        self._socket.connect(("localhost", self._port));

        pkgSize = [] 
        while (True):
            
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
                self.__private__sendResponse_with_message_type(msgId, NULL, _messageType.PONG)
                continue

            if (msgType == _messageType.USER):
                self._executor.submit(self.__private__executeCommand_async, msgId, msg)
                continue

        print("Server Stopped")

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
            'Content' : self.get_content()
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
        msg = {
            "Type" : "Log",
            "Level": level[0],
            "ExecutionId": self._executionId,
            "Message": message
        }

        msgId = str(uuid.uuid4())
        msgStr = json.dumps(msg);
        self._executor.submit(self._comProxy.send_direct_message_type, msgId, msgStr, _messageType.SYSTEM)

class vgrainTools:
    def __init__(self, args: list[str], logger: loggerBase) -> None:
        self._args = args
        self._logger = logger
        pass

    @property
    def get_logger(self):
        return self._logger

    @property
    def get_args(self) -> list[str]:
        return self._args

def execFunc(command=vgrainCommand, tools=vgrainTools): ...

class vgrain:
    """ Base class of VGRain used execute democrite jobs"""
    
    def __init__(self, args: list[str]):

        # Search in arguments the command information "--cmd:'CMD_JSON_BASE64'"
        for arg in args:

            if (arg.startswith("--cmd:'")):
                remainLen = len(arg) - 1
                self._command = arg[7:remainLen]
                args.remove(arg)

            if (arg.startswith("--port:")):
                remainLen = len(arg)
                self._remotePort = int(arg[7:remainLen])
                args.remove(arg)

        self._arguments = args
        
    def test(self, data, func: execFunc):
        """ Execute the command pass by argument to test """
        
        testCommandContainer = { 
            'FlowUid' : uuid.uuid4().hex, 
            'ExecutionId' : uuid.uuid4().hex, 
            'Content' : data 
        }
        
        formatCommand = self.__private_format_command(testCommandContainer)
        result, _ = self.__private_execute_command(formatCommand, func)

        if (result != NULL):
            print("Result : " + json.dumps(result))

        print("Original Data : " + json.dumps(data))

    def execute(self, func: execFunc) -> None:
        """ Execute the command pass by command line arguments in oneshot """

        if self._command == NULL:
            raise KeyError("Command not found, ensure the command is pass using --cmd:'CMD_JSON_BASE64'")

        response, cmd  = self.__private_execute_command(self._command, func)
        formatResponse = self.__private_format_command(response)
        print(cmd.get_execution_uid() + ":" + formatResponse)

    def run(self, func: execFunc):
        """ Run in background and process all command send """
        callback = lambda compressedMsg=str : self.__private_execute_command_and_format(compressedMsg, func)
        self._remoteServer = _remoteCommandServer(self._remotePort, callback)
        self._remoteServer.run()

    def __private_execute_command(self, compressCommand: str, func: execFunc) -> dict[str, any] | vgrainCommand:
        """ Execute  compressCommand (UTF-8 Json in base64 ) """

        bytes = base64.b64decode(compressCommand)
        cmd = json.loads(bytes)

        vgrainCmd = vgrainCommand(cmd["FlowUid"], cmd["ExecutionId"], cmd["Content"])

        result = ""
        exceptionMessage = ""
        errorCode = "-1"
        success = False
        
        try:
            if (hasattr(self, "_remoteServer")):
                logger = _remoteLogger(self._remoteServer, vgrainCmd.get_execution_uid())
            else:
                logger = _printLogger()
            result = func(vgrainCmd, vgrainTools(self._arguments, logger))
            success = True
            errorCode = "0"
        except Exception as ex:
            exceptionMessage = str(ex)

        response = {
            'ExecutionId' : cmd["FlowUid"],
            'Success' : success,
            'Message' : exceptionMessage,
            'ErrorCode' : errorCode
        }
        
        if (len(str(result)) > 0):
            response['Content'] = result

        return response, vgrainCmd
    
    def __private_format_command(self, cmd) -> str:
        jsonValue = json.dumps(cmd);
        byte_data = jsonValue.encode('utf-8')
        formatCommand = base64.b64encode(byte_data);
        return codecs.utf_8_decode(formatCommand)[0]
    
    def __private_execute_command_and_format(self, compressCommand: str, func: execFunc) -> dict[str, any] | vgrainCommand:
        response, cmd  = self.__private_execute_command(compressCommand, func)
        return self.__private_format_command(response)