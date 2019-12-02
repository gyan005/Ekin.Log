using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using Ekin.Encryption;
using System.Net;

namespace Ekin.Log
{
    public static class Extensions
    {
        #region Object serialisation using Newtonsoft.Json

        public static string GetJson(this object obj, bool AllowReferenceLoops, bool IgnoreNullValues, string EncryptionKey = "")
        {
            if (obj == null)
                return null;

            JsonSerializerSettings serializerSettings = new JsonSerializerSettings();

            if (AllowReferenceLoops)
                serializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
            else
                serializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Error;

            if (IgnoreNullValues)
                serializerSettings.NullValueHandling = NullValueHandling.Ignore;
            else
                serializerSettings.NullValueHandling = NullValueHandling.Include;

            try
            {
                string content = JsonConvert.SerializeObject(obj, serializerSettings);
                if (!string.IsNullOrWhiteSpace(EncryptionKey) && !string.IsNullOrWhiteSpace(content))
                {
                    return content.Encrypt(EncryptionKey);
                }
                else
                {
                    return content;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static string GetJson(this object obj)
        {
            return obj.GetJson(true, true);
        }

        public static void LoadFromJson(this object obj, string json, string EncryptionKey = "")
        {
            try
            {
                JsonConvert.PopulateObject((!string.IsNullOrWhiteSpace(EncryptionKey) && !string.IsNullOrWhiteSpace(json)) ? json.Decrypt(EncryptionKey) : json, obj);
            }
            catch (Exception e)
            {
            }
        }

        #endregion

        #region System.IO File operations

        public static void SaveToFile(this object obj, FileInfo file, string EncryptionKey = "")
        {
            if (file == null || string.IsNullOrWhiteSpace(file.FullName))
                throw new Exception("Filename cannot be empty");

            file.Directory.Create(); // If the directory already exists, this method does nothing.
            File.WriteAllText(file.FullName, obj.GetJson(true, true, EncryptionKey));
        }

        public static void SaveToFile(this object obj, string Filepath, string EncryptionKey = "")
        {
            if (string.IsNullOrWhiteSpace(Filepath))
                throw new Exception("Filename cannot be empty");
            obj.SaveToFile(new FileInfo(Filepath), EncryptionKey);
        }

        public static void LoadFromFile(this object obj, FileInfo file, string EncryptionKey = "")
        {
            if (file == null || string.IsNullOrWhiteSpace(file.FullName))
                throw new Exception("Filename cannot be empty");
            obj.LoadFromJson(File.ReadAllText(file.FullName), EncryptionKey);
        }

        public static void LoadFromFile(this object obj, string Filepath, string EncryptionKey = "")
        {
            if (string.IsNullOrWhiteSpace(Filepath))
                throw new Exception("Filename cannot be empty");
            obj.LoadFromFile(new FileInfo(Filepath), EncryptionKey);
        }

        #endregion

        #region Error message formatting

        public static string GetFormattedErrorMessage(this object Error)
        {
            if (Error is WebException)
            {
                WebException ex = Error as WebException;
                return String.Format("[{0}] {1}", ex.StatusCode(), ex.Message);
            }
            else
            {
                return string.Empty;
            }
        }

        public static int StatusCode(this WebException ex)
        {
            if (ex.Status == WebExceptionStatus.ProtocolError)
            {
                var response = ex.Response as HttpWebResponse;
                if (response != null)
                {
                    return (int)response.StatusCode;
                }
            }
            return 0;
        }

        #endregion

    }
}
