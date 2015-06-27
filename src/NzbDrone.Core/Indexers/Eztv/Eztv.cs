using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using NLog;

namespace NzbDrone.Core.Indexers.Eztv
{
    public class Eztv : HttpIndexerBase<EztvSettings>
    {
        public override string Name
        {
            get
            {
                return "EZTV";
            }
        }

        public override DownloadProtocol Protocol { get { return DownloadProtocol.Torrent; } }

        public Eztv(IIndexerStatusService indexerStatusService, IHttpClient httpClient, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(indexerStatusService, httpClient, configService, parsingService, logger)
        {

        }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new EztvRequestGenerator() { Settings = Settings };
        }

        public override IParseIndexerResponse GetParser()
        {
            return new EzrssTorrentRssParser();
        }
    }
}