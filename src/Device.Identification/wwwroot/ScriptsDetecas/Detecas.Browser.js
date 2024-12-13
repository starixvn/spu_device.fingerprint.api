/* global Detecas */

Detecas.Browser = (function (window, document, navigator, config, userAgent, debug, common) {
    "use strict";

    var result = null;
    var uaResult = {};

    var isCanvasSupported = function () {
        var elem = document.createElement("canvas");
        return !!(elem.getContext && elem.getContext("2d"));
    };

    var isRTCSupported = function () {
        return !!(window.webkitRTCPeerConnection || window.PeerConnection);
    };

    var isJSONSupported = function () {
        return typeof window.JSON === "object";
    };

    var superCookiesTest = function () {
        var localStorage = !!window.localStorage ? '1' : '0';
        var sessionStorage = !!window.sessionStorage ? '1' : '0';
        var globalStorage = !!window.globalStorage ? '1' : '0';
        var webSql = !!window.openDatabase ? '1' : '0';
        var indexedDb = !!window.indexedDB ? '1' : '0';

        var ieStorage = function userData(name) {
            try {
                var div = common.createTransparentDOMElement("div", "_ud");
                document.body.appendChild(div);

                if (div.addBehavior) {
                    div.style.behavior = "url(#default#userData)";
                    name = name.replace(/[^a-z]+/g, '');
                    return true;
                } else {
                    return false;
                }
            } catch (exception) {
            }
        };

        var ieUserData = !!ieStorage('test', 'test') ? '1' : '0';

        return localStorage + sessionStorage + globalStorage + webSql + indexedDb + ieUserData;
    };

    var updateBrowser = function () {
        var browser = uaResult.Browser;
        if (browser) {
            result.name = browser.name || config.na;
            result.version = browser.version || config.na;
        }
    };

    var updateEngine = function () {
        var engine = uaResult.Engine;
        if (engine) {
            result.engine = engine.name || config.na;
            result.engineVersion = engine.version || 0;
        }
    };

    var updateOperationSystem = function () {
        var operationSystem = uaResult.OS;
        if (operationSystem) {
            result.OS = (operationSystem.name && operationSystem.version)
                ? [operationSystem.name, operationSystem.version].join(" ")
                : (navigator.platform || config.na);
        }
    };

    var updateDevice = function () {
        var deviceNames = ["Others", "Desktop", "Tablet", "Mobile"];
        var deviceName = "Desktop";
        var device = uaResult.Device;
        if (device && device.type) {
            var deviceType = device.type.toLowerCase();
            for (var i = 1; i < deviceNames.length; i++) {
                if (deviceType === deviceNames[i].toLowerCase()) {
                    deviceName = deviceNames[i];
                    break;
                }
            }
        }

        result.deviceType = deviceNames.indexOf(deviceName);
        result.deviceName = deviceName;
    };

    var getResult = function () {
        try {
            if (result === null) {
                result = {
                    name: config.na,
                    version: 0,
                    engine: config.na,
                    engineVersion: 0,
                    OS: config.na,
                    deviceType: 1, //config.na,
                    deviceName: "Desktop",
                    JSON: isJSONSupported(),
                    RTC: isRTCSupported(),
                    canvas: isCanvasSupported(),
                    superCookies: superCookiesTest(),
                    ep: config.ep,
                    sep: config.sep
                };

                uaResult = userAgent.getResult();
                updateBrowser();
                updateDevice();
                updateEngine();
                updateOperationSystem();
            }
        } catch (exception) {
            //debug.addException(exception, "Detecas.Browser");
        }

        return result;
    };

    return {
        getResult: getResult
    };
})(window, document, navigator, Detecas.Config, Detecas.UserAgent, Detecas.Debug, Detecas.Common);