/* global Detecas */

Detecas.UserAgent = (function (window, debug) {
    "use strict";

    if (fpsChkIntr('Detecas.UserAgent') == true) {
        return;
    }

    var result = null;
    function getResult() {
        try {
            if (result === null) {
                var parser = new window.UAParser();
                var uaResult = parser.getResult();
                result = {
                    Browser: uaResult.browser,
                    Device: uaResult.device,
                    Engine: uaResult.engine,
                    OS: uaResult.os
                };
            }
        }
        catch (exception) {
            //debug.addException(exception, "Detecas.UserAgent.getResult");
        }

        return result;
    }

    return {
        getResult: getResult
    };
})(window, Detecas.Debug);