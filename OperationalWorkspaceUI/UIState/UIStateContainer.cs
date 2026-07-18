using OperationalWorkspaceApplication.Dtos;
using OperationalWorkspaceApplication.Responses;
using OperationalWorkspaceApplication.Responses.WorkspaceContextResponse;
using System;

namespace OperationalWorkspaceUI.UIState;

public class UIStateContainer
{
    // Centralized memory tracking primitives
    public bool IsAuthenticated { get; private set; }
    public string SenderEmail { get; private set; } = string.Empty;
    public string SenderName { get; private set; } = string.Empty;
    public string ActiveUserEmail { get; private set; } = string.Empty;
    public string UserRoleScope { get; private set; } = "Sales;Consultant"; // Fallback development scope

    // Cached Customer 360 payload model state
    public WorkspaceContextResponse? CurrentContextResponse { get; private set; }

    public event Action? OnStateChanged;

    public void SetAuthenticatedState(bool isAuthenticated)
    {
        if (IsAuthenticated == isAuthenticated) return;
        IsAuthenticated = isAuthenticated;
        NotifyStateChanged();
    }

    public void SetIdentityContext(string senderEmail, string senderName, string activeUserEmail)
    {
        SenderEmail = senderEmail?.Trim() ?? string.Empty;
        SenderName = senderName?.Trim() ?? string.Empty;
        ActiveUserEmail = activeUserEmail?.Trim() ?? string.Empty;
        NotifyStateChanged();
    }

    public void SetUserRoleScope(string roleScope)
    {
        if (!string.IsNullOrWhiteSpace(roleScope))
        {
            UserRoleScope = roleScope.Trim();
            NotifyStateChanged();
        }
    }

    public void SetWorkspaceContextResponse(WorkspaceContextResponse response)
    {
        CurrentContextResponse = response;
        NotifyStateChanged();
    }

    /// <summary>
    /// Phase 2 Audit Resolution: Batches multi-property session updates into 
    /// a single atomic refresh signal to prevent wasteful Blazor UI repaints.
    /// </summary>
    public void UpdateSession(bool isAuthenticated, string senderEmail, string senderName, string activeUserEmail, string? roleScope, WorkspaceContextResponse? response)
    {
        IsAuthenticated = isAuthenticated;
        SenderEmail = senderEmail?.Trim() ?? string.Empty;
        SenderName = senderName?.Trim() ?? string.Empty;
        ActiveUserEmail = activeUserEmail?.Trim() ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(roleScope))
        {
            UserRoleScope = roleScope.Trim();
        }

        CurrentContextResponse = response;

        NotifyStateChanged(); // Fires exactly once for the entire update batch
    }

    public void ClearSessionStore()
    {
        IsAuthenticated = false;
        SenderEmail = string.Empty;
        SenderName = string.Empty;
        ActiveUserEmail = string.Empty;
        CurrentContextResponse = null;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnStateChanged?.Invoke();
}
