'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

var CryptoJS = require('crypto-js');
var FingerprintJS = require('@fingerprintjs/fingerprintjs');
var uaParserJs = require('ua-parser-js');

function _interopDefaultLegacy (e) { return e && typeof e === 'object' && 'default' in e ? e : { 'default': e }; }

var CryptoJS__default = /*#__PURE__*/_interopDefaultLegacy(CryptoJS);
var FingerprintJS__default = /*#__PURE__*/_interopDefaultLegacy(FingerprintJS);

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
    var dataString = result.ua || navigator.vendor;
    const isMobile = /(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows (ce|phone)|xda|xiino/i.test(dataString) ||
        /1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-/i.test(dataString.substr(0, 4));
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

var _isDebug = false;
var _hiddenName = "__sk";
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
                    processData(result, false, sessionKeyValue);
                }
            }
            else {
                processData(result, true, null);
            }
        }
    })
        .then(() => {
        let end = Date.now() - start;
        console.log(`milliseconds elapsed of collect fp = ${end}ms`);
    });
}
function processData(result, isInject, sessionKey) {
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
