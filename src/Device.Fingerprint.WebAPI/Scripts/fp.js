var subscriberId = '__subscriberId__';
var sessionKey = '__sessionKey__';
var apiUrl = '__apiUrl__';
var portalAPIUrl = '__portalAPIUrl__';
var version = '2023042704';

var fp = {
    requireJS: function (_url) {
        return new Promise(function (resolve, reject) {
            try {
                var script = document.createElement('script');
                script.type = 'text/javascript';
                script.src = _url;
                script.async = true;
                script.defer = true;
                script.addEventListener('load', function () {
                    return resolve(script);
                }, false);
                script.addEventListener('error', function () {
                    return reject(script);
                }, false);

                document.head.appendChild(script);
            }
            catch (err) {
                fp.log(portalAPIUrl, "Load Script", err.message + '|' + sessionKey);
            }
        });
    },
    get: function (url) {
        return fetch(url, {
            method: "GET",
            headers: {
                "Content-Type": "text/plain",
                "Cache-Control": "no-cache"
            }
        });
    },
    getWithCache: function (url) {
        return fetch(url, {
            method: "GET",
            headers: {
                "Content-Type": "text/plain",
                "Cache-Control": "max-age=31536000, immutable"
            }
        });
    },
    inject: function (value) {
        var gfpInput = document.getElementById("__fdi");
        if (gfpInput === null) {
            var container = document.forms[0] || document.body;
            gfpInput = document.createElement("INPUT");
            gfpInput.id = gfpInput.name = "__fdi";
            gfpInput.setAttribute("type", "hidden");
            gfpInput.setAttribute("value", value)
            container.appendChild(gfpInput);
        }
    },
    post: function (url, data) {
        fetch(url, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(data),
        });
    },
    postAuthor: function (url, data, gjt) {
        fetch(url, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Authorization": "Bearer " + gjt
            },
            body: JSON.stringify(data),
        });
    },
    log: function (url, functionName, exception) {
        var data = {
            function: functionName,
            exception: exception,
        };
        fp.post(url + "/cle", data);
    },
    checkDomReady: function () {

        fp.inject(sessionKey);

        fp.get(apiUrl + '/scr/gjt')
            .then(response => {
                return response.text();
            })
            .then((gjt) => {

                fp.requireJS(apiUrl + '/scr/' + version + '/crt').then(() => {
                    fp.requireJS(apiUrl + '/scr/' + version + '/fp').then(() => {
                        return globalFP.getFP(crypt.hashMD5);
                    }).then(res => {
                        var data = {
                            'fingerprint': {
                                "Id": res.Id,
                                "Attributes": res.Properties
                            },
                            'SessionKey': sessionKey,
                            'SubscriberId': subscriberId
                        };

                        fp.postAuthor(portalAPIUrl + '/fp', data, gjt);
                    }).catch((err) => {
                        fp.log(portalAPIUrl, "Load Script fp", err.message + '|' + sessionKey);
                    });

                    fp.requireJS(apiUrl + '/scr/' + version + '/ce').then(() => {
                        fpNewCookie.getStorage().then((cookie) => {
                            var data = {
                                'Cookie': cookie,
                                'SessionKey': sessionKey,
                                'SubscriberId': subscriberId
                            };

                            fp.postAuthor(portalAPIUrl + '/ce', data, gjt);
                            fpNewCookie.setStorage(cookie);
                            fp.getWithCache(portalAPIUrl + "/etg/" + subscriberId + "/" + sessionKey + "/" + cookie);
                        });
                    }).catch((err) => {
                        fp.log(portalAPIUrl, "Load Script ce", err.message + '|' + sessionKey);
                    });

                    fp.requireJS(apiUrl + '/scr/' + version + '/hl').then(() => {
                        return fpHeadless.getHeadlessScore();
                    }).then(res => {
                        var data = {
                            'HeadlessBrowser': {
                                "Score": res.score,
                                "TestsRun": res.testsRun,
                                "TestsDetail": res.testsDetail
                            },
                            'SessionKey': sessionKey,
                            'SubscriberId': subscriberId
                        };

                        fp.postAuthor(portalAPIUrl + '/hl', data, gjt);
                    }).catch((err) => {
                        fp.log(portalAPIUrl, "Load Script hl", err.message + '|' + sessionKey);
                    });

                    fp.requireJS(apiUrl + '/scr/' + version + '/bi').then(() => {
                        return fpBrowserDetect.getBrowserInfo();
                    }).then(res => {
                        var data = {
                            'BrowserDetect': {
                                "OSName": res.OSName,
                                "BrowserName": res.BrowserName,
                                "Platform": res.Platform,
                                "Vendor": res.Vendor,
                                "IsMobileDevice": res.IsMobileDevice
                            },
                            'SessionKey': sessionKey,
                            'SubscriberId': subscriberId
                        };

                        fp.postAuthor(portalAPIUrl + '/bi', data, gjt);

                    }).catch((err) => {
                        fp.log(portalAPIUrl, "Load Script bi", err.message + '|' + sessionKey);
                    });
                }).catch((err) => {
                    fp.log(portalAPIUrl, "Load Script crt", err.message + '|' + sessionKey);
                });
            }).catch((err) => {
                fp.log(portalAPIUrl, "Load Script gjt", err.message + '|' + sessionKey);
            });
    }
}

try {
    if (document.readyState == 'loading') {
        // still loading, wait for the event
        document.addEventListener('DOMContentLoaded', fp.checkDomReady);

    } else {
        // DOM is ready!
        fp.checkDomReady();
    }
} catch (exception) {
    console.log('exception', exception);
}
