var subscriberId = '__subscriberId__';
var sessionKey = '__sessionKey__';
var cdnAPIUrl = '__cdnAPIUrl__';
var portalAPIUrl = '__portalAPIUrl__';
var version = '__version__';

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
        return fetch(url, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(data),
        });
    },
    postAuthor: function (url, data, gjt) {
        return fetch(url, {
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
        let date = new Date();
        let startDate = date;
        console.log('checkDomReady');

        fp.inject(sessionKey);
        fp.get(cdnAPIUrl + '/scr/gjt')
            .then(response => {
                date = new Date();
                console.log('gjt Response: ', date - startDate);

                return response.text();
            })
            .then((gjt) => {
                date = new Date();
                console.log('gjt Done: ', date - startDate);

                fp.requireJS(cdnAPIUrl + '/scr/cdn/crt').then(() => {
                    date = new Date();
                    console.log('crt Done: ', date - startDate);

                    //fp.requireJS(cdnAPIUrl + '/scr/cdn/fp').then(() => {
                    //    date = new Date();
                    //    console.log('fp executeScript Start: ', date - startDate);

                    //    return globalFP.getFP();
                    //}).then(res => {
                    //    date = new Date();
                    //    console.log('fp executeScript Done: ', date - startDate);

                    //    var data = {
                    //        'fingerprint': {
                    //            "Id": res.Id,
                    //            "Attributes": res.Properties
                    //        },
                    //        'SessionKey': sessionKey,
                    //        'SubscriberId': subscriberId
                    //    };

                    //    crypt.encryptingWithKey(data).then(res => {
                    //        fp.postAuthor(portalAPIUrl + '/fp', res, gjt);
                    //    });
                    //}).catch((err) => {
                    //    fp.log(portalAPIUrl, "Load Script fp", err.message + '|' + sessionKey);
                    //});



                    fp.requireJS(cdnAPIUrl + '/scr/cdn/ce').then(() => {

                        date = new Date();
                        console.log('ce executeScript Start: ', date - startDate);

                        const result = fpCookie.storageHandler((data) => {
                                console.log('data: ', data);
                            });
                        console.log('result: ', result);
                        date = new Date();
                        console.log('ce executeScript Done: ', date - startDate);

                        var data = {
                            'Cookie': result,
                            'SessionKey': sessionKey,
                            'SubscriberId': subscriberId
                        };
                        crypt.encryptingWithKey(data).then(res => {
                            fp.postAuthor(portalAPIUrl + '/ce', res, gjt);
                        });
                    }).catch((err) => {
                        fp.log(portalAPIUrl, "Load Script ce", err.message + '|' + sessionKey);
                    });




                    //fp.requireJS(cdnAPIUrl + '/scr/cdn/hl').then(() => {
                    //    date = new Date();
                    //    console.log('hl executeScript Start: ', date - startDate);

                    //    return fpHeadless.getHeadlessScore();
                    //}).then(res => {

                    //    date = new Date();
                    //    console.log('hl executeScript Done: ', date - startDate);

                    //    var data = {
                    //        'HeadlessBrowser': {
                    //            "Score": res.score,
                    //            "TestsRun": res.testsRun,
                    //            "TestsDetail": res.testsDetail
                    //        },
                    //        'SessionKey': sessionKey,
                    //        'SubscriberId': subscriberId
                    //    };

                    //    crypt.encryptingWithKey(data).then(res => {
                    //        fp.postAuthor(portalAPIUrl + '/hl', res, gjt);
                    //    });
                    //}).catch((err) => {
                    //    fp.log(portalAPIUrl, "Load Script hl", err.message + '|' + sessionKey);
                    //});

                    //fp.requireJS(cdnAPIUrl + '/scr/cdn/bi').then(() => {
                    //    date = new Date();
                    //    console.log('bi executeScript Start: ', date - startDate);

                    //    return fpBrowserDetect.getBrowserInfo();
                    //}).then(res => {

                    //    date = new Date();
                    //    console.log('bi executeScript Done: ', date - startDate);

                    //    var data = {
                    //        'BrowserDetect': {
                    //            "OSName": res.OSName,
                    //            "BrowserName": res.BrowserName,
                    //            "Platform": res.Platform,
                    //            "Vendor": res.Vendor,
                    //            "IsMobileDevice": res.IsMobileDevice
                    //        },
                    //        'SessionKey': sessionKey,
                    //        'SubscriberId': subscriberId
                    //    };

                    //    crypt.encryptingWithKey(data).then(res => {
                    //        fp.postAuthor(portalAPIUrl + '/bi', res, gjt);
                    //    });
                    //}).catch((err) => {
                    //    fp.log(portalAPIUrl, "Load Script bi", err.message + '|' + sessionKey);
                    //});
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
