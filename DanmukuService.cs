using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CookComputing.XmlRpc;

namespace DanmukuRPCServer
{
    struct Danmu
    {
        public string user;
        public string text;
        public string warn;
    }

    [XmlRpcService(AutoDocumentation = true, Description = "Danmuku Service")]
    class DanmukuService : XmlRpcListenerService
    {
        public static ConcurrentBag<Danmu> MsgQueue = new ConcurrentBag<Danmu>();
        public static bool close = false;
        public static bool danmu = false;

        [XmlRpcMethod("fire_danmuku")]
        [return: XmlRpcReturnValue(Description = "Send Danmuku")]
        public void SendDanmuku(string user, string text, string warn)
        {
            MsgQueue.Add(new Danmu()
            {
                user = user,
                text = text,
                warn = warn
            });
        }

        [XmlRpcMethod("shutdown_danmuku")]
        [return: XmlRpcReturnValue(Description = "Shutdown Danmuku")]
        public void ShutdownDanmuku()
        {
            close = true;
        }

        [XmlRpcMethod("toogle_danmuku")]
        [return: XmlRpcReturnValue(Description = "Shutdown Danmuku")]
        public void ToogleDanmuku()
        {
            danmu = !danmu;
        }
    }
}
