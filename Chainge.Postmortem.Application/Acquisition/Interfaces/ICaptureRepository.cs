using Chainge.Postmortem.Application.Acquisition.Models;

namespace Chainge.Postmortem.Application.Acquisition.Interfaces;

public interface ICaptureRepository
{
    Task<bool> HasSuccessfulCaptureAsync(CaptureQuery query, CancellationToken cancellationToken);
    Task SaveAttemptAsync(CaptureQuery query, CaptureResponse response, bool succeeded, CancellationToken cancellationToken);
    Task WriteManifestAsync(CancellationToken cancellationToken);
}
