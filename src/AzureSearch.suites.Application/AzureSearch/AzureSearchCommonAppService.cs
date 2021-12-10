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
        public AzureSearchCommonAppService(IConfiguration configuration)
        {
            _configuration = configuration;
            InitializeDocSearch();
        }

        private void InitializeDocSearch()
        {
            try
            {
                _docSearch = new DocumentSearchClient(_configuration);
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
        /// <param name="facets"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public SearchResultViewModel Search(string q, string facets = "",  int page = 1)
        {
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
                currentPage = page
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
                documentResult = _docSearch.GetDocuments(searchParams.q, searchParams.searchFacets, searchParams.currentPage, searchParams.polygonString),
                query = searchParams.q,
                selectedFacets = searchParams.searchFacets,
                currentPage = searchParams.currentPage,
                searchId = searchidId ?? null,
                applicationInstrumentationKey = _configuration.GetSection("InstrumentationKey")?.Value,
                searchServiceName = _configuration.GetSection("SearchServiceName")?.Value,
                indexName = _configuration.GetSection("SearchIndexName")?.Value,
                facetableFields = _docSearch.Model.Facets.Select(k => k.Name).ToArray()
            };
            return viewModel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="term"></param>
        /// <param name="fuzzy"></param>
        /// <returns></returns>
        public List<string> Suggest(string term, bool fuzzy = true)
        {
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
        }
    }
    
}
/// <summary>
/// 
/// </summary>
