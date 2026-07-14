// This script runs safely on both Outlook Desktop and Browser platforms
Office.onReady((info) => {
    if (info.host === Office.HostType.Outlook && Office.context.mailbox.item) {
        const item = Office.context.mailbox.item;

        const senderEmail = item.from ? item.from.emailAddress : "";
        const senderName = item.from ? item.from.displayName : "";
        const activeUser = Office.context.mailbox.userProfile ? Office.context.mailbox.userProfile.emailAddress : "";

        const queryParams = `?senderEmail=${encodeURIComponent(senderEmail)}` +
            `&senderName=${encodeURIComponent(senderName)}` +
            `&activeUser=${encodeURIComponent(activeUser)}`;

        // This ensures the iframe safely redirects itself to your Blazor Home.razor screen 
        // without breaking the parent browser tab window
        window.location.replace(`https://workspace.local{queryParams}`);
    }
});

