var fpNewCookie = (function (exports) {
    'use strict';

    var cookieKey = 'gfp';
    var storageHandlers = [localStorage, indexedDb, cookie, windowName];
    var isSet = false;
    var timeout = 200;
    function localStorage(key, value) {
        const result = getSetLocalStorage(key, value);
        return result;
    }
    function getSetLocalStorage(key, value) {
        try {
            const localStorageObj = window.localStorage;
            if (localStorageObj) {
                if (isSet) {
                    localStorageObj.setItem(key, value);
                    return Promise.resolve(null);
                }
                else {
                    value = localStorageObj.getItem(key);
                    return Promise.resolve(value);
                }
            }
        }
        catch (exception) {
        }
    }
    function indexedDb(key, value) {
        const result = getSetIndexedDb('indexedDB_ls', 'ls', key, value, isSet);
        return result;
    }
    function getSetIndexedDb(dbName, storeName, key, value, isSet) {
        return new Promise(function (resolve, reject) {
            const indexedDbObj = window.indexedDB || window.mozIndexedDB || window.webkitIndexedDB || window.msIndexedDB || window.shimIndexedDB;
            if (indexedDbObj) {
                const request = indexedDbObj.open(dbName);
                request.onerror = function () {
                    console.log('Failed to open database');
                    resolve(null);
                };
                request.onupgradeneeded = function (event) {
                    var db = event.target.result;
                    if (db.objectStoreNames.contains(storeName)) {
                        db.deleteObjectStore(storeName);
                    }
                    db.createObjectStore(storeName);
                };
                request.onsuccess = function (event) {
                    const db = event.target.result;
                    if (db) {
                        if (!db.objectStoreNames.contains(storeName)) {
                            db.close();
                            indexedDB.deleteDatabase(dbName);
                            const request2 = indexedDB.open(dbName, 1);
                            request2.onupgradeneeded = function (event) {
                                const db2 = event.target.result;
                                db2.createObjectStore(storeName);
                            };
                            request2.onsuccess = function (event) {
                                const db2 = event.target.result;
                                const transaction = db2.transaction(storeName, "readwrite");
                                const store = transaction.objectStore(storeName);
                                store.get(key).onsuccess = function (innerEvent) {
                                    if (isSet) {
                                        store.put(value, key);
                                        resolve(null);
                                    }
                                    else {
                                        const data = innerEvent.target.result;
                                        if (data) {
                                            resolve(data);
                                        }
                                        else {
                                            resolve(null);
                                        }
                                    }
                                };
                            };
                        }
                        else {
                            const transaction = db.transaction(storeName, 'readwrite');
                            if (transaction.db.objectStoreNames.contains(storeName)) {
                                const store = transaction.objectStore(storeName);
                                store.get(key).onsuccess = function (innerEvent) {
                                    if (isSet) {
                                        store.put(value, key);
                                        resolve(null);
                                    }
                                    else {
                                        const data = innerEvent.target.result;
                                        if (data) {
                                            resolve(data);
                                        }
                                        else {
                                            resolve(null);
                                        }
                                    }
                                };
                            }
                        }
                    }
                };
            }
        });
    }
    function cookie(key, value) {
        const result = getSetCookie(key, value);
        return result;
    }
    function getSetCookie(key, value) {
        return new Promise(function (resolve, reject) {
            try {
                if (isSet) {
                    let expires = '';
                    const days = 30;
                    if (days) {
                        const date = new Date();
                        date.setTime(date.getTime() + days * 24 * 60 * 60 * 1000);
                        expires = '; expires=' + date.toUTCString();
                    }
                    const setValue = key + '=' + (value || '') + expires + '; SameSite=None; Secure; path=/';
                    document.cookie = setValue;
                    resolve(null);
                }
                else {
                    let cookie = null;
                    const cookieArr = document.cookie.split(';');
                    for (let i = 0; i < cookieArr.length; i++) {
                        const cookiePair = cookieArr[i].split('=');
                        if (key == cookiePair[0].trim()) {
                            cookie = decodeURIComponent(cookiePair[1]);
                        }
                    }
                    resolve(cookie);
                }
            }
            catch (exception) {
            }
        });
    }
    function windowName(key, value) {
        const result = getSetWindowName(key, value);
        return result;
    }
    function getSetWindowName(key, value) {
        return new Promise(function (resolve, reject) {
            try {
                if (isSet) {
                    var currentValue = window[key] || '';
                    if (!isValidObject(currentValue) || validateUUID(currentValue)) {
                        window[key] = value;
                    }
                    resolve(null);
                }
                else {
                    value = window[key];
                    if (!validateUUID(value)) {
                        value = null;
                    }
                    resolve(value);
                }
            }
            catch (exception) {
            }
        });
    }
    function isValidObject(obj) {
        return typeof obj !== 'undefined' && !!obj;
    }
    function generateUUIDv4() {
        let uuid = '';
        for (let i = 0; i < 32; i++) {
            if (i === 8 || i === 12 || i === 16 || i === 20) {
                uuid += '-';
            }
            const random = (Math.random() * 16) | 0;
            if (i === 12) {
                uuid += '4';
            }
            else if (i === 16) {
                uuid += ((random & 3) | 8).toString(16);
            }
            else {
                uuid += random.toString(16);
            }
        }
        return uuid;
    }
    function validateUUID(uuid) {
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
    }
    function waitAllWithTimeout(promises) {
        const timeoutPromise = new Promise((resolve, reject) => {
            setTimeout(() => resolve(), timeout);
        });
        return Promise.all(promises.map((p) => Promise.race([p, timeoutPromise])));
    }
    function getFirstMaxCountValue(arr) {
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
    }
    function get(key) {
        try {
            isSet = false;
            const promises = storageHandlers.map((handler) => {
                return new Promise((resolve) => {
                    resolve(handler(key, null));
                });
            });
            return waitAllWithTimeout(promises).then((cookies) => {
                return getFirstMaxCountValue(cookies).then((cookieValue) => {
                    return Promise.resolve(cookieValue);
                });
            });
        }
        catch (exception) {
        }
    }
    function set(key, value) {
        try {
            isSet = true;
            storageHandlers.forEach(function (handler) {
                handler(key, value);
            });
        }
        catch (exception) {
        }
    }
    function getStorage(key = cookieKey) {
        return get(key);
    }
    function setStorage(key = cookieKey, value) {
        if (key && value)
            set(key, value);
    }

    exports.getStorage = getStorage;
    exports.setStorage = setStorage;

    Object.defineProperty(exports, '__esModule', { value: true });

    return exports;

})({});
