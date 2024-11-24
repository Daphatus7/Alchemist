using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Script.Utilities.ServiceLocator
{
    [DefaultExecutionOrder(-100)]
    public class ServiceLocator : ServiceLocatorSingleton<ServiceLocator, IGameService> 
    {
        private readonly Dictionary<string, List<IGameService>> services = new ();
    }
}