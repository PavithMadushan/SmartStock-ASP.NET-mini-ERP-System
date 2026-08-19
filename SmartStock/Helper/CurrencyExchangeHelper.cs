using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace SmartStock.Helpers
{
    // Maps one row of the Frankfurter /v2/rates response:
    // { "date": "2026-08-18", "base": "LKR", "quote": "USD", "rate": 0.00301 }
    public class CurrencyRateRow
    {
        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("base")]
        public string Base { get; set; }

        [JsonProperty("quote")]
        public string Quote { get; set; }

        [JsonProperty("rate")]
        public decimal Rate { get; set; }
    }

    public class CurrencyRatesResult
    {
        public bool Success { get; set; }
        public string AsOfDate { get; set; }
        public Dictionary<string, decimal> Rates { get; set; }

        public CurrencyRatesResult()
        {
            Rates = new Dictionary<string, decimal>();
        }
    }

    /// <summary>
    /// Fetches today's LKR -> USD/AUD/NZD exchange rates from the free Frankfurter API
    /// (api.frankfurter.dev/v2), which blends 84 central banks and includes LKR.
    /// Results are cached in memory for the calendar day, since the dashboard doesn't
    /// need a fresh fetch on every single page load, and a temporary API outage
    /// shouldn't affect users after the first successful fetch that day.
    /// </summary>
    public static class CurrencyExchangeHelper
    {
        private static CurrencyRatesResult _cachedResult;
        private static DateTime _cachedOn = DateTime.MinValue;
        private static readonly object _lock = new object();

        public static CurrencyRatesResult GetLkrRates()
        {
            lock (_lock)
            {
                // Reuse today's cached rates if we already fetched successfully
                if (_cachedResult != null && _cachedResult.Success && _cachedOn.Date == DateTime.Now.Date)
                {
                    return _cachedResult;
                }

                var result = FetchFromApi();

                // Only cache SUCCESSFUL results, so a failed attempt gets retried
                // on the next page load instead of being stuck failing all day.
                if (result.Success)
                {
                    _cachedResult = result;
                    _cachedOn = DateTime.Now;
                }

                return result;
            }
        }

        private static CurrencyRatesResult FetchFromApi()
        {
            var result = new CurrencyRatesResult { Success = false };

            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                string apiUrl = ConfigurationManager.AppSettings["FrankfurterApiUrl"];
                if (string.IsNullOrWhiteSpace(apiUrl))
                {
                    // Fallback in case the appSetting is ever missing
                    apiUrl = "https://api.frankfurter.dev/v2/rates?base=LKR&quotes=USD,AUD,NZD";
                }

                string json = DownloadWithTimeout(apiUrl, 5000);

                var rows = JsonConvert.DeserializeObject<List<CurrencyRateRow>>(json);

                if (rows != null && rows.Count > 0)
                {
                    foreach (var row in rows)
                    {
                        result.Rates[row.Quote] = row.Rate;
                    }
                    result.AsOfDate = rows[0].Date;

                    // Only mark success if we actually got all 3 currencies we need
                    result.Success = result.Rates.ContainsKey("USD")
                                   && result.Rates.ContainsKey("AUD")
                                   && result.Rates.ContainsKey("NZD");
                }
            }
            catch
            {
                // Network issue, API downtime, DNS failure, etc.
                // This is an enhancement feature - it must never break the Dashboard,
                // so swallow the error and simply report failure.
                result.Success = false;
            }

            return result;
        }

        private static string DownloadWithTimeout(string url, int timeoutMs)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Accept = "application/json";
            request.Timeout = timeoutMs;               
            request.ReadWriteTimeout = timeoutMs;

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}