using OperationalWorkspaceApplication.Dtos;
using System.Threading.Tasks;
using OperationalWorkspaceApplication.Requests.TicketRequest;
using OperationalWorkspaceApplication.Responses.TicketResponse;
using OperationalWorkspaceApplication.Responses.WorkspaceContextResponse;
using OperationalWorkspaceUI.Models; // Reference the unified Result wrapper

namespace OperationalWorkspaceUI.UIServices;

public interface IWorkspaceApiService
{
    /// <summary>
    /// Initializes the outward Syracuse authorization sequence.
    /// </summary>
    Task<Result<OauthInitResponseDto>> InitializeOAuthChallengeAsync();

    /// <summary>
    /// Exchanges an incoming callback parameter code context for active user authorization tokens.
    /// </summary>
    Task<Result<TokenExchangeResponseDto>> ExchangeTokenCodeAsync(string code);

    /// <summary>
    /// Resolves the unified Customer 360 overview profile metrics from the backend API.
    /// </summary>
    Task<Result<WorkspaceContextResponse>> GetWorkspaceContextAsync(string email, string name, string activeUser, string userToken);

    /// <summary>
    /// Submits a newly generated support incident track to the workspace logging registry.
    /// </summary>
    Task<Result<TicketActionResponse>> SubmitIncidentTicketAsync(CreateTicketCommand command, string userToken);
}

// Keep your inline DTO structures untouched exactly as they exist in your file
public record OauthInitResponseDto(string LoginUrl, string ExpectedState);
public record TokenExchangeResponseDto(string Token, string AssignedUserScope);
