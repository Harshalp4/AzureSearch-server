using Abp.Application.Services;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PhoneticAzureSearch.Model;
//using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace AzureSearch.suites.AzureSearch
{
    /// <summary>
    /// 
    /// </summary>
    public class AzureSearchPhoneticAppService : IApplicationService
    {
        private IConfiguration _configuration { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public AzureSearchPhoneticAppService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        //public DocumentSearchResult<IndexProduct> Search(string q)
        //{
        //}

        public DocumentSearchResult<IndexProduct> SearchPhrase(string phrase, bool lucene = true)
        {
            //Console.WriteLine("Phrase: {0}", phrase);
            //Console.WriteLine("Lucene Syntax: {0}", lucene ? "Yes" : "No");
            //Console.WriteLine("Columns: {0}", string.Join(',', columns));

            //string[] columns = { "name", "namePhonetic" };
            string[] columns = { "name, namePhonetic" };

            var indexClient = CreateIndexAndGetClient();

            var data = indexClient.Documents.Search<IndexProduct>(phrase, new SearchParameters()
            {
                //SearchFields = new List<string>(columns),
                //SearchMode = SearchMode.Any,
                IncludeTotalResultCount = true,
                //QueryType = lucene ? QueryType.Full : QueryType.Simple, //For Lucene Syntax,
                Top = 1000
            });

            return data;
        }
        private ISearchIndexClient CreateIndexAndGetClient()
        {
            var searchServiceName = _configuration.GetSection("SearchServiceName")?.Value;
            var apiKey = _configuration.GetSection("SearchApiKey")?.Value;

            var serviceClient = new SearchServiceClient(searchServiceName, new SearchCredentials(apiKey));
            var index = CreateIndexDefinition();

            if (!serviceClient.Indexes.Exists(index.Name))
                serviceClient.Indexes.Create(index);

            var indexClient = serviceClient.Indexes.GetClient(index.Name);

            return indexClient;
        }
        private Index CreateIndexDefinition()
        {
            var phoneticAnalizer = CreatePhoneticCustomAnalyzer();

            var definition = new Index()
            {
                Name = _configuration.GetSection("EntitySearchIndexName")?.Value, // Config.GetValue<string>("AzureSearch:AzureSearchIndexName"),
                Fields = FieldBuilder.BuildForType<IndexProduct>(),
                Analyzers = new List<Analyzer> { phoneticAnalizer },
            };

            return definition;
        }
        private CustomAnalyzer CreatePhoneticCustomAnalyzer()
        {
            var analyzer = new CustomAnalyzer
            {
                TokenFilters = new List<TokenFilterName> { TokenFilterName.Phonetic, TokenFilterName.AsciiFolding, TokenFilterName.Lowercase },
                Tokenizer = TokenizerName.Standard,
                Name = "PhoneticCustomAnalyzer"
            };

            return analyzer;
        }
    }
}

