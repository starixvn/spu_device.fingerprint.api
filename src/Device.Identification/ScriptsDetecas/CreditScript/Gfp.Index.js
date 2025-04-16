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

    //function getCookie(key) {
    //    var cookies = document.cookie.split('; ');

    //    for (var i = 0; i < cookies.length; i++) {
    //        var cookieParts = cookies[i].split('=');
    //        var cookieName = cookieParts[0];
    //        var cookieValue = cookieParts[1];

    //        cookieName = cookieName.trim();
    //        if (cookieName === key) {
    //            return decodeURIComponent(cookieValue);
    //        }
    //    }

    //    return "";
    //}
    //function sendXHRWith(method, url, data, callback) {
    //    var xhr = new XMLHttpRequest();
    //    xhr.open(method, url, true);
    //    xhr.onreadystatechange = function () {
    //        if (xhr.readyState === 4 && callback && typeof callback === 'function') {
    //            if (xhr.status >= 200 && xhr.status < 300) {
    //                callback(null, xhr.responseText);
    //            } else {
    //                callback(new Error(xhr.statusText));
    //            }
    //        }
    //    };

    //    if (method === "POST" && data) {
    //        xhr.setRequestHeader("Content-Type", "application/json");
    //        xhr.send(data);
    //    } else {
    //        xhr.send();
    //    }
    //}

    function getEverTagging() {
        if (gfpCandy && typeof gfpCandy.getStorages === 'function') {
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

    function init() {

        inject(spotConfig.fdiElementKey, spotConfig.sessionKey + ":" + spotConfig.dbIndex);

        getEverTagging();

        loadScript(pickleConfig.cdn + "/tk")
            .then(function () {
                if (ConfigJWT != null &&
                    typeof ConfigJWT === "object" &&
                    typeof ConfigJWT.injectToken === "function") {
                    ConfigJWT.injectToken();
                }
            })
            .then(function () {
                loadScript(pickleConfig.cdn + "/comm")
                    .then(function () {
                        Detecas.Core.start();

                    })
                    .catch(function () {
                        console.error('Failed to load credit script');
                    });
            })
            .catch(function () {
                console.error('Failed to load token script');
            });

        loadScript(spotConfig.apiUrl + '/s/c/r/t')
            .then(function () {
                loadScript(spotConfig.apiUrl + '/s/f/p')
                    .then(function () {
                        if (gfpCode && typeof gfpCode.getResult == 'function'
                            && gfpMask && typeof gfpMask.hashMD5 == 'function') {
                            return gfpCode.getResult(gfpMask.hashMD5);
                        }
                    })
                    .then(function (fpResult) {
                        var rawData = {
                            "Platform": getPlatform()
                        };
                        if (fpResult) {
                            rawData.Attributes = fpResult["Properties"];
                        }

                        var encodeData = gfpMask.encode(JSON.stringify(rawData), spotConfig.encodeKey);

                        var data = {
                            "dt": encodeData,
                            "sk": getValueByHiddenField(fdiElementKey),
                            "sv": version
                        };

                        fp.post(spotConfig.portalAPIUrl + '/f/p', data);
                    })
                    .catch(function () {
                        console.error('Failed to load credit script');
                    });
            })
            .catch(function () {
                console.error('Failed to load token script');
            });
    }

    init();

})(document, GfpPickle.Config, GfpSpot.Config);

