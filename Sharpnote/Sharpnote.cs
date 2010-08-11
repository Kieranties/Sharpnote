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
    /// <summary>
    /// Singleton class used to handle all calls to Simplenote
    /// </summary>
    public sealed class Sharpnote
    {
        private static readonly Sharpnote _instance = new Sharpnote();
        private static readonly Settings _settings = Settings.Default;
        private static string _authToken = string.Empty;
        private static string _email = string.Empty;   
        private static string AuthQsParams
        {
            get { return string.Format("auth={0}&email={1}", _authToken, _email); }
        }

        private Sharpnote() { }

        public static Sharpnote Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// Logs a user in to Simplenote and returns the succes status.
        /// The user logged in will be used for all further calls to Simplenote
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Fetchs the index of notes from Simplenote
        /// </summary>
        /// <typeparam name="T">The type of note to return.  Must implement INote and be instantiable</typeparam>
        /// <returns></returns>
        public IEnumerable<T> FetchIndex<T>(bool getDeleted = false) where T : Interfaces.INote, new()
        {
            CheckAuthKey();
            using (var resp = ProcessRequest(_settings.IndexPath, "GET", queryParams: AuthQsParams))
            {
                var jsonArr = JArray.Parse(ReadResponseContent(resp)).AsEnumerable();
                if (getDeleted) jsonArr = jsonArr.Where(entry => !entry["deleted"].Value<bool>());
                return jsonArr.Select(token => new T
                                      {
                                          Key = token["key"].Value<string>(),
                                          Modified = token["modify"].Value<DateTime>()
                                      });
            }
        }

        /// <summary>
        /// Fetches the full content of a note
        /// </summary>
        /// <typeparam name="T">The type of note to return.  Must implement INote and be instantiable</typeparam>
        /// <param name="key">The key of the note to retrieve</param>
        /// <returns>A note of type T</returns>
        public T FetchFullNote<T>(string key) where T : Interfaces.INote, new()
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

        /// <summary>
        /// Searches the Simplenote store using the given query
        /// </summary>
        /// <typeparam name="T">The type of note to return.  Must implement INote and be instantiable</typeparam>
        /// <param name="query">The query to search Simplenote</param>
        /// <param name="max">The maximum number of results (defaults to 10, must be greater than 0)</param>
        /// <param name="offset">Used for indexing results (defaults to 0, must be greater than -1)</param>
        /// <returns>A Tuple of an enumerable collection of T and the total results returned</returns>
        public Tuple<IEnumerable<T>, int> SearchNotes<T>(string query = null, int max = 10, int offset = 0) 
            where T : Interfaces.INote, new()
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
        
        /// <summary>
        /// Saves/Updates the given content to Simplenote
        /// </summary>
        /// <param name="content">The content of the note to be saved as a plain string</param>
        /// <param name="key">Optional: The key of the note to update</param>
        /// <returns>The key of the note if successful</returns>
        public string SaveNote(string content, string key = null)
        {
            CheckAuthKey();
            var queryParams = string.IsNullOrEmpty(key) ? AuthQsParams : string.Format("{0}&key={1}", AuthQsParams, key);
            using(var resp = ProcessRequest(_settings.NotePath, "POST", content, queryParams))
            {
                return ReadResponseContent(resp);
            }
        }

        /// <summary>
        /// Deletes the given note from Simplenote
        /// </summary>
        /// <param name="key">The key of the note to delete</param>
        /// <param name="destroy">If true, the note is permanently deleted, else the note will be fully removed once the user
        /// syncs with the iPhone application</param>
        /// <returns></returns>
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

        /// <summary>
        /// Generic method to process a request to Simplenote.
        /// All publicly expose methods which interact with the store are processed though this.
        /// </summary>
        /// <param name="requestPath"></param>
        /// <param name="method"></param>
        /// <param name="content"></param>
        /// <param name="queryParams"></param>
        /// <returns></returns>
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
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Helper method to encode as string as base-64
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string Encode(string value)
        {
            var bytes = ASCIIEncoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Helper method to decode a base-64 string
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string Decode(string value)
        {
            var data = Convert.FromBase64String(value);
            return ASCIIEncoding.UTF8.GetString(data);            
        }

        /// <summary>
        /// Reads the content from the response object
        /// </summary>
        /// <param name="resp"></param>
        /// <returns></returns>
        private static string ReadResponseContent(HttpWebResponse resp)
        {
            if (resp == null) throw new ArgumentNullException("resp");
            using (var sr = new StreamReader(resp.GetResponseStream()))
            {
                return sr.ReadToEnd();
            }
        }

        /// <summary>
        /// Helper method to check that a user auth key has been retrieved.
        /// Does not check if the auth key is still valid
        /// </summary>
        private void CheckAuthKey()
        {
            if (string.IsNullOrEmpty(_authToken)) throw new SharpnoteAuthorisationException();
        }

        /// <summary>
        /// String parameter helper method.
        /// Checks for null or empty, throws ArgumentNullException if true
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="value"></param>
        private void StringParamCheck(string paramName, string value)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(paramName, "Value must not be null or string.Empty");
        }
    }
}