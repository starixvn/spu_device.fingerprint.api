/* global Detecas */

Detecas.Device = (function (navigator, sha1, md5, json, gpu, webgl, config, browser, screen, fonts, debug) {
    "use strict";

    // Constant
    var stringBlank = " ";
    var browserResult = browser.getResult();
    var pruneAndParseJson = function (rawObject, deph) {
        var resultObject = json.prune(rawObject, {
            depthDecr: deph
        });
        return json.parse(resultObject);
    };

    // Note: This function doesn't support IE 10 and backward version because they don't store plugin information in navigator.
    var getBrowserPlugins = function () {
        var comparePlugins = function (first, second) {
            if (first.name < second.name) {
                return -1;
            }
            if (first.name > second.name) {
                return 1;
            }

            return 0;
        };

        var prunePlugins = function (rawPlugins) {
            var prunedPlugins = [];
            for (var key in rawPlugins) {
                if (rawPlugins.hasOwnProperty(key) && rawPlugins[key].name) {
                    var value = rawPlugins[key];
                    var plugin = {
                        description: value.description,
                        filename: value.filename,
                        length: value.length,
                        name: value.name,
                        version: value.version
                    };

                    prunedPlugins.push(plugin);
                }
            }

            return prunedPlugins;
        };

        var distinctPlugins = function (rawPlugins) {
            var nameFlags = {};
            var descriptionFlags = {};
            return rawPlugins.filter(function (value) {
                if (nameFlags[value.name] && descriptionFlags[value.description]) {
                    return false;
                }

                nameFlags[value.name] = descriptionFlags[value.description] = true;
                return true;
            });
        };

        var getPluginsFromNavigator = function () {
            return navigator.plugins === undefined || navigator.plugins.length <= 0
                ? []
                : pruneAndParseJson(navigator.plugins, 2);
        };

        try {
            var plugins = getPluginsFromNavigator();
            if (plugins.length > 0) {
                plugins = prunePlugins(plugins);
                plugins = distinctPlugins(plugins);
                plugins = plugins.sort(comparePlugins);
                return json.stringify(plugins);
            }
        } catch (exception) {
            //debug.addException(exception, "Detecas.Device.getBrowserPlugins");
        }

        return json.stringify([]);
    };

    var getMimeTypes = function () {
        return navigator.mimeTypes === undefined || navigator.mimeTypes.length <= 0
            ? "[]"
            : json.stringify(pruneAndParseJson(navigator.mimeTypes, 2));
    };

    var getScreenInformation = function () {
        var prunedScreen = json.prune(screen, {
            depthDecr: 1,
            arrayMaxLength: 3
        });

        prunedScreen = json.parse(prunedScreen);
        prunedScreen.availHeight = undefined;
        prunedScreen.availWidth = undefined;
        prunedScreen.availTop = undefined;
        prunedScreen.availLeft = undefined;
        prunedScreen.top = undefined;
        prunedScreen.left = undefined;

        delete prunedScreen.availHeight;
        delete prunedScreen.availWidth;
        delete prunedScreen.availTop;
        delete prunedScreen.availLeft;
        delete prunedScreen.left;
        delete prunedScreen.top;

        return json.prune(prunedScreen);
    };

    function getMonitorInformation() {
        var monitor = screen.width + browserResult.sep + screen.height + browserResult.sep + screen.colorDepth;
        return monitor.toString();
    }

    function stripGuid(text) {
        // Regex for guid (\(){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\))
        return typeof text === "string"
            ? text.replace(/(\(){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\))/, "")
            : text;
    }

    function getNavigator(navigator) {
        var prunedNavigator = navigator;
        try {
            prunedNavigator = pruneAndParseJson(navigator, 3);
            prunedNavigator.appVersion = false;

            if (typeof prunedNavigator.connection === "object") {

                if (typeof prunedNavigator.connection.downlink != "undefined") {
                    prunedNavigator.connection.downlink = null;
                }

                if (typeof prunedNavigator.connection.downlinkMax != "undefined") {
                    prunedNavigator.connection.downlinkMax = null;
                }

                prunedNavigator.connection.rtt = null;
            }

            prunedNavigator.plugins = false;
            prunedNavigator.mimeTypes = false;
            prunedNavigator.userAgent = false;
            prunedNavigator.language = false;
            prunedNavigator.cookieEnabled = false;
            prunedNavigator.doNotTrack = false;
            delete prunedNavigator.userActivation;
            prunedNavigator.battery = (typeof prunedNavigator.battery === "object") && (prunedNavigator.battery.level !== 1);
            prunedNavigator.mozBattery = (typeof prunedNavigator.mozBattery === "object") && (prunedNavigator.mozBattery.level !== 1);
            prunedNavigator.webkitBattery = (typeof prunedNavigator.webkitBattery === "object") && (prunedNavigator.webkitBattery.level !== 1);
        } catch (exception) {
            //debug.addException(exception, "Detecas.Device.getNavigator");
        }

        return pruneNavigator(prunedNavigator);
    }

    function getOldNavigator(navigator) {
        var prunedNavigator = navigator;
        try {
            prunedNavigator = pruneAndParseJson(navigator, 3);

            if (typeof prunedNavigator.connection === "object") {

                if (typeof prunedNavigator.connection.downlink != "undefined") {
                    prunedNavigator.connection.downlink = null;
                }

                if (typeof prunedNavigator.connection.downlinkMax != "undefined") {
                    prunedNavigator.connection.downlinkMax = null;
                }

                prunedNavigator.connection.rtt = null;
            }

            prunedNavigator.plugins = "";
            prunedNavigator.appVersion = "";
            prunedNavigator.battery = (typeof prunedNavigator.battery === "object") && (prunedNavigator.battery.level !== 1);
            prunedNavigator.mozBattery = (typeof prunedNavigator.mozBattery === "object") && (prunedNavigator.mozBattery.level !== 1);
            prunedNavigator.webkitBattery = (typeof prunedNavigator.webkitBattery === "object") && (prunedNavigator.webkitBattery.level !== 1);
        } catch (exception) {
            //debug.addException(exception, "Detecas.Device.getOldNavigator");
        }

        return pruneNavigator(prunedNavigator);
    }

    function pruneNavigator(navigator) {
        try {
            var stripGuidCallback = {
                "userAgent": stripGuid,
                "appVersion": stripGuid
            };
            return json.prune(navigator, {
                depthDecr: 3,
                stripFuncs: stripGuidCallback
            });
        }
        catch (exception) {
            //debug.addException(exception, "Detecas.Device.pruneNavigator");
        }
    }

    function getDeviceDetail() {
        var devicedetail = {};
        try {
            devicedetail.browser = (browserResult.name === config.na) ? config.na : [browserResult.name, browserResult.version].join(stringBlank);
            devicedetail.deviceName = browserResult.deviceName;
            devicedetail.deviceType = browserResult.deviceType;
            devicedetail.engine = (browserResult.engine === config.na) ? config.na : [browserResult.engine, browserResult.engineVersion].join(stringBlank);
            devicedetail.OS = browserResult.OS;
            devicedetail.plugins = getBrowserPlugins();
            devicedetail.mimetypes = getMimeTypes();
            devicedetail.useragent = navigator.userAgent;
            devicedetail.screen = getScreenInformation();
            devicedetail.navigator = getNavigator(navigator);
            devicedetail.accept = config.accept;
            devicedetail.headers = navigator.vendor + "@" + config.headers;
        } catch (exception) {
            //debug.addException(exception, "Detecas.Device.getDeviceDetail");
        }

        return devicedetail;
    }

    function filterArray(array) {
        return array.filter(function (value) {
            return value !== null && value !== undefined && value !== "";
        });
    }

    function getOldFingerprint(devicedetail) {
        var fingerprint = {};
        try {
            var oldNavigator = getOldNavigator(navigator);
            fingerprint.navigator = sha1.hash(oldNavigator);
            fingerprint.plugins = devicedetail.plugins !== undefined ? sha1.hash(devicedetail.plugins) : "";
            fingerprint.screen = sha1.hash(devicedetail.screen);
            fingerprint.monitor = getMonitorInformation();
            fingerprint.timezone = new Date().getTimezoneOffset().toString();
            fingerprint.systemTimeOffset = "0";
            var codes = [fingerprint.navigator,
            fingerprint.plugins,
            fingerprint.screen,
            fingerprint.timezone,
            fingerprint.systemTimeOffset,
            fingerprint.monitor];
            codes = filterArray(codes);
            fingerprint.code = sha1.hash(codes.join("$"));
        } catch (exception) {
            //debug.addException(exception, "Detecas.Device.getOldFingerprint");
        }

        return fingerprint;
    }

    function getFingerprint(devicedetail) {
        var fingerprint = {};

        try {
            fingerprint.navigator = md5.hash(devicedetail.navigator);
            fingerprint.plugins = devicedetail.plugins !== undefined ? md5.hash(devicedetail.plugins) : "";
            fingerprint.mimetypes = md5.hash(devicedetail.mimetypes);
            fingerprint.useragent = md5.hash(devicedetail.useragent);
            fingerprint.fonts = md5.hash(fonts.getFonts());
            fingerprint.screen = md5.hash(devicedetail.screen);
            fingerprint.monitor = getMonitorInformation();
            fingerprint.timezone = new Date().getTimezoneOffset().toString();
            fingerprint.GPUCode = md5.hash(gpu.getGPUCode());
            fingerprint.WebGL = md5.hash(webgl.getWebGL());
            fingerprint.language = navigator.languages ? navigator.languages[0] : (navigator.language || navigator.userLanguage);
            fingerprint.cookieEnabled = !!navigator.cookieEnabled;
            fingerprint.doNotTrack = navigator.doNotTrack === "1" ? true : false;
            fingerprint.superCookies = browserResult.superCookies;
            fingerprint.systemTimeOffset = "0";
            fingerprint.browser = devicedetail.browser;
            fingerprint.deviceType = devicedetail.deviceType;
            fingerprint.accept = md5.hash(devicedetail.accept);
            fingerprint.headers = md5.hash(config.headers);

            var codes = [fingerprint.navigator, fingerprint.plugins, fingerprint.mimetypes, fingerprint.useragent, fingerprint.fonts,
            fingerprint.screen, fingerprint.monitor, fingerprint.timezone, fingerprint.GPUCode, fingerprint.WebGL, fingerprint.language,
            fingerprint.cookieEnabled, fingerprint.doNotTrack, fingerprint.superCookies, fingerprint.systemTimeOffset, fingerprint.browser, fingerprint.deviceType];
            codes = filterArray(codes);
            fingerprint.code = md5.hash(codes.join("$"));
        } catch (exception) {
            //debug.addException(exception, "Detecas.Device.getFingerprint");
        }

        return fingerprint;
    }

    function getDeviceInformation() {
        var userIp = {
            ip: Detecas.Config.ip,
            localIps: '[]'
        };

        var deviceDetail = getDeviceDetail();
        var oldFingerprint = getOldFingerprint(deviceDetail);
        var fingerprint = getFingerprint(deviceDetail);
        fingerprint.oldCode = oldFingerprint.code;
        deviceDetail.deviceCode = fingerprint.code;
        return {
            deviceDetail: deviceDetail,
            fingerprint: fingerprint,
            userIP: userIp
        };
    }

    return {
        getDeviceInformation: getDeviceInformation
    };
})(navigator, Detecas.Sha1, Detecas.Md5, Detecas.JSON, Detecas.GPU, Detecas.WebGL, Detecas.Config, Detecas.Browser, screen, Detecas.Fonts, Detecas.Debug);