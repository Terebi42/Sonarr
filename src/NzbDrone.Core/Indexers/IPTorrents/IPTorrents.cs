using System;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using NLog;

namespace NzbDrone.Core.Indexers.IPTorrents
{
    public class IPTorrents : HttpIndexerBase<IPTorrentsSettings>
    {
        public override string Name
        {
            get
            {
                return "IP Torrents";
            }
        }

        public override DownloadProtocol Protocol { get { return DownloadProtocol.Torrent; } }
        public override Boolean SupportsSearch { get { return false; } }
        public override Int32 PageSize { get { return 0; } }

        public IPTorrents(IIndexerStatusService indexerStatusService, IHttpClient httpClient, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(indexerStatusService, httpClient, configService, parsingService, logger)
        {

        }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new IPTorrentsRequestGenerator() { Settings = Settings };
        }

        public override IParseIndexerResponse GetParser()
        {
            return new TorrentRssParser() { ParseSizeInDescription = true };
        }
    }
}