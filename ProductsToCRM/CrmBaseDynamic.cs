using System;
using System.Net;
using System.ServiceModel.Description;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System.Configuration;
using Microsoft.Xrm.Tooling.Connector;
using Microsoft.Crm.Sdk.Messages;

namespace IntoIT.GilCar.Interfaces.ProductsToCRM
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
                    //ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; ;// SecurityProtocolType.Tls12;
                    //service = new CrmServiceClient(ConfigurationManager.ConnectionStrings["CRM_Online"].ConnectionString);
                    //return service;
                    CrmServiceClient crmConn = new CrmServiceClient(ConfigurationManager.ConnectionStrings["CRM_Online"].ConnectionString);
                    var svcContext = new XrmSvc(crmConn);

                    WhoAmIRequest request = new WhoAmIRequest();
                    WhoAmIResponse response = (WhoAmIResponse)
                    crmConn.Execute(request);

                    return crmConn;

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
