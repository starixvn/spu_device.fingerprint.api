/* global Detecas */

Detecas.GPU = (function (window, document, common) {
    "use strict";

    if (fpsChkIntr('Detecas.GPU') == true) {
        return;
    }

    var getGpuCode = function () {
        try {
            var div = common.createTransparentDOMElement("div", "__canvasContainer");
            document.body.appendChild(div);
            var canvasElement = common.createTransparentDOMElement("canvas");
            canvasElement.setAttribute("width", 80);
            canvasElement.setAttribute("height", 20);
            div.appendChild(canvasElement);

            //var opts = { desynchronized: true, preserveDrawingBuffer: true };
            
            var context;
            canvasElement.getContext && (context = canvasElement.getContext("2d"));
            if (!context) {
                return "";
            }

            //if (context.getContextAttributes().desynchronized) {}

            // text with lowercase/uppercase/punctuation symbols
            var text = "azAZ~!@#$%^&*()_+|{};?<>";
            context.textBaseline = "top";

            // the most common type
            context.font = "7px 'Arial'";
            context.textBaseline = "alphabetic";
            context.fillStyle = "#f60";
            context.fillRect(125, 1, 62, 20);

            // some tricks for color mixing
            context.fillStyle = "#069";
            if (context.fillText) {
                context.fillText(text, 2, 15);
            }
            context.fillStyle = "rgba(102, 204, 0, 0.7)";
            if (context.fillText) {
                context.fillText(text, 4, 17);
            }

            var dataURL = canvasElement.toDataURL();
            if (dataURL) {
                return dataURL.replace("data:image/png;base64,", "");
            }
        } catch (exception) {
            Detecas.Debug.addException(exception, "Detecas.GPU");
        }

        return "";
    };

    return {
        getGPUCode: getGpuCode
    };
})(window, document, Detecas.Common);