var apiUrl = '__apiUrl__';
var subscriberId = '__subscriberId__';
var sessionKey = '__sessionKey__';
var cdnAPIUrl = '__cdnAPIUrl__';
var fp = {
    requireJS: function(_url) {
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
                fp.log(apiUrl, "Load Script", err.message + '|' + sessionKey);
            }            
        });
    },
    post: function(url, data) {
        return fetch(url, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify(data),
        });
    },
    log: function(url, functionName, exception) {
        var data = {
            function: functionName,
            exception: exception,
        };
        fp.post(url + "/cle", data);
    }
}

try {    
    fp.requireJS(cdnAPIUrl)
        .then(() => {           
            try {
                globalFP.capture(apiUrl, subscriberId, sessionKey);
            }
            catch (err) {
                fp.log(apiUrl, "Capture Data", err.code + '|' + err.name + '|' + err.stack + '|' + err.message + '|' + sessionKey);
            }


            Promise.resolve(fp.post(apiUrl + '/ctl', {
                'SessionKey': sessionKey,
                'SubscriberId': subscriberId,
                'Url': window.location && window.location.href
                    ? window.location.href
                    : "",
                'UserAgent': navigator.userAgent,
                'Information': 'Capture data successful'
            }));
        })
        .catch((error) => {
            fp.log(apiUrl, "Load Script", error.code + '|' + error.name + '|' + error.stack + '|' + error.message + '|' + sessionKey);
        });
}
catch (err) {
    fp.log(apiUrl, "Load API", err.message);
}




