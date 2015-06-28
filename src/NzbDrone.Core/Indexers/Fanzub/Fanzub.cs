﻿using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Indexers.Fanzub
{
    public class Fanzub : HttpIndexerBase<FanzubSettings>
    {
        public override string Name
        {
            get
            {
                return "Fanzub";
            }
        }

        public override DownloadProtocol Protocol { get { return DownloadProtocol.Usenet; } }

        public Fanzub(IIndexerStatusService indexerStatusService, IHttpClient httpClient, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(indexerStatusService, httpClient, configService, parsingService, logger)
        {

        }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new FanzubRequestGenerator() { Settings = Settings };
        }

        public override IParseIndexerResponse GetParser()
        {
            return new RssParser() { UseEnclosureUrl = true, UseEnclosureLength = true };
        }
    }
}
