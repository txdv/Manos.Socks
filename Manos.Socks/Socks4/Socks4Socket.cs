using System;

using Manos;
using Manos.IO;

namespace Manos.Socks
{
	public class Socks4Socket : Socket
	{
		private Socket Socket { get; set; }
		
		private bool FirstMessage { get; set; }
		
		private Action ActionCallback { get; set; }
		
		public string ProxyHost { get; set; }
		
		public int ProxyPort { get; set; }
		
		public string UserId { get; set; }
		
		public Socks4Socket(Context context, string proxyHost, int proxyPort, string userId)
			: base(context)
		{
			Socket = context.CreateSocket();
			ProxyHost = proxyHost;
			ProxyPort = proxyPort;
			UserId = userId;
		}
		
		public override void Connect(string host, int port, Action callback)
		{
			Socket.Connect(ProxyHost, ProxyPort, delegate {
				Stream stream = Socket.GetSocketStream();
				stream.Write(Socks4Packet.CreatePacket(host, port, UserId));
				FirstMessage = true;
				stream.Read(OnData, delegate (Exception exception) { }, delegate { });
				ActionCallback = callback;
			});
		}
		
		public void OnData(ByteBuffer buffer)
		{
			if (!FirstMessage) {
				return;
			}
			
			if (buffer.Length != 8) {
				ActionCallback();
			}
			
			switch (buffer.Bytes[buffer.Position + 1]) {
			case Socks4Packet.CMD_REPLY_REQUEST_GRANTED:
				ActionCallback();
				break;
			case Socks4Packet.CMD_REPLY_REQUEST_REJECTED_CANNOT_CONNECT_TO_IDENTD:
				throw new Socks4Exception("Request reject, cannot connect to identd");
			case Socks4Packet.CMD_REPLY_REQUEST_REJECTED_DIFFERENT_IDENTD:
				throw new Socks4Exception("Different identd");
			case Socks4Packet.CMD_REPLY_REQUEST_REJECTED_OR_FAILED:
				throw new Socks4Exception("Reject or failed");
			default:
				throw new Socks4Exception("Failed to parse answer packet");
			}
			
			FirstMessage = false;
		}
		
		public override void Listen(string host, int port, Action<Socket> callback)
		{
			Socket.Listen(host, port, delegate (Socket socket) {
				callback(Socket);
			});
		}
		
		public override Stream GetSocketStream()
		{
			return Socket.GetSocketStream();
		}
		
		public override void Close()
		{
			Socket.Close();
		}
		
		public override Context Context {
			get {
				return Socket.Context;
			}
		}
		
	}
}

