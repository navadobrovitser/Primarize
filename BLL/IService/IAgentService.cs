using System;
using System.Collections.Generic;
using System.Text;

using DTO;

namespace BLL.IServices
{
    public interface IAgentService
    {
        Task<string> AskAsync(List<ChatMessageDto> history, CancellationToken ct);
        IAsyncEnumerable<string> AskStreamingAsync(List<ChatMessageDto> history, CancellationToken ct);
    }
}