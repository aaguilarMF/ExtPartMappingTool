//this is a service class for the knockout component. 
var ProductionService = function () {
    var
        _AcesPiesFilesContentsCheck = 1,
        _Message_ImportAcesPies = 'Importing Aces and Pies Files',
        _Message_SendToProduction = 'Sending files in Complete folder to production Aces/Pies',
        
        acesPiesContents = function (callbackSuccess, beforeSendCall) {
            $.ajax({
                type: 'GET',
                url: '/api/ProcessFiles/Production/AcesPiesContents',
                success: function (result) {
                    callbackSuccess(result);
                },
                error: function (result) {
                    alert(result.statusText);
                },
                beforeSend: function (xhr) {
                    beforeSendCall();
                }
            });
        },
        importAcesPies = function (callbackSuccess, beforeSendCall) {
            $.ajax({
                type: 'GET',
                url: '/api/ProcessFiles/Production/Import',
                success: function (result) {
                    callbackSuccess()
                },
                error: function (result) {
                    alert(result.statusText);
                },
                beforeSend: function (xhr) {
                    beforeSendCall(_Message_ImportAcesPies)
                }
            });
        },
        completeContents = function (callbackSuccess, beforeSendCall) {
            $.ajax({
                type: 'GET',
                url: '/api/ProcessFiles/Production/CompleteContents',
                success: function (result) {
                    callbackSuccess(result);
                },
                error: function (result) {
                    alert(result.statusText);
                },
                beforeSend: function (xhr) {
                    beforeSendCall();
                }
            });
        },
        sendToProduction = function (callbackSuccess, beforeSendCall) {
            $.ajax({
                type: 'GET',
                url: '/api/ProcessFiles/Production/SendToProduction',
                success: function (result) {
                    callbackSuccess();
                },
                error: function (result) {
                    alert(result.statusText);
                },
                beforeSend: function (xhr) {
                    beforeSendCall();
                }
            });
        };
    return {
        acesPiesContents: acesPiesContents,
        importAcesPies: importAcesPies,
        completeContents: completeContents,
        sendToProduction: sendToProduction
    }
}