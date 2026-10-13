using Chainge.Postmortem.Application.Acquisition.Models;

namespace Chainge.Postmortem.Application.Acquisition.Interfaces;

public interface ICaptureClient
{
    Task<CaptureResponse> PostAsync(CaptureQuery query, CancellationToken cancellationToken);
}
