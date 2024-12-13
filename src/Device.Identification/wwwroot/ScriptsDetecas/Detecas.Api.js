/*
File Name: api.js
Description : Integrate with API licensee
@waiting for DI's scripts call back within 5 seconds BEFORE post (in timeout) deviceId from cache or N/A (means NO success)
*/
/* global Detecas */

Detecas.Api = (function (window, document, common, device, debug) {
    "use strict";

    function init() {
        try {
            var div = common.createTransparentDOMElement("div", "__api");
            div.innerHTML = "<form action='__action__' target='fpsFrame' name='fpsForm' id='fpsForm' method='post'> " +
            "<input type='hidden' id='__di' name='__di' /> </form>  " +
            "<iframe border='0' height='0' width='0' name='fpsFrame'></iframe>";
            document.body.appendChild(div);
        } catch (exception) {
            //debug.addException(exception, "Detecas.Api.init");
        }
    }

    function injectDeviceInfoToClient(deviceInfoSubscriber) {
        document.fpsForm.elements.namedItem("__di").value = deviceInfoSubscriber;
    }

    function cleanApi() {
        try {
            common.removeDOMElement(
                "__api"
            );
            common.deleteWindowObjects(
                window.API_CALLBACK
            );
        } catch (exception) {
            //debug.addException(exception, "Detecas.Api.cleanApi");
        }
    }

    window.API_CALLBACK = function (collectedDeviceInfo) {
        try {
            injectDeviceInfoToClient(collectedDeviceInfo);
            document.fpsForm.submit();

            // Wait form submit completed in 3 seconds
            setTimeout(function () {
                cleanApi();
            }, 3000);
        } catch (exception) {
            //debug.addException(exception, "Detecas.API_CALLBACK");
        }
    };

    function postTimeoutTransaction() {
        try {
            document.fpsForm.submit();
            cleanApi();
        } catch (exception) {
            //debug.addException(exception, "Detecas.Api.postTimeoutTransaction");
        }
    }

    function start() {
        try {
            var scriptCachedDeviceId = common.getDeviceIdFromScriptCache();

            // get deviceId from cached script to inject diSubscriber ASAP
            if (common.isValidDeviceId(scriptCachedDeviceId)) {
                var deviceInfo = device.getDeviceInformation();
                var fingerprint = deviceInfo.fingerprint;
                var collectedDeviceInfo = common.collectDeviceInfoToInjectToClient(scriptCachedDeviceId, fingerprint);
                injectDeviceInfoToClient(collectedDeviceInfo);
            }
        } catch (exception) {
            //debug.addException(exception, "Detecas.Api.start");
        }
    }

    window.setTimeout(function () {
        postTimeoutTransaction();
    }, 5000);

    return {
        init: init,
        start: start
    };
} (window, document, Detecas.Common, Detecas.Device, Detecas.Debug));

(function () {
    "use strict";

    Detecas.Api.init();
    Detecas.Api.start();
})();