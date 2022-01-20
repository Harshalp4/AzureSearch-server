using Abp.Application.Services;
using CognitiveSearch.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using CognitiveSearch.UI.Models;

namespace AzureSearch.suites.AzureSearch
{

    /// <summary>
    /// 
    /// </summary>
    public class AzureSearchCommonAppService : IApplicationService
    {
        private IConfiguration _configuration { get; set; }
        private DocumentSearchClient _docSearch { get; set; }
        private string _configurationError { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// 
        string[] Indexnames = { "beidermorse-index", "caverphone1-index", "caverphone2-index", "doublemetaphone-index",
            "nysiis-index", "soundex-index","cologne-index"};

        public AzureSearchCommonAppService(IConfiguration configuration)
        {
            _configuration = configuration;
            //InitializeDocSearch("nysiis-index");
        }

        private void InitializeDocSearch(string indexname)
        {
            try
            {
                _docSearch = new DocumentSearchClient(_configuration, indexname);
            }
            catch (Exception e)
            {
                _configurationError = $"The application settings are possibly incorrect. The server responded with this message: " + e.Message.ToString();
            }
        }
        /// <summary>
        /// Checks that the search client was intiailized successfully.
        /// If not, it will add the error reason to the ViewBag alert.
        /// </summary>
        /// <returns>A value indicating whether the search client was initialized succesfully.</returns>
        public bool CheckDocSearchInitialized()
        {
            if (_docSearch == null)
            {
                //ViewBag.Style = "alert-warning";
                //ViewBag.Message = _configurationError;
                return false;
            }

            return true;
        }

        //public IActionResult Index()
        //{
        //    CheckDocSearchInitialized();

        //    return View();
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="q"></param>
        /// <param name="accuracylevel"></param>
        /// <param name="indexname"></param>
        /// <param name="facets"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public SearchResultViewModel Search(string q, string indexname, string accuracylevel, string facets = "", int page = 1)
        {
       // indexname=indexname.Trim();

            InitializeDocSearch("cosmosdb-index");
            if (facets == null)
                facets = "";
            if (q == null)
                q = "";
            // Split the facets.
            //  Expected format: &facets=key1_val1,key1_val2,key2_val1
            var searchFacets = facets
                // Split by individual keys
                .Split(",", StringSplitOptions.RemoveEmptyEntries)
                // Split key/values
                .Select(f => f.Split("_", StringSplitOptions.RemoveEmptyEntries))
                // Group by keys
                .GroupBy(f => f[0])
                // Select grouped key/values into SearchFacet array
                .Select(g => new SearchFacet { Key = g.Key, Value = g.Select(f => f[1]).ToArray() })
                .ToArray();

            var viewModel = SearchView(new SearchOptions
            {
                q = q,
                searchFacets = searchFacets,
                currentPage = page,
                accuracy= accuracylevel,
                indexname = indexname,
            });
            

            return viewModel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchParams"></param>
        /// <returns></returns>
        public SearchResultViewModel SearchView(SearchOptions searchParams)
        {
            int highValue = 100;
            int lowvalue = 0;
            if (!string.IsNullOrEmpty(searchParams.accuracy))
            {
                string [] accracyrange=searchParams.accuracy.Split(',');
                
                lowvalue =Convert.ToInt32(accracyrange[0]);
                highValue = Convert.ToInt32(accracyrange[1]);
            }

           


            if (searchParams.q == null)
                searchParams.q = "*";
            if (searchParams.searchFacets == null)
                searchParams.searchFacets = new SearchFacet[0];
            if (searchParams.currentPage == 0)
                searchParams.currentPage = 1;

            string searchidId = null;

            if (CheckDocSearchInitialized())
                searchidId = _docSearch.GetSearchId().ToString();

            var viewModel = new SearchResultViewModel
            {
                documentResult = _docSearch.GetDocuments(searchParams.q, 0,searchParams.searchFacets, searchParams.currentPage, searchParams.polygonString),
                query = searchParams.q,
                selectedFacets = searchParams.searchFacets,
                currentPage = searchParams.currentPage,
                searchId = searchidId ?? null,
                applicationInstrumentationKey = _configuration.GetSection("InstrumentationKey")?.Value,
                searchServiceName = _configuration.GetSection("SearchServiceName")?.Value,
                //indexName = !string.IsNullOrEmpty(searchParams.indexname) ? searchParams.indexname :_configuration.GetSection("SearchIndexName")?.Value,
                //indexName = _configuration.GetSection("SearchIndexName")?.Value,
                facetableFields = _docSearch.Model.Facets.Select(k => k.Name).ToArray()
            };


           

            var scores = viewModel.documentResult.Results.Select(s => s.Score).Distinct().ToList();
            var scrlist = percentile(scores, scores.Count());

            var findscores = scrlist.Where(s => s.percent >= lowvalue && s.percent <= highValue).Select(s => s.score);

            var filteredResult = viewModel.documentResult.Results.Where(w => findscores.Contains(w.Score.Value)).ToList();

            viewModel.documentResult.Resultslist = filteredResult;

            

            List<Documentlist> docs = (from o in filteredResult
                                       select new Documentlist()
                                       {
                                           name = o.Document.Where(s=>s.Key== "nameoriginal").Select(s=>s.Value).FirstOrDefault().ToString(),
                                           percent= scrlist.Where(s=>s.score==o.Score.Value).Select(s=>s.percent).FirstOrDefault(),
                                           score=o.Score.Value
                                       }).ToList();

            if (highValue == 100 && lowvalue==100)
            {
                viewModel.documentResult.DocumnetList = docs.Where(s => s.name.Replace(" ","").ToLower() == searchParams.q.Replace(" ","").ToLower()).ToList();
            }
            else
            {
                viewModel.documentResult.DocumnetList = docs;
            }
            // //foreach (var scr in scores)
            // //{
            // //    var scrorepercent = scrlist.Where(s => s.score == scr).Select(s => s.percent).FirstOrDefault(); 

            // //    viewModel.documentResult.Results.Where(w => w.Score == scr).ToList().ForEach(s => s.Score = scrorepercent);
            // //}
            // viewModel.documentResult.Results = filteredResult;

            return viewModel;
        }


        public List<scorePercentile> percentile(List<double?> arr, int n)
        {
            List<scorePercentile> scores = new List<scorePercentile>();
            int i, count;
            double percent;
            // Start of the loop that calculates percentile
            for (i = 0; i < n; i++)
            {
                count = 0;
                for (int j = 0; j < n; j++)
                {

                    // Comparing the marks of student i
                    // with all other students
                    if (arr[i] > arr[j])
                    {
                        count++;
                    }
                }
                percent = (count * 100) / (n - 1);

                scores.Add(new scorePercentile { percent = percent, score = arr[i].Value });



                //Console.Write("\nPercentile of Student "
                //+ (i + 1) + " = " + percent);
            }
            return scores.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="term"></param>
        /// <param name="fuzzy"></param>
        /// <returns></returns>
        public List<string> Suggest(string term, bool fuzzy = true)
        {
            InitializeDocSearch("itsname-suggester-index");
            // Change to _docSearch.Suggest if you would prefer to have suggestions instead of auto-completion
            var response = _docSearch.Autocomplete(term, fuzzy);

            List<string> suggestions = new List<string>();
            if (response != null)
            {
                foreach (var result in response.Results)
                {
                    suggestions.Add(result.Text);
                }
            }

            // Get unique items
            List<string> uniqueItems = suggestions.Distinct().ToList();

            return  uniqueItems;          

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DocumentResult GetDocumentById(string id = "")
        {
            var result = _docSearch.GetDocumentById(id);

            return result;
        }

        public class SearchOptions
        {
            public string q { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public SearchFacet[] searchFacets { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int currentPage { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string polygonString { get; set; }

            public string accuracy { get; set; }

            public string indexname { get; set; }
        }
    }
    
}
/// <summary>
/// 
/// </summary>
