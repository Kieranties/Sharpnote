using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Sharpnote.Properties;


namespace Sharpnote
{
    public sealed class Sharpnote<T> where T : Interfaces.INote, new()
    {
        private static readonly Sharpnote<T> _instance = new Sharpnote<T>();
        private static readonly Settings _settings = Settings.Default;
        private static string _authToken = string.Empty;
        private static string _email = string.Empty;        private static string AuthQsParams
        {
            get { return string.Format("auth={0}&email={1}", _authToken, _email); }
        }

        private Sharpnote() { }

        public static Sharpnote<T> Instance
        {
            get { return _instance; }
        }

        public bool Login(string email, string password)
        {
            StringParamCheck("email", email);
            StringParamCheck("password", password);

            var data = string.Format("email={0}&password={1}", email, password);
            using (var resp = ProcessRequest(_settings.LoginPath, "POST", data))
            {
                if (resp != null)
                {
                    _authToken = resp.Cookies["auth"].Value;
                    _email = email;
                    return true;
                }
                return false;
            }
        }

        public IEnumerable<T> FetchIndex()
        {
            CheckAuthKey();
            using (var resp = ProcessRequest(_settings.IndexPath, "GET", queryParams: AuthQsParams))
            {
                var jsonArr = JArray.Parse(ReadResponseContent(resp));
                return jsonArr.Where(entry => !entry["deleted"].Value<bool>())                               
                              .Select(token => new T
                              {
                                  Key = token["key"].Value<string>(),
                                  Modified = token["modify"].Value<DateTime>()
                              });
            }
        }

        public T FetchFullNote(string key)
        {
            CheckAuthKey();
            StringParamCheck("key", key);
            var queryParams = string.Format("{0}&key={1}&encode=base64", AuthQsParams, key);
            using (var resp = ProcessRequest(_settings.NotePath, "GET", queryParams: queryParams))
            {                
                return new T { Key = key, 
                               Content = Decode(ReadResponseContent(resp)),
                               Modified = DateTime.Parse(resp.GetResponseHeader("note-modifydate")),
                               Created = DateTime.Parse(resp.GetResponseHeader("note-createdate"))};                
            }
        }

        public Tuple<IEnumerable<T>, int> SearchNotes(string query = null, int max = 10, int offset = 0)
        {
            CheckAuthKey();
            if (max < 1) throw new System.ArgumentOutOfRangeException("max", max, "Value must be one or greater");
            if (offset < 0) throw new System.ArgumentOutOfRangeException("offset", offset, "Value must be zero or greater");

            var queryParams = string.Format("{0}&query={1}&results={2}&offset={3}", AuthQsParams, query, max, offset);
            using (var resp = ProcessRequest(_settings.SearchPath, "GET", queryParams: queryParams))
            {
                var jObj = JObject.Parse(ReadResponseContent(resp));
                var total = jObj["Response"]["totalRecords"].Value<int>();
                var notes = JArray.FromObject(jObj["Response"]["Results"])
                                  .Select(token =>new T
                                  {
                                      Key = token["key"].Value<string>(),
                                      Content = token["content"].Value<string>()
                                  });

                return new Tuple<IEnumerable<T>, int>(notes, total);
            }
        }
        
        public string SaveNote(string content, string key = null)
        {
            CheckAuthKey();
            var queryParams = string.IsNullOrEmpty(key) ? AuthQsParams : string.Format("{0}&key={1}", AuthQsParams, key);
            using(var resp = ProcessRequest(_settings.NotePath, "POST", content, queryParams))
            {
                return ReadResponseContent(resp);
            }
        }

        public bool DeleteNote(string key, bool destroy = false)
        {
            CheckAuthKey();
            StringParamCheck("key", key);

            var queryParams = string.Format("{0}&key={1}&dead={2}", AuthQsParams, key, destroy ? 1 : 0);
            using (var resp = ProcessRequest(_settings.DeletePath, "GET", queryParams: queryParams))
            {
                return resp == null;
            }
        }

        private static HttpWebResponse ProcessRequest(string requestPath, string method,
                                                      string content = null, string queryParams = null)
        {
            try
            {
                var url = string.Format("{0}{1}{2}", _settings.Scheme, _settings.Domain, requestPath);
                if (!string.IsNullOrEmpty(queryParams)) url += "?" + queryParams;
                var req = WebRequest.Create(url) as HttpWebRequest;
                req.CookieContainer = new CookieContainer();
                req.Method = method;

                if (string.IsNullOrEmpty(content)) req.ContentLength = 0;
                else
                {
                    using (var sw = new StreamWriter(req.GetRequestStream()))
                    {
                        sw.Write(Encode(content));
                    }
                }

                return (HttpWebResponse)req.GetResponse();
            }
            catch (WebException ex)
            {
                //switch (ex.Status)
                //{
                    
                //}
                Console.Out.WriteLine("");
            }
            return null;
        }

        private static string Encode(string value)
        {
            var bytes = ASCIIEncoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(bytes);
        }

        private static string Decode(string value)
        {
            var data = Convert.FromBase64String(value);
            return ASCIIEncoding.UTF8.GetString(data);            
        }

        private static string ReadResponseContent(HttpWebResponse resp)
        {
            if (resp == null) throw new ArgumentNullException("resp");
            using (var sr = new StreamReader(resp.GetResponseStream()))
            {
                return sr.ReadToEnd();
            }
        }

        private void CheckAuthKey()
        {
            if (string.IsNullOrEmpty(_authToken)) throw new SharpnoteAuthorisationException();
        }

        private void StringParamCheck(string paramName, string value)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(paramName, "Value must not be null or string.Empty");
        }
    }
}