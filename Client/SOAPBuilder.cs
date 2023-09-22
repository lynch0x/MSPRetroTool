using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Retro.Client
{
    internal class SOAPBuilder
    {
        public readonly List<SOAPDataHolder> dataHolders = new List<SOAPDataHolder>();
        public readonly List<SOAPObject> headers = new List<SOAPObject>();
        public readonly List<SOAPObject> bodyValues = new List<SOAPObject>();
        public string Action { get; private set; }
        public SOAPBuilder(string action)
        {
            Action = action;
        }
        public void AddHeader(string headerName, params SOAPObject[] values)
        {
            SOAPObject header = new SOAPObject(headerName);
            header.Value = new Dictionary<string, object>();
            foreach (var item in values)
            {
                ((Dictionary<string, object>)header.Value).Add(item.Name, item.Value);
            }
            headers.Add(header);
        }
   

        public void AddBodyItem(string name,object value)
        {
            AddBodyItem(new SOAPObject(name, value));
        }
        public void AddBodyItem(SOAPObject o)
        {
            bodyValues.Add(o);
        }
       //public void AddObject(string name,Dictionary<string,object>values,bool isHeader = false)
       // {
       //     SOAPDataHolder dataHolder = new SOAPDataHolder();
       //     dataHolder.isHeader = isHeader;
       //     dataHolder.Name = name;
       //     dataHolder.Value = values;
       //     dataHolder.isObject = true;
       //     dataHolders.Add(dataHolder);
       // }
       // public void AddItem(string name, object value)
       // {
       //     SOAPDataHolder dataHolder = new SOAPDataHolder();
       //     dataHolder.Name = name;
       //     dataHolder.Value = value;
       //     dataHolders.Add(dataHolder);
       // }
        public string BuildSOAP()
        {
            string soap = "<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:s=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">";
            if (headers.Count > 0)
            {
                soap += "<SOAP-ENV:Header>";
                foreach (var item in headers)
                {
                    soap += $"<tns:{item.Name} xmlns:tns=\"http://moviestarplanet.com/\">";
                    var dict = (Dictionary<string, object>)item.Value;
                    foreach (var item2 in dict)
                    {
                        soap += $"<tns:{item2.Key}>{item2.Value}</tns:{item2.Key}>";
                    }
                    soap += $"</tns:{item.Name}>";
                }
                soap += "</SOAP-ENV:Header>";
            }
            soap += $"<SOAP-ENV:Body><tns:{Action} xmlns:tns=\"http://moviestarplanet.com/\">";
            foreach (var item in bodyValues)
            {
                FromSOAPObject(ref soap, item);
            }
            soap += $"</tns:{Action}></SOAP-ENV:Body></SOAP-ENV:Envelope>";
            return soap;
        }
       
        public void FromSOAPObject(ref string soap, SOAPObject item)
        {
            if (item.Value is Dictionary<string, object>)
            {
                soap += $"<tns:{item.Name}>";
                var dict = (Dictionary<string, object>)item.Value;
                FromDictionary(ref soap, dict);
                soap += $"</tns:{item.Name}>";
            }
            else
            {
                soap += $"<tns:{item.Name}>{item.Value}</tns:{item.Name}>";
            }
        }
        public void FromDictionary(ref string soap, Dictionary<string,object> dict)
        {
            foreach (var item2 in dict)
            {
                if(item2.Value is SOAPObject)
                {
                    FromSOAPObject(ref soap, (SOAPObject)item2.Value);
                }
                else
                soap += $"<tns:{item2.Key}>{item2.Value}</tns:{item2.Key}>";
            }
        }
        public void FromSOAPObjectJson(ref string json, SOAPObject item)
        {
         
            if (item.Value is Dictionary<string, object>)
            {
                FromDictionaryJson(ref json, item);
            }
            else
            {
                json += "\"tns:" + item.Name + "\":[\"" + item.Value + "\"],";
            }
        }
        public void FromDictionaryJson(ref string json, SOAPObject item)
        {
            json += "\"tns:" + item.Name + "\":[{";
            var dict = (Dictionary<string, object>)item.Value;
            int index = 0;
            foreach (var item2 in dict)
            {
                if(item2.Value is SOAPObject)
                {
                    FromSOAPObjectJson(ref json, (SOAPObject)item2.Value);
                    return;
                }
                json += "\"tns:" + item2.Key + "\":[\"" + item2.Value + "\"]" + (index + 1 < dict.Count ? "," : "}");
                index++;
            }
            json += "]}";
        }
        public string BuildStringForChecksum()
        {
            int index = 0;
            string json = "{\"SOAP-ENV:Envelope\":{\"$\":{\"xmlns:SOAP-ENV\":\"http://schemas.xmlsoap.org/soap/envelope/\",\"xmlns:s\":\"http://www.w3.org/2001/XMLSchema\",\"xmlns:xsi\":\"http://www.w3.org/2001/XMLSchema-instance\"},";
         
            if (headers.Count > 0)
            {
                json+="\"SOAP-ENV:Header\":[";
                
                foreach (var item in headers)
                {
                    json += "{\"tns:" + item.Name + "\":[{\"$\":{\"xmlns:tns\":\"http://moviestarplanet.com/\"},";
                    
                    var dict = (Dictionary<string, object>)item.Value;
                    foreach (var item2 in dict)
                    {
                        json += "\"tns:"+item2.Key+"\":[\""+item2.Value +"\"]"+(index +1<dict.Count?",":"}");
                        index++;
                    }
                    json += "]}";
                }
                json+="],";
            }
            json += "\"SOAP-ENV:Body\":[{\"tns:" + Action + "\":[{\"$\":{\"xmlns:tns\":\"http://moviestarplanet.com/\"},";
            index = 0;
            foreach (var item3 in bodyValues)
            {
                if(!(item3.Value is Dictionary<string,object>))
                {
                    json += "\"tns:" + item3.Name + "\":[\"" + item3.Value + "\"]" + (index + 1 < bodyValues.Count ? "," : "}");
                }
                else
                {
                    FromSOAPObjectJson(ref json, item3);
                  //  json += "\"tns:" + item3.Name + "\":[\"" + item3.Value + "\"],";
                }
                index++;
            }
                
                    
                //else
                //{
                //    json += "\"tns:"+item.Name+"\":[{";
                //    var item2 = (Dictionary<string, object>)item.Value;
                //    for (int l = 0; l < item2.Count; l++)
                //    {
                //        var item3 = item2.ToArray()[l];
                //        json += "\"tns:" + item3.Key + "\":[\"" + item3.Value + "\"]" + (l + 1 < item2.Count ? "," : "}");

                //    }
                //    json += "]}";
                //}
            
            json += "]}]}}"+Action+"KmSqayB#ep8&dje!9k7!XzJAtCG!cFFe@Ebsy7c9";
           
            return json;

        }
    }
}
