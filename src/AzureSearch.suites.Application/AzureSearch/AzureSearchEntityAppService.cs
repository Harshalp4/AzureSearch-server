

using Abp.Application.Services;
using CognitiveSearch.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using CognitiveSearch.UI.Models;
using System.Data.SqlClient;
using System.Data;

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
                //SearchOptionsEntity searchParams = new SearchOptionsEntity();
               // _docSearch = new DocumentSearchClient(_configuration, _configuration.GetSection("EntitySearchIndexName")?.Value, searchParams.IsMongo );
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
 
        public SearchResultViewModel SearchEntity(string q, string facets = "", int page = 1, bool isProperty = false, bool isMongoIndex = false, bool globalIndex = false)
        {
            //_docSearch = new DocumentSearchClient(_configuration, _configuration.GetSection("EntitySearchIndexName")?.Value, isMongoIndex);
            string indexName = globalIndex ? _configuration.GetSection("GlobalSearchIndexName")?.Value
            : isMongoIndex ? _configuration.GetSection("MongoSearchIndexName")?.Value
            : _configuration.GetSection("EntitySearchIndexName")?.Value;

            //
            _docSearch = new DocumentSearchClient(_configuration, indexName);

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
                .Select(f => f.Split("__", StringSplitOptions.RemoveEmptyEntries))
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
                isProperty = isProperty,
                IsMongo = isMongoIndex,
                IsGlobal=globalIndex
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
            var indexName = "EntitySearchIndexName";
            if (CheckDocSearchInitialized())
                searchidId = _docSearch.GetSearchId().ToString();

            if (searchParams.IsMongo) 
            {
                indexName = _configuration.GetSection("MongoSearchIndexName")?.Value;
            }

            var viewModel = new SearchResultViewModel
            {
                documentResult = _docSearch.GetDocuments(searchParams.q,0, searchParams.searchFacets, searchParams.currentPage, searchParams.polygonString,searchParams.isProperty,searchParams.IsMongo),
                query = searchParams.q,
                selectedFacets = searchParams.searchFacets,
                currentPage = searchParams.currentPage,
                searchId = searchidId ?? null,
                applicationInstrumentationKey = _configuration.GetSection("InstrumentationKey")?.Value,
                searchServiceName = _configuration.GetSection("SearchServiceName")?.Value,
                indexName = indexName,
                facetableFields = _docSearch.Model.Facets.Select(k => k.Name).ToArray()
            };     
            if(searchParams.IsMongo)
            {
                var details = GetCasesDetails(viewModel);
                viewModel.EntityDetails = details.ToArray().GroupBy(s => s.entity_key).Select(s => s.First()).ToList();
            }
            else if(searchParams.IsGlobal)
            {
                var details = GetEntityDetails(viewModel);
                viewModel.EntityDetails = details.ToArray().GroupBy(s => s.case_id).Select(s => s.First()).ToList();
            }
            else
            {
                var details = GetEntityDetails(viewModel);
                viewModel.EntityDetails = details.ToArray().GroupBy(s => s.entity_key).Select(s => s.First()).ToList();
            } 

            
            return viewModel;
        }

        private IEnumerable<EntityDetailsDto> GetCasesDetails(SearchResultViewModel viewModel)
        {
            var details = from o in viewModel.documentResult.Results
                          select new EntityDetailsDto()
                          {
                              case_id = o.Document.Where(s => s.Key == "CASE_ID").Select(s => Convert.ToInt32(s.Value)).FirstOrDefault(),



                              case_type = o.Document.Where(s => s.Key == "CASE_TYPE").Select(s => s.Value).FirstOrDefault() == null ? ""
                                        : o.Document.Where(s => s.Key == "CASE_TYPE").Select(s => s.Value).FirstOrDefault().ToString(),


                              case_status = o.Document.Where(s => s.Key == "CASE_STATUS").Select(s => s.Value).FirstOrDefault() == null ? ""
                                        : o.Document.Where(s => s.Key == "CASE_STATUS").Select(s => s.Value).FirstOrDefault().ToString(),

                              creation_reason = o.Document.Where(s => s.Key == "CREATION_REASON").Select(s => s.Value).FirstOrDefault() == null ? ""
                                        : o.Document.Where(s => s.Key == "CREATION_REASON").Select(s => s.Value).FirstOrDefault().ToString(),

                              source = o.Document.Where(s => s.Key == "source").Select(s => s.Value).FirstOrDefault() == null ? ""
                                        : o.Document.Where(s => s.Key == "source").Select(s => s.Value).FirstOrDefault().ToString(),

                          };

            return details;
        }

        private IEnumerable<EntityDetailsDto> GetEntityDetails(SearchResultViewModel viewModel) 
        {
            var details = from o in viewModel.documentResult.Results
                          select new EntityDetailsDto()
                          {
                              entity_key = o.Document.Where(s => s.Key == "entity_key").Select(s => Convert.ToInt32(s.Value)).FirstOrDefault(),

                              name = o.Document.Where(s => s.Key == "name").Select(s => s.Value).FirstOrDefault() == null ? ""
                                        : o.Document.Where(s => s.Key == "name").Select(s => s.Value).FirstOrDefault().ToString(),


                              address_line_1 = o.Document.Where(s => s.Key == "address_line_1").Select(s => s.Value).FirstOrDefault() == null ? ""
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


                              state_province = o.Document.Where(s => s.Key == "state_province").Select(s => s.Value).FirstOrDefault() == null ? ""
                                             : o.Document.Where(s => s.Key == "state_province").Select(s => s.Value).FirstOrDefault().ToString(),

                              case_id = o.Document.Where(s => s.Key == "CASE_ID").Select(s => Convert.ToInt32(s.Value)).FirstOrDefault(),

                              case_type = o.Document.Where(s => s.Key == "CASE_TYPE").Select(s => s.Value).FirstOrDefault() == null ? ""
                                        : o.Document.Where(s => s.Key == "CASE_TYPE").Select(s => s.Value).FirstOrDefault().ToString(),


                              case_status = o.Document.Where(s => s.Key == "CASE_STATUS").Select(s => s.Value).FirstOrDefault() == null ? ""
                                        : o.Document.Where(s => s.Key == "CASE_STATUS").Select(s => s.Value).FirstOrDefault().ToString(),

                              creation_reason = o.Document.Where(s => s.Key == "CREATION_REASON").Select(s => s.Value).FirstOrDefault() == null ? ""
                                        : o.Document.Where(s => s.Key == "CREATION_REASON").Select(s => s.Value).FirstOrDefault().ToString(),

                              source = o.Document.Where(s => s.Key == "Source").Select(s => s.Value).FirstOrDefault() == null ? ""
                                        : o.Document.Where(s => s.Key == "Source").Select(s => s.Value).FirstOrDefault().ToString(),

                          };

            return details;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="term"></param>
        /// <param name="fuzzy"></param>
        /// <returns></returns>
        public List<string> SuggestEntity(string term, bool fuzzy = true,bool isglobal=false)
        {

            string indexName = isglobal==false ? _configuration.GetSection("EntitySearchIndexName")?.Value
            : _configuration.GetSection("GlobalSearchIndexName")?.Value;
            _docSearch = new DocumentSearchClient(_configuration, indexName);

            // Change to _docSearch.Suggest if you would prefer to have suggestions instead of auto-completion
            //var response = _docSearch.Autocomplete(term, fuzzy);
            var response = _docSearch.Suggest(term, fuzzy);
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

            public bool IsMongo { get; set; }

            public bool IsGlobal { get; set; }
        }

        public async Task<List<EntityDetailsDto>> GetDetailsByentityKey(int entitykey)
        
        {
            var connstring = _configuration.GetConnectionString("Default");
            DataTable dt = new DataTable();

            using (SqlConnection connection = new SqlConnection(connstring))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("getentitydetails", connection);
                command.CommandType = CommandType.StoredProcedure;
                SqlParameter param = new SqlParameter();
                param.ParameterName = "@entity_key";
                param.Value = entitykey;
                command.Parameters.Add(param);
                //rowsAffected = command.ExecuteNonQuery();
                DataTable dt2 = new DataTable();
                SqlDataAdapter adpter = new SqlDataAdapter(command);
                adpter.Fill(dt);

            }
            var myEnumerable = dt.AsEnumerable();

            var details = from o in myEnumerable
                          select new EntityDetailsDto()
                          {
                              name = o.Field<string>("name"),
                              entity_key = o.Field<int>("entity_key"),
                              address_line_1 = o.Field<string>("address_line_1"),
                              address_line_2 = o.Field<string>("address_line_2"),
                              area_code = o.Field<string>("area_code"),
                              city = o.Field<string>("city"),
                              email_address = o.Field<string>("email_address"),
                              first_name = o.Field<string>("first_name"),
                              last_name = o.Field<string>("last_name"),
                              middle_name = o.Field<string>("middle_name"),
                              phone_extension = o.Field<string>("phone_extension"),
                              phone_number = o.Field<string>("phone_number"),
                              postal_code = o.Field<string>("postal_code"),
                              state_province = o.Field<string>("state_province"),
                              Status = o.Field<string>("Status"),
                              Type = o.Field<string>("Type")
                          };
            return details.ToList();

        }
    }
}
