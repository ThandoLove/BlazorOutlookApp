let loginDialogComponent;

window.officeBridge = {
    // Universal Attachment Engine using the official Office.js execution rules
    attachFileToActiveEmail: function (fileUrl, fileName) {
        if (!Office.context.mailbox || !Office.context.mailbox.item) {
            alert("This action requires an active email compilation state framework.");
            return;
        }

        Office.context.mailbox.item.addItemAttachmentAsync(
            fileUrl,
            fileName,
            { isInline: false },
            function (asyncResult) {
                if (asyncResult.status === Office.AsyncResultStatus.Failed) {
                    alert("OfficeJS Attachment rejection error: " + asyncResult.error.message);
                } else {
                    alert("Success! Attached " + fileName + " straight to email composition frame.");
                }
            }
        );
    },

    // FIX: Appended official dialogue window lifecycle listener routines
    openSecureLoginWindow: function (targetLoginUrl) {
        Office.context.ui.displayDialogAsync(targetLoginUrl, { height: 60, width: 40, displayInIframe: false },
            function (asyncResult) {
                if (asyncResult.status === Office.AsyncResultStatus.Failed) {
                    console.error("Failed to generate authentication popup container frame: " + asyncResult.error.message);
                    return;
                }

                loginDialogComponent = asyncResult.value;

                // Track information strings passed backward down the child container pipeline
                loginDialogComponent.addEventHandler(Office.EventType.DialogMessageReceived, function (arg) {
                    const payload = JSON.parse(arg.message);
                    loginDialogComponent.close();

                    if (payload.status === "success") {
                        // FIX: Targets your renamed client assembly explicitly to clear runtime lookup blocks
                        DotNet.invokeMethodAsync('OperationalWorkspaceUI.Client', 'ProcessTokenExchangeCallback', payload.authCode);
                    }
                });
            }
        );
    }
};
