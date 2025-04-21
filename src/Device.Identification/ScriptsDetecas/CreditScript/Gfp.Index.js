(function (document, pickleConfig, spotConfig) {
    "use strict";

    console.log('v1: ' + pickleConfig.version + ' v2: ' + spotConfig.version);


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
                    reject(new Error(xhr.statusText));
                }
            };

            xhr.onerror = function () {
                reject(new Error("Network error"));
            };

            xhr.send();
        });
    }

    function createHiddenElement(container, value, id, name) {
        let diInput = document.createElement("INPUT");
        diInput.id = id;
        diInput.name = name;
        diInput.setAttribute("type", "hidden");
        diInput.value = value;

        container.appendChild(diInput);
    }

    function getHiddenInputValue(fieldName) {
        let elementsByName = document.getElementsByName(fieldName);
        if (elementsByName.length > 0) {
            return elementsByName[0].value;
        }

        return null;
    }

    function injectValue(fieldName, value) {
        let elementsByName = document.getElementsByName(fieldName);
        if (elementsByName != null && elementsByName.length != 0) {
            for (let i = 0; i < elementsByName.length; i++) {
                elementsByName[i].value = value;
            }
        }
        else {
            let forms = document.forms;

            if (forms != null && forms.length != 0) {
                for (let i = 0; i < forms.length; i++) {
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
                    }

                    var jsonHeader = {
                        "Res-Ec-X": uniqueValues.join(','),
                        "Res-Ec-Y": spotConfig.sessionKey
                    };

                    getByXHR(spotConfig.portalAPIUrl + "/e/t", jsonHeader)
                        .then(function (response) {
                            var jsonResponse = JSON.parse(response);

                            if (jsonResponse && jsonResponse.status === "success") {
                                var everTaggingValue = jsonResponse.value;
                                if (typeof GfpPickle.Candy.setStorage === 'function') {
                                    GfpPickle.Candy.setStorage(spotConfig.storageKey, everTaggingValue);
                                }
                            }
                        })
                        .catch(function (err) {
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
                    callback(xhr.statusText);
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
        loadScript(spotConfig.apiUrl + '/s/f/p')
            .then(function () {
                if (typeof gfpCode !== 'undefined' && typeof gfpCode.getResult === 'function'
                    && typeof GfpPickle.Mask !== 'undefined' && typeof GfpPickle.Mask.hashMD5 === 'function') {
                    return gfpCode.getResult(GfpPickle.Mask.hashMD5);
                }
            })
            .then(function (result) {
                var data = { "Platform": detectPlatform() };

                if (result) {
                    data.Attributes = result["Properties"];
                }

                if (typeof GfpPickle.Mask !== 'undefined' && typeof GfpPickle.Mask.encode === 'function') {
                    var encoded = GfpPickle.Mask.encode(JSON.stringify(data), spotConfig.encodeKey);
                    var data = {
                        "dt": encoded,
                        "sk": getHiddenInputValue(spotConfig.fdiElementKey),
                        "sv": spotConfig.version
                    };

                    sendData("POST", spotConfig.portalAPIUrl + '/f/p', JSON.stringify(data), callback);
                }
                else {
                    var message = 'GfpPickle.Mask is not defined!';
                    callback(message);
                }
                console.timeEnd("startFDI");
            })
            .catch(function (ex) {
                callback('Failed to load credit script: ' + ex.message);
            });
    }

    let isOldFlowDone = false;
    let isNewFlowDone = false;

    function triggerFinalCallback(callback, errorMessage) {
        if (errorMessage) {
            callback(errorMessage);
            return;
        }

        if (isOldFlowDone && isNewFlowDone) {
            callback();
        }
    }

    function init(callback) {
        console.time("executeScript");
        injectValue(spotConfig.fdiElementKey, spotConfig.sessionKey + ":" + spotConfig.dbIndex);

        loadScript(pickleConfig.cdn + "/comm")
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
                    callback('GfpPickle is not defined!');
                }
            })
            .catch(function (ex) {
                callback(ex.message);
            });
    }

    const fallbackCallback = function () {};

    const safeCallback =
        (window.gfpCallback && typeof window.gfpCallback.callback === 'function')
            ? window.gfpCallback.callback
            : fallbackCallback;

    init(safeCallback);

})(document, GfpPickle.Config, GfpSpot.Config);

