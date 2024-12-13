/* global Detecas */

Detecas.DebugData = {
    startTime: (new Date()).getTime(),
    version: '',
    exceptions: [],
    executions: [],
    storages: [],
    devices: [],
    enable: true
};

Detecas.Debug = (function (document, json, debugData, hiddenFieldId, undefined) {
    "use strict";

    var getTime = function () {
        var now = (new Date()).getTime();
        return now - debugData.startTime;
    };

    var createElement = function (container, jsonString, id)
    {
        var diInput = document.createElement("INPUT");
        diInput.id = id;
        diInput.name = hiddenFieldId;
        diInput.setAttribute("type", "hidden");
        diInput.value = jsonString;
        container.appendChild(diInput);
    };
    var bindData = function () {
        var elByNames = document.getElementsByName(hiddenFieldId);
        var jsonString = json.stringify(debugData);

        if (elByNames == null || elByNames.length <= 0) {
            var forms = document.forms;
            if (forms != null && forms.length != 0) {
                for (var i = 0; i < forms.length; i++) {
                    createElement(document.forms[i], jsonString, (i == 0 ? hiddenFieldId : hiddenFieldId + i));
                }
            }
            else {
                createElement(document.body, jsonString, hiddenFieldId );
            }
        }
        else {
            if (elByNames != null) {
                for (var i = 0; i < elByNames.length; i++) {
                    
                    elByNames[i].value = jsonString;
                }
            }
        }          
    };    

    //var bindData = function () {

    //    //var els = createElement();

    //    //if (els != null) {
    //    //    for (var i = 0; i < els.length; i++) {
    //    //        var jsonString = json.stringify(debugData);
    //    //        els[i].value = jsonString;
    //    //    }
    //    //}

    //    //if (hiddenField === null) {
    //    //    hiddenField = document.getElementById(hiddenFieldId);
    //    //}

    //    //if (hiddenField !== null) {
    //    //    var jsonString = json.stringify(debugData);
    //    //    hiddenField.value = jsonString;
    //    //}       
    //};

    var setVersion = function (version) {
        debugData.version = version;
        bindData();
    };

    var addException = function (e, module) {
        var exception = { message: e.message, module: module, time: getTime() };
        debugData.exceptions.push(exception);
        bindData();
    };

    var addExecuteTime = function (step) {
        var executeStep = { step: step, time: getTime() };
        debugData.executions.push(executeStep);
        bindData();
    };

    var addStorage = function (name, success) {
        var storage = {
            name: name,
            success: success,
            time: getTime()
        };
        debugData.storages.push(storage);
        bindData();
    };

    var addDevice = function (name, value) {
        var device = {
            name: name,
            value: value,
            time: getTime()
        };
        debugData.devices.push(device);
        bindData();
    };

    return {
        setVersion: setVersion,
        addException: addException,
        addExecuteTime: addExecuteTime,
        addStorage: addStorage,
        addDevice: addDevice
    };
})(document, Detecas.JSON, Detecas.DebugData, "detecas-analysis");