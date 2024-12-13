var gfpSpot = (function () {
    'use strict';

    var apiUrl = '__apiUrl__';
    var portalAPIUrl = '__portalAPIUrl__';
    var version = '202406211000';
    var encodeKey = '__key__';
    var sessionKey = '__sessionKey__';
    var dbIndex = '__dbIndex__';

    var jsChallengeAPIUrl = '__jsChallengeAPIUrl__';
    var jsChallengeVersion = '__jsChallengeVersion__';

    var fdiElementKey = '__fdi';
    var storageKey = 'gfp';

    if (typeof window !== 'undefined' && window !== null && window.location) {
        apiUrl = window.location.protocol + apiUrl;
    }

    var fp = {
        requireJS: function (_url) {
            return new Promise(function (resolve, reject) {
                try {
                    var script = document.createElement('script');
                    script.type = 'text/javascript';
                    script.src = _url;
                    script.async = true;
                    script.defer = true;

                    script.onload = function () {
                        resolve(script);
                    };

                    script.onerror = function (event) {
                        var errorObj = {
                            message: "Load Script requireJS: " + _url,
                            url: _url,
                            event: event
                        };
                        reject(errorObj);
                    };

                    document.head.appendChild(script);
                }
                catch (err) {
                    fp.log(portalAPIUrl, "Load Script requireJS", err.message);
                }
            });
        },
        checkServerAvailability: function (url) {
            var xhr = new XMLHttpRequest();

            xhr.onerror = function () {
                fp.log(portalAPIUrl, "CheckServerAvailability", url);
            };

            xhr.open('GET', url, true);
            xhr.send();
        },
        get: function (url) {
            return fetch(url, {
                method: "GET",
                headers: {
                    "Content-Type": "application/json",
                    "Cache-Control": "no-cache"
                }
            });
        },

        getByXHR: function (url, additionalHeaders) {
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
        },

        getPlatform: function () {
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
        },

        getCookie: function (key) {
            var cookies = document.cookie.split('; ');

            for (var i = 0; i < cookies.length; i++) {
                var cookieParts = cookies[i].split('=');
                var cookieName = cookieParts[0];
                var cookieValue = cookieParts[1];

                cookieName = cookieName.trim();
                if (cookieName === key) {
                    return decodeURIComponent(cookieValue);
                }
            }

            return "";
        },

        getDeviceCode: function () {
            var di = document.getElementById("__di");
            if (!di || !di.value) {
                return "";
            }

            if (typeof Detecas !== 'object' || typeof Detecas.Base64 !== 'object' || typeof Detecas.Base64.decode !== 'function') {
                return "";
            }

            try {
                var decodeValue = Detecas.Base64.decode(di.value);
                var json = JSON.parse(decodeValue);

                if (json && typeof json === 'object') {
                    return json.deviceId || "";
                }
                return "";
            }
            catch {
                return "";
            }
        },
        getWithCache: function (url) {
            return fetch(url, {
                method: "GET",
                headers: {
                    "Content-Type": "text/plain",
                    "Cache-Control": "max-age=31536000, immutable"
                }
            });
        },


        createHiddenElement: function (container, diValue, id, name) {
            let diInput = document.createElement("INPUT");
            diInput.id = id;
            diInput.name = name;
            diInput.setAttribute("type", "hidden");
            diInput.value = diValue;

            container.appendChild(diInput);
        },

        inject: function (fieldName, value) {
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
                        fp.createHiddenElement(document.forms[i], value, (i == 0 ? fieldName : fieldName + i), fieldName);
                    }
                }
                else {
                    fp.createHiddenElement(document.body, value, fieldName, fieldName);
                }
            }
        },

        post: function (url, data) {
            fp.sendXHRWithCallback("POST", url, JSON.stringify(data), null);
        },
        log: function (url, functionName, exception) {
            var currentOrigin = "";

            if (typeof window !== 'undefined' && window !== null && window.location) {
                if (window.location.origin) {
                    currentOrigin = window.location.origin;
                } else {
                    currentOrigin = window.location.protocol + '//' + window.location.hostname + (window.location.port ? ':' + window.location.port : '');
                }
            }

            var newCookieValue = fp.getCookie(storageKey);
            var deviceCode = fp.getDeviceCode();

            var data = {
                "sv": version,
                "fn": functionName,
                "ex": exception,
                "au": apiUrl,
                "hn": currentOrigin,
                "sk": sessionKey,
                "dc": deviceCode,
                "nc": newCookieValue
            };

            fp.post(url + "/c/l/e", data);
        },


        getEverTagging: function () {
            var getHeaders = function (storageValues) {
                try {

                    var storageArray = storageValues ? storageValues.split(',') : [];
                    var combinedValues = [sessionKey != null ? String(sessionKey) : ''].concat(storageArray);
                    var uniqueParts = combinedValues.filter(function (value, index, self) {
                        return value && self.indexOf(value) === index;
                    });

                    var collectEcValues = uniqueParts.join(',');

                    var headers = {
                        "Res-Ec-X": collectEcValues,
                        "Res-Ec-Y": sessionKey,
                    };

                    return headers;
                } catch (err) {
                    fp.log(portalAPIUrl, "GetHdEt", err.message);
                    return {};
                }
            };

            var handleResponse = function (res, callback) {
                try {
                    var key = (typeof res === 'string' && res.trim() !== '') ? res : sessionKey;

                    callback(key);

                    if (gfpCandy && typeof gfpCandy.setStorage === 'function') {
                        gfpCandy.setStorage(storageKey, key);
                    }
                    else {
                        fp.log(portalAPIUrl, "gCd", err.message);
                    }
                } catch (err) {
                    fp.log(portalAPIUrl, "rpEt", err.message);
                }
            };

            var requestEverTagging = function (callback, storageValues) {
                var headers = getHeaders(storageValues);

                fp.getByXHR(portalAPIUrl + '/e/t', headers)
                    .then(function (res) {
                        handleResponse(res, callback);
                    }).catch(function (err) {
                        fp.log(portalAPIUrl, "rqEt", err.message);
                    });
            };

            return new Promise(function (resolve, reject) {
                try {
                    if (gfpCandy && typeof gfpCandy.getStorages === 'function') {
                        gfpCandy.getStorages().then(function (storageValues) {
                            return requestEverTagging(resolve, storageValues);
                        }).catch(function (err) {
                            reject(err.message);
                        });
                    } else {
                        return requestEverTagging(resolve);
                    }
                } catch (err) {
                    reject(err.message);
                }
            });
        },

        getValueByHiddenField: function (fieldName) {
            let elementsByName = document.getElementsByName(fieldName);
            if (elementsByName.length > 0) {
                return elementsByName[0].value;
            }

            return null;
        },

        postFpData: function (sessionKey) {
            fp.getEverTagging();

            fp.requireJS(apiUrl + '/s/f/p')
                .then(function () {
                    if (gfpCode && typeof gfpCode.getResult == 'function'
                        && gfpMask && typeof gfpMask.hashMD5 == 'function') {
                        return gfpCode.getResult(gfpMask.hashMD5);
                    }
                    else {
                        fp.log(portalAPIUrl, "/s/f/p", "fn is undefined!");
                        return Promise.resolve();
                    }
                })
                .then(function (fpResult) {
                    var rawData = {
                        "Platform": fp.getPlatform()
                    };
                    if (fpResult) {
                        rawData.Attributes = fpResult["Properties"];
                    }

                    var encodeData = gfpMask.encode(JSON.stringify(rawData), encodeKey);

                    var data = {
                        "dt": encodeData,
                        "sk": fp.getValueByHiddenField(fdiElementKey),
                        "sv": version
                    };

                    fp.post(portalAPIUrl + '/f/p', data);
                }).catch(function (err) {
                    fp.log(portalAPIUrl, "/s/f/p", err.message);
                    return Promise.resolve();
                });
        },

        sendXHRWithPromise: function (method, url, data) {
            return new Promise((resolve, reject) => {
                var xhr = new XMLHttpRequest();
                xhr.open(method, url, true);
                xhr.onreadystatechange = function () {
                    if (xhr.readyState === 4) {
                        if (xhr.status >= 200 && xhr.status < 300) {
                            resolve(xhr.responseText);
                        } else {
                            reject(new Error(xhr.statusText));
                        }
                    }
                };

                if (method === "POST" && data) {
                    xhr.setRequestHeader("Content-Type", "application/json");
                    xhr.send(JSON.stringify(data));
                } else {
                    xhr.send();
                }
            });
        },
        sendXHRWithCallback: function (method, url, data, callback) {
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
        },

        getSessionKeyWithDbIndex: function (key, index) {
            if (index) {
                return key + ':' + index;
            }
            return key;
        },

        injectFdi: function () {
            fp.inject(fdiElementKey, fp.getSessionKeyWithDbIndex(sessionKey, dbIndex));
        },

        start: function () {
            try {
                if (typeof jsChallengeAPIUrl === "string" && jsChallengeAPIUrl.length > 0) {

                    fp.requireJS(jsChallengeAPIUrl + "/f/p/i/orchestrate_chl/" + jsChallengeVersion)
                        .catch(function (err) {
                            fp.log(portalAPIUrl, "Orchestrate", err.message);
                        });
                }

                 fp.injectFdi();

                fp.requireJS(apiUrl + '/s/c/r/t').then(function () {
                    fp.postFpData(sessionKey);
                }).catch(function (err) {
                    fp.log(portalAPIUrl, "s/c/r/t", err.message);
                });


                fp.checkServerAvailability(apiUrl + "/hc");

                if (typeof Promise === "undefined" || Promise.toString().indexOf("[native code]") === -1) {
                    fp.log(portalAPIUrl, "Browser", "Promise is unavailable!");
                }
            }
            catch (err) {
                fp.log(portalAPIUrl, "js start", err.toString());
            }
        },
    };

    return {
        start: fp.start,
        injectFdi: fp.injectFdi
    }
})();