using System;
using System.Net;
using System.ServiceModel.Description;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System.Configuration;
using Microsoft.Xrm.Tooling.Connector;
using Microsoft.Crm.Sdk.Messages;

namespace IntoIT.GilCar.Interfaces.CustomersCarInstallations
{
    public class CrmBase
    {
        private string crmUri;
        private CrmServiceClient service;

        // Variable of active service (for reinitialize service)


        /// <summary>
        ///  Create CRM Server URI
        /// </summary>
        /// 
        public CrmBase() : this(null, null) { }

        /// <summary>
        ///  Create CRM Server URI
        /// </summary>
        /// <param name="crmServerName"> CRM Server Name (optional), for example: crmorg </param>
        /// <param name="crmOrganizationName"> CRM Organization Name (optional), for example: MaccabiHealthcareServices </param>
        public CrmBase(string crmServerName, string crmOrganizationName)
        {
            if (String.IsNullOrWhiteSpace(crmServerName))
                crmServerName = GetSettings("XrmServerUrl");
            if (String.IsNullOrWhiteSpace(crmOrganizationName))
                crmOrganizationName = GetSettings("XrmOrganizationName");
            crmUri = String.Format("http://{0}/{1}/XRMServices/2011/Organization.svc", crmServerName, crmOrganizationName);
        }

        /// <summary>
        ///  Create CRM Server URI
        /// </summary>
        /// <param name="crmServerUri"> CRM Server URI, for example: http://crmorg/MaccabiHealthcareServices </param>
        public CrmBase(string crmServerUri)
        {


        }

        public CrmServiceClient XrmService
        {
            get
            {
                try
                {
                    // ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; // SecurityProtocolType.Tls12;
                    //ServicePointManager.SecurityProtocol =  SecurityProtocolType.Tls12;
                    //service = new CrmServiceClient(ConfigurationManager.ConnectionStrings["CRM_Online"].ConnectionString);
                    //return service;


                   // IOrganizationService orgService = GetOrganizationServiceClientSecret("2f4995e4-1ff6-41f2-975f-e9195d1f914c", "CKP8Q~gH-6W131LxUydsaMbqRztCzMo7xH15VasI", "https://gil-car.crm4.dynamics.com");

                    // var crmConn = new CrmServiceClient($@"AuthType=ClientSecret;url=https://gil-car.crm4.dynamics.com;ClientId=2f4995e4-1ff6-41f2-975f-e9195d1f914c;ClientSecret=kZu8Q~AN8o9KtmIh8f4zwvwKzRSkkR0TLUIAnbiU");

                    //IOrganizationService os =  crmConn.OrganizationWebProxyClient != null ? crmConn.OrganizationWebProxyClient : (IOrganizationService)crmConn.OrganizationServiceProxy;
                    //var svcContext = new XrmSvc(crmConn);

                    WhoAmIRequest request = new WhoAmIRequest();
                   //WhoAmIResponse response = (WhoAmIResponse)
                   // crmConn.Execute(request);

                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; ;// SecurityProtocolType.Tls12;
                    service = new CrmServiceClient(ConfigurationManager.ConnectionStrings["CRM_Online"].ConnectionString);
                    return service;


                   // return crmConn;

                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }
            }
        }


        public string GetSettings(string sKey)
        {
            string val = System.Configuration.ConfigurationManager.AppSettings.Get(sKey);
            if (val == null)
                val = String.Empty;
            return val;
        }

        public static IOrganizationService GetOrganizationServiceClientSecret(string clientId, string clientSecret, string organizationUri)
        {
            try
            {
                var conn = new CrmServiceClient($@"AuthType=ClientSecret;url={organizationUri};ClientId={clientId};ClientSecret={clientSecret}");

                return conn.OrganizationWebProxyClient != null ? conn.OrganizationWebProxyClient : (IOrganizationService)conn.OrganizationServiceProxy;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while connecting to CRM " + ex.Message);
                Console.ReadKey();
                return null;
            }
        }

    }

    internal class XrmSvc
    {
        private CrmServiceClient crmConn;

        public XrmSvc(CrmServiceClient crmConn)
        {
            this.crmConn = crmConn;
        }
    }

   

}
