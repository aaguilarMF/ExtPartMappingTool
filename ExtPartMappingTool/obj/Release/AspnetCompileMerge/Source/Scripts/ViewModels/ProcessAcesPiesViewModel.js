var ProcessAcesPiesViewModel = function (StagingService, ProductionService) {
    var dummy = 1,
        _StagingAcesPiesFilesIsEmptyCheck = 0,
        _StagingAcesPiesFilesContentsCheck = 1,
        _Message_CheckingPipelineStatus = 'Checking For currently active Processes',
        _Message_CheckingAcesPiesFilesIsEmpty = 'Checking for live files in Aces and Pies Folders under Staging Environment',
        _Message_RetrievingAcesPiesFiles_Staging = 'Retrieving contents from Aces and/or Pies folders under Staging Environment',
        _Message_RetrievingAcesPiesFiles_Production = 'Retrieving contents from Aces and/or Pies folders under Production Environment',
        _Message_RetrievingCompleteFiles_Staging = 'Retrieving contents from Complete folder under Staging Environment',
        _Message_RetrievingCompleteFiles_Production = 'Retrieving contents from Complete folder under Production Environment',
        _Message_StagingFilesImportAcesPies = 'Importing Aces and Pies Files',
        _Message_SendingFilesToProduction = 'Sending Files from Staging Complete to Production Aces/Pies',
        _Disabled = 'Disabled',
        _Staging_SendAcesPiesToComplete_StandBy = 2,
        _Staging_SendAcesPiesToComplete = 3,
        _Staging_CompleteFilesCheck = 4,
        _Staging_SendToProduction_StandBy = 5,
        _Staging_SendToProduction = 6,
        _Production_AcesPiesFilesContentsCheck = 7,
        _Production_SendAcesPiesToComplete_StandBy = 8,
        _Production_SendAcesPiesToComplete = 9,
        _Production_CompleteFilesCheck = 10,
        _Production_Done = 11,
        buttonPrompt = ko.observable(),
        buttonText = ko.observable(_Disabled),
        activeButton = ko.observable(false),
        pipelineStatus = new PipelineStatus(),
        error = ko.observable(false),
        pushMe = function () {
            activeButton(false);
            buttonPrompt('');

            switch (currentStep()) {
                case _Staging_SendAcesPiesToComplete_StandBy:
                    pipelineStage(_Staging_SendAcesPiesToComplete);
                    break;
                case _Staging_SendToProduction_StandBy:
                    pipelineStage(_Staging_SendToProduction);
                    break;
                case _Production_SendAcesPiesToComplete_StandBy:
                    pipelineStage(_Production_SendAcesPiesToComplete);
                    break;
            }
            buttonText(_Disabled)
        },
        currentStep = ko.observable(),
        currentIntervalProcessId = ko.observable(),
        pipelineStage = ko.observable(),
        panelMessage = ko.observable(),
        infoPanelMessage = ko.observable(),
        hideMessagePanel = ko.observable(true),
        hideInfoPanel = ko.observable(true),
        stagingFilesOnStandby = ko.observable(false),
        stagingAcesFolderContents = ko.observableArray([]),
        stagingPiesFolderContents = ko.observableArray([]),
        stagingCompleteFolderContents = ko.observableArray([]),
        //Viewmodel methods
        callbackLaunchProcess = function(timer, message){
            launchProcess(displayMessage, timer, message);
        },
        launchProcess = function (intervalProcess, timer, message) {
            ceaseProcess(currentIntervalProcessId());
            currentIntervalProcessId(setInterval(displayMessage, timer, message));
        },
        ceaseProcess = function (processId) {
            clearInterval(processId);
            hideMessagePanel(true);
        },
        usePanel = function (message) {
            panelMessage(message);
            hideMessagePanel(false);
        },
        displayInfoPanel = function (message) {
            infoPanelMessage(message);
            hideInfoPanel(false);
        },
        clearPanel = function () {
            stagingAcesFolderContents([]);
            stagingPiesFolderContents([]);
            stagingCompleteFolderContents([]);
        },
        globalTracker = 3,
        displayMessage = function (message) {
            globalTracker = (globalTracker +1) % 4;
            switch (globalTracker) {
                case 0:
                    usePanel(message);
                    break;
                case 1:
                    usePanel(message + '.');
                    break;
                case 2:
                    usePanel(message + '..');
                    break;
                case 3:
                    usePanel(message + '...');
                    break;
            }
        },
        launchPrompt = function (step) {
            currentStep(step);
            switch (step) {
                case _Staging_SendAcesPiesToComplete_StandBy:
                    displayInfoPanel('Aces/Pies File Contents:');
                    buttonPrompt('Import Aces and Pies Files?');
                    buttonText('Submit');
                    activeButton(true);
                    break;
                case _Staging_SendToProduction_StandBy:
                    displayInfoPanel('Contents in staging Complete Folder:');
                    buttonPrompt('Import Files From staging complete to Production Aces/Pies?');
                    buttonText('Submit');
                    activeButton(true);
                    break;
                case _Production_SendAcesPiesToComplete_StandBy:
                    displayInfoPanel('Aces/Pies File Contents (Production):');
                    buttonPrompt('Import Aces and Pies Files?');
                    buttonText('Submit');
                    activeButton(true);
                    break;
            }
        },
        //CAlls To API
        processFilesPipelineStatus = function(){
            $.ajax({
                type: 'GET',
                url: '/api/ProcessFiles/PipelineStatus',
                success: function (result) {
                    if (result.Active === false) {
                        //if no active then go to first step
                        pipelineStage(_StagingAcesPiesFilesIsEmptyCheck);
                        return;
                    } else {
                        switch (result.ActiveStage) {
                            case 1: //files exist in aces/pies (staging)
                                pipelineStatus.Staging.uploadedToAcesPies.complete(true);
                                pipelineStage(_StagingAcesPiesFilesContentsCheck);
                                break;
                            case 2: //imported files (staging)
                                pipelineStatus.Staging.uploadedToAcesPies.complete(true);
                                pipelineStatus.Staging.importedAces.complete(true);
                                pipelineStatus.Staging.importedPies.complete(true);
                                pipelineStage(_Staging_CompleteFilesCheck)
                                break;
                            case 3: //files have been "inialized" in aces/pies (Production)
                                pipelineStatus.Staging.uploadedToAcesPies.complete(true);
                                pipelineStatus.Staging.importedAces.complete(true);
                                pipelineStatus.Staging.importedPies.complete(true);
                                pipelineStatus.Staging.sentToProduction.complete(true);
                                pipelineStage(_Production_AcesPiesFilesContentsCheck);
                                break;
                            case 4: //imported files and now just need to check to close pipeline (Production)
                                pipelineStatus.Staging.uploadedToAcesPies.complete(true);
                                pipelineStatus.Staging.importedAces.complete(true);
                                pipelineStatus.Staging.importedPies.complete(true);
                                pipelineStatus.Staging.sentToProduction.complete(true);
                                pipelineStatus.Production.uploadedToAcesPies.complete(true);
                                pipelineStatus.Production.importedAces.complete(true);
                                pipelineStatus.Production.importedPies.complete(true);
                                pipelineStage(_Production_CompleteFilesCheck);
                                break;

                        }
                    }
                },
                error: function (result) {
                    alert(result.statusText);
                },
                beforeSend: function (xhr) {
                    launchProcess(displayMessage, 1000, _Message_CheckingPipelineStatus)
                }
            });
        },
        init = function () {
            pipelineStage.subscribe(function (val) {
                switch (val) {
                    case _StagingAcesPiesFilesIsEmptyCheck:
                        StagingService.acesPiesFilesIsEmpty(
                            function () {
                                pipelineStatus.Staging.uploadedToAcesPies.complete(true);
                                pipelineStage(_StagingAcesPiesFilesContentsCheck);
                            },
                            function (message) {
                                launchProcess(displayMessage, 1000, message);
                                pipelineStatus.Staging.uploadedToAcesPies.incomplete(true);
                                error(true);
                            },
                            function () {
                                launchProcess(displayMessage, 1000, _Message_CheckingAcesPiesFilesIsEmpty)
                                pipelineStatus.Staging.uploadedToAcesPies.inProgress(true);
                            });
                        break;
                    case _StagingAcesPiesFilesContentsCheck:
                        StagingService.acesPiesContents(
                            function (result) {
                                stagingAcesFolderContents(result.FileNamesAces);
                                stagingPiesFolderContents(result.FileNamesPies);
                                ceaseProcess(currentIntervalProcessId());
                                pipelineStage(_Staging_SendAcesPiesToComplete_StandBy);
                            },
                            function () {
                                launchProcess(displayMessage, 1000, _Message_RetrievingAcesPiesFiles_Staging);
                            });
                        break;
                    case _Staging_SendAcesPiesToComplete_StandBy:
                        launchPrompt(_Staging_SendAcesPiesToComplete_StandBy);
                        break;
                    case _Staging_SendAcesPiesToComplete:
                        StagingService.importAcesPies(
                            function (result) {
                                hideInfoPanel(true);
                                clearPanel();
                                ceaseProcess(currentIntervalProcessId());
                                pipelineStatus.Staging.importedAces.complete(true);
                                pipelineStatus.Staging.importedPies.complete(true);
                                //pipelineStage(_Staging_SendToProduction_StandBy); left off here... check contents of complete folder and display
                                pipelineStage(_Staging_CompleteFilesCheck); //stagingCompleteFolderContents(result.FileNamesComplete);
                            },
                            function (message) {
                                pipelineStatus.Staging.importedAces.inProgress(true);
                                pipelineStatus.Staging.importedPies.inProgress(true);
                                launchProcess(displayMessage, 1000, message);
                                
                            });
                        break;
                    case _Staging_CompleteFilesCheck:
                        StagingService.completeContents(
                            function (result) {
                                ceaseProcess(currentIntervalProcessId());
                                stagingCompleteFolderContents(result.FileNamesComplete);
                                pipelineStage(_Staging_SendToProduction_StandBy);
                            },
                            function () {
                                launchProcess(displayMessage, 1000, _Message_RetrievingCompleteFiles_Staging);
                            });
                        break;
                    case _Staging_SendToProduction_StandBy:
                        launchPrompt(_Staging_SendToProduction_StandBy);
                        break;
                    case _Staging_SendToProduction:
                        StagingService.sendToProduction(
                            function (result) {
                                ceaseProcess(currentIntervalProcessId());
                                pipelineStatus.Staging.sentToProduction.complete(true);
                                hideInfoPanel(true);
                                clearPanel();
                                pipelineStage(_Production_AcesPiesFilesContentsCheck);

                            },
                            function () {
                                pipelineStatus.Staging.sentToProduction.inProgress(true);
                                launchProcess(displayMessage, 1000, _Message_SendingFilesToProduction);
                                //hideInfoPanel(true);
                            });
                        break;
                    case _Production_AcesPiesFilesContentsCheck:
                        ProductionService.acesPiesContents(
                            function (result) {
                                pipelineStatus.Production.uploadedToAcesPies.complete(true);
                                stagingAcesFolderContents(result.FileNamesAces);
                                stagingPiesFolderContents(result.FileNamesPies);
                                ceaseProcess(currentIntervalProcessId());
                                pipelineStage(_Production_SendAcesPiesToComplete_StandBy);
                            },
                            function () {
                                pipelineStatus.Production.uploadedToAcesPies.inProgress(true);
                                launchProcess(displayMessage, 1000, _Message_RetrievingAcesPiesFiles_Production);
                            });
                        break;
                    case _Production_SendAcesPiesToComplete_StandBy:
                        launchPrompt(_Production_SendAcesPiesToComplete_StandBy);
                        break;
                    case _Production_SendAcesPiesToComplete:
                        ProductionService.importAcesPies(
                            function (result) {
                                hideInfoPanel(true);
                                clearPanel();
                                ceaseProcess(currentIntervalProcessId());
                                pipelineStatus.Production.importedAces.complete(true);
                                pipelineStatus.Production.importedPies.complete(true);
                                //pipelineStage(_Staging_SendToProduction_StandBy); left off here... check contents of complete folder and display
                                pipelineStage(_Production_CompleteFilesCheck); //stagingCompleteFolderContents(result.FileNamesComplete);
                            },
                            function (message) {
                                pipelineStatus.Production.importedAces.inProgress(true);
                                pipelineStatus.Production.importedPies.inProgress(true);
                                launchProcess(displayMessage, 1000, message);

                            });
                        break;
                    case _Production_CompleteFilesCheck:
                        ProductionService.completeContents(
                            function (result) {
                                clearPanel();
                                ceaseProcess(currentIntervalProcessId());
                                pipelineStatus.Production.checkedComplete.complete(true);
                                stagingCompleteFolderContents(result.FileNamesComplete);
                                pipelineStage(_Production_Done);
                            },
                            function () {
                                pipelineStatus.Production.checkedComplete.inProgress(true);
                                launchProcess(displayMessage, 1000, _Message_RetrievingCompleteFiles_Production);
                            });
                        break;
                    case _Production_Done:
                        usePanel('Done!');
                        break;
                }
            });
            processFilesPipelineStatus();
        };
    init();
    return {
        stagingFilesOnStandby: stagingFilesOnStandby,
        stagingAcesFolderContents: stagingAcesFolderContents,
        stagingPiesFolderContents: stagingPiesFolderContents,
        stagingCompleteFolderContents: stagingCompleteFolderContents,
        panelMessage: panelMessage,
        infoPanelMessage: infoPanelMessage,
        hideMessagePanel: hideMessagePanel,
        hideInfoPanel: hideInfoPanel,
        buttonPrompt: buttonPrompt,
        buttonText: buttonText,
        pushMe: pushMe,
        activeButton: activeButton,
        pipelineStatus: pipelineStatus,
        error: error
    }
}