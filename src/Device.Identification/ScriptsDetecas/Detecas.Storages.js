/* global Detecas */

Detecas.Storages = (function (window, document, navigator, config, common, debug, undefined) {
    "use strict";

    if (fpsChkIntr('Detecas.Storages') == true) {
        return;
    }

    var cookieStorage = config.cookieStorage;
    var isComplete = false;
    var completeTimeout = null;
    var completeCallBack = null;
    var storageCallback = null;
    var storageHandlers = config.storages.split(",");
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

    function userData(name, value) {
        var storageKey = "UD";

        try {
            var div = common.createTransparentDOMElement("div", "_ud");
            document.body.appendChild(div);

            if (div.addBehavior) {
                div.style.behavior = "url(#default#userData)";
                name = name.replace(/[^a-z]+/g, '');

                if (isSet) {
                    div.setAttribute(name, value);
                    div.save(name);
                } else {
                    div.load(name);
                    addStorageValue(storageKey, div.getAttribute(name));
                }
            } else {
                onStorageUnsupported(storageKey);
            }
        } catch (exception) {
            onStorageError(storageKey);
        }
    }

    return {
        localStorage: localStorage,
        webSql: webSql,
        indexedDb: indexedDb,
        windowName: windowName,
        fileStorage: fileStorage,
        userData: userData,
        set: set,
        get: get
    };
} (window, document, navigator, Detecas.Config, Detecas.Common, Detecas.Debug));