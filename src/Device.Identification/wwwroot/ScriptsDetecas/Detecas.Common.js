/* global Detecas */

Detecas.Common = (function (window, document, navigator, rc4, hex, base64, json, config, battery, geolocation, debug) {
    "use strict";

    geolocation.start();
    //battery.start();

    function isValidObject(obj) {
        return typeof (obj) !== "undefined" && !!obj;
    }

    function isValidDeviceId(deviceId) {
        return 100 > deviceId.length && 10 < deviceId.length && hex.decode(deviceId).length === 32; // Valid device cookie
    }

    function getDeviceIdFromScriptCache() {
        return config.cachedDeviceId || config.defaultDeviceId;
    }

    function collectDeviceInfoToInjectToClient(deviceId, fingerprint) {
        var encodeDeviceId = rc4.decrypt("&％#@?,:*!~^.", hex.decode(deviceId));
        var collectedDeviceInfo = {
            na: config.na,
            deviceCode: isValidObject(fingerprint) ? [fingerprint.oldCode, fingerprint.code].join(";") : "",
            appVersion:  navigator.appVersion || config.na,
            timeZone: isValidObject(fingerprint) ? fingerprint.timezone : "",
            userAgent: navigator.userAgent || config.na,
            screen: {
                width: screen.width,
                height: screen.height,
                colorDepth: screen.colorDepth
            },
            deviceId: encodeDeviceId,
            href: location.href,
            capturedDate: config.capturedDate,
            geolocation: geolocation.getGeolocation(),
            version: config.version
            //botD: botD
            //battery: battery.getBatteryInfo(),            
        };
        var jsonString = json.stringify(collectedDeviceInfo);
        var base64String = base64.encode(jsonString);
        return base64String;
    }    

    function removeDomElement() {
        var length = arguments.length;
        for (var i = 0; i < length; i++) {
            var obj = arguments[i];
            var element = document.getElementById(obj);
            if (element) {
                element.parentNode.removeChild(element);
            }
        }
    }

    function deleteWindowObjects() {
        var length = arguments.length;
        for (var i = 0; i < length; i++) {
            var name = arguments[i];
            try {
                delete window[name];
            } catch (exception) {
                //debug.addException(exception, "Detecas.Common.deleteWindowObjects");
            }
        }
    }

    function createTransparentDomElement(type, id) {
        var element = id !== undefined && document.getElementById(id)
            ? document.getElementById(id)
            : document.createElement(type);

        try {
            element.style.position = "absolute";
            element.style.width = "1px";
            element.style.height = "1px";
            element.style.overflow = "hidden";
            element.style.display = "block";
            element.style.zIndex = "-10";

            // Transparent
            element.filter = "alpha(opacity = 1)";
            element.MozOpacity = 0.1;
            element.KhtmlOpacity = 0.1;
            element.style.opacity = 0.1;
        } catch (exception) {
            //debug.addException(exception, "Detecas.Common.createTransparentDomElement");
        }

        if (id && element) {
            element.setAttribute("id", id);
        }

        return element;
    }

    function getHost() {
        return window.location.host.replace(/:\d+/, '');
    }

    return {
        getHost: getHost,
        isValidObject: isValidObject,
        removeDOMElement: removeDomElement,
        deleteWindowObjects: deleteWindowObjects,
        createTransparentDOMElement: createTransparentDomElement,
        isValidDeviceId: isValidDeviceId,
        collectDeviceInfoToInjectToClient: collectDeviceInfoToInjectToClient        
    };
})(window, document, navigator, Detecas.Rc4, Detecas.Hex, Detecas.Base64, Detecas.JSON, Detecas.Config, Detecas.Battery, Detecas.Geolocation, Detecas.Debug);