using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace SmsProxyApi.Controllers
{
    public class SmsProxyController : ApiController
    {
        private static readonly HttpClient http = new HttpClient();

        /// <summary>
        /// מקבל XML כ-string מה-Body ושולח ל-CellAct
        /// </summary>
        [HttpPost]
        [Route("api/sms/send")]
        public async Task<IHttpActionResult> SendXml([FromBody] XmlRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.XmlString))
                return BadRequest("xmlString is empty");

            string url = "https://la.cellactpro.com/unistart5.asp";

            // Cellact מקבלים body בפורמט: xmlstring=<XML>
            var body = $"xmlstring={request.XmlString}";
            var content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");

            try
            {
                var response = await http.PostAsync(url, content);
                string result = await response.Content.ReadAsStringAsync();

                return Ok(result);
            }
            catch (HttpRequestException ex)
            {
                return InternalServerError(ex);
            }
        }
    }

    public class XmlRequest
    {
        public string XmlString { get; set; }
    }
}
