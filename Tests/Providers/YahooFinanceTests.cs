﻿using Moq;
using NUnit.Framework;
using Spread.Betting.Model;
using Spread.Betting.Providers;
using Spread.Betting.Providers.Interfaces;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Spread.Betting.Tests.Providers
{
    [TestFixture]
    public class YahooFinanceTests
    {
        private YahooFinanceProvider _yahooFinance;
        private Mock<IHttpProvider> _httpProvider;
        private Mock<IMarketDataProvider> _currencyPairProvider;
        private Mock<IFormatProvider<Quote>> _formatter;

        [SetUp]
        public void SetUp()
        {
            _httpProvider = new Mock<IHttpProvider>();
            _currencyPairProvider = new Mock<IMarketDataProvider>();
            _formatter = new Mock<IFormatProvider<Quote>>();
        }

        [Test]
        public void given_market_center_is_open_process_response()
        {
            var url = WebUtility.UrlEncode("https://query.yahooapis.com/v1/public/yql?q=select * from yahoo.finance.xchange where pair in ('AUDCAD,AUDCHF')") + "&format=json&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys";
            var pairs = new List<CurrencyPair>
            {
                new CurrencyPair {Symbol = "AUDCAD"},
                new CurrencyPair {Symbol = "AUDCHF"}
            };

            var response = new Quote
            {
                Created = DateTime.Now,
                Rates = new List<Rate>()
            };

            response.Rates.Add(new Rate { CurrencyPair = new CurrencyPair { Symbol = "AUDCAD" } });
            response.Rates.Add(new Rate { CurrencyPair = new CurrencyPair { Symbol = "AUDCHF" } });

            _currencyPairProvider.Setup(x => x.Market).Returns(new Market{IsOpen = true});
            _currencyPairProvider.Setup(x => x.Pairs).Returns(pairs);
            _httpProvider.Setup(x => x.GetAsync(url, _formatter.Object)).Returns(Task.FromResult(response));

            _yahooFinance = new YahooFinanceProvider(_httpProvider.Object,
                                                                    _currencyPairProvider.Object,
                                                                    _formatter.Object);
            var result =_yahooFinance.GetQuote();

            _httpProvider.Verify();
        }

        [Test]
        public void given_market_center_closed_open_ignore_response()
        {
            _currencyPairProvider.Setup(x => x.Market).Returns(new Market { IsOpen = false });

            _yahooFinance = new YahooFinanceProvider(_httpProvider.Object,
                                                                    _currencyPairProvider.Object,
                                                                    _formatter.Object);
            var result = _yahooFinance.GetQuote().Result;

            Assert.That(result, Is.Null);
        }
    }
}
