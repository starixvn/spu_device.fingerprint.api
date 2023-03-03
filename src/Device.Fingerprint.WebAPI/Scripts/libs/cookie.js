
var diInput = null;
var fpsStartTime = new Date();
var fpsTimeElapsed = 0;
var fpsIsStop = false;
var fpsMaxTime = 4000; // (total = 5s) use remaining 1s  for Detecas.Browser, Detecas.Device and Detecas.Core
var fpsIsMob = /iPhone|iPad|iPod|Android|BlackBerry|IEMobile/i.test(navigator.userAgent);
function fpsChkIntr(item, maxtime) {

    if (!fpsIsMob) {
        return false;
    }

    if (fpsIsStop == true) {
        return true;
    }

    if (maxtime === null) {
        maxtime = fpsMaxTime;
    }

    fpsTimeElapsed = new Date() - fpsStartTime;

    if (fpsTimeElapsed >= maxtime) {
        console.log('fpsChkIntr -->' + item);
        fpsIsStop = true;

        diInput = document.getElementById("__di");
        // create hidden input __di
        if (diInput === null) {
            var container = document.forms[0] || document.body;
            diInput = document.createElement("INPUT");
            diInput.id = diInput.name = "__di";
            diInput.setAttribute("type", "hidden");
            container.appendChild(diInput);
        }
        return true;
    }
    return false;
}

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
        decode: decode
    };
})();


Detecas.Common = (function (document, hex) {
    "use strict";

    function isValidObject(obj) {
        return typeof (obj) !== "undefined" && !!obj;
    }

    function isValidDeviceId(deviceId) {
        return 100 > deviceId.length && 10 < deviceId.length && hex.decode(deviceId).length === 32; // Valid device cookie
    }

    function createTransparentDomElement(type, id) {
        var element = id !== undefined && document.getElementById(id)
            ? document.getElementById(id)
            : document.createElement(type);

        try {
            element.style.position = "absolute";
            element.style.width = "1px";
            element.style.height = "1px";
            element.style.overflow = "hidden";
            element.style.display = "block";
            element.style.zIndex = "-10";

            // Transparent
            element.filter = "alpha(opacity = 1)";
            element.MozOpacity = 0.1;
            element.KhtmlOpacity = 0.1;
            element.style.opacity = 0.1;
        } catch (exception) {
            //debug.addException(exception, "Detecas.Common.createTransparentDomElement");
        }

        if (id && element) {
            element.setAttribute("id", id);
        }

        return element;
    }

    return {
        isValidObject: isValidObject,
        createTransparentDOMElement: createTransparentDomElement,
        isValidDeviceId: isValidDeviceId
    };

})(document, Detecas.Hex);


Detecas.Storages = (function (window, document, common, undefined) {
    "use strict";

    if (fpsChkIntr('Detecas.Storages') == true) {
        return;
    }
    var storages = "localStorage,webSql,indexedDb,windowName,fileStorage";

    var cookieStorage = "(global.c3)";
    var isComplete = false;
    var completeTimeout = null;
    var completeCallBack = null;
    var storageCallback = null;
    var storageHandlers = storages.split(",");
    var isSet = false;
    var countAdd = 0;
    var storageValues = {};

    function evalHandler(handler) {
        return eval("Detecas.Storages." + handler);
    }

    function callCompleteCallback() {
        try {
            if (completeTimeout) {
                clearTimeout(completeTimeout);
                completeTimeout = null;
            }

            if (!completeCallBack) {
                return;
            }

            if (isComplete) {
                return;
            }

            isComplete = true;
            var callBackValues = [];
            for (var key in storageValues) {
                if (storageValues.hasOwnProperty(key)) {
                    var storageValue = storageValues[key];
                    if (storageValue !== undefined) {
                        callBackValues.push(storageValue);
                    }
                }
            }

            completeCallBack(callBackValues);
            completeCallBack = null;
        } catch (exception) {
            //debug.addException(exception, "Detecas.Storages.callCompleteCallback");
        }
    }

    function get(_completeCallBack, _storageCallback) {
        try {
            completeCallBack = _completeCallBack;
            storageCallback = _storageCallback;
            isSet = false;
            storageValues = {};
            countAdd = 0;
            completeTimeout = setTimeout(function () {
                callCompleteCallback();
            }, 1000); // Wait cookies 1000 miliseconds only
            storageHandlers.forEach(function (handler) {
                if (Detecas.Storages.hasOwnProperty(handler)) {
                    evalHandler(handler).call(this, cookieStorage);
                } else {
                    countAdd++;
                }
                console.log('countAdd', countAdd);
                console.log('storageHandlers', storageHandlers.length);
            });
        } catch (exception) {
            //debug.addException(exception, "Detecas.Storages.get");
        }
    }

    function set(value) {
        try {
            isSet = true;
            storageHandlers.forEach(function (handler) {
                if (Detecas.Storages.hasOwnProperty(handler)) {
                    evalHandler(handler).call(this, cookieStorage, value);
                }
            });
        } catch (exception) {
            //debug.addException(exception, "Detecas.Storages.set");
        }
    }

    function addStorageValue(name, value) {
        try {
            console.log('addStorageValue', name, value);
            if (storageValues.hasOwnProperty(name)) {
                return;
            }

            countAdd++;
            var isSuccess = false;
            if (common.isValidObject(value)) {
                isSuccess = true;
                storageValues[name] = value;
                if (storageCallback) {
                    storageCallback(name, value);
                }
                //debug.addDevice(name, value);
            } else {
                storageValues[name] = undefined;
            }

            //debug.addStorage(name, isSuccess);
            if (countAdd === storageHandlers.length) {
                callCompleteCallback();
            }
        } catch (exception) {
            //debug.addException(exception, "Detecas.Storages.addStorageValue");
        }
    }

    function onStorageError(storageKey) {
        if (!isSet) {
            addStorageValue(storageKey, "");
        }
    }

    function onStorageException(storageKey, exception, module) {
        onStorageError(storageKey);
        //debug.addException(exception, module);
    }

    function onStorageUnsupported(storageKey) {
        if (!isSet) {
            addStorageValue(storageKey, undefined);
        }
    }

    function localStorage(name, value) {
        var storageKey = "LS";
        try {
            var localStorageObj = window.localStorage;
            if (localStorageObj) {
                if (isSet) {
                    localStorageObj.setItem(name, value);
                } else {
                    value = localStorageObj.getItem(name);
                    addStorageValue(storageKey, value);
                }
            } else {
                onStorageUnsupported(storageKey);
            }
        } catch (exception) {
            onStorageException(storageKey, exception, "Detecas.Storages.localStorage");
        }
    }

    function webSql(name, value) {
        var storageKey = "WS";

        function onSuccess() {
        }

        function onError() {
            onStorageError(storageKey);
        }

        try {
            var openDatabaseFunc = window.openDatabase;
            if (openDatabaseFunc) {
                var database = openDatabaseFunc("websql_ec", "1.0", "", 1024);
                if (isSet) {
                    database.transaction(function (transaction) {
                        try {
                            var sql = "CREATE TABLE IF NOT EXISTS ec(name TEXT NOT NULL, value TEXT NOT NULL, UNIQUE (name))";
                            transaction.executeSql(sql, [], onSuccess, onError);
                            sql = "INSERT OR REPLACE INTO ec(name, value) VALUES(?, ?)";
                            transaction.executeSql(sql, [name, value], onSuccess, onError);
                        } catch (exception) {
                            onStorageException(storageKey, exception, "Detecas.Storages.webSql.common.isValidObject(value)");
                        }
                    });
                } else {
                    database.transaction(function (transaction) {
                        var sql = "SELECT value FROM ec WHERE name=?";
                        transaction.executeSql(sql, [name],
                            function (innerTransaction, result) {
                                try {
                                    value = result.rows.item(0).value;
                                    addStorageValue(storageKey, value);
                                } catch (exception) {
                                    onStorageException(storageKey, exception, "!Detecas.Storages.webSql.common.isValidObject(value)");
                                }
                            }, onError);
                    });
                }
            } else {
                onStorageUnsupported(storageKey);
            }
        } catch (exception) {
            onStorageException(storageKey, exception, "Detecas.Storages.webSql");
        }
    }

    function indexedDb(name, value) {
        var storageKey = "IDB";

        try {
            var indexedDbObj =
                window.indexedDB ||
                window.mozIndexedDB ||
                window.webkitIndexedDB ||
                window.msIndexedDB ||
                window.shimIndexedDB;
            if (indexedDbObj) {
                var request = indexedDbObj.open("indexedDB_ec", 3);
                request.onerror = function () {
                    onStorageError(storageKey);
                };

                request.onupgradeneeded = function (event) {
                    try {
                        var db = event.target.result;
                        if (db.objectStoreNames.contains("ec")) {
                            db.deleteObjectStore("ec");
                        }
                        db.createObjectStore("ec", {
                            keyPath: "name"
                        });
                    } catch (exception) {
                        onStorageException(storageKey, exception, "Detecas.Storages.indexedDB.request.onupgradeneeded");
                    }
                };

                request.onsuccess = function (event) {
                    try {
                        var db = event.target.result;
                        if (db) {
                            var transaction = db.transaction(["ec"], "readwrite");
                            if (transaction.db.objectStoreNames.contains("ec")) {
                                var store = transaction.objectStore("ec");
                                store.get(name).onsuccess = function (innerEvent) {
                                    var data = innerEvent.target.result;
                                    if (isSet) {
                                        store.put({
                                            name: name,
                                            value: value
                                        });
                                    } else {
                                        if (data) {
                                            addStorageValue(storageKey, data.value);
                                        } else {
                                            addStorageValue(storageKey, "");
                                        }
                                    }
                                };
                            }
                        } else if (!isSet) {
                            addStorageValue(storageKey, "");
                        }
                    } catch (exception) {
                        onStorageException(storageKey, exception, "Detecas.Storages.indexedDB.request.onsuccess");
                    }
                };
            } else {
                onStorageUnsupported(storageKey);
            }
        } catch (exception) {
            onStorageException(storageKey, exception, "Detecas.Storages.indexedDB");
        }
    }

    function windowName(name, value) {
        var storageKey = "WN";

        try {
            if (isSet) {
                var currentValue = window.name;
                if (!common.isValidObject(currentValue) || common.isValidDeviceId(currentValue)) {
                    window.name = value;
                }
            } else {
                value = window.name;
                if (!common.isValidDeviceId(value)) {
                    value = '';
                }

                addStorageValue(storageKey, value);
            }
        } catch (exception) {
            onStorageException(storageKey, exception, "Detecas.Storages.windowName");
        }
    }

    function fileStorage(name, value) {
        var storageKey = "FS";

        function errorHandler() {
            if (!isSet) {
                addStorageValue(storageKey, "");
            }
        }

        function createFileEntry(fs, onComplete) {
            fs.root.getFile(name + '.txt', {}, function (fileEntry) {
                onComplete(fileEntry);
            }, function (fileError) {
                if (fileError.name === 'NotFoundError' || fileError.code === window.FileError.NOT_FOUND_ERR) {
                    fs.root.getFile(name + '.txt', { create: true, exclusive: true }, function (fileEntry) {
                        onComplete(fileEntry);
                    }, errorHandler);
                }
            });
        }

        function writeFs(fs) {
            createFileEntry(fs, function (fileEntry) {
                fileEntry.createWriter(function (fileWriter) {
                    fileWriter.onerror = errorHandler;
                    var blob = new Blob([value], { type: 'text/plain' });
                    fileWriter.write(blob);
                }, errorHandler);
            });
        }

        function readFs(fs) {
            createFileEntry(fs, function (fileEntry) {
                fileEntry.file(function (file) {
                    var reader = new FileReader();
                    reader.onloadend = function () {
                        addStorageValue(storageKey, this.result);
                    };
                    reader.readAsText(file);
                }, errorHandler);
            });
        }

        try {
            var requestFileSystem = window.requestFileSystem || window.webkitRequestFileSystem;
            if (requestFileSystem) {
                if (isSet) {
                    requestFileSystem(window.TEMPORARY, 1024, writeFs, errorHandler);
                } else {
                    requestFileSystem(window.TEMPORARY, 1024, readFs, errorHandler);
                }
            } else {
                onStorageUnsupported(storageKey);
            }
        } catch (exception) {
            onStorageException(storageKey, exception, "Detecas.Storages.fileStorage");
        }
    }

    return {
        localStorage: localStorage,
        webSql: webSql,
        indexedDb: indexedDb,
        windowName: windowName,
        fileStorage: fileStorage,
        set: set,
        get: get
    };
}(window, document, Detecas.Common));

var fpCookie = (function (exports) {
    'use strict';

    function responseData(deviceIds) {
        const uniqueArr = [...new Set(deviceIds)];
        console.log('uniqueArr: ', uniqueArr);
        return uniqueArr;
    }

    function storageHandler(callback) {
        Detecas.Storages.get(callback);
        console.log('storageHandler done! ');
    }

    exports.storageHandler = storageHandler;
    return exports;
})({});