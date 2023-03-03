using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;

namespace LesegaisParcer
{
    internal class Program
    {
        private static DatabaseAccess _dbAccess;
        private static HttpClient _httpClient;
        
        private static readonly Uri _uri = new Uri("https://www.lesegais.ru/open-area/graphql");

        private static System.Timers.Timer _timer;
        private static readonly int _delay = (int)TimeSpan.FromMinutes(10).TotalMilliseconds;
        private static readonly int _delayBetweenHttpRequests = (int)TimeSpan.FromSeconds(10).TotalMilliseconds;
        private static bool _fetchAndProcessRunning = false;
        
        private static readonly SearchReportWoodDealQuery _query = new SearchReportWoodDealQuery();
        private static readonly int _querySize = 1000;

        static void Main(string[] args)
        {
            Console.WriteLine("\nPress the Enter key to exit the application...\n");
            Console.WriteLine($"The application started at {DateTime.Now}");

            try
            {
                SetDatabaseAccess();
                SetHttpClient();

                SetTimer();
                StartFetchAndProcessData();

                Console.ReadLine();
            }
            catch
            {
                Console.WriteLine("Something gone wrong. Application will be terminated");
            }
            finally
            {
                OnExit();
            }
        }

        private static void SetDatabaseAccess()
        {
            _dbAccess = new DatabaseAccess();
            _dbAccess.EnsureTableExists();
        }

        private static void SetHttpClient()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "*");
        }

        private static void SetTimer()
        {
            _timer = new System.Timers.Timer(_delay);
            _timer.Elapsed += StartFetchAndProcessData;
            _timer.AutoReset = true;
            _timer.Start();
        }

        private static void StartFetchAndProcessData(object source = null, ElapsedEventArgs e = null)
        {
            if (_fetchAndProcessRunning) return;
            Task.Run(() => FetchAndProcessData());
        }

        private static void FetchAndProcessData()
        {
            _fetchAndProcessRunning = true;
            Console.WriteLine("\nFetch and process");
            Console.WriteLine($"\tstarted: {DateTime.Now}");

            int queryNumber = 0;
            int totalFetched = 0;

            while (true)
            {
                _query.SetVariables(_querySize, queryNumber);
                queryNumber++;

                IEnumerable<Deal> data = Enumerable.Empty<Deal>();
                try
                {
                    data = FetchData(JsonSerializer.Serialize(_query)).Result;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\tcannot fetch data: {ex.Message}");
                    break;
                }
                
                if (!data.Any()) break;

                totalFetched += data.Count();

                Console.WriteLine($"\n\tfetched: {data.Count()}, total fetched: {totalFetched}");

                _dbAccess.ProcessData(data);

                Console.WriteLine($"\tprocessed");

                Task.Delay(_delayBetweenHttpRequests).Wait();
            }
            
            _fetchAndProcessRunning = false;
            Console.WriteLine($"\n\tcompleted: {DateTime.Now}");
        }

        private static async Task<IEnumerable<Deal>> FetchData(string query)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, _uri)
            {
                Content = new StringContent(query)
            };

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            string res = await response.Content.ReadAsStringAsync();

            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                var result = await JsonSerializer.DeserializeAsync<SearchReportWoodDealQueryResponse>(stream);
                return result.Data.SearchReportWoodDeal.Content;
            }
        }

        private static void OnExit()
        {
            _timer?.Stop();
            _timer?.Dispose();

            _httpClient?.Dispose();
        }
    }
}
