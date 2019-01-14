using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DocDbRepo.Sample
{
    class Person
    {
        [JsonProperty(PropertyName = "id")]
        public string FullName => $"{FirstName} {LastName}";

        [JsonProperty(PropertyName = "first")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "last")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "birthday")]
        public DateTime Birthday { get; set; }

        [JsonProperty(PropertyName = "phoneNumbers")]
        public Collection<PhoneNumber> PhoneNumbers { get; set; }

        [JsonProperty(PropertyName = "allThings")]
        public Dictionary<string, string> AllThings { get; set; }

        [JsonConverter(typeof(EpochConverter))]
        [JsonProperty(PropertyName = "_ts")]
        public DateTime Modified { get; set; }

        [JsonProperty(PropertyName = "_etag")]
        public string ETag { get; set; }

        public override string ToString()
        {
            var phones = PhoneNumbers.Any() ? string.Join(", ", PhoneNumbers.Select(p => p.ToString())) : "-";
            return string.Format($"{FirstName} {LastName}, Birthday {Birthday:MM-dd-yyyy} Phone numbers: {phones}");
        }
    }
}
