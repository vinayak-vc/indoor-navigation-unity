using System;
using System.Threading;
using System.Threading.Tasks;

using IndoorNavigation.Core.Enums;
using IndoorNavigation.Core.Models;

namespace IndoorNavigation.Core.Interfaces {
    public interface ILocalizationProvider {
        event Action<LocalizationPose> LocalizationSucceeded;
        event Action<string> LocalizationFailed;
        event Action<LocalizationStatus> StatusChanged;

        LocalizationStatus Status { get; }

        Task StartLocalizationAsync(CancellationToken cancellationToken);
        void RequestRelocalization();
        void StopLocalization();
    }
}