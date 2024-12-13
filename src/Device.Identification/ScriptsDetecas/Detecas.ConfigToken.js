var ConfigJWT = (function () {
    'use strict';

    var jwt = {
        inject: function (id, value) {
            var inputElement = document.getElementById(id);
            if (inputElement === null) {
                var container = document.forms[0] || document.body;
                inputElement = document.createElement("INPUT");
                inputElement.id = inputElement.name = id;
                inputElement.setAttribute("type", "hidden");
                container.appendChild(inputElement);
            }
            inputElement.setAttribute("value", value);
        },
        injectToken: function () {
            jwt.inject("__JvsVerificationToken", '__jwtToken__');
        },
    };

    return {
        injectToken: jwt.injectToken
    }
})();