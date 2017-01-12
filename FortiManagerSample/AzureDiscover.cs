using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Azure Management dependencies
using Microsoft.Rest.Azure.Authentication;


//Fluent libraries
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.Compute.Fluent.Models;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Resource.Fluent;
using Microsoft.Azure.Management.Resource.Fluent.Authentication;
using Microsoft.Azure.Management.Resource.Fluent.Core;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.Network.Fluent.Models;

namespace AzureAutoDiscover
{
    public class AzureContext
    {
        string tenantId = "<Tenant ID goes here>";
        string clientId = "<Client ID goes here>";
        string secret = "<Secret goes here>";
        string subscriptionId = "<Subscription ID goes here>";
        static public AzureEnvironment azureEnv;
        static IAzure azure;
        public const string NETWORK_INTERFACE = "networkinterface";
        public const string IP_CONFIGURATIONS = "ipConfigurations";

        public void SetContext()
        {
            if (new List<string> { tenantId, clientId, secret, subscriptionId }.Any(i => String.IsNullOrEmpty(i)))
            {
                Console.WriteLine("Unable to connect to Azure - Check Tenant ID, Client ID, Secret & Subscription ID");
            }
            else
            {
                // Build the service credentials and Azure Resource Manager object thru Service Principal
                //NOTE: We can create service principal from a file stored in disk as well
                azureEnv = new AzureEnvironment();
                azureEnv.GraphEndpoint = "https://graph.windows.net/";
                azureEnv.ManagementEnpoint = "https://management.core.windows.net/";
                azureEnv.ResourceManagerEndpoint = "https://management.azure.com/";
                azureEnv.AuthenticationEndpoint = "https://login.windows.net/";


                var credentials = AzureCredentials.FromServicePrincipal(clientId, secret, tenantId, azureEnv);

                azure = Azure
                             .Configure()
                             .WithLogLevel(HttpLoggingDelegatingHandler.Level.BASIC)
                             .Authenticate(credentials)
                             .WithDefaultSubscription();
            }
        }

        public List<string> FindVNETVMMapping()
        {
            var tags = "";
            List<string> rgTags = new List<string>();

            Write("Listing resource groups:");
            foreach (var rg in azure.ResourceGroups.List())
            {
                tags = "";
                foreach (var vNet in azure.Networks.ListByGroup(rg.Name))
                {

                    if (vNet.Tags.Count > 0)
                        foreach (var tag in vNet.Tags)
                        {
                            tags += string.Format("{0}-{1}", tag.Key, tag.Value);
                        }
                    Console.WriteLine("RG Name : {0}, VNET Name: {1}, Tags: {2}", vNet.ResourceGroupName, vNet.Name, tags);
                    ListVNetResources(vNet.Id);
                }
            }

            return rgTags;

        }


        public void AddTagsToVirtualMachinesbyID(string vmID, string tagKey, string tagValue)
        {
            var vm = azure.VirtualMachines.GetById(vmID);
            vm.Tags.Add(tagKey, tagValue);
        }


        public IDictionary<string, string> GetVMTagsbyID(string vmID)
        {
            var vm = azure.VirtualMachines.GetById(vmID);
            return vm.Tags;
        }

        public void AddTagsToAllVirtualNetworks(string rgName, string tagKey, string tagValue)
        {
            foreach (var vNet in azure.Networks.ListByGroup(rgName))
            {
                AddTagsToVirtualNetworks(vNet.Id, tagKey, tagValue);
            }
        }

        public void AddTagsToVirtualNetworks(string id, string tagKey, string tagValue)
        {
            var vNet = azure.Networks.GetById(id);
            try
            {
                vNet.Tags.Add(tagKey, tagValue);
                //TO DO: Add log entry on successfull addition of tags
            }
            catch (Exception ex)
            {
                //TO DO: Add log entry for failed scenario.
            }


        }

        public List<string> ListVNetResources(string id)
        {
            var vNet = azure.Networks.GetById(id);
            List<string> nicsList = new List<string>();

            foreach (var subnet in vNet.Subnets)
            {
                ISubnet sub = subnet.Value;
                foreach (var ip in sub.Inner.IpConfigurations)
                {
                    if (ip.Id.ToLower().Contains(NETWORK_INTERFACE))
                    {
                        string nicID = ip.Id.Substring(0, ip.Id.LastIndexOf(IP_CONFIGURATIONS) - 1);
                        nicsList.Add(nicID);
                        Console.WriteLine("NIC ID   - {0} ", nicID);
                        var nic = azure.NetworkInterfaces.GetById(nicID);
                        Console.WriteLine("Attached to VM - {0}", nic.VirtualMachineId);
                    }
                }
            }

            return nicsList;
        }

        public string FetchVMIDfromNIC(string id)
        {
            var nic = azure.NetworkInterfaces.GetById(id);
            return nic.VirtualMachineId;
        }


        private static void Write(string format, params object[] items)
        {
            Console.WriteLine(String.Format(format, items));
        }

        private static bool IsNull(object obj)
        {
            return (obj == null);
        }

        private static void GetTagPolicy()
        {
            //TODO: Fetch policy information based on Tag names

        }

        private static void UpdateTagPolicy()
        {
            //TODO: Update policy information based on Tag names
        }

    }
}
