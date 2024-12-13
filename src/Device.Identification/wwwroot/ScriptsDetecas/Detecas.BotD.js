Detecas.BotD = 
(function () {
    'use strict';

    /******************************************************************************
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
    /* global Reflect, Promise */

    var extendStatics = function(d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (Object.prototype.hasOwnProperty.call(b, p)) d[p] = b[p]; };
        return extendStatics(d, b);
    };

    function __extends(d, b) {
        if (typeof b !== "function" && b !== null)
            throw new TypeError("Class extends value " + String(b) + " is not a constructor or null");
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
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

    function __generator(thisArg, body) {
        var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
        return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
        function verb(n) { return function (v) { return step([n, v]); }; }
        function step(op) {
            if (f) throw new TypeError("Generator is already executing.");
            while (g && (g = 0, op[0] && (_ = 0)), _) try {
                if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
                if (y = 0, t) op = [op[0] & 2, t.value];
                switch (op[0]) {
                    case 0: case 1: t = op; break;
                    case 4: _.label++; return { value: op[1], done: false };
                    case 5: _.label++; y = op[1]; op = [0]; continue;
                    case 7: op = _.ops.pop(); _.trys.pop(); continue;
                    default:
                        if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                        if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                        if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                        if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                        if (t[2]) _.ops.pop();
                        _.trys.pop(); continue;
                }
                op = body.call(thisArg, _);
            } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
            if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
        }
    }

    function __spreadArray(to, from, pack) {
        if (pack || arguments.length === 2) for (var i = 0, l = from.length, ar; i < l; i++) {
            if (ar || !(i in from)) {
                if (!ar) ar = Array.prototype.slice.call(from, 0, i);
                ar[i] = from[i];
            }
        }
        return to.concat(ar || Array.prototype.slice.call(from));
    }

    /**
     * Fingerprint BotD v1.1.10 - Copyright (c) FingerprintJS, Inc, 2023 (https://fingerprint.com)
     * Licensed under the MIT (http://www.opensource.org/licenses/mit-license.php) license.
     */

    var version = "1.1.10";

    /**
     * Enum for types of bots.
     * Specific types of bots come first, followed by automation technologies.
     *
     * @readonly
     * @enum {string}
     */
    var BotKind = {
        // Object is used instead of Typescript enum to avoid emitting IIFE which might be affected by further tree-shaking.
        // See example of compiled enums https://stackoverflow.com/q/47363996)
        Awesomium: 'awesomium',
        Cef: 'cef',
        CefSharp: 'cefsharp',
        CoachJS: 'coachjs',
        Electron: 'electron',
        FMiner: 'fminer',
        Geb: 'geb',
        NightmareJS: 'nightmarejs',
        Phantomas: 'phantomas',
        PhantomJS: 'phantomjs',
        Rhino: 'rhino',
        Selenium: 'selenium',
        Sequentum: 'sequentum',
        SlimerJS: 'slimerjs',
        WebDriverIO: 'webdriverio',
        WebDriver: 'webdriver',
        HeadlessChrome: 'headless_chrome',
        Unknown: 'unknown',
    };
    /**
     * Bot detection error.
     */
    var BotdError = /** @class */ (function (_super) {
        __extends(BotdError, _super);
        /**
         * Creates a new BotdError.
         *
         * @class
         */
        function BotdError(state, message) {
            var _this = _super.call(this, message) || this;
            _this.state = state;
            _this.name = 'BotdError';
            Object.setPrototypeOf(_this, BotdError.prototype);
            return _this;
        }
        return BotdError;
    }(Error));

    function detectAppVersion(_a) {
        var appVersion = _a.appVersion;
        if (appVersion.state !== 0 /* State.Success */)
            return false;
        if (/headless/i.test(appVersion.value))
            return BotKind.HeadlessChrome;
        if (/electron/i.test(appVersion.value))
            return BotKind.Electron;
        if (/slimerjs/i.test(appVersion.value))
            return BotKind.SlimerJS;
    }

    function arrayIncludes(arr, value) {
        return arr.indexOf(value) !== -1;
    }
    function strIncludes(str, value) {
        return str.indexOf(value) !== -1;
    }
    function arrayFind(array, callback) {
        if ('find' in array)
            return array.find(callback);
        for (var i = 0; i < array.length; i++) {
            if (callback(array[i], i, array))
                return array[i];
        }
        return undefined;
    }

    function getObjectProps(obj) {
        return Object.getOwnPropertyNames(obj);
    }
    function includes(arr) {
        var keys = [];
        for (var _i = 1; _i < arguments.length; _i++) {
            keys[_i - 1] = arguments[_i];
        }
        var _loop_1 = function (key) {
            if (typeof key === 'string') {
                if (arrayIncludes(arr, key))
                    return { value: true };
            }
            else {
                var match = arrayFind(arr, function (value) { return key.test(value); });
                if (match != null)
                    return { value: true };
            }
        };
        for (var _a = 0, keys_1 = keys; _a < keys_1.length; _a++) {
            var key = keys_1[_a];
            var state_1 = _loop_1(key);
            if (typeof state_1 === "object")
                return state_1.value;
        }
        return false;
    }
    function countTruthy(values) {
        return values.reduce(function (sum, value) { return sum + (value ? 1 : 0); }, 0);
    }

    function detectDocumentAttributes(_a) {
        var documentElementKeys = _a.documentElementKeys;
        if (documentElementKeys.state !== 0 /* State.Success */)
            return false;
        if (includes(documentElementKeys.value, 'selenium', 'webdriver', 'driver')) {
            return BotKind.Selenium;
        }
    }

    function detectErrorTrace(_a) {
        var errorTrace = _a.errorTrace;
        if (errorTrace.state !== 0 /* State.Success */)
            return false;
        if (/PhantomJS/i.test(errorTrace.value))
            return BotKind.PhantomJS;
    }

    function getBrowserEngineKind() {
        var _a, _b;
        // Based on research in October 2020. Tested to detect Chromium 42-86.
        var w = window;
        var n = navigator;
        if (countTruthy([
            'webkitPersistentStorage' in n,
            'webkitTemporaryStorage' in n,
            n.vendor.indexOf('Google') === 0,
            'webkitResolveLocalFileSystemURL' in w,
            'BatteryManager' in w,
            'webkitMediaStream' in w,
            'webkitSpeechGrammar' in w,
        ]) >= 4) {
            return "chromium" /* BrowserEngineKind.Chromium */;
        }
        if (countTruthy([
            'ApplePayError' in w,
            'CSSPrimitiveValue' in w,
            'Counter' in w,
            n.vendor.indexOf('Apple') === 0,
            'getStorageUpdates' in n,
            'WebKitMediaKeys' in w,
        ]) >= 4) {
            return "webkit" /* BrowserEngineKind.Webkit */;
        }
        if (countTruthy([
            'buildID' in navigator,
            'MozAppearance' in ((_b = (_a = document.documentElement) === null || _a === void 0 ? void 0 : _a.style) !== null && _b !== void 0 ? _b : {}),
            'onmozfullscreenchange' in w,
            'mozInnerScreenX' in w,
            'CSSMozDocumentRule' in w,
            'CanvasCaptureMediaStream' in w,
        ]) >= 4) {
            return "gecko" /* BrowserEngineKind.Gecko */;
        }
        return "unknown" /* BrowserEngineKind.Unknown */;
    }
    function getBrowserKind() {
        var _a;
        var userAgent = (_a = navigator.userAgent) === null || _a === void 0 ? void 0 : _a.toLowerCase();
        if (strIncludes(userAgent, 'wechat')) {
            return "wechat" /* BrowserKind.WeChat */;
        }
        else if (strIncludes(userAgent, 'firefox')) {
            return "firefox" /* BrowserKind.Firefox */;
        }
        else if (strIncludes(userAgent, 'opera') || strIncludes(userAgent, 'opr')) {
            return "opera" /* BrowserKind.Opera */;
        }
        else if (strIncludes(userAgent, 'chrome')) {
            return "chrome" /* BrowserKind.Chrome */;
        }
        else if (strIncludes(userAgent, 'safari')) {
            return "safari" /* BrowserKind.Safari */;
        }
        else if (strIncludes(userAgent, 'trident') || strIncludes(userAgent, 'msie')) {
            return "internet_explorer" /* BrowserKind.IE */;
        }
        else {
            return "unknown" /* BrowserKind.Unknown */;
        }
    }
    // Source: https://github.com/fingerprintjs/fingerprintjs/blob/master/src/utils/browser.ts#L223
    function isAndroid() {
        var browserEngineKind = getBrowserEngineKind();
        var isItChromium = browserEngineKind === "chromium" /* BrowserEngineKind.Chromium */;
        var isItGecko = browserEngineKind === "gecko" /* BrowserEngineKind.Gecko */;
        // Only 2 browser engines are presented on Android.
        // Actually, there is also Android 4.1 browser, but it's not worth detecting it at the moment.
        if (!isItChromium && !isItGecko)
            return false;
        var w = window;
        // Chrome removes all words "Android" from `navigator` when desktop version is requested
        // Firefox keeps "Android" in `navigator.appVersion` when desktop version is requested
        return (countTruthy([
            'onorientationchange' in w,
            'orientation' in w,
            isItChromium && !('SharedWorker' in w),
            isItGecko && /android/i.test(navigator.appVersion),
        ]) >= 2);
    }
    // Source: https://github.com/fingerprintjs/fingerprintjs/blob/43a84516755440a89f3093a3b27a021d3d7351b6/src/utils/browser.ts#L100C1-L113C2
    function isDesktopSafari() {
        var w = window;
        return (countTruthy([
            'safari' in w,
            !('ongestureend' in w),
            !('TouchEvent' in w),
            !('orientation' in w),
            'HTMLElement' in w && !('autocapitalize' in HTMLElement.prototype),
            'Document' in w && 'pointerLockElement' in Document.prototype,
        ]) >= 4);
    }
    function getDocumentFocus() {
        if (document.hasFocus === undefined) {
            return false;
        }
        return document.hasFocus();
    }
    function isChromium86OrNewer() {
        // Checked in Chrome 85 vs Chrome 86 both on desktop and Android
        var w = window;
        return (countTruthy([
            !('MediaSettingsRange' in w),
            'RTCEncodedAudioFrame' in w,
            '' + w.Intl === '[object Intl]',
            '' + w.Reflect === '[object Reflect]',
        ]) >= 3);
    }

    function detectEvalLengthInconsistency(_a) {
        var evalLength = _a.evalLength;
        if (evalLength.state !== 0 /* State.Success */)
            return;
        var length = evalLength.value;
        var browser = getBrowserKind();
        var browserEngine = getBrowserEngineKind();
        if (browserEngine == "unknown" /* BrowserEngineKind.Unknown */) {
            return false;
        }
        return ((length === 37 && !arrayIncludes(["webkit" /* BrowserEngineKind.Webkit */, "gecko" /* BrowserEngineKind.Gecko */], browserEngine)) ||
            (length === 39 && !arrayIncludes(["internet_explorer" /* BrowserKind.IE */], browser)) ||
            (length === 33 && !arrayIncludes(["chromium" /* BrowserEngineKind.Chromium */], browserEngine)));
    }

    function detectFunctionBind(_a) {
        var functionBind = _a.functionBind;
        if (functionBind.state === -2 /* State.NotFunction */)
            return BotKind.PhantomJS;
    }

    function detectLanguagesLengthInconsistency(_a) {
        var languages = _a.languages;
        if (languages.state === 0 /* State.Success */ && languages.value.length === 0) {
            return BotKind.HeadlessChrome;
        }
    }

    function detectMimeTypesConsistent(_a) {
        var mimeTypesConsistent = _a.mimeTypesConsistent;
        if (mimeTypesConsistent.state === 0 /* State.Success */ && !mimeTypesConsistent.value) {
            return BotKind.Unknown;
        }
    }

    function detectPluginsArray(_a) {
        var pluginsArray = _a.pluginsArray;
        if (pluginsArray.state === 0 /* State.Success */ && !pluginsArray.value)
            return BotKind.HeadlessChrome;
    }

    function detectPluginsLengthInconsistency(_a) {
        var pluginsLength = _a.pluginsLength;
        if (pluginsLength.state !== 0 /* State.Success */)
            return;
        var browserKind = getBrowserKind();
        var browserEngineKind = getBrowserEngineKind();
        if (browserKind !== "chrome" /* BrowserKind.Chrome */ || isAndroid() || browserEngineKind !== "chromium" /* BrowserEngineKind.Chromium */)
            return;
        if (pluginsLength.value === 0)
            return BotKind.HeadlessChrome;
    }

    function detectProcess(_a) {
        var _b;
        var process = _a.process;
        if (process.state !== 0 /* State.Success */)
            return false;
        if (process.value.type === 'renderer' || ((_b = process.value.versions) === null || _b === void 0 ? void 0 : _b.electron) != null)
            return BotKind.Electron;
    }

    function detectProductSub(_a) {
        var productSub = _a.productSub;
        if (productSub.state !== 0 /* State.Success */)
            return false;
        var browserKind = getBrowserKind();
        if ((browserKind === "chrome" /* BrowserKind.Chrome */ ||
            browserKind === "safari" /* BrowserKind.Safari */ ||
            browserKind === "opera" /* BrowserKind.Opera */ ||
            browserKind === "wechat" /* BrowserKind.WeChat */) &&
            productSub.value !== '20030107') {
            return BotKind.Unknown;
        }
        else if (browserKind == "firefox" /* BrowserKind.Firefox */ && productSub.value === '20030107') {
            return BotKind.Unknown;
        }
    }

    function detectUserAgent(_a) {
        var userAgent = _a.userAgent;
        if (userAgent.state !== 0 /* State.Success */)
            return false;
        if (/PhantomJS/i.test(userAgent.value))
            return BotKind.PhantomJS;
        if (/Headless/i.test(userAgent.value))
            return BotKind.HeadlessChrome;
        if (/Electron/i.test(userAgent.value))
            return BotKind.Electron;
        if (/slimerjs/i.test(userAgent.value))
            return BotKind.SlimerJS;
    }

    function detectWebDriver(_a) {
        var webDriver = _a.webDriver;
        if (webDriver.state === 0 /* State.Success */ && webDriver.value)
            return BotKind.HeadlessChrome;
    }

    function detectWebGL(_a) {
        var webGL = _a.webGL;
        if (webGL.state === 0 /* State.Success */) {
            var _b = webGL.value, vendor = _b.vendor, renderer = _b.renderer;
            if (vendor == 'Brian Paul' && renderer == 'Mesa OffScreen') {
                return BotKind.HeadlessChrome;
            }
        }
    }

    function detectWindowExternal(_a) {
        var windowExternal = _a.windowExternal;
        if (windowExternal.state !== 0 /* State.Success */)
            return false;
        if (/Sequentum/i.test(windowExternal.value))
            return BotKind.Sequentum;
    }

    function detectWindowSize(_a) {
        var _b;
        var windowSize = _a.windowSize;
        if (windowSize.state !== 0 /* State.Success */)
            return false;
        var _c = windowSize.value, outerWidth = _c.outerWidth, outerHeight = _c.outerHeight;
        // When a page is opened in a new tab without focusing it right away, the window outer size is 0x0
        if (!getDocumentFocus())
            return;
        if (getBrowserKind() == "safari" /* BrowserKind.Safari */) {
            var userAgent_1 = (_b = navigator.userAgent) === null || _b === void 0 ? void 0 : _b.toLowerCase();
            var listExcluded = ['iphone; cpu iphone os 10', 'iphone; cpu iphone os 11', 'iphone; cpu iphone os 12',
                'ipad; cpu os 10', 'ipad; cpu os 11', 'ipad; cpu os 12'];
            var checkInclude = listExcluded.some(function (keyword) {
                return userAgent_1.includes(keyword);
            });
            if (checkInclude) {
                return;
            }
        }
        if (outerWidth === 0 && outerHeight === 0)
            return BotKind.HeadlessChrome;
    }

    function excludeRules(ruleName) {
        var _a;
        var userAgent = (_a = navigator.userAgent) === null || _a === void 0 ? void 0 : _a.toLowerCase();
        if (ruleName == 'detectNotificationPermissions') {
            if (strIncludes(userAgent, 'samsungbrowser') || strIncludes(userAgent, 'heytapbrowser')) {
                return true;
            }
        }
        if (ruleName == 'detectVmFromMemoryScreenDimensions') {
            var tvUserAgentRegex = /smart[- ]?tv|apple\s?tv|googletv|android tv|opera tv|dtvnetbrowser|sonycebrowser|bravia 4k|netflix|pov_tv-hdmi-200bt|dlnadoc\/|hbbtv|tv\s?safari|roku\/dvp|nsz-gs7\/gx70|crkey|aftm|aftt|gtv100|nintendobrowser|playstation/i;
            if (tvUserAgentRegex.test(userAgent)) {
                return true;
            }
        }
        return false;
    }

    function detectVmFromMemoryScreenDimensions(_a) {
        var deviceMemory = _a.deviceMemory, screenDimensions = _a.screenDimensions;
        if (deviceMemory.state !== 0 /* State.Success */ || screenDimensions.state !== 0 /* State.Success */)
            return;
        if (excludeRules('detectVmFromMemoryScreenDimensions'))
            return false;
        // If it's a desktop device with 1GB RAM and screen bigger than 1024x768, it's probably a VM
        if (deviceMemory.value <= 1 && screenDimensions.value.width >= 1024 && screenDimensions.value.height >= 768) {
            return BotKind.Unknown;
        }
        return false;
    }

    function detectDistinctiveProperties(_a) {
        var distinctiveProps = _a.distinctiveProps;
        if (distinctiveProps.state !== 0 /* State.Success */)
            return false;
        var value = distinctiveProps.value;
        var bot;
        for (bot in value)
            if (value[bot])
                return bot;
    }

    // eslint-disable-next-line @typescript-eslint/explicit-module-boundary-types
    var detectors = {
        detectAppVersion: detectAppVersion,
        detectDocumentAttributes: detectDocumentAttributes,
        detectErrorTrace: detectErrorTrace,
        detectEvalLengthInconsistency: detectEvalLengthInconsistency,
        detectFunctionBind: detectFunctionBind,
        detectLanguagesLengthInconsistency: detectLanguagesLengthInconsistency,
        detectPluginsArray: detectPluginsArray,
        detectPluginsLengthInconsistency: detectPluginsLengthInconsistency,
        detectProcess: detectProcess,
        detectUserAgent: detectUserAgent,
        detectWebDriver: detectWebDriver,
        detectWebGL: detectWebGL,
        detectWindowExternal: detectWindowExternal,
        detectWindowSize: detectWindowSize,
        detectMimeTypesConsistent: detectMimeTypesConsistent,
        detectProductSub: detectProductSub,
        detectVmFromMemoryScreenDimensions: detectVmFromMemoryScreenDimensions,
        detectDistinctiveProperties: detectDistinctiveProperties,
    };

    function getAppVersion() {
        var appVersion = navigator.appVersion;
        if (appVersion == undefined) {
            throw new BotdError(-1 /* State.Undefined */, 'navigator.appVersion is undefined');
        }
        return appVersion;
    }

    function getDocumentElementKeys() {
        if (document.documentElement === undefined) {
            throw new BotdError(-1 /* State.Undefined */, 'document.documentElement is undefined');
        }
        var documentElement = document.documentElement;
        if (typeof documentElement.getAttributeNames !== 'function') {
            throw new BotdError(-2 /* State.NotFunction */, 'document.documentElement.getAttributeNames is not a function');
        }
        return documentElement.getAttributeNames();
    }

    function getErrorTrace() {
        try {
            // eslint-disable-next-line @typescript-eslint/ban-ts-comment
            // @ts-ignore
            null[0]();
        }
        catch (error) {
            if (error instanceof Error && error['stack'] != null) {
                return error.stack.toString();
            }
        }
        throw new BotdError(-3 /* State.UnexpectedBehaviour */, 'errorTrace signal unexpected behaviour');
    }

    function getEvalLength() {
        return eval.toString().length;
    }

    function getFunctionBind() {
        if (Function.prototype.bind === undefined) {
            throw new BotdError(-2 /* State.NotFunction */, 'Function.prototype.bind is undefined');
        }
        return Function.prototype.bind.toString();
    }

    function getLanguages() {
        var n = navigator;
        var result = [];
        var language = n.language || n.userLanguage || n.browserLanguage || n.systemLanguage;
        if (language !== undefined) {
            result.push([language]);
        }
        if (Array.isArray(n.languages)) {
            var browserEngine = getBrowserEngineKind();
            // Starting from Chromium 86, there is only a single value in `navigator.language` in Incognito mode:
            // the value of `navigator.language`. Therefore, the value is ignored in this browser.
            if (!(browserEngine === "chromium" /* BrowserEngineKind.Chromium */ && isChromium86OrNewer())) {
                result.push(n.languages);
            }
        }
        else if (typeof n.languages === 'string') {
            var languages = n.languages;
            if (languages) {
                result.push(languages.split(','));
            }
        }
        return result;
    }

    function areMimeTypesConsistent() {
        if (navigator.mimeTypes === undefined) {
            throw new BotdError(-1 /* State.Undefined */, 'navigator.mimeTypes is undefined');
        }
        var mimeTypes = navigator.mimeTypes;
        var isConsistent = Object.getPrototypeOf(mimeTypes) === MimeTypeArray.prototype;
        for (var i = 0; i < mimeTypes.length; i++) {
            isConsistent && (isConsistent = Object.getPrototypeOf(mimeTypes[i]) === MimeType.prototype);
        }
        return isConsistent;
    }

    function getPluginsArray() {
        if (navigator.plugins === undefined) {
            throw new BotdError(-1 /* State.Undefined */, 'navigator.plugins is undefined');
        }
        if (window.PluginArray === undefined) {
            throw new BotdError(-1 /* State.Undefined */, 'window.PluginArray is undefined');
        }
        return navigator.plugins instanceof PluginArray;
    }

    function getPluginsLength() {
        if (navigator.plugins === undefined) {
            throw new BotdError(-1 /* State.Undefined */, 'navigator.plugins is undefined');
        }
        if (navigator.plugins.length === undefined) {
            throw new BotdError(-3 /* State.UnexpectedBehaviour */, 'navigator.plugins.length is undefined');
        }
        return navigator.plugins.length;
    }

    function getProcess() {
        if (window.process === undefined) {
            throw new BotdError(-1 /* State.Undefined */, 'window.process is undefined');
        }
        return window.process;
    }

    function getProductSub() {
        var productSub = navigator.productSub;
        if (productSub === undefined) {
            throw new BotdError(-1 /* State.Undefined */, 'navigator.productSub is undefined');
        }
        return productSub;
    }

    function getUserAgent() {
        return navigator.userAgent;
    }

    function getWebDriver() {
        if (navigator.webdriver == undefined) {
            throw new BotdError(-1 /* State.Undefined */, 'navigator.webdriver is undefined');
        }
        return navigator.webdriver;
    }

    function getWebGL() {
        var canvasElement = document.createElement('canvas');
        if (typeof canvasElement.getContext !== 'function') {
            throw new BotdError(-2 /* State.NotFunction */, 'HTMLCanvasElement.getContext is not a function');
        }
        var webGLContext = canvasElement.getContext('webgl');
        if (webGLContext === null) {
            throw new BotdError(-4 /* State.Null */, 'WebGLRenderingContext is null');
        }
        if (typeof webGLContext.getParameter !== 'function') {
            throw new BotdError(-2 /* State.NotFunction */, 'WebGLRenderingContext.getParameter is not a function');
        }
        var vendor = webGLContext.getParameter(webGLContext.VENDOR);
        var renderer = webGLContext.getParameter(webGLContext.RENDERER);
        return { vendor: vendor, renderer: renderer };
    }

    function getWindowExternal() {
        if (window.external === undefined) {
            throw new BotdError(-1 /* State.Undefined */, 'window.external is undefined');
        }
        var external = window.external;
        if (typeof external.toString !== 'function') {
            throw new BotdError(-2 /* State.NotFunction */, 'window.external.toString is not a function');
        }
        return external.toString();
    }

    function getWindowSize() {
        return {
            outerWidth: window.outerWidth,
            outerHeight: window.outerHeight,
            innerWidth: window.innerWidth,
            innerHeight: window.innerHeight,
        };
    }

    function getDeviceMemory() {
        if (navigator.deviceMemory === undefined) {
            throw new BotdError(-1 /* State.Undefined */, 'navigator.deviceMemory is undefined');
        }
        return navigator.deviceMemory;
    }

    function getScreenDimensions() {
        if (screen === undefined) {
            throw new BotdError(-1 /* State.Undefined */, 'screen is undefined');
        }
        return { width: screen.width, height: screen.height };
    }

    function checkDistinctiveProperties() {
        var _a;
        // The order in the following list matters, because specific types of bots come first, followed by automation technologies.
        var distinctivePropsList = (_a = {},
            _a[BotKind.Awesomium] = {
                window: ['awesomium'],
            },
            _a[BotKind.Cef] = {
                window: ['RunPerfTest'],
            },
            _a[BotKind.CefSharp] = {
                window: ['CefSharp'],
            },
            _a[BotKind.CoachJS] = {
                window: ['emit'],
            },
            _a[BotKind.FMiner] = {
                window: ['fmget_targets'],
            },
            _a[BotKind.Geb] = {
                window: ['geb'],
            },
            _a[BotKind.NightmareJS] = {
                window: ['__nightmare', 'nightmare'],
            },
            _a[BotKind.Phantomas] = {
                window: ['__phantomas'],
            },
            _a[BotKind.PhantomJS] = {
                window: ['callPhantom', '_phantom'],
            },
            _a[BotKind.Rhino] = {
                window: ['spawn'],
            },
            _a[BotKind.Selenium] = {
                window: ['_Selenium_IDE_Recorder', '_selenium', 'calledSelenium', /^([a-z]){3}_.*_(Array|Promise|Symbol)$/],
                document: ['__selenium_evaluate', 'selenium-evaluate', '__selenium_unwrapped'],
            },
            _a[BotKind.WebDriverIO] = {
                window: ['wdioElectron'],
            },
            _a[BotKind.WebDriver] = {
                window: [
                    'webdriver',
                    '__webdriverFunc',
                    '__lastWatirAlert',
                    '__lastWatirConfirm',
                    '__lastWatirPrompt',
                    '_WEBDRIVER_ELEM_CACHE',
                    'ChromeDriverw',
                ],
                document: [
                    '__webdriver_script_fn',
                    '__driver_evaluate',
                    '__webdriver_evaluate',
                    '__fxdriver_evaluate',
                    '__driver_unwrapped',
                    '__webdriver_unwrapped',
                    '__fxdriver_unwrapped',
                    '__webdriver_script_fn',
                    '__webdriver_script_func',
                    '__webdriver_script_function',
                    '$cdc_asdjflasutopfhvcZLmcf',
                    '$cdc_asdjflasutopfhvcZLmcfl_',
                    '$chrome_asyncScriptInfo',
                    '__$webdriverAsyncExecutor',
                ],
            },
            _a[BotKind.HeadlessChrome] = {
                window: ['domAutomation', 'domAutomationController'],
            },
            _a);
        var botName;
        var result = {};
        var windowProps = getObjectProps(window);
        var documentProps = [];
        if (window.document !== undefined)
            documentProps = getObjectProps(window.document);
        for (botName in distinctivePropsList) {
            var props = distinctivePropsList[botName];
            if (props !== undefined) {
                var windowContains = props.window === undefined ? false : includes.apply(void 0, __spreadArray([windowProps], props.window, false));
                var documentContains = props.document === undefined || !documentProps.length ? false : includes.apply(void 0, __spreadArray([documentProps], props.document, false));
                result[botName] = windowContains || documentContains;
            }
        }
        return result;
    }

    function getBrowserInfo() {
        return {
            browserEngineKind: getBrowserEngineKind(),
            browserKind: getBrowserKind(),
            isAndroid: isAndroid(),
            isDesktopSafari: isDesktopSafari(),
            isChromium86OrNewer: isChromium86OrNewer(),
        };
    }

    var sources = {
        userAgent: getUserAgent,
        appVersion: getAppVersion,
        windowSize: getWindowSize,
        pluginsLength: getPluginsLength,
        pluginsArray: getPluginsArray,
        errorTrace: getErrorTrace,
        productSub: getProductSub,
        windowExternal: getWindowExternal,
        mimeTypesConsistent: areMimeTypesConsistent,
        evalLength: getEvalLength,
        webGL: getWebGL,
        webDriver: getWebDriver,
        languages: getLanguages,
        documentElementKeys: getDocumentElementKeys,
        functionBind: getFunctionBind,
        process: getProcess,
        deviceMemory: getDeviceMemory,
        screenDimensions: getScreenDimensions,
        distinctiveProps: checkDistinctiveProperties,
        browserInfo: getBrowserInfo,
    };

    /**
     * Class representing a bot detector.
     *
     * @class
     * @implements {BotDetectorInterface}
     */
    var BotDetector = /** @class */ (function () {
        function BotDetector() {
            this.components = undefined;
            this.detections = undefined;
        }
        BotDetector.prototype.getComponents = function () {
            return this.components;
        };
        BotDetector.prototype.getDetections = function () {
            return this.detections;
        };
        // eslint-disable-next-line @typescript-eslint/explicit-module-boundary-types
        BotDetector.prototype.getSources = function () {
            return sources;
        };
        // eslint-disable-next-line @typescript-eslint/explicit-module-boundary-types
        BotDetector.prototype.getDetectors = function () {
            return detectors;
        };
        /**
         * @inheritdoc
         */
        BotDetector.prototype.detect = function () {
            if (this.components === undefined) {
                throw new Error("BotDetector.detect can't be called before BotDetector.collect");
            }
            var components = this.components;
            var detectors = this.getDetectors();
            var detections = {};
            var finalDetection = {
                bot: false,
            };
            for (var detectorName in detectors) {
                var detector = detectors[detectorName];
                var detectorRes = detector(components);
                var detection = { bot: false };
                if (typeof detectorRes === 'string') {
                    detection = { bot: true, botKind: detectorRes };
                }
                else if (detectorRes) {
                    detection = { bot: true, botKind: BotKind.Unknown };
                }
                detections[detectorName] = detection;
                if (detection.bot) {
                    finalDetection = detection;
                }
            }
            this.detections = detections;
            return finalDetection;
        };
        /**
         * @inheritdoc
         */
        BotDetector.prototype.collect = function () {
            return __awaiter(this, void 0, void 0, function () {
                var sources, components, sourcesKeys;
                var _this = this;
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0:
                            sources = this.getSources();
                            components = {};
                            sourcesKeys = Object.keys(sources);
                            return [4 /*yield*/, Promise.all(sourcesKeys.map(function (sourceKey) { return __awaiter(_this, void 0, void 0, function () {
                                    var res, _a, _b, error_1;
                                    var _c;
                                    return __generator(this, function (_d) {
                                        switch (_d.label) {
                                            case 0:
                                                res = sources[sourceKey];
                                                _d.label = 1;
                                            case 1:
                                                _d.trys.push([1, 3, , 4]);
                                                _a = components;
                                                _b = sourceKey;
                                                _c = {};
                                                return [4 /*yield*/, res()];
                                            case 2:
                                                _a[_b] = (_c.value = _d.sent(),
                                                    _c.state = 0 /* State.Success */,
                                                    _c);
                                                return [3 /*break*/, 4];
                                            case 3:
                                                error_1 = _d.sent();
                                                if (error_1 instanceof BotdError) {
                                                    components[sourceKey] = {
                                                        state: error_1.state,
                                                        error: "".concat(error_1.name, ": ").concat(error_1.message),
                                                    };
                                                }
                                                else {
                                                    components[sourceKey] = {
                                                        state: -3 /* State.UnexpectedBehaviour */,
                                                        error: error_1 instanceof Error ? "".concat(error_1.name, ": ").concat(error_1.message) : String(error_1),
                                                    };
                                                }
                                                return [3 /*break*/, 4];
                                            case 4: return [2 /*return*/];
                                        }
                                    });
                                }); }))];
                        case 1:
                            _a.sent();
                            this.components = components;
                            return [2 /*return*/, this.components];
                    }
                });
            });
        };
        return BotDetector;
    }());

    /**
     * Sends an unpersonalized AJAX request to collect installation statistics
     */
    function monitor() {
        // The FingerprintJS CDN (https://github.com/fingerprintjs/cdn) replaces `window.__fpjs_d_m` with `true`
        if (window.__fpjs_d_m || Math.random() >= 0.001) {
            return;
        }
        try {
            var request = new XMLHttpRequest();
            request.open('get', "https://m1.openfpcdn.io/botd/v".concat(version, "/npm-monitoring"), true);
            request.send();
        }
        catch (error) {
            // console.error is ok here because it's an unexpected error handler
            // eslint-disable-next-line no-console
            console.error(error);
        }
    }
    function load(_a) {
        var _b = _a === void 0 ? {} : _a, _c = _b.monitoring, monitoring = _c === void 0 ? true : _c;
        return __awaiter(this, void 0, void 0, function () {
            var detector;
            return __generator(this, function (_d) {
                switch (_d.label) {
                    case 0:
                        if (monitoring) {
                            monitor();
                        }
                        detector = new BotDetector();
                        return [4 /*yield*/, detector.collect()];
                    case 1:
                        _d.sent();
                        return [2 /*return*/, detector];
                }
            });
        });
    }

    function getResult() {
        try {
            var botdPromise = load({
                monitoring: false
            });
            return botdPromise
                .then(function (botd) {
                var resultDetect = botd.detect();
                var detections = botd.getDetections();
                var botDetections = Object.keys(detections).filter(function (key) { return detections[key].bot; }).join(',');
                var botComponents = '';
                if (resultDetect && resultDetect.bot) {
                    botComponents = JSON.stringify(botd.getComponents());
                }
                return {
                    result: resultDetect,
                    detection: botDetections,
                    component: botComponents,
                    isVer3: true
                };
            }).catch(function (error) {
                return {
                    error: error.message
                };
            });
        }
        catch (error) {
            return Promise.resolve({ error: error.message });
        }
    }

    return getResult;

})();
