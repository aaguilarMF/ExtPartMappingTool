//this is a service class for the knockout component. 
var StagingService = function () {
    var dummy = 1,
        _AcesPiesFilesContentsCheck = 1,
        _Message_ImportAcesPies = 'Importing Aces and Pies Files',
        _Message_SendToProduction = 'Sending files in Complete folder to production Aces/Pies',
        acesPiesFilesIsEmpty = function (callbackSuccess, callbackFail, beforeSendCall) {
            $.ajax({
                type: 'GET',
                url: '/api/ProcessFiles/Staging/AcesPiesIsEmpty',
                success: function (result) {
                    if (result) {
                        callbackFail('There are no files in Aces nor Pies. Please populate either or both these folders in order to continue.');
                    } else {
                        // there are files so schedule to check contents
                        callbackSuccess();
                    }
                },
                error: function (result) {
                    alert(result.statusText);
                },
                beforeSend: function (xhr) {
                    beforeSendCall();
                }
            });
        },
        acesPiesContents = function (callbackSuccess, beforeSendCall) {
            $.ajax({
                type: 'GET',
                url: '/api/ProcessFiles/Staging/AcesPiesContents',
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
                url: '/api/ProcessFiles/Staging/Import',
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
                url: '/api/ProcessFiles/Staging/CompleteContents',
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
                url: '/api/ProcessFiles/Staging/SendToProduction',
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
        acesPiesFilesIsEmpty: acesPiesFilesIsEmpty,
        acesPiesContents: acesPiesContents,
        importAcesPies: importAcesPies,
        completeContents: completeContents,
        sendToProduction: sendToProduction
    }
}