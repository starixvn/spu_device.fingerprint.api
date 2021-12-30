'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

var CryptoJS = require('crypto-js');
var FingerprintJS = require('@fingerprintjs/fingerprintjs');
var uaParserJs = require('ua-parser-js');
var localForage = require('localforage');

function _interopDefaultLegacy (e) { return e && typeof e === 'object' && 'default' in e ? e : { 'default': e }; }

var CryptoJS__default = /*#__PURE__*/_interopDefaultLegacy(CryptoJS);
var FingerprintJS__default = /*#__PURE__*/_interopDefaultLegacy(FingerprintJS);
var localForage__default = /*#__PURE__*/_interopDefaultLegacy(localForage);

/*! *****************************************************************************
Copyright (c) Microsoft Corporation.

Permission to use, copy, modify, and/or distribute this software for any
purpose with or without fee is hereby granted.

THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH
REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY
AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT,
INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM
LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR
OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR
PERFORMANCE OF THIS SOFTWARE.
***************************************************************************** */

function __rest(s, e) {
    var t = {};
    for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p) && e.indexOf(p) < 0)
        t[p] = s[p];
    if (s != null && typeof Object.getOwnPropertySymbols === "function")
        for (var i = 0, p = Object.getOwnPropertySymbols(s); i < p.length; i++) {
            if (e.indexOf(p[i]) < 0 && Object.prototype.propertyIsEnumerable.call(s, p[i]))
                t[p[i]] = s[p[i]];
        }
    return t;
}

function __awaiter(thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
}

function Encrypting(data, key) {
    var iv = CryptoJS__default["default"].enc.Hex.parse("c3eef06037e837f4ee390bda69c05023");
    var pass = CryptoJS__default["default"].enc.Utf8.parse(key);
    var salt = CryptoJS__default["default"].enc.Utf8.parse("234292a9fce031f2");
    var key128Bits1000Iterations = CryptoJS__default["default"].PBKDF2(pass.toString(CryptoJS__default["default"].enc.Utf8), salt, { keySize: 128 / 32, iterations: 1000 });
    var cfg = { mode: CryptoJS__default["default"].mode.CBC, iv: iv, padding: CryptoJS__default["default"].pad.Pkcs7 };
    var encrypted = CryptoJS__default["default"].AES.encrypt(JSON.stringify(data), key128Bits1000Iterations, cfg);
    return CryptoJS__default["default"].enc.Base64.stringify(encrypted.ciphertext);
}
function HashMD5(data) {
    return CryptoJS__default["default"].MD5(data).toString();
}

function getDevicePixelRatio() {
    return window.devicePixelRatio;
}
function getCurrentResolution() {
    let ratio = getDevicePixelRatio();
    return screen.width * ratio + "x" + screen.height * ratio;
}
function getPixelDepth() {
    return screen.pixelDepth;
}
function getScreenPrint() {
    return {
        currentResolution: getCurrentResolution(),
        pixelDepth: getPixelDepth(),
    };
}
function getCanvas() {
    let canvas = FingerprintJS.sources.canvas();
    return {
        winding: canvas.winding,
        geometry: canvas.geometry,
        text: "",
    };
}
function componentsToCanonicalString(components) {
    let result = "";
    for (const componentKey of Object.keys(components).sort()) {
        const component = components[componentKey];
        const value = component.error ? "error" : JSON.stringify(component.value);
        result += `${result ? "|" : ""}${componentKey.replace(/([:|\\])/g, "\\$1")}:${value}`;
    }
    return "{" + result + "}";
}
function getFontPreference() {
    return __awaiter(this, void 0, void 0, function* () {
        let font = yield FingerprintJS.sources.fontPreferences();
        return {
            default: font["default"],
            apple: font["apple"],
            serif: font["serif"],
            sans: font["sans"],
            min: font["min"],
            system: font["system"],
        };
    });
}
function getLanguages() {
    return {
        language: window.navigator.language,
        languages: window.navigator.languages.join(","),
        userLanguage: window.navigator["userLanguage"],
        browserLanguage: window.navigator["browserLanguage"],
        systemLanguage: window.navigator["systemLanguage"],
    };
}
function getDI() {
    let visitorId;
    let properties;
    const fpPromise = FingerprintJS__default["default"].load({
        monitoring: false,
    });
    return fpPromise
        .then((fp) => fp.get())
        .then((result) => __awaiter(this, void 0, void 0, function* () {
        const _a = result.components, components = __rest(_a, ["screenResolution", "screenFrame", "canvas", "fontPreferences", "languages"]);
        const customComponents = Object.assign(Object.assign({}, components), { screenPrint: { value: getScreenPrint(), duration: 0 }, canvasPrint: { value: getCanvas(), duration: 0 }, fontPreferencePrint: {
                value: yield getFontPreference(),
                duration: 0,
            }, languagesPrint: {
                value: getLanguages(),
                duration: 0,
            } });
        visitorId = FingerprintJS__default["default"].hashComponents(customComponents);
        properties = componentsToCanonicalString(customComponents);
        return Promise.resolve({ Id: visitorId, Properties: properties });
    }));
}

var apiUrl = "__apiUrl__";
var subscriberId = "__subscriberId__";
function set(appAPIUrl, subscriber) {
    apiUrl = appAPIUrl;
    subscriberId = subscriber.toString();
}

function getDeviceInfo() {
    const parser = new uaParserJs.UAParser();
    const result = parser.getResult();
    const isMobile = isMobileAndroid(result.ua) ||
        isMobileBlackBerry(result.ua) ||
        isMobileIOS(result.ua) ||
        isMobileOpera(result.ua) ||
        isMobileWindows(result.ua);
    const isDesktop = isWindows(result.os) ||
        isMac(result.os) ||
        isLinux(result.os) ||
        isSolaris(result.os) ||
        isUbuntu(result.os);
    return {
        ua: result.ua,
        device: result.device,
        browser: result.browser,
        engine: result.engine,
        os: result.os,
        cpu: result.cpu,
        isMobile: isMobile,
        isDesktop: isDesktop,
    };
}
function isWindows(os) {
    return /Windows/i.test(os.name);
}
function isMac(os) {
    return /Mac/i.test(os.name);
}
function isLinux(os) {
    return /Linux/i.test(os.name);
}
function isSolaris(os) {
    return /Solaris/i.test(os.name);
}
function isUbuntu(os) {
    return /Ubuntu/i.test(os.name);
}
function isMobileAndroid(ua) {
    if (ua.match(/Android/i)) {
        return true;
    }
    return false;
}
function isMobileOpera(ua) {
    if (ua.match(/Opera Mini/i)) {
        return true;
    }
    return false;
}
// Is Mobile Windows.  Check if the browser is on a windows mobile device.
function isMobileWindows(ua) {
    if (ua.match(/IEMobile/i)) {
        return true;
    }
    return false;
}
// Is Mobile BlackBerry.  Check if the browser is on a blackberry mobile device.
function isMobileBlackBerry(ua) {
    if (ua.match(/BlackBerry/i)) {
        return true;
    }
    return false;
}
// Is Mobile iOS.  Check if the browser is on an Apple iOS device.
function isMobileIOS(ua) {
    if (ua.match(/iPhone|iPad|iPod/i)) {
        return true;
    }
    return false;
}

function initStorage() {
    let localForageDrives = [];
    if (isIndexDB()) {
        localForageDrives.push(localForage__default["default"].INDEXEDDB);
    }
    if (isWebSQL()) {
        localForageDrives.push(localForage__default["default"].WEBSQL);
    }
    if (isLocalStorage()) {
        localForageDrives.push(localForage__default["default"].LOCALSTORAGE);
    }
    return Promise.resolve(localForageDrives);
}
function isWebSQL() {
    const supports_webSQL = "openDatabase" in window;
    return supports_webSQL;
}
function isIndexDB() {
    if (window.indexedDB)
        return true;
    return false;
}
function isLocalStorage() {
    if (window.localStorage)
        return true;
    return false;
}
function cookieHandler(cookieValue) {
    var value = cookieValue != null ? "/" + cookieValue : "";
    return fetch(apiUrl + "/getCookie" + value, {
        method: "GET",
        redirect: "follow",
    })
        .then((res) => res.json())
        .then((res) => {
        return res;
    });
}
function setStorage(name, value) {
    initStorage().then((drivers) => {
        drivers.forEach((drive) => {
            var storageDriver = localForage__default["default"].createInstance({
                driver: drive,
                name: drive + "-EC",
                storeName: "ec",
            });
            storageDriver.setItem(name, value);
        });
    });
}
function storageHandler(name) {
    return initStorage()
        .then((driver) => {
        return driver.map((drive) => __awaiter(this, void 0, void 0, function* () {
            var storageDriver = localForage__default["default"].createInstance({
                driver: drive,
                name: drive + "-EC",
                storeName: "ec",
            });
            return yield storageDriver.getItem(name);
        }));
    })
        .then((res) => {
        return Promise.all(res).then((cookies) => {
            var latestCookie = null;
            for (const cookie of cookies) {
                if (cookie != null) {
                    latestCookie = cookie;
                    break;
                }
            }
            return cookieHandler(latestCookie).then((result) => {
                setStorage(name, result.code);
                return result.id;
            });
        });
    });
}

var _isDebug = false;
var _hiddenName = "__sk";
var _cookieName = "__fp";
function isDebug(isDebug) {
    _isDebug = isDebug;
    return this;
}
function config(apiUrl, subscriberId) {
    set(apiUrl, subscriberId);
}
const promiseAny = (iterable) => __awaiter(void 0, void 0, void 0, function* () {
    return Promise.all([...iterable].map((promise) => {
        return new Promise((resolve, reject) => Promise.resolve(promise).then(reject, resolve));
    })).then((errors) => Promise.reject(errors), (value) => Promise.resolve(value));
});
function injectSessionKey(value) {
    var container = document.forms[0] || document.body;
    var fkInput = document.createElement("input");
    fkInput.setAttribute("type", "hidden");
    fkInput.setAttribute("name", _hiddenName);
    fkInput.setAttribute("id", _hiddenName);
    fkInput.setAttribute("value", value);
    container.appendChild(fkInput);
}
function getFP() {
    let start = Date.now();
    storageHandler(_cookieName).then((cookieId) => {
        getDI()
            .then((result) => {
            if (_isDebug) {
                console.log(result.Id);
                console.log(result.Properties);
            }
            else {
                let sessionKeyElement = document.getElementById(_hiddenName);
                if (sessionKeyElement) {
                    let sessionKeyValue = sessionKeyElement.value;
                    if (sessionKeyValue != null && sessionKeyValue != "") {
                        processData(result, false, sessionKeyValue, cookieId);
                    }
                }
                else {
                    processData(result, true, null, cookieId);
                }
            }
        })
            .then(() => {
            let end = Date.now() - start;
            console.log(`milliseconds elapsed of collect fp = ${end}ms`);
        });
    });
}
function processData(result, isInject, sessionKey, cookieId) {
    getDomain()
        .then((res) => res.text())
        .then((req) => {
        var requests = JSON.parse(req).map(function (url) {
            return fetch(url + "/api/fp/hc", {
                cache: "no-cache",
            });
        });
        promiseAny(requests).then((res) => {
            if (res.status == 200) {
                var str = res.url;
                var index = str.lastIndexOf("/");
                var url = str.substring(0, index);
                getConfig(url).then((conf) => __awaiter(this, void 0, void 0, function* () {
                    if (conf.ok) {
                        const response = yield conf.json();
                        const deviceInfo = getDeviceInfo();
                        const device = JSON.stringify(deviceInfo);
                        let encryptedFP = Encrypting({
                            di: result.Id,
                            attr: result.Properties,
                            fk: isInject ? response.c : sessionKey,
                            subs: subscriberId,
                            dev: device,
                            devCode: HashMD5(device),
                            cookieId: cookieId,
                        }, response.a);
                        postData(url, encryptedFP, response.b).then((data) => {
                            if (data.ok && isInject) {
                                injectSessionKey(response.c);
                            }
                        });
                    }
                }));
            }
        });
    });
}
function postData(url, data, token) {
    return __awaiter(this, void 0, void 0, function* () {
        return fetch(url, {
            method: "POST",
            cache: "no-cache",
            credentials: "include",
            headers: {
                "Content-Type": "application/json",
                Authorization: "Bearer " + token,
            },
            redirect: "follow",
            referrerPolicy: "no-referrer",
            body: JSON.stringify(data),
        });
    });
}
function getConfig(url) {
    return fetch(url + "/conf", {
        method: "GET",
        redirect: "follow",
    });
}
function getDomain() {
    return fetch(apiUrl + "/subscriber/" + subscriberId, {
        method: "GET",
        redirect: "follow",
    });
}
getFP();

exports.config = config;
exports.isDebug = isDebug;
