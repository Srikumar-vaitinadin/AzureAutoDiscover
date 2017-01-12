using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.IO;

// Azure Management dependencies
using Microsoft.Rest.Azure.Authentication;
//using Microsoft.Azure.Management.ResourceManager;
//using Microsoft.Azure.Management.ResourceManager.Models;
//using Microsoft.Azure.Management.Network.Models;


//Fluent libraries
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.Compute.Fluent.Models;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Resource.Fluent;
using Microsoft.Azure.Management.Resource.Fluent.Authentication;
using Microsoft.Azure.Management.Resource.Fluent.Core;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.Network.Fluent.Models;
using AzureAutoDiscover;
//using Microsoft.Azure.Management.Samples.Common;

namespace AzureAutoDiscover
{
    public class MainClass
    {

        public static void Main(string[] args)
        {
            AzureContext ac = new AzureContext();
            ac.SetContext();
            ac.FindVNETVMMapping();
            Console.WriteLine("---------------- End of Execution ----------------------");
            Console.ReadLine();
        }

      
    }
}
