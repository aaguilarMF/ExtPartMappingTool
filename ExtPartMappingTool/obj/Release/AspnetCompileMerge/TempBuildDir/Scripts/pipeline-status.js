var StageStatus = function () {
    var incomplete = ko.observable(true),
        inProgress = ko.observable(false),
        complete = ko.observable(false),
        init = function () {
            incomplete.subscribe(function (val) {
                if (val) {
                    inProgress(false);
                    complete(false);
                }
            });
            inProgress.subscribe(function (val) {
                if (val) {
                    incomplete(false);
                    complete(false);
                }
            });
            complete.subscribe(function (val) {
                if (val) {
                    incomplete(false);
                    inProgress(false);
                }
            });
        };
    init();
    return {
        incomplete: incomplete,
        inProgress: inProgress,
        complete: complete
    };
};
var PipelineStatus = function () {
    var Staging = new function () {
        var uploadedToAcesPies = new StageStatus(),
        importedAces = new StageStatus(),
        importedPies = new StageStatus(),
        sentToProduction = new StageStatus();
        return {
            uploadedToAcesPies: uploadedToAcesPies,
            importedAces: importedAces,
            importedPies: importedPies,
            sentToProduction: sentToProduction
        }
    },
    Production = new function () {
        var uploadedToAcesPies = new StageStatus(),
        importedAces = new StageStatus(),
        importedPies = new StageStatus(),
        checkedComplete = new StageStatus();
        return {
            uploadedToAcesPies: uploadedToAcesPies,
            importedAces: importedAces,
            importedPies: importedPies,
            checkedComplete: checkedComplete
        }
    };
    return {
        Staging: Staging,
        Production: Production
    }
};