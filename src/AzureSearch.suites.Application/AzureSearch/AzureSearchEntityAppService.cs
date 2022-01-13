﻿using Abp.Application.Services;
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
    public class AzureSearchEntityAppService : IApplicationService
    {
        private IConfiguration _configuration { get; set; }
        private DocumentSearchClient _docSearch { get; set; }
        private string _configurationError { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public AzureSearchEntityAppService(IConfiguration configuration)
        {
            _configuration = configuration;
            InitializeDocSearch();
        }

        private void InitializeDocSearch()
        {
            try
            {
                _docSearch = new DocumentSearchClient(_configuration, _configuration.GetSection("EntitySearchIndexName")?.Value);
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
        public SearchResultViewModel SearchEntity(string q, string facets = "", int page = 1,bool isProperty=false)
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

            var viewModel = SearchViewEntity(new SearchOptionsEntity
            {
                q = q,
                searchFacets = searchFacets,
                currentPage = page,
                isProperty = isProperty
            });
          
           
            return viewModel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchParams"></param>
        /// <returns></returns>
        public SearchResultViewModel SearchViewEntity(SearchOptionsEntity searchParams)
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
                documentResult = _docSearch.GetDocuments(searchParams.q,0, searchParams.searchFacets, searchParams.currentPage, searchParams.polygonString,searchParams.isProperty),
                query = searchParams.q,
                selectedFacets = searchParams.searchFacets,
                currentPage = searchParams.currentPage,
                searchId = searchidId ?? null,
                applicationInstrumentationKey = _configuration.GetSection("InstrumentationKey")?.Value,
                searchServiceName = _configuration.GetSection("SearchServiceName")?.Value,
                indexName = _configuration.GetSection("EntitySearchIndexName")?.Value,
                facetableFields = _docSearch.Model.Facets.Select(k => k.Name).ToArray()
            };            
            var details = from o in viewModel.documentResult.Results
                          select new EntityDetailsDto()
                          {
                              entity_key = o.Document.Where(s => s.Key == "entity_key").Select(s =>Convert.ToInt32(s.Value)).FirstOrDefault(),
                              
                              name = o.Document.Where(s => s.Key == "name").Select(s => s.Value).FirstOrDefault() == null ? ""
                                        : o.Document.Where(s => s.Key == "name").Select(s => s.Value).FirstOrDefault().ToString(),


                              address_line_1 = o.Document.Where(s=>s.Key== "address_line_1").Select(s=>s.Value).FirstOrDefault() == null ? ""
                                        : o.Document.Where(s => s.Key == "address_line_1").Select(s => s.Value).FirstOrDefault().ToString(),


                              address_line_2 = o.Document.Where(s => s.Key == "address_line_2").Select(s => s.Value).FirstOrDefault() == null ? ""
                                        : o.Document.Where(s => s.Key == "address_line_2").Select(s => s.Value).FirstOrDefault().ToString(),

                              first_name = o.Document.Where(s => s.Key == "first_name").Select(s => s.Value).FirstOrDefault() == null ? ""
                                        : o.Document.Where(s => s.Key == "first_name").Select(s => s.Value).FirstOrDefault().ToString(),

                              middle_name = o.Document.Where(s => s.Key == "middle_name").Select(s => s.Value).FirstOrDefault() == null ? ""
                                        : o.Document.Where(s => s.Key == "middle_name").Select(s => s.Value).FirstOrDefault().ToString(),

                              last_name = o.Document.Where(s => s.Key == "last_name").Select(s => s.Value).FirstOrDefault() == null ? ""
                                        : o.Document.Where(s => s.Key == "last_name").Select(s => s.Value).FirstOrDefault().ToString(),

                              area_code = o.Document.Where(s => s.Key == "area_code").Select(s => s.Value).FirstOrDefault() == null ? ""
                                        : o.Document.Where(s => s.Key == "area_code").Select(s => s.Value).FirstOrDefault().ToString(),


                              city = o.Document.Where(s => s.Key == "city").Select(s => s.Value).FirstOrDefault() == null ? ""
                                        : o.Document.Where(s => s.Key == "city").Select(s => s.Value).FirstOrDefault().ToString(),


                              email_address = o.Document.Where(s => s.Key == "email_address").Select(s => s.Value).FirstOrDefault() == null ? ""
                                        : o.Document.Where(s => s.Key == "email_address").Select(s => s.Value).FirstOrDefault().ToString(),


                              Type = o.Document.Where(s => s.Key == "Type").Select(s => s.Value).FirstOrDefault() == null ? ""
                                        : o.Document.Where(s => s.Key == "Type").Select(s => s.Value).FirstOrDefault().ToString(),


                              Status = o.Document.Where(s => s.Key == "Status").Select(s => s.Value).FirstOrDefault() == null ? ""
                                        : o.Document.Where(s => s.Key == "Status").Select(s => s.Value).FirstOrDefault().ToString(),


                              phone_number = o.Document.Where(s => s.Key == "phone_number").Select(s => s.Value).FirstOrDefault() == null ? ""
                                        : o.Document.Where(s => s.Key == "phone_number").Select(s => s.Value).FirstOrDefault().ToString(),


                              postal_code = o.Document.Where(s => s.Key == "postal_code").Select(s => s.Value).FirstOrDefault() == null ? ""
                                        : o.Document.Where(s => s.Key == "postal_code").Select(s => s.Value).FirstOrDefault().ToString(),


                              phone_extension = o.Document.Where(s => s.Key == "phone_extension").Select(s => s.Value).FirstOrDefault() == null ? ""
                                        : o.Document.Where(s => s.Key == "phone_extension").Select(s => s.Value).FirstOrDefault().ToString(),
                              
                              
                              state_province= o.Document.Where(s => s.Key == "state_province").Select(s => s.Value).FirstOrDefault()==null ? "" 
                                             : o.Document.Where(s => s.Key == "state_province").Select(s => s.Value).FirstOrDefault().ToString()
                          };
            viewModel.EntityDetails = details.ToArray().GroupBy(s=>s.entity_key).Select(s=>s.First()).ToList();

            return viewModel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="term"></param>
        /// <param name="fuzzy"></param>
        /// <returns></returns>
        public List<string> SuggestEntity(string term, bool fuzzy = true)
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

            return uniqueItems;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DocumentResult GetEntityDocumentById(string id = "")
        {
            var result = _docSearch.GetDocumentById(id);

            return result;
        }

        public class SearchOptionsEntity
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

            public bool isProperty { get; set; }
        }
    }
}