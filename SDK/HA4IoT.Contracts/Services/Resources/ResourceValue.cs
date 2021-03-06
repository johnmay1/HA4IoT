﻿using Newtonsoft.Json;

namespace HA4IoT.Contracts.Services.Resources
{
    public class ResourceValue
    {
        [JsonRequired]
        public string LanguageCode { get; set; }

        [JsonRequired]
        public string Value { get; set; }
    }
}
