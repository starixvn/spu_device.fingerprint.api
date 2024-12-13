/* global Detecas */

Detecas.DeviceDetail = (function (window, document, common, cookie, base64, config, debug) {
    "use strict";

    if (fpsChkIntr('Detecas.DeviceDetail') == true) {
        return;
    }

    function isNewDeviceDetail(deviceDetail) {
        var deviceCode = deviceDetail.deviceCode;
        var cookieName = "dct";
        var tkn = cookie.read(cookieName);
        if (tkn === deviceCode) {
            return false;
        }

        cookie.write(cookieName, deviceCode, 30);
        return true;
    }

    function insertDeviceDetail(deviceDetail) {
        try {
            var version = encodeURIComponent(base64.encode(config.version));
            var detail = base64.encode(JSON.stringify(deviceDetail));

            // Using cross-domain iframe to post detail information to server
            // Be aware that Form submit is lock and synchronous
            // CORS is slow and non-cross-browser currently so we do not use it now.
            var div = common.createTransparentDOMElement("DIV");
            div.id = "iframe" + config.ip;

            // Event for submit process done
            div.innerHTML = "<iframe name='iframe" + config.ip + "' src='about:blank' style='visibility: hidden; position: absolute'></iframe>";
            var form = common.createTransparentDOMElement("FORM");
            form.id = "form" + config.ip; // Mark version for removal

            // Different IP from encode time and decode time issue (old IP in cached script) so need to re-send ip
            form.setAttribute("action", config.host + "/dd.ashx?v=" + version + "&deviceCode=" + deviceDetail.deviceCode);
            form.setAttribute("method", "POST");
            form.target = "iframe" + config.ip;

            var input = common.createTransparentDOMElement("INPUT");
            input.value = detail;
            input.name = "deviceDetail";

            form.appendChild(input);
            document.body.appendChild(div);
            document.body.appendChild(form);
            form.submit();
        } catch (exception) {
            //debug.addException(exception, "Detecas.insertDeviceDetail");
        }
    }

    return {
        isNewDeviceDetail: isNewDeviceDetail,
        insertDeviceDetail: insertDeviceDetail
    };
} (window, document, Detecas.Common, Detecas.Cookie, Detecas.Base64, Detecas.Config, Detecas.Debug));