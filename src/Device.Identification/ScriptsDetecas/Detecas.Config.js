var Detecas = {};

Detecas.Config = {
    host: "__host__", // Current host of script
    cdn: "__cdn__",
    ip: "127.0.0.1", // Client ip
    encodedIp: "2130706433", // Encoded Client ip
    cookieStorage: "__cookieStorage__", // Storage name
    version: "__version__", // Server script version
    ep: "__encryptpassword__",
    sep: "__separator__",
    na: "__na__",
    storages: "__storages__",
    cachedDeviceId: "__cachedDeviceId__", // Cached DeviceId (get from Cross-domain cookie data for device identifier)
    defaultDeviceId: "__defaultDeviceId__", // New device id or get value of cachedDeviceId.
    accept: "__accept__", // Http Accept
    headers: "__headers__", // Http Headers
    capturedDate: "__capturedDate__", // DeviceId's captured date
};

(function (window, document, config) {
    "use strict";

    var domReadyCount = 0;
    var domReadyTimeout = null;
    var isStarted = false;

    console.log('v' + config.version);

    function checkDomReady() {
        try {
            
            if (typeof gfpSpot === 'object' && typeof gfpSpot.injectFdi === 'function') {
                gfpSpot.injectFdi();
            }

            var regex = new RegExp("loaded|complete|interactive");
            if (typeof (document.readyState) === 'undefined' || regex.test(document.readyState)) {
                if (domReadyTimeout) {
                    clearTimeout(domReadyTimeout);
                    domReadyTimeout = null;
                }

                if (!isStarted) {
                    isStarted = true;
                    Detecas.Core.start();

                    if (typeof gfpSpot === 'object' && typeof gfpSpot.start === 'function') {
                        gfpSpot.start();
                    }
                }
            } else {
                domReadyCount++;
                if (domReadyCount < 100) {
                    domReadyTimeout = setTimeout(checkDomReady, 100);
                } else {
                    //debug.addExecuteTime(0);
                }
            }
        }
        catch (exception) {
            console.error(exception);          
        }
    }

    function loadCommonScript(commUrl) {
        var script = document.createElement("SCRIPT");
        script.type = "text/javascript";
        script.src = commUrl;
        script.async = true;
        script.defer = true;

        script.onload = function () {
            try {
                if (document.addEventListener) {
                    document.addEventListener("DOMContentLoaded", checkDomReady, false);
                }
                checkDomReady();

            } catch (exception) {
                console.log(exception);
            }
        };

        document.getElementsByTagName("head")[0].appendChild(script);
    }
    function loadTkScript(tkexpUrl) {
        var scriptTkexp = document.createElement("SCRIPT");
        scriptTkexp.type = "text/javascript";
        scriptTkexp.src = tkexpUrl;
        scriptTkexp.async = true;
        scriptTkexp.defer = true;
        scriptTkexp.onload = function () {

            try {
                if (ConfigJWT != null &&
                    typeof ConfigJWT === "object" &&
                    typeof ConfigJWT.injectToken === "function") {
                    ConfigJWT.injectToken();
                }
            } catch (exception) {
                console.log(exception);
            }

            loadCommonScript(Detecas.Config.cdn + "/comm");
        };

        document.getElementsByTagName("head")[0].appendChild(scriptTkexp);
    }    

    loadTkScript(Detecas.Config.cdn + "/tk");
})(window, document, Detecas.Config);