using System.Threading;
using System.Threading.Tasks;

namespace BuildingSoftwareWithLLMs.Application.Abstractions.LLM;

public interface ILLMProvider
{
    Task<LlmResponse> CompleteAsync(
        LlmRequest request,
        CancellationToken cancellationToken = default);
}
