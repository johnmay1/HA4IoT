﻿using System.Xml.Linq;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Contracts.Configuration
{
    public interface IConfigurationExtender
    {
        string Namespace { get; }
        
        IDevice ParseDevice(XElement element);

        IBinaryOutput ParseBinaryOutput(XElement element);

        IBinaryInput ParseBinaryInput(XElement element);
    }
}
