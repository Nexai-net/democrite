import sys
import os
import democrite

def calc(command: democrite.vgrainCommand, tools: democrite.vgrainTools):
    """ Eval and calculate the mathematical result """
    formula = command.get_content()
    result = eval(formula)
    tools.get_logger.logInformation("Test Result " + formula + "=" + str(result))
    return result

app = democrite.vgrain(sys.argv)

app.execute(calc);