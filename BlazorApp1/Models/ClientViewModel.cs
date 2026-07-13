using System.ComponentModel.DataAnnotations;

namespace OperationalWorkspaceUI.Models;

public class TicketSubmissionModel
{
    [Required(ErrorMessage = "Action subject criteria statement required.")]
    [StringLength(100, ErrorMessage = "Subject constraints capped at 100 characters.")]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please supply detailed task diagnostic notes.")]
    public string Description { get; set; } = string.Empty;
}

public class DocumentViewerModel
{
    public string ActiveDocumentNumber { get; set; } = string.Empty;
    public string TargetMimeType { get; set; } = "application/pdf";
    public bool IsViewerPaneOpen { get; set; }

    public void PresentDocumentInPane(string docNo, string mime)
    {
        ActiveDocumentNumber = docNo;
        TargetMimeType = mime;
        IsViewerPaneOpen = true;
    }

    public void ClosePane() => IsViewerPaneOpen = false;
}

public class TicketSubmissionViewModel
{
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsSubmitting { get; set; }
    public string SuccessFeedbackMessage { get; set; } = string.Empty;
    public string ErrorFeedbackMessage { get; set; } = string.Empty;

    public void ResetForm()
    {
        Subject = string.Empty;
        Description = string.Empty;
        IsSubmitting = false;
        SuccessFeedbackMessage = string.Empty;
        ErrorFeedbackMessage = string.Empty;
    }
}
