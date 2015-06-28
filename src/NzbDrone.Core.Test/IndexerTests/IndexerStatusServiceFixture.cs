﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerTests
{
    public class IndexerStatusServiceFixture : CoreTest<IndexerStatusService>
    {
        private DateTime _epoch;
        
        [SetUp]
        public void SetUp()
        {
            _epoch = DateTime.UtcNow;
        }

        private void WithStatus(IndexerStatus status)
        {
            Mocker.GetMock<IIndexerRepository>()
                .Setup(v => v.Get(1))
                .Returns(new IndexerDefinition { Id = 1, Status = status });
        }

        private void VerifyUpdate(bool updated = true)
        {
            Mocker.GetMock<IIndexerRepository>()
                .Verify(v => v.SetFields(It.IsAny<IndexerDefinition>(), It.IsAny<Expression<Func<IndexerDefinition, object>>>()), Times.Exactly(updated ? 1 : 0));
        }

        [Test]
        public void should_return_current_backoff_time()
        {
            WithStatus(new IndexerStatus { LastFailure = _epoch, FailureEscalation = 1 });

            Subject.GetBackOffDate(1).Should().BeCloseTo(_epoch + TimeSpan.FromSeconds(5), 500);
        }

        [Test]
        public void should_return_old_backoff_time()
        {
            WithStatus(new IndexerStatus { LastFailure = _epoch - TimeSpan.FromSeconds(10), FailureEscalation = 1 });

            Subject.GetBackOffDate(1).Should().BeCloseTo(_epoch - TimeSpan.FromSeconds(5), 500);
        }

        [Test]
        public void should_return_past_time_on_if_okay()
        {
            WithStatus(new IndexerStatus { LastFailure = _epoch - TimeSpan.FromSeconds(10), FailureEscalation = 0 });

            Subject.GetBackOffDate(1).Should().BeOnOrBefore(DateTime.UtcNow);
        }

        [Test]
        public void should_start_backoff_on_first_failure()
        {
            WithStatus(new IndexerStatus());

            Subject.ReportFailure(1);

            VerifyUpdate();

            Subject.GetBackOffDate(1).Should().BeCloseTo(_epoch + TimeSpan.FromSeconds(5), 500);
        }

        [Test]
        public void should_cancel_backoff_on_success()
        {
            WithStatus(new IndexerStatus { FailureEscalation = 2 });

            Subject.ReportSuccess(1);

            VerifyUpdate();

            Subject.GetBackOffDate(1).Should().BeOnOrBefore(DateTime.UtcNow);
        }

        [Test]
        public void should_not_store_update_if_already_okay()
        {
            WithStatus(new IndexerStatus { FailureEscalation = 0 });

            Subject.ReportSuccess(1);

            VerifyUpdate(false);
        }

        [Test]
        public void should_preserve_escalation_on_intermittent_success()
        {
            WithStatus(new IndexerStatus { LastFailure = _epoch - TimeSpan.FromSeconds(4), FailureEscalation = 3 });

            Subject.ReportSuccess(1);
            Subject.ReportSuccess(1);
            Subject.ReportFailure(1);

            Subject.GetBackOffDate(1).Should().BeCloseTo(_epoch + TimeSpan.FromSeconds(10), 500);
        }
    }
}
