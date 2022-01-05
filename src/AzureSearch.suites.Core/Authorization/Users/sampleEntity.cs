using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureSearch.suites.Authorization.Users
{
    [Table("sampleEntitys")]
    public  class sampleEntity : FullAuditedEntity<long>
    {
        public string name { get; set; }

        public string lastname { get; set; }


    }
}
