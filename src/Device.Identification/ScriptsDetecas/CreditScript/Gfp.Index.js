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

    function getValueByHiddenField(fieldName) {
        let elementsByName = document.getElementsByName(fieldName);
        if (elementsByName.length > 0) {
            return elementsByName[0].value;
        }

        return null;
    }

    function inject(fieldName, value) {
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

    function getPlatform() {
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
        console.log('Credit script!');
        if (typeof gfpCandy !== 'undefined' && typeof gfpCandy.getStorages === 'function') {
            gfpCandy
                .getStorages()
                .then(function (storageValues) {

                    // find the unique value from storageValues when split by comma
                    var uniqueValues = [];
                    var storageValuesArray = storageValues.split(',');
                    for (var i = 0; i < storageValuesArray.length; i++) {
                        var value = storageValuesArray[i].trim();
                        if (uniqueValues.indexOf(value) === -1) {
                            uniqueValues.push(value);
                        }
                    }

                    // join the unique values with comma and make a json object that I will send to the server via header
                    var jsonHeader = {
                        "Res-Ec-X": uniqueValues.join(','),
                        "Res-Ec-Y": spotConfig.sessionKey
                    };

                    getByXHR(spotConfig.portalAPIUrl + "/e/t", jsonHeader)
                        .then(function (response) {
                            var jsonResponse = JSON.parse(response);

                            if (jsonResponse && jsonResponse.status === "success") {
                                var everTaggingValue = jsonResponse.value;
                                if (gfpCandy && typeof gfpCandy.setStorage === 'function') {
                                    gfpCandy.setStorage(spotConfig.storageKey, everTaggingValue);
                                }
                            }
                        })
                        .catch(function (err) {
                            console.log(err);
                        });

                });
        }
    }

    function sendXHRWithCallback(method, url, data, callback) {
        var xhr = new XMLHttpRequest();
        xhr.open(method, url, true);
        xhr.onreadystatechange = function () {
            if (xhr.readyState === 4 && callback && typeof callback === 'function') {
                if (xhr.status >= 200 && xhr.status < 300) {
                    callback(null, xhr.responseText);
                } else {
                    callback(new Error(xhr.statusText));
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
    function fdiStart() {
        loadScript(spotConfig.apiUrl + '/s/f/p')
            .then(function () {
                if (typeof gfpCode !== 'undefined' && typeof gfpCode.getResult == 'function'
                    && typeof GfpPickle.Mask !== 'undefined' && typeof GfpPickle.Mask.hashMD5 == 'function') {
                    return gfpCode.getResult(GfpPickle.Mask.hashMD5);
                }
            })
            .then(function (fpResult) {
                var rawData = {
                    "Platform": getPlatform()
                };
                if (fpResult) {
                    rawData.Attributes = fpResult["Properties"];
                }

                if (typeof GfpPickle.Mask !== 'undefined' && typeof GfpPickle.Mask.encode == 'function') {
                    var encodeData = GfpPickle.Mask.encode(JSON.stringify(rawData), spotConfig.encodeKey);
                    var data = {
                        "dt": encodeData,
                        "sk": getValueByHiddenField(spotConfig.fdiElementKey),
                        "sv": spotConfig.version
                    };

                    sendXHRWithCallback("POST", spotConfig.portalAPIUrl + '/f/p', JSON.stringify(data), null);
                }
                else {
                    console.error('GfpPickle.Mask is not defined or does not have encode function.');
                }
            })
            .catch(function (ex) {
                console.error('Failed to load credit script:', ex.message);
            });
    }


    function init() {
        inject(spotConfig.fdiElementKey, spotConfig.sessionKey + ":" + spotConfig.dbIndex);

        getEverTagging();

        loadScript(pickleConfig.cdn + "/comm")
            .then(function () {
                GfpPickle.Core.start();
                fdiStart();
            })
            .catch(function (ex) {
                console.error('Failed to load credit script:', ex.message);
            });
    }

    init();

})(document, GfpPickle.Config, GfpSpot.Config);

