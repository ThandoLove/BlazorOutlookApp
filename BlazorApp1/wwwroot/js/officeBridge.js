let loginDialogComponent;

window.officeBridge = {
    // Phase 6 Audit Requirement: Registers high-performance event-based activation tracking (Section 6)
    initializeEventHandlers: function () {
        if (typeof Office === 'undefined' || !Office.context || !Office.context.mailbox) {
            console.warn("OfficeJS framework context is unavailable or running outside an active Outlook host.");
            return;
        }

        // Section 6 Resolution: Detects when a user changes their active email selection inside the inbox list
        Office.context.mailbox.addHandlerAsync(Office.EventType.ItemChanged, function (eventArgs) {
            console.log("[OfficeJS] Intercepted mail item selection change event.");

            const currentItem = Office.context.mailbox.item;
            if (currentItem) {
                const mailPayload = {
                    senderEmail: currentItem.sender ? currentItem.sender.emailAddress : "",
                    senderName: currentItem.sender ? currentItem.sender.displayName : "",
                    messageId: currentItem.itemId || ""
                };

                // Command Blazor UI state container to parse the new context variables instantly
                // Targets your client assembly 'OperationalWorkspaceUI.Client' to match your routing loops
                DotNet.invokeMethodAsync('OperationalWorkspaceUI.Client', 'NotifyMailItemSelectionChanged', mailPayload);
            }
        });
    },

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

    // Appended official dialogue window lifecycle listener routines
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
                        // Targets your renamed client assembly explicitly to clear runtime lookup blocks
                        DotNet.invokeMethodAsync('OperationalWorkspaceUI.Client', 'ProcessTokenExchangeCallback', payload.authCode);
                    }
                });
            }
        );
    }
};

// Auto-trigger wire hooks when OfficeJS finishes initializing natively inside the taskpane container
if (typeof Office !== 'undefined') {
    Office.onReady(function (info) {
        if (info.host === Office.HostType.Outlook) {
            window.officeBridge.initializeEventHandlers();
        }
    });
}
