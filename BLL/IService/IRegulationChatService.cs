using DTO.RegulationChat;
using System.Threading;
using System.Threading.Tasks;

namespace BLL.IServices
{
    public interface IRegulationChatService
    {
        // קבלת שאלה והחזרת תשובה המבוססת על התקנון.
        Task<RegulationChatResponseDto> AskAsync(
            RegulationChatRequestDto request,
            CancellationToken cancellationToken = default);
    }
}