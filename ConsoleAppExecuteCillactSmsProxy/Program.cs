using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppExecuteCillactSmsProxy
{
    //class Program
    //{
    //    static void Main(string[] args)
    //    {
    //    }
        internal class Program
        {
            static async Task Main(string[] args)
            {
                Console.WriteLine("Starting IIS XML sender...");

                // ---- כאן להכניס את ה-URL של השרת IIS שלך ----
                string url = "http://cellactsms:9090/api/sms";

                // ---- כאן מכינים XML לדוגמה ----
                string xml = @"
<PALO>
    <HEAD>
        <FROM>gilcar</FROM> 
        <APP USER='gilcar' PASSWORD='Q3qmkC8c'>LA</APP> 
        <CMD>sendtextmt</CMD> 
        <TTL>180</TTL>
    </HEAD> 
    <BODY> 
        <SENDER>Gil Car</SENDER>
        <CONTENT>Hello from console app</CONTENT> 
        <DEST_LIST>
            <TO>0501234567</TO> 
        </DEST_LIST> 
    </BODY> 
</PALO>";

                try
                {
                    using (var client = new HttpClient())
                    {
                        var content = new StringContent(xml, Encoding.UTF8, "application/xml");

                        Console.WriteLine("Sending request...");
                        var response = await client.PostAsync(url, content);

                        string respText = await response.Content.ReadAsStringAsync();

                        Console.WriteLine("=== RESPONSE ===");
                        Console.WriteLine(respText);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR:");
                    Console.WriteLine(ex);
                }

                Console.WriteLine("Done.");
                Console.ReadLine();
            }
        }
    }
