var Detecas = {};
Detecas.Hex = (function () {
    "use strict";

    // refer: http://stackoverflow.com/questions/21647928/javascript-unicode-string-to-hex
    function encode(str) {
        var hex = "";
        for (var i = 0; i < str.length; i++) {
            hex += "" + str.charCodeAt(i).toString(16);
        }
        return hex;
    }

    function decode(hex) {
        var str = "";
        for (var i = 0; i < hex.length; i += 2) {
            str += String.fromCharCode(parseInt(hex.substr(i, 2), 16));
        }
        return str;
    }

    return {
        encode: encode,
        decode: decode,
    };
})();

Detecas.Common = (function (hex) {
    "use strict";

    function isValidObject(obj) {
        return typeof obj !== "undefined" && !!obj;
    }

    function isValidDeviceId(deviceId) {
        return (
            100 > deviceId.length &&
            10 < deviceId.length &&
            hex.decode(deviceId).length === 32
        ); // Valid device cookie
    }

    function isGuid(cookieCode) {
        var regexGuid =
            /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;
        return regexGuid.test(cookieCode);
    }

    function generateUUID() {
        // Public Domain/MIT
        var d = new Date().getTime(); //Timestamp
        var d2 =
            (typeof performance !== "undefined" &&
                performance.now &&
                performance.now() * 1000) ||
            0; //Time in microseconds since page-load or 0 if unsupported
        return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(
            /[xy]/g,
            function (c) {
                var r = Math.random() * 16; //random number between 0 and 16
                if (d > 0) {
                    //Use timestamp until depleted
                    r = (d + r) % 16 | 0;
                    d = Math.floor(d / 16);
                } else {
                    //Use microseconds since page-load if supported
                    r = (d2 + r) % 16 | 0;
                    d2 = Math.floor(d2 / 16);
                }
                return (c === "x" ? r : (r & 0x3) | 0x8).toString(16);
            }
        );
    }

    return {
        isValidObject: isValidObject,
        isValidDeviceId: isValidDeviceId,
        isGuid: isGuid,
        generateUUID: generateUUID,
    };
})(Detecas.Hex);

Detecas.Storages = (function (window, document, common) {
    "use strict";

    var storages = "LS,IDB,CK,WN";

    var cookieStorage = "(global.c3)";
    var storageHandlers = storages.split(",");
    var isSet = false;
    var timeout = 200;

    function evalHandler(handler) {
        return eval("Detecas.Storages." + handler);
    }

    function waitAllWithTimeout(promises) {
        const timeoutPromise = new Promise((resolve, reject) => {
            setTimeout(() => resolve(), timeout);
        });

        return Promise.all(
            promises.map((p) => Promise.race([p, timeoutPromise]))
        );
    }

    const getFirstMaxCountValue = function (arr) {
        let count = {};
        let maxCount = 0;
        let maxValue = null;

        if (arr && Array.isArray(arr) && arr.length > 0) {
            for (let i = 0; i < arr.length; i++) {
                if (count[arr[i]] === undefined) {
                    count[arr[i]] = 1;
                } else {
                    count[arr[i]]++;
                }
                if (count[arr[i]] > maxCount && arr[i] != undefined) {
                    maxCount = count[arr[i]];
                    maxValue = arr[i];
                }
            }
        }

        return new Promise((resolve) => {
            resolve(
                common.isGuid(maxValue) ? maxValue : common.generateUUID()
            );
        });
    };

    function get() {
        try {
            isSet = false;

            const promises = storageHandlers.map((handler) => {
                return new Promise((resolve) => {
                    if (Detecas.Storages.hasOwnProperty(handler)) {
                        resolve(evalHandler(handler).call(this, cookieStorage));
                    } else resolve("");
                });
            });

            console.log("promises", promises);
            return waitAllWithTimeout(promises).then((cookies) => {
                console.log("cookies", cookies);

                return getFirstMaxCountValue(cookies).then((cookieValue) => {
                    console.log("cookieValue", cookieValue);
                    Detecas.Storages.set(cookieValue);
                    return cookieValue;
                });
            });
        } catch (exception) { }
    }

    function set(value) {
        try {
            isSet = true;
            storageHandlers.forEach(function (handler) {
                if (Detecas.Storages.hasOwnProperty(handler)) {
                    evalHandler(handler).call(this, cookieStorage, value);
                }
            });
        } catch (exception) { }
    }

    function LS(name, value) {
        var result = gsLS(name, value);
        return result;
    }

    function gsLS(name, value) {
        try {
            return new Promise(function (resolve, reject) {
                var lsObj = window.localStorage;
                if (lsObj) {
                    if (isSet) {
                        lsObj.setItem(name, value);
                        resolve();
                    } else {
                        value = lsObj.getItem(name);
                        resolve(value);
                    }
                }
                resolve();
            });
        } catch (exception) { }
    }

    function IDB(name, value) {
        var result = gsIDB(
            "indexedDB_ec",
            "ec",
            name,
            value,
            isSet
        );
        return result;
    }

    function gsIDB(dbName, storeName, key, value, isSet) {
        return new Promise(function (resolve, reject) {
            var IDBObj =
                window.indexedDB ||
                window.mozIndexedDB ||
                window.webkitIndexedDB ||
                window.msIndexedDB ||
                window.shimIndexedDB;

            if (IDBObj) {
                var request = IDBObj.open(dbName);

                request.onerror = function () {
                    reject("Failed to open database");
                };

                request.onupgradeneeded = function (event) {
                    var db = event.target.result;
                    if (db.objectStoreNames.contains(storeName)) {
                        db.deleteObjectStore(storeName);
                    }
                    db.createObjectStore(storeName, {
                        keyPath: "name",
                    });
                };

                request.onsuccess = function (event) {
                    var db = event.target.result;
                    if (db) {
                        var transaction = db.transaction(
                            [storeName],
                            "readwrite"
                        );
                        if (
                            transaction.db.objectStoreNames.contains(storeName)
                        ) {
                            var store = transaction.objectStore(storeName);
                            store.get(dbName).onsuccess = function (
                                innerEvent
                            ) {
                                var data = innerEvent.target.result;
                                if (isSet) {
                                    store.put({
                                        name: dbName,
                                        value: value,
                                    });
                                } else {
                                    if (data) {
                                        resolve(data.value);
                                    } else {
                                        resolve(null);
                                    }
                                }
                            };
                        }
                    }
                };
            }
        });
    }

    function CK(name, value) {
        var result = gsCK(name, value);
        return result;
    }

    function gsCK(name, value) {
        return new Promise(function (resolve, reject) {
            try {
                if (isSet) {
                    var expires = "";
                    var days = 30;
                    if (days) {
                        var date = new Date();
                        date.setTime(
                            date.getTime() + days * 24 * 60 * 60 * 1000
                        );
                        expires = "; expires=" + date.toUTCString();
                    }

                    document.cookie =
                        name +
                        "=" +
                        (value || "") +
                        expires +
                        "; SameSite=None; Secure; path=/";
                } else {
                    var cookie = null;
                    var cookieArr = document.cookie.split(";");

                    for (var i = 0; i < cookieArr.length; i++) {
                        var cookiePair = cookieArr[i].split("=");
                        if (name == cookiePair[0].trim()) {
                            cookie = decodeURIComponent(cookiePair[1]);
                        }
                    }

                    resolve(cookie);
                }
            } catch (exception) {
                console.log(exception);
            }
        });
    }

    function WN(name, value) {
        var result = gsWN(name, value);
        return result;
    }

    function gsWN(name, value) {
        return new Promise(function (resolve, reject) {
            try {
                if (isSet) {
                    var currentValue = window.name;
                    if (
                        !common.isValidObject(currentValue) ||
                        common.isValidDeviceId(currentValue)
                    ) {
                        window.name = value;
                    }
                } else {
                    value = window.name;
                    if (!common.isValidDeviceId(value)) {
                        value = null;
                    }

                    resolve(value);
                }
            } catch (exception) {
                console.log(exception);
            }
        });
    }

    return {
        LS: LS,
        IDB: IDB,
        CK: CK,
        WN: WN,
        set: set,
        get: get,
    };
})(window, document, Detecas.Common);

var fpCookie = (function (exports) {
    "use strict";

    function storageHandler() {
        return Detecas.Storages.get();
    }

    exports.storageHandler = storageHandler;
    return exports;
})({});
