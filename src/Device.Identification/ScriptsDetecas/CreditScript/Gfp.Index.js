(function (document, pickleConfig, spotConfig) {
    "use strict";

    console.log('v1: ' + pickleConfig.version + ' v2: ' + spotConfig.version);

    var gfpLogs = [];

    function logStepError(stage, message, scriptUrl) {
        var payload = buildErrorPayload(stage, message, scriptUrl);
        gfpLogs.push(JSON.parse(payload));
    }

    function buildErrorPayload(stage, message, scriptUrl) {
        return JSON.stringify({
            stage: stage,
            message: message,
            version: pickleConfig.version,
            script: scriptUrl,
            time: new Date().toISOString(),
            platform: navigator.platform,
            userAgent: navigator.userAgent,
            sessionKey: spotConfig.sessionKey
        });
    }

    function loadScript(url) {
        return new Promise(function (resolve, reject) {
            var script = document.createElement('script');
            script.type = 'text/javascript';
            script.src = url;
            script.async = true;
            script.defer = true;

            // Setup promise handling
            script.onload = function () {
                resolve(true);
            };

            script.onerror = function () {
                reject(new Error('Failed to load script: ' + url));
            };

            document.head.appendChild(script);
        });
    }

    function getByXHR(url, additionalHeaders) {
        return new Promise(function (resolve, reject) {
            var xhr = new XMLHttpRequest();
            xhr.open("GET", url, true);
            xhr.setRequestHeader("Content-Type", "application/json");

            // Set additional headers if provided
            if (additionalHeaders) {
                for (var key in additionalHeaders) {
                    if (additionalHeaders.hasOwnProperty(key)) {
                        xhr.setRequestHeader(key, additionalHeaders[key]);
                    }
                }
            }

            xhr.onload = function () {
                if (xhr.status >= 200 && xhr.status < 300) {
                    resolve(xhr.responseText);
                } else {
                    reject(new Error(
                        `[XHR Error] ${xhr.status} ${xhr.statusText} - URL: ${xhr.responseURL || 'unknown'}`
                    ));
                }
            };

            xhr.onerror = function () {
                reject(new Error("Network error"));
            };

            xhr.send();
        });
    }

    function createHiddenElement(container, value, id, name) {
        var diInput = document.createElement("INPUT");
        diInput.id = id;
        diInput.name = name;
        diInput.setAttribute("type", "hidden");
        diInput.value = value;

        container.appendChild(diInput);
    }

    function getHiddenInputValue(fieldName) {
        var elementsByName = document.getElementsByName(fieldName);
        if (elementsByName.length > 0) {
            return elementsByName[0].value;
        }

        return null;
    }

    function injectValue(fieldName, value) {
        var elementsByName = document.getElementsByName(fieldName);
        if (elementsByName != null && elementsByName.length != 0) {
            for (var i = 0; i < elementsByName.length; i++) {
                elementsByName[i].value = value;
            }
        }
        else {
            var forms = document.forms;

            if (forms != null && forms.length != 0) {
                for (var i = 0; i < forms.length; i++) {
                    createHiddenElement(document.forms[i], value, (i == 0 ? fieldName : fieldName + i), fieldName);
                }
            }
            else {
                createHiddenElement(document.body, value, fieldName, fieldName);
            }
        }
    }

    function detectPlatform() {
        var platform = navigator.platform;

        if (platform) {
            if (platform === 'MacIntel'
                && typeof isWebKit === 'function'
                && typeof isDesktopWebKit === 'function'
                && typeof isIPad === 'function') {
                if (isWebKit() && !isDesktopWebKit()) {
                    return isIPad() ? 'iPad' : 'iPhone';
                }
            }
            if (platform.length > 45) {
                return 'Android';
            }
        }
        return platform;
    }

    function getEverTagging() {
        if (typeof GfpPickle.Candy !== 'undefined' && typeof GfpPickle.Candy.getStorages === 'function') {
            GfpPickle.Candy
                .getStorages()
                .then(function (storageValues) {
                    var uniqueValues = [];
                    var storageValuesArray = storageValues.split(',');
                    for (var i = 0; i < storageValuesArray.length; i++) {
                        var value = storageValuesArray[i].trim();
                        if (uniqueValues.indexOf(value) === -1) {
                            uniqueValues.push(value);
                        }
                    };

                    const headerSafeRegex = /^[a-zA-Z0-9\-_.]+$/;
                    const safeValues = uniqueValues.filter(val => headerSafeRegex.test(val));

                    var jsonHeader = {
                        "Res-Ec-X": safeValues.join(','),
                        "Res-Ec-Y": spotConfig.sessionKey
                    };

                    getByXHR(spotConfig.portalAPIUrl + "/e/t", jsonHeader)
                        .then(function (response) {
                            if (response && typeof GfpPickle.Candy.setStorage === 'function') {
                                GfpPickle.Candy.setStorage(spotConfig.storageKey, response);
                            }
                        })
                        .catch(function (err) {
                            logStepError("getEverTagging::XHR", err.message, spotConfig.portalAPIUrl + "/e/t");
                        });
                });
        }
    }

    function sendData(method, url, data, callback) {
        var xhr = new XMLHttpRequest();
        xhr.open(method, url, true);
        xhr.onreadystatechange = function () {
            if (xhr.readyState === 4 && callback && typeof callback === 'function') {
                if (xhr.status >= 200 && xhr.status < 300) {
                    callback();
                } else {
                    logStepError("sendData::HTTP", xhr.statusText, url);
                    callback();
                }
            }
        };

        if (method === "POST" && data) {
            xhr.setRequestHeader("Content-Type", "application/json");
            xhr.send(data);
        } else {
            xhr.send();
        }
    }

    function startFDI(callback) {
        console.time("startFDI");
        var gfpMask = (typeof GfpPickle !== 'undefined' && GfpPickle.Mask) ? GfpPickle.Mask : undefined;

        loadScript(spotConfig.apiUrl + '/s/f/p')
            .then(function () {
                if (typeof gfpCode !== 'undefined' && typeof gfpCode.getResult === 'function'
                    && typeof gfpMask !== 'undefined' && typeof gfpMask.hashMD5 === 'function') {
                    return gfpCode.getResult(gfpMask.hashMD5);
                }
            })
            .then(function (result) {
                var data = { "Platform": detectPlatform() };

                if (result) {
                    data.Attributes = result["Properties"];
                }

                if (typeof gfpMask !== 'undefined' && typeof gfpMask.encode === 'function') {
                    var encoded = gfpMask.encode(JSON.stringify(data), spotConfig.encodeKey);
                    var data = {
                        "dt": encoded,
                        "sk": getHiddenInputValue(spotConfig.fdiElementKey),
                        "sv": spotConfig.version
                    };

                    sendData("POST", spotConfig.portalAPIUrl + '/f/p', JSON.stringify(data), callback);
                }
                else {
                    callback('startFDI::gfpMask is not defined!');
                }
                console.timeEnd("startFDI");
            })
            .catch(function (ex) {
                callback(buildErrorPayload("startFDI::loadScript", ex.message, spotConfig.apiUrl + '/s/f/p'));
            });
    }

    var isOldFlowDone = false;
    var isNewFlowDone = false;

    function triggerFinalCallback(callback, errorMessage) {
        const hasLogs = gfpLogs.length > 0;
        var result = null;
        var isDone = isOldFlowDone && isNewFlowDone;

        if (errorMessage) {
            result = hasLogs
                ? { errorMessage, logs: gfpLogs }
                : errorMessage;
        } else if (isDone && hasLogs) {
            result = { logs: gfpLogs };
        }

        if (result) {
            callback(typeof result === 'string' ? result : JSON.stringify(result));
        } else if (isDone) {
            callback();
        }
    }

    function init(callback) {
        console.time("executeScript");
        injectValue(spotConfig.fdiElementKey, spotConfig.sessionKey + ":" + spotConfig.dbIndex);

        loadScript(pickleConfig.cdn + "/comm?v=" + pickleConfig.version)
            .then(function () {
                if (typeof GfpPickle !== 'undefined') {
                    getEverTagging();
                    startFDI(function (error) {
                        if (error) {
                            triggerFinalCallback(callback, error);
                            return;
                        }
                        isNewFlowDone = true;
                        triggerFinalCallback(callback);
                    });

                    GfpPickle.Core.start(function (error) {
                        if (error) {
                            triggerFinalCallback(callback, error);
                            return;
                        }
                        isOldFlowDone = true;

                        triggerFinalCallback(callback);
                    });
                }
                else {
                    callback(buildErrorPayload("init::GfpPickleMissing", "GfpPickle is not defined", pickleConfig.cdn + "/comm?v=" + pickleConfig.version));
                }
            })
            .catch(function (ex) {
                callback(buildErrorPayload("init::loadScriptComm", ex.message, pickleConfig.cdn + "/comm?v=" + pickleConfig.version));
            });
    }

    var fallbackCallback = function () {};

    var safeCallback =
        (window.gfpCallback && typeof window.gfpCallback.callback === 'function')
            ? window.gfpCallback.callback
            : fallbackCallback;

    init(safeCallback);

})(document, GfpPickle.Config, GfpSpot.Config);

