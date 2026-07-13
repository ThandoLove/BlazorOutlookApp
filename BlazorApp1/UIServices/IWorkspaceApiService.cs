using OperationalWorkspaceApplication.Dtos;
using System.Threading.Tasks;
using OperationalWorkspaceApplication.Requests.TicketRequest;
using OperationalWorkspaceApplication.Responses.TicketResponse;
using OperationalWorkspaceApplication.Responses.WorkspaceContextResponse;

namespace OperationalWorkspaceUI.UIServices;

public interface IWorkspaceApiService
{
    Task<OauthInitResponseDto?> InitializeOAuthChallengeAsync();
    Task<TokenExchangeResponseDto?> ExchangeTokenCodeAsync(string code);
    Task<WorkspaceContextResponse?> GetWorkspaceContextAsync(string email, string name, string activeUser, string userToken);
    Task<TicketActionResponse?> SubmitIncidentTicketAsync(CreateTicketCommand command, string userToken);
}

public record OauthInitResponseDto(string LoginUrl, string ExpectedState);
public record TokenExchangeResponseDto(string Token, string AssignedUserScope);
