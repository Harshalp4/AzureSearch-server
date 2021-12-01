using Abp.Application.Services;
using AspNetCoreAzureSearch;
using Azure.Search.Documents.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureSearch.suites.PersonSearch
{
    public class AzuresearchProvideAppService : IApplicationService
    {
        private readonly SearchProviderAutoComplete _searchProviderAutoComplete;
        public string SearchText { get; set; }
        public SuggestResults<PersonCity> PersonCities;
        public AzuresearchProvideAppService(SearchProviderAutoComplete searchProviderAutoComplete)
        {
            _searchProviderAutoComplete = searchProviderAutoComplete;
        }

        public async Task<SuggestResults<PersonCity>> GetAll(string term)
        {
           
                PersonCities = await _searchProviderAutoComplete.Suggest(true, true, term);
              SearchText = term;

                return PersonCities;
            
    }
    }
}
