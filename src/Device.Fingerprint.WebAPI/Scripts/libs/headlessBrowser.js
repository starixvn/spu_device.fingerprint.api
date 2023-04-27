var fpHeadless = (function (exports) {
    'use strict';

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

    function __awaiter(thisArg, _arguments, P, generator) {
        function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
        return new (P || (P = Promise))(function (resolve, reject) {
            function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
            function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
            function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
            step((generator = generator.apply(thisArg, _arguments || [])).next());
        });
    }

    let allTestFunctions = [
        { name: "UserAgent", testFunction: testUserAgent, active: 1 },
        { name: "ChromeWindow", testFunction: testChromeWindow, active: 1 },
        { name: "Plugins", testFunction: testPlugins, active: 0 },
        { name: "AppVersion", testFunction: testAppVersion, active: 1 },
        { name: "ConnectionRtt", testFunction: testConnectionRtt, active: 1 },
        { name: "Webdriver", testFunction: testWebdriver, active: 1 },
    ];
    //* All Tests *//
    function testWebdriver() {
        let webdriver = navigator.webdriver;
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
        const macosPlatforms = ["Macintosh", "MacIntel", "MacPPC", "Mac68K"];
        const platform = navigator.platform;
        return macosPlatforms.indexOf(platform) !== -1;
    }
    function isIOS() {
        const arrDevice = [
            "iPad Simulator",
            "iPhone Simulator",
            "iPod Simulator",
            "iPad",
            "iPhone",
            "iPod",
        ];
        const platform = navigator.platform;
        return ((!!platform && !!arrDevice.find((v) => platform.includes(v))) ||
            // iPad on iOS 13 detection
            (navigator.userAgent.includes("Mac") && "ontouchend" in document));
    }
    // Notification Permissions
    function testNotificationPermissions() {
        return __awaiter(this, void 0, void 0, function* () {
            if (isIOS() || macOS())
                return 0;
            if (navigator.permissions &&
                typeof navigator.permissions.query === "function") {
                return yield navigator.permissions
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
                });
            }
            return 0;
        });
    }
    // No Plugins
    function testPlugins() {
        let length = navigator.plugins ? navigator.plugins.length : 0;
        return length === 0 ? 1 : 0;
    }
    // App Version
    function testAppVersion() {
        let appVersion = navigator.appVersion;
        return /headless/i.test(appVersion) ? 1 : 0;
    }
    // Connection Rtt
    function testConnectionRtt() {
        let connection = navigator.connection;
        let connectionRtt = connection ? connection.rtt : undefined;
        if (connectionRtt === undefined) {
            return 0; // Flag doesn't even exists so just return NOT HEADLESS
        }
        else {
            return connectionRtt === 0 ? 0.5 : 0;
        }
    }
    //* Main Functions *//
    function getHeadlessScore() {
        return __awaiter(this, void 0, void 0, function* () {
            let score = 0;
            let testsRun = 0;
            let testRunDetail = "";
            return yield testNotificationPermissions()
                .then((x) => {
                score = x;
                testsRun++;
                testRunDetail = "NotificationPermissions : " + x + ", ";
            })
                .then(() => {
                allTestFunctions.forEach((test) => {
                    if (test.active == 1) {
                        let result = test.testFunction();
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
            });
        });
    }

    exports.getHeadlessScore = getHeadlessScore;

    Object.defineProperty(exports, '__esModule', { value: true });

    return exports;

})({});
