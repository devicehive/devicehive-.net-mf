using System;
using Microsoft.SPOT;
using System.Net;
using System.IO;
using Json.Serialization;
using System.Text;
using System.Collections;
using System.Threading;

namespace DeviceHive
{
    /// <summary>
    /// HTTP client for restful HTTP service
    /// </summary>
    /// <remarks>
    /// Complies to HTTP protocol v.6.
    /// Implements device-specific commands for .NET Micro Framework device.
    /// </remarks>
    public class HttpClient : IFrameworkClient
    {
        private string CloudUrl;

        private const string PutMethod = "PUT";
        private const string GetMethod = "GET";
        private const string PostMethod = "POST";
        private const string DeleteMethod = "DELETE";

        private const string ContentType = "application/json";

        //private const string PutDeviceCommand = "/device";
        private const string InfoCommand = "/info/";
        private const string DeviceCommand = "/device/";
        private const string NotificationCommand = "/notification";
        private const string PollCommandName = "/command/poll?timestamp=";
        //private const string GetCommandName = "/command?start=";
        private const string CommandStatus = "/command/";
        // v6
        private const string IdHeader = "Auth-DeviceID";
        private const string KeyHeader = "Auth-DeviceKey";

        private Queue CommandQueue;
        private string lastTimeStamp;
        private int AsyncTimeout;

        /// <summary>
        /// Constructs an HTTP client object
        /// </summary>
        /// <param name="Url">URL of the DeviceHive server</param>
        /// <param name="RequestTimeout">Maximum request timeout</param>
        /// <param name="TimeStamp">Timastamp of the last command</param>
        /// <remarks>
        /// If the caller has no intention to set a specific time stamp, DeteTime.MinValue can be used.
        /// </remarks>
        public HttpClient(string Url, int RequestTimeout, string TimeStamp = null)
        {
            CloudUrl = Url;
            
            CommandQueue = new Queue();
            AsyncTimeout = RequestTimeout;
            LastTimeStamp = TimeStamp;

        }

        private string LastTimeStamp
        {
            get
            {
                if (lastTimeStamp == null)
                {
                    lastTimeStamp = GetApiInfo().serverTimestamp;
                }
                return lastTimeStamp;
            }
            set
            {
                lastTimeStamp = value;
            }
        }

        //public Device GetDevice(Guid id)
        //{
        //    HttpWebRequest req = HttpWebRequest.Create(CloudUrl + GetDeviceCommand + id.ToString()) as HttpWebRequest;
        //    req.Method = GetMethod;
        //    WebResponse resp = req.GetResponse();
        //    Stream stm = resp.GetResponseStream();
        //    JsonSerializer js = new JsonSerializer();
        //    return (Device)js.Deserialize(stm, typeof(Device));
        //}

        /// <summary>
        /// Performs an asynchronous request and returns its response
        /// </summary>
        /// <param name="req">Request to be executed</param>
        /// <returns>HTTP response; it can be null in case of an error</returns>
        private HttpWebResponse GetResponseAsync(HttpWebRequest req)
        {
            HttpWebResponse resp;
            //_req = req;
            resp = null;
            Thread th = new Thread(() =>
            {
                try
                {
                    resp = (HttpWebResponse)req.GetResponse();
                }
                catch (Exception) { }
                //_recvEvent.Set();
            });
            
            th.Start();
            if (!th.Join(AsyncTimeout))
            {
                th.Abort();
                
                throw new System.Net.WebException("HTTP connection timeout");//InvalidOperationException("Timeout!");
            }
            return resp;
        }

        private WebResponse MakeRequest(string url, object obj, string Method, string id, string key)
        {
            //lock (this)
            //{
            Debug.Print("Request: " + url);
            using (HttpWebRequest req = HttpWebRequest.Create(url) as HttpWebRequest)
            {

                req.Method = Method;
                req.ContentType = ContentType;
                req.KeepAlive = false;
                req.Headers.Add(IdHeader, id);
                req.Headers.Add(KeyHeader, key);
                req.ReadWriteTimeout = AsyncTimeout;
                req.Timeout = AsyncTimeout;
                if (obj != null)
                {
                    JsonFormatter js = new JsonFormatter();

                    string s = js.ToJson(obj);
                    req.ContentLength = s.Length;
                    using (Stream rs = req.GetRequestStream())
                    {
                        rs.Write(Encoding.UTF8.GetBytes(s), 0, s.Length);
                        rs.Close();
                    }
                }

                HttpWebResponse resp = GetResponseAsync(req); //(HttpWebResponse)req.GetResponse(); //
                Debug.Print("Done. Response code = " + resp.StatusCode.ToString());
                return resp;
            }
          
        }

        /// <summary>
        /// Gets API info.
        /// </summary>
        /// <returns><see cref="ApiInfo"/> object.</returns>
        public ApiInfo GetApiInfo()
        {
            byte[] bts = null;
            HttpWebResponse resp = MakeRequest(CloudUrl + InfoCommand, null, GetMethod, null, null) as HttpWebResponse;

            if (resp.ContentType.IndexOf(ContentType) >= 0)
            {

                if (resp.ContentLength > 0)
                {
                    using (Stream rs = resp.GetResponseStream())
                    {
                        bts = new byte[(int)rs.Length];
                        rs.Read(bts, 0, bts.Length);
                        rs.Close();
                    }
                }
            }
            resp.Close();

            if (bts != null)
            {
                JsonFormatter js = new JsonFormatter();
                object o = js.FromJson(bts, typeof(ApiInfo));

                ApiInfo ai = o as ApiInfo;
                if (ai != null)
                {
                    return ai;
                }
            }
            throw new Exception("Unknown error while getting ApiInfo.");
        }

        /// <summary>
        /// Registers a device
        /// </summary>
        /// <param name="device">Device data structure</param>
        /// <returns>True, if registration succeeds; false - otherwise</returns>
        /// <remarks>
        /// This method is called by device to register it at the server.
        /// </remarks>
        public bool SetDevice(Device device)
        {
            //bool rv = false;
            HttpWebResponse resp = MakeRequest(CloudUrl + DeviceCommand + device.id.ToString(), 
                device, 
                PutMethod, 
                device.id.ToString(), 
                device.key) as HttpWebResponse;
            return resp.StatusCode == HttpStatusCode.OK || resp.StatusCode == HttpStatusCode.NoContent;
        }

        /// <summary>
        /// Sends a notification
        /// </summary>
        /// <param name="device">Device data of the device that is sending the notification</param>
        /// <param name="notification">Notification to be sent</param>
        /// <returns>True if the notification succeeds; false - otherwise</returns>
        /// <remarks>
        /// This method can be used by devices to send notifications.
        /// </remarks>
        public bool PostNotification(Device device, INotification notification)
        {
            //bool rv = false;

            HttpWebResponse resp = MakeRequest(
                CloudUrl + DeviceCommand + device.id.ToString() + NotificationCommand,
                notification.Data,
                PostMethod,
                device.id.ToString(),
                device.key) as HttpWebResponse;
            return resp.StatusCode == HttpStatusCode.OK || resp.StatusCode == HttpStatusCode.Created;
                //resp.Close();
        }

        /// <summary>
        /// Polls command for execution
        /// </summary>
        /// <param name="device">Device data of the device that is polling a command</param>
        /// <param name="onlyUnprocessed">Get commands with empty status only</param>
        /// <returns>Command data structure or null if there are no commands</returns>
        /// <remarks>
        /// This method returns the next command from the command queue. If there are no commands in the queue, it waits for ~30 seconds to receive a new command.
        /// </remarks>
        public DeviceCommand PollCommand(Device device, bool onlyUnprocessed = true)
        {
            if (CommandQueue.Count != 0)
            {
                return (DeviceCommand)CommandQueue.Dequeue();
            }
            string TimeString = UriExtensions.EscapeDataString(LastTimeStamp);
            string Url = CloudUrl + DeviceCommand + device.id.ToString() + PollCommandName + TimeString;

            byte[] bts = null;
            WebResponse resp = MakeRequest(Url, null, GetMethod, device.id.ToString(), device.key);

            if (resp.ContentType.IndexOf(ContentType) >= 0)
            {

                if (resp.ContentLength > 0)
                {
                    using (Stream rs = resp.GetResponseStream())
                    {
                        bts = new byte[(int)rs.Length];
                        rs.Read(bts, 0, bts.Length);
                        rs.Close();
                    }
                }
            }
            resp.Close();

            if (bts != null)
            {
                JsonFormatter js = new JsonFormatter();
                object o = js.FromJson(bts, typeof(DeviceCommand));

                DeviceCommand dc = o as DeviceCommand;
                if (dc != null && (!onlyUnprocessed || StringExtensions.IsNullOrEmpty(dc.status)))
                {
                    return dc;
                }
                ArrayList al = o as ArrayList;
                if (al != null)
                {
                    if (al.Count == 0) return null;

                    for (int x = 0; x < al.Count; ++x)
                    {
                        dc = (DeviceCommand)al[x];
                        if (!onlyUnprocessed || StringExtensions.IsNullOrEmpty(dc.status))
                        {
                            CommandQueue.Enqueue(dc);
                        }
                    }
                    DeviceCommand ldc = (DeviceCommand)al[al.Count - 1];
                    LastTimeStamp = ldc.timestamp;

                    return CommandQueue.Count == 0 ? null : (DeviceCommand)CommandQueue.Dequeue();
                }
            }
            return null;
        }

        /// <summary>
        /// Gets a command
        /// </summary>
        /// <param name="device">Device data of the device that is getting a command</param>
        /// <param name="onlyUnprocessed">Get commands with empty status only</param>
        /// <returns>Command data structure or null if there are no pending commands</returns>
        /// <remarks>
        /// This method returns all the commands that are waiting at the server since last GetCommand call. It returns immediately, regardless if there are commands to execute or not.
        /// </remarks>
        public DeviceCommand GetCommand(Device device, bool onlyUnprocessed = true)
        {
            if (CommandQueue.Count != 0)
            {
                return (DeviceCommand)CommandQueue.Dequeue();
            }
            string TimeString = UriExtensions.EscapeDataString(LastTimeStamp);
            string Url = CloudUrl + DeviceCommand + device.id.ToString() + PollCommandName + TimeString + "&waitTimeout=0";

            byte[] bts = null;
            WebResponse resp = MakeRequest(Url, null, GetMethod, device.id.ToString(), device.key);

            if (resp.ContentType.IndexOf(ContentType) >= 0)
            {

                if (resp.ContentLength > 0)
                {
                    using (Stream rs = resp.GetResponseStream())
                    {
                        bts = new byte[(int)rs.Length];
                        rs.Read(bts, 0, bts.Length);
                        rs.Close();
                    }
                }
            }
            resp.Close();

            if (bts != null)
            {
                JsonFormatter js = new JsonFormatter();
                object o = js.FromJson(bts, typeof(DeviceCommand));

                DeviceCommand dc = o as DeviceCommand;
                if (dc != null && (!onlyUnprocessed || StringExtensions.IsNullOrEmpty(dc.status)))
                {
                    return dc;
                }
                ArrayList al = o as ArrayList;
                if (al != null)
                {
                    if (al.Count == 0) return null;

                    for (int x = 0; x < al.Count; ++x)
                    {
                        dc = (DeviceCommand)al[x];
                        if (!onlyUnprocessed || StringExtensions.IsNullOrEmpty(dc.status))
                        {
                            CommandQueue.Enqueue(dc);
                        }
                    }
                    DeviceCommand ldc = (DeviceCommand)al[al.Count - 1];
                    LastTimeStamp = ldc.timestamp;

                    return CommandQueue.Count == 0 ? null : (DeviceCommand)CommandQueue.Dequeue();
                }
            }
            return null;
        }

        /// <summary>
        /// Updates command status
        /// </summary>
        /// <param name="device">Device data of the device that is updating a command status</param>
        /// <param name="CommandId">ID of the command to be updated</param>
        /// <param name="status">Status of the command</param>
        /// <returns>True if the status update was successfull; false - otherwise</returns>
        /// <remarks>
        /// The devices are using this method to notify the server of command completion and its result. 
        /// </remarks>
        public bool UpdateCommandStatus(Device device, string CommandId, CommandStatus status)
        {
            //bool rv = false;
            HttpWebResponse resp = MakeRequest(
                CloudUrl + DeviceCommand + device.id.ToString() + CommandStatus + CommandId,
                status,
                PutMethod,
                device.id.ToString(),
                device.key) as HttpWebResponse;

            return resp.StatusCode == HttpStatusCode.OK || resp.StatusCode == HttpStatusCode.NoContent;
        }
    }
}
