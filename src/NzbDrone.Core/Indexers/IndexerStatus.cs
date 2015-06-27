using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers
{
    public class IndexerStatus
    {
        public DateTime FirstFailure { get; set; }
        public DateTime LastFailure { get; set; }
        public int FailureEscalation { get; set; }

        public DateTime LastRecentSearch { get; set; }
        public ReleaseInfo LastRecentReleaseInfo { get; set; }
    }
}
