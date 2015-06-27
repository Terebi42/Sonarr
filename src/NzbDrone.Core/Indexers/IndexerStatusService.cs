using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers
{
    public interface IIndexerStatusService
    {
        DateTime GetBackOffDate(int indexerId);
        void ReportSuccess(int indexerId);
        void ReportFailure(int indexerId, TimeSpan minimumBackOff = default(TimeSpan));

        ReleaseInfo GetLastRecentReleaseInfo(int indexerId);
        void UpdateLastRecentReleaseInfo(int indexerId, ReleaseInfo releaseInfo);
    }

    public class IndexerStatusService : IIndexerStatusService
    {
        const int MinimumBackOffPeriod = 5;
        const int MaximumBackOffPeriod = 86400;
        const int MaximumEscalation = 17;


        private readonly IIndexerRepository _indexerRepository;
        private readonly ICached<IndexerStatus> _statusCache;
        private readonly Logger _logger;

        public IndexerStatusService(IIndexerRepository indexerRepository, ICacheManager cacheManager, Logger logger)
        {
            _indexerRepository = indexerRepository;
            _statusCache = cacheManager.GetCache<IndexerStatus>(typeof(IndexerStatusService));
            _logger = logger;
        }

        private IndexerStatus GetIndexerStatus(int indexerId)
        {
            return _statusCache.Get(indexerId.ToString(), () => _indexerRepository.Get(indexerId).Status);
        }

        private void UpdateIndexerStatus(int indexerId, IndexerStatus status)
        {
            _indexerRepository.SetFields(new IndexerDefinition { Id = indexerId, Status = status }, i => i.Status);
        }

        public DateTime GetBackOffDate(int indexerId)
        {
            var status = GetIndexerStatus(indexerId);

            if (status.FailureEscalation == 0)
            {
                return DateTime.UtcNow;
            }

            var backOffPeriod = Math.Min(MaximumBackOffPeriod, MinimumBackOffPeriod << (status.FailureEscalation - 1));

            var backOffDate = status.LastFailure + TimeSpan.FromSeconds(backOffPeriod);

            return backOffDate;
        }

        public void ReportSuccess(int indexerId)
        {
            var status = GetIndexerStatus(indexerId);

            if (status.FailureEscalation == 0)
            {
                return;
            }

            status.FailureEscalation--;

            UpdateIndexerStatus(indexerId, status);
        }

        public void ReportFailure(int indexerId, TimeSpan minimumBackOff = default(TimeSpan))
        {
            var status = GetIndexerStatus(indexerId);

            if (status.FailureEscalation == 0)
            {
                status.FirstFailure = DateTime.UtcNow;
            }

            status.LastFailure = DateTime.UtcNow;
            status.FailureEscalation = Math.Min(MaximumEscalation, status.FailureEscalation + 1);

            if (minimumBackOff != TimeSpan.Zero)
            {
                var minimumBackOffDate = DateTime.UtcNow + minimumBackOff;

                while (status.FailureEscalation != MaximumEscalation && GetBackOffDate(indexerId) < minimumBackOffDate)
                {
                    status.FailureEscalation = Math.Min(MaximumEscalation, status.FailureEscalation + 1);
                }
            }

            UpdateIndexerStatus(indexerId, status);
        }

        public ReleaseInfo GetLastRecentReleaseInfo(int indexerId)
        {
            var status = GetIndexerStatus(indexerId);

            return status.LastRecentReleaseInfo;
        }

        public void UpdateLastRecentReleaseInfo(int indexerId, ReleaseInfo releaseInfo)
        {
            var status = GetIndexerStatus(indexerId);

            status.LastRecentSearch = DateTime.UtcNow;
            status.LastRecentReleaseInfo = releaseInfo;

            UpdateIndexerStatus(indexerId, status);
        }
    }
}
