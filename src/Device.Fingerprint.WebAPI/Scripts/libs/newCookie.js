var fpNewCookie = (function (exports) {
    'use strict';

    const fnStorages = (function () {
        const LS = function (name, value) {
            var result = gsLS(name, value);
            return result;
        };
        const gsLS = function (name, value) {
            try {
                var lsObj = window.localStorage;
                if (lsObj) {
                    if (isSet) {
                        lsObj.setItem(name, value);
                    }
                    else {
                        value = lsObj.getItem(name);
                        return Promise.resolve(value);
                    }
                }
            }
            catch (exception) {
                console.log("gsLS: ", exception);
            }
        };
        const IDB = function (name, value) {
            var result = gsIDB("indexedDB_ec", "ec", name, value, isSet);
            return result;
        };
        const gsIDB = function (dbName, storeName, key, value, isSet) {
            return new Promise(function (resolve, reject) {
                var IDBObj = window.indexedDB ||
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
                            var transaction = db.transaction([storeName], "readwrite");
                            if (transaction.db.objectStoreNames.contains(storeName)) {
                                var store = transaction.objectStore(storeName);
                                store.get(dbName).onsuccess = function (innerEvent) {
                                    var data = innerEvent.target
                                        .result;
                                    if (isSet) {
                                        store.put({
                                            name: dbName,
                                            value: value,
                                        });
                                    }
                                    else {
                                        if (data) {
                                            resolve(data.value);
                                        }
                                        else {
                                            resolve(null);
                                        }
                                    }
                                };
                            }
                        }
                    };
                }
            });
        };
        const CK = function (name, value) {
            var result = gsCK(name, value);
            return result;
        };
        function gsCK(name, value) {
            return new Promise(function (resolve, reject) {
                try {
                    if (isSet) {
                        var expires = "";
                        var days = 30;
                        if (days) {
                            var date = new Date();
                            date.setTime(date.getTime() + days * 24 * 60 * 60 * 1000);
                            expires = "; expires=" + date.toUTCString();
                        }
                        document.cookie =
                            name +
                                "=" +
                                (value || "") +
                                expires +
                                "; SameSite=None; Secure; path=/";
                    }
                    else {
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
                }
                catch (exception) {
                    console.log("gsCK: ", exception);
                }
            });
        }
        const WN = function (name, value) {
            var result = gsWN(name, value);
            return result;
        };
        const gsWN = function (name, value) {
            return new Promise(function (resolve, reject) {
                try {
                    if (isSet) {
                        var currentValue = window.name;
                        if (!isValidObject(currentValue) ||
                            validateUUID(currentValue)) {
                            window.name = value;
                        }
                    }
                    else {
                        value = window.name;
                        if (!validateUUID(value)) {
                            value = null;
                        }
                        resolve(value);
                    }
                }
                catch (exception) {
                    console.log("gsWN: ", exception);
                }
            });
        };
        return {
            LS: LS,
            IDB: IDB,
            CK: CK,
            WN: WN,
        };
    })();
    const isValidObject = function (obj) {
        return typeof obj !== "undefined" && !!obj;
    };
    const generateUUIDv4 = function () {
        let uuid = "";
        for (let i = 0; i < 32; i++) {
            if (i === 8 || i === 12 || i === 16 || i === 20) {
                uuid += "-";
            }
            const random = (Math.random() * 16) | 0;
            if (i === 12) {
                uuid += "4";
            }
            else if (i === 16) {
                uuid += ((random & 3) | 8).toString(16);
            }
            else {
                uuid += random.toString(16);
            }
        }
        return uuid;
    };
    const validateUUID = function (uuid) {
        const uuidRegex = /^[a-f\d]{8}-[a-f\d]{4}-4[a-f\d]{3}-[8|9|aA|bB][a-f\d]{3}-[a-f\d]{12}$/i;
        if (!uuidRegex.test(uuid)) {
            return false;
        }
        // Kiểm tra phiên bản UUID
        const versionBits = parseInt(uuid.charAt(14), 16);
        if (versionBits !== 4) {
            return false;
        }
        // Kiểm tra trường hợp của ký tự
        if (uuid !== uuid.toLowerCase()) {
            return false;
        }
        return true;
    };
    var cookieStorage = "(global.c3)";
    var storageHandlers = [fnStorages.LS, fnStorages.IDB, fnStorages.CK, fnStorages.WN];
    var isSet = false;
    var timeout = 200;
    const waitAllWithTimeout = function (promises) {
        const timeoutPromise = new Promise((resolve, reject) => {
            setTimeout(() => resolve(), timeout);
        });
        return Promise.all(promises.map((p) => Promise.race([p, timeoutPromise])));
    };
    const getFirstMaxCountValue = function (arr) {
        let count = {};
        let maxCount = 0;
        let maxValue = null;
        if (arr && Array.isArray(arr) && arr.length > 0) {
            for (let i = 0; i < arr.length; i++) {
                if (count[arr[i]] === undefined) {
                    count[arr[i]] = 1;
                }
                else {
                    count[arr[i]]++;
                }
                if (count[arr[i]] > maxCount && arr[i] != undefined) {
                    maxCount = count[arr[i]];
                    maxValue = arr[i];
                }
            }
        }
        return new Promise((resolve) => {
            resolve(validateUUID(maxValue) ? maxValue : generateUUIDv4());
        });
    };
    const get = function () {
        try {
            isSet = false;
            const promises = storageHandlers.map((handler) => {
                return new Promise((resolve) => {
                    resolve(handler(cookieStorage, null));
                });
            });
            console.log("promises", promises);
            return waitAllWithTimeout(promises).then((cookies) => {
                console.log("cookies", cookies);
                return getFirstMaxCountValue(cookies).then((cookieValue) => {
                    console.log("cookieValue", cookieValue);
                    return Promise.resolve(cookieValue);
                });
            });
        }
        catch (exception) {
            console.log("Get error: ", exception);
        }
    };
    const set = function (value) {
        try {
            isSet = true;
            storageHandlers.forEach(function (handler) {
                handler(cookieStorage, value);
            });
        }
        catch (exception) {
            console.log("Set error: ", exception);
        }
    };
    function getStorage() {
        return get();
    }
    function setStorage(value) {
        if (value)
            set(value);
    }

    exports.getStorage = getStorage;
    exports.setStorage = setStorage;

    Object.defineProperty(exports, '__esModule', { value: true });

    return exports;

})({});
