import sys
import os
import democrite

def compute(command: democrite.vgrainCommand, tools: democrite.vgrainTools):
    """ Eval and compute the mathematical result """
    formula = command.get_content()
    result = eval(formula)
    tools.get_logger.logInformation("Test Result " + formula + "=" + str(result))
    return {
        "result" : result,
        "formula": formula
    }

app = democrite.vgrain(sys.argv)

app.run(compute);
