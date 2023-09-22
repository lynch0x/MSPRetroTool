using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using Newtonsoft.Json;
using System.Net;
using Retro.Client;
using System.Drawing;
using System.Threading;

namespace Retro
{
   
    static class Program
    {
        static string ticket;
        static object locaid;
        static void Main(string[] args)
        {
            Console.Title = "MSP retro picture changer";
            SOAPBuilder b = new SOAPBuilder("Login");
            Console.Write("Username: ");
            b.AddBodyItem("username", Console.ReadLine());
            Console.Write("Password: ");
            b.AddBodyItem("password", Console.ReadLine());
            Console.WriteLine(b.BuildStringForChecksum());
            string loginxml = SendSOAP(b);
            ticket = loginxml.GetContent("Ticket");
            if (ticket == null)
            {
                Console.WriteLine("Request failed or invalid credentials!");
                Console.ReadLine();
                Environment.Exit(0);
            }
            locaid = loginxml.GetContent("ActorId");
            Console.WriteLine();
            Console.WriteLine("Logged in!");
          

            Console.Write("Image url: ");

            b = new SOAPBuilder("SaveEntitySnapshot");
            b.AddHeader("TicketHeader", new SOAPObject("Ticket",ticket));
            b.AddBodyItem("EntityType", "moviestar");
            b.AddBodyItem("entityId", locaid);
            using (WebClient c = new WebClient())
                b.AddBodyItem("data", Convert.ToBase64String(ResizeImage(c.DownloadData(Console.ReadLine()))));
            bool boolean = SendSOAP(b) == null;
            Console.WriteLine("SUCCESS: " + !boolean);
            Console.ReadLine();
        }
        static void SendMailTo(object aid)
        {
            var b = new SOAPBuilder("CreateMail");
            b.AddHeader("TicketHeader", new SOAPObject("Ticket",ticket));
            b.AddBodyItem("mail", new Dictionary<string, object>
            {
                {"ToActorId",aid },
                {"Subject","" },
                {"Message","" },
                {"wDate","2000-01-31T23:00:00Z" },
                {"Status",0 }
            });
            Console.WriteLine(b.BuildStringForChecksum());
            SendSOAP(b);
        }
        static void CreateMovie()
        {
            string emptyBytes = Convert.ToBase64String(new byte[1]);
            var request = new SOAPBuilder("SaveMovie");
            request.AddHeader("TicketHeader", new SOAPObject("Ticket", ticket));
            request.AddBodyItem(new SOAPObject("movie", new Dictionary<string, object>
            {
                //{"_",new SOAPObject("MovieActorRels",new Dictionary<string, object>
                //{
                //    {"a",new SOAPObject("MovieActorRel",new Dictionary<string, object>
                //    {
                //        {"ActorId",locaid }
                //    }) },
                //    {"b",new SOAPObject("MovieActorRel",new Dictionary<string, object>
                //    {
                //        {"ActorId",locaid }
                //    }) }
                //}) },
                {"Name","mname" },
                {"ActorId",locaid },
                {"Guid",Guid.NewGuid().ToString().Replace("-","").ToUpper() },
                {"MovieData", emptyBytes},
                {"Complexity",8 },
                {"ActorClothesData",emptyBytes }
                
            }));
            Console.WriteLine(request.BuildSOAP());
            Console.WriteLine(request.BuildStringForChecksum());
            SendSOAP(request);
        }
        static byte[] ResizeImage(byte[] original)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (MemoryStream memoryStream = new MemoryStream(original))
                {
                    using (var img = Image.FromStream(memoryStream))
                    using (Bitmap map = new Bitmap(img, 100, 100))
                    {
                        map.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                }
                return stream.ToArray();
            }
        }

        static string SendSOAP(SOAPBuilder data)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.mspretro.com/Service");
            request.Method = "POST";
            request.ContentType = "text/xml; charset=utf-8";
            request.Headers["checksum-client"] = data.BuildStringForChecksum().createSHA1();
            request.Headers["soapaction"] = "http://moviestarplanet.com/" + data.Action;

            try
            {
                using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
                    writer.Write(data.BuildSOAP());
                WebResponse response = request.GetResponse();
                string xml = "";
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    xml = reader.ReadToEnd();
                return xml;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
