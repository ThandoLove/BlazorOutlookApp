window.officeBridge = {
    // Universal Attachment Engine using the official Office.js execution rules
    attachFileToActiveEmail: function (fileUrl, fileName) {
        if (!Office.context.mailbox || !Office.context.mailbox.item) {
            alert("This action requires an active email compilation state framework.");
            return;
        }

        // Invoke native asynchronous attachment layer pushing absolute binary server endpoints
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
    }
};
