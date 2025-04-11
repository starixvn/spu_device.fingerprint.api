/* global Detecas */

Detecas.Core = (function (window, document, common, cookie, rc4, hex, base64, json, config, rtc, device, deviceDetail, storages, debug, botD) {
    "use strict";

    if (!fpsIsMob) {
        rtc.init();
    }
    
    let deviceInformation = {};
    let isScriptDeviceIdFromCache = false;
    let isGetDeviceIdFromStorage = false;
    let currentDeviceId = config.defaultDeviceId;
    if (config.cachedDeviceId) {
        currentDeviceId = config.cachedDeviceId;
        isScriptDeviceIdFromCache = true;
    }

    function packDeviceInfo() {
        deviceInformation.userIP.localIps = !fpsIsMob && common.isValidObject(rtc.localIps) ? rtc.localIps : '[]';
        let packedDeviceInfo = json.stringify({
            device: deviceInformation.fingerprint,
            ip: config.ip,
            localIps: deviceInformation.userIP.localIps,
            domainName: window.location.host
        });

        packedDeviceInfo = base64.encode(rc4.encrypt(config.ip, packedDeviceInfo));
        return packedDeviceInfo;
    }

    function checkingStoragesOnServerSide() {
        let version = encodeURIComponent(base64.encode(config.version));
        let packedDeviceInfo = encodeURIComponent(packDeviceInfo());
        let encodedIp = encodeURIComponent(base64.encode(rc4.encrypt("PI", config.ip)));
        let encodedVersion = encodeURIComponent(base64.encode(config.version));
        let params = ["deviceInfo=" + packedDeviceInfo, "ip=" + encodedIp, "version=" + encodedVersion].join("&");

        let script = document.createElement("SCRIPT");
        script.type = "text/javascript";
        script.id = "evercookie" + config.ip; // Mark version for removal
        script.src = config.host + "/ec?v=" + version + "&" + params;
        script.async = true;
       // script.setAttribute("crossorigin", "anonymous");
        document.getElementsByTagName("head")[0].appendChild(script);
    }

    function cleanup() {
        try {
            common.removeDOMElement(
                "evercookie" + config.ip,
                "iframe" + config.ip,
                "form" + config.ip,
                "__canvasContainer"
            );
        } catch (exception) {
        }
    }

    function synchronizeStorages(deviceId) {
        storages.set(deviceId);
    }

    function getDeviceIdFromStorage(storageName, storageValue) {
        try {
            if (storageValue !== currentDeviceId) {
                if (isGetDeviceIdFromStorage) {
                    return;
                }

                isGetDeviceIdFromStorage = true;
                currentDeviceId = storageValue;
                injectDeviceInfoToClient();
            }
        } catch (exception) {
            debug.addException(exception, "Detecas.Core.getDeviceIdFromStorage");
        }
    }

    function getDeviceIdsFromStorages(deviceIds) {
        try {
            let deviceIdsFromStorages = [currentDeviceId];
            deviceIds.forEach(function (deviceId) {
                if (common.isValidDeviceId(deviceId) && deviceIdsFromStorages.indexOf(deviceId) === -1) {
                    deviceIdsFromStorages.push(deviceId);
                }
            });

            deviceInformation.fingerprint.ClientDeviceIds = deviceIdsFromStorages.join();
            checkingStoragesOnServerSide();
        } catch (exception) {
        }
    }

    function generateUUID() {
        var uuid;

        if (typeof window !== 'undefined' && window !== null && window.crypto) {
            uuid = function () {
                return ('10000000100040008000100000000000').replace(/[018]/g, function (c) {
                    return (c ^ (window.crypto.getRandomValues(new Uint8Array(1))[0] & (15 >> (c / 4)))).toString(16);
                });
            };
        } else {
            uuid = function () {
                var randomHex = function () {
                    return ('0000' + Math.floor(Math.random() * 0x10000).toString(16)).slice(-4);
                };
                var generateSegment = function (length) {
                    var segment = '';
                    for (var i = 0; i < length; i++) {
                        segment += randomHex();
                    }
                    return segment;
                };
                return generateSegment(2) +
                    randomHex() +
                    '4' + randomHex().substr(1) +
                    (8 + Math.floor(Math.random() * 4)).toString(16) +
                    randomHex().substr(1) +
                    generateSegment(3);
            };
        }

        return uuid();
    }

    function start() {
        try {
            debug.setVersion(config.version);

            var jscKey = generateUUID();
            injectBotDToClient(jscKey);
            injectValueIntoHiddenField('__jsc', jscKey);

            // inject currentDeviceId immediately
            if (common.isValidDeviceId(currentDeviceId)) {
                injectDeviceInfoToClient();
            }

            deviceInformation = device.getDeviceInformation();
            injectDeviceInfoToClient();

            if (isScriptDeviceIdFromCache) {
                storages.get(getDeviceIdsFromStorages);
            } else {
                storages.get(getDeviceIdsFromStorages, getDeviceIdFromStorage);
            }
        } catch (exception) {
        }
    }

    function callback(deviceId) {
        try {
            currentDeviceId = deviceId;
            if (common.isValidDeviceId(deviceId)) {
                if (typeof window.API_CALLBACK === "function") { // api subscriber
                    var collectedDeviceInfo = common.collectDeviceInfoToInjectToClient(deviceId, deviceInformation.fingerprint);
                    window.API_CALLBACK.call(window, collectedDeviceInfo);
                } else {
                    injectDeviceInfoToClient();
                    //injectBotDToClient();
                }


                if (deviceDetail.isNewDeviceDetail(deviceInformation.deviceDetail)) {
                    deviceDetail.insertDeviceDetail(deviceInformation.deviceDetail);
                }

                synchronizeStorages(deviceId);
            }

            setTimeout(function () {
                cleanup();
            }, 1000);
        } catch (exception) {
        }
    }

    function createHiddenElement(container, diValue, id, name) {
        
        let diInput = document.createElement("INPUT");
        diInput.id = id;
        diInput.name = name;
        diInput.setAttribute("type", "hidden");
        diInput.value = diValue;

        container.appendChild(diInput);
    }

    function injectValueIntoHiddenField(fieldName, value) {
        try {

            let elements = document.getElementsByName(fieldName);
            if (elements != null && elements.length != 0) {
                for (let i = 0; i < elements.length; i++) {
                    elements[i].value = value;
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
        } catch (exception) {
            // debug.addException(exception, "injectValueIntoHiddenField");
        }
    }

    function injectBotDToClient(jscKey) {
        try {
            if (botD && typeof botD === 'function') {
                botD().then(result => {
                    if (result) {
                        result.JscKey = jscKey;
                    }

                    let biValue = base64.encode(json.stringify(result));
                    injectValueIntoHiddenField("__bi", biValue);
                }).catch(function (err) {
                    // debug.addException(exception, "injectBotDToClient");
                });
            }
        } catch (exception) {
            // debug.addException(exception, "injectBotDToClient");
        }
    }

    function injectDeviceInfoToClient() {
        try {
            let diValue = common.collectDeviceInfoToInjectToClient(currentDeviceId, deviceInformation.fingerprint);
            injectValueIntoHiddenField("__di", diValue);
        } catch (exception) {
            debug.addException(exception, "injectDevice");
        }
    }


    return {
        start: start,
        callback: callback
    };
}(window, document, Detecas.Common, Detecas.Cookie, Detecas.Rc4, Detecas.Hex, Detecas.Base64, Detecas.JSON, Detecas.Config, Detecas.RTC, Detecas.Device, Detecas.DeviceDetail, Detecas.Storages, Detecas.Debug, Detecas.BotD));