var fpHeadless = (function (exports) {
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

    var allTestFunctions = [
        { name: "UserAgent", testFunction: testUserAgent, active: 1 },
        { name: "ChromeWindow", testFunction: testChromeWindow, active: 1 },
        { name: "Plugins", testFunction: testPlugins, active: 0 },
        { name: "AppVersion", testFunction: testAppVersion, active: 1 },
        { name: "ConnectionRtt", testFunction: testConnectionRtt, active: 1 },
        { name: "Webdriver", testFunction: testWebdriver, active: 1 },
    ];
    //* All Tests *//
    function testWebdriver() {
        var webdriver = navigator.webdriver;
        return webdriver ? 1 : 0;
    }
    // User Agent
    function testUserAgent() {
        if (/Headless/.test(navigator.userAgent)) {
            // Headless
            return 1;
        }
        else {
            // Not Headless
            return 0;
        }
    }
    // Window.Chrome
    function testChromeWindow() {
        if (eval.toString().length == 33 && !window.chrome) {
            // Headless
            return 1;
        }
        else {
            // Not Headless
            return 0;
        }
    }
    function macOS() {
        var macosPlatforms = ["Macintosh", "MacIntel", "MacPPC", "Mac68K"];
        var platform = navigator.platform;
        return macosPlatforms.indexOf(platform) !== -1;
    }
    function isIOS() {
        var arrDevice = [
            "iPad Simulator",
            "iPhone Simulator",
            "iPod Simulator",
            "iPad",
            "iPhone",
            "iPod",
        ];
        var platform = navigator.platform;
        return ((!!platform && !!arrDevice.find(function (v) { return platform.includes(v); })) ||
            // iPad on iOS 13 detection
            (navigator.userAgent.includes("Mac") && "ontouchend" in document));
    }
    // Notification Permissions
    function testNotificationPermissions() {
        return __awaiter(this, void 0, void 0, function () {
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        if (isIOS() || macOS())
                            return [2 /*return*/, 0];
                        if (!(navigator.permissions &&
                            typeof navigator.permissions.query === "function")) return [3 /*break*/, 2];
                        return [4 /*yield*/, navigator.permissions
                                .query({ name: "notifications" })
                                .then(function (permissionStatus) {
                                if (typeof Notification != "undefined" &&
                                    Notification.permission === "denied" &&
                                    permissionStatus.state === "prompt") {
                                    // Headless
                                    return 0.5;
                                }
                                else {
                                    // Not Headless
                                    return 0;
                                }
                            })];
                    case 1: return [2 /*return*/, _a.sent()];
                    case 2: return [2 /*return*/, 0];
                }
            });
        });
    }
    // No Plugins
    function testPlugins() {
        var length = navigator.plugins ? navigator.plugins.length : 0;
        return length === 0 ? 1 : 0;
    }
    // App Version
    function testAppVersion() {
        var appVersion = navigator.appVersion;
        return /headless/i.test(appVersion) ? 1 : 0;
    }
    // Connection Rtt
    function testConnectionRtt() {
        var connection = navigator.connection;
        var connectionRtt = connection ? connection.rtt : undefined;
        if (connectionRtt === undefined) {
            return 0; // Flag doesn't even exists so just return NOT HEADLESS
        }
        else {
            return connectionRtt === 0 ? 0.5 : 0;
        }
    }
    //* Main Functions *//
    function getHeadlessScore() {
        return __awaiter(this, void 0, void 0, function () {
            var score, testsRun, testRunDetail;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        score = 0;
                        testsRun = 0;
                        testRunDetail = "";
                        return [4 /*yield*/, testNotificationPermissions()
                                .then(function (x) {
                                score = x;
                                testsRun++;
                                testRunDetail = "NotificationPermissions : " + x + ", ";
                            })
                                .then(function () {
                                allTestFunctions.forEach(function (test) {
                                    if (test.active == 1) {
                                        var result = test.testFunction();
                                        score += result;
                                        testsRun++;
                                        testRunDetail += test.name + " : " + result + ", ";
                                    }
                                });
                                return {
                                    score: score,
                                    testsRun: testsRun,
                                    testsDetail: testRunDetail.substring(0, testRunDetail.length - 2),
                                };
                            })];
                    case 1: return [2 /*return*/, _a.sent()];
                }
            });
        });
    }

    exports.getHeadlessScore = getHeadlessScore;

    Object.defineProperty(exports, '__esModule', { value: true });

    return exports;

})({});
