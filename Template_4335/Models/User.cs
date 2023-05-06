using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template_4335.Models
{
    internal class User
    {
        public int Id { get; set; }
        [JsonProperty("CodeClient")]
        public string CodeClient { get; set; }
        [JsonProperty("FullName")]
        public string Name { get; set; }
        [JsonProperty("E_mail")]
        public string Email { get; set; }
        [JsonProperty("Street")]
        public string Street { get; set; }
    }
}
