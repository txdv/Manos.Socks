using System;
using System.Collections.Generic;
using System.Text;
using Manos.IO;

namespace Manos.Socks
{
	public class Socks4ServerInfo
	{
		public Socks4ServerInfo()
		{
			Overhead = new Queue<byte[]>();
		}
		
		public bool FirstMessage { get; set; }
		public Socket DestinationSocket { get; set; }
		public Socket SourceSocket { get; set; }
		public bool DestinationReady { get; set; }
		public Queue<byte[]> Overhead { get; set; }
	}
	
	public class Socks4Server
	{
		private Dictionary<int, Socks4ServerInfo> info = new Dictionary<int, Socks4ServerInfo>();
		
		private Context Context { get; set; }
		private Socket Socket { get; set; }
		
		public bool VersionCheck { get; set; }
		
		public Socks4Server(Context context, Socket socket)
		{
			Context = context;
			Socket = socket;
			VersionCheck = true;
		}
		
		public void Listen(string host, int port, Func<Socket, Socks4Packet, bool> checkUser)
		{
			Socket.Listen(host, port, delegate (Socket sourceSocket) {
				var sourceStream = sourceSocket.GetSocketStream();
				var sourcePort = sourceSocket.Port;
				
				info[sourcePort] = new Socks4ServerInfo() { FirstMessage = true };
				
				sourceStream.Read(delegate (ByteBuffer buffer) {
					OnData(sourceSocket, sourceStream, buffer, checkUser);
				}, delegate (Exception exception) { }, delegate {
					info.Remove(sourcePort);
				});
			});
		}
		
		private void OnData(Socket sourceSocket, Stream sourceStream, ByteBuffer buffer, Func<Socket, Socks4Packet, bool> checkUser)
		{
			var sourcePort = sourceSocket.Port;
			
			if (info[sourcePort].FirstMessage) {

				var data = Socks4Packet.GetPacketInfo(buffer.Bytes, buffer.Position, buffer.Length);
				
				if (!VersionCheck || !data.ValidVersion) {
					// invalid packet header
					sourceStream.Write(Socks4Packet.CreatePacket(Socks4Packet.CMD_REPLY_REQUEST_REJECTED_OR_FAILED), delegate {
						sourceSocket.Close();
					});
					return;
				}
				switch (data.Command) {
				case Socks4Packet.CMD_CONNECT:
					
					if (!checkUser(sourceSocket, data)) {
						
						sourceStream.Write(Socks4Packet.CreatePacket(Socks4Packet.CMD_REPLY_REQUEST_REJECTED_OR_FAILED), delegate {
							sourceSocket.Close();
						});
						
						return;
					}
					
					var destinationSocket = Context.CreateSocket();
					info[sourcePort].DestinationSocket = destinationSocket;
					destinationSocket.Connect(data.IPAddress.ToString(), data.Port, delegate {
						var destinationStream = destinationSocket.GetSocketStream();
						info[sourcePort].DestinationReady = true;
						
						var queue = info[sourcePort].Overhead;
						while (queue.Count > 0) {
							destinationStream.Write(queue.Dequeue());
						}
						destinationStream.Read(delegate (ByteBuffer destinationReadBuffer) {
							sourceStream.Write(destinationReadBuffer);
						}, delegate (Exception exception) { }, delegate {
							sourceSocket.Close();
						});
					});
					
					sourceStream.Write(Socks4Packet.CreatePacket(Socks4Packet.CMD_REPLY_REQUEST_GRANTED));
					
					info[sourcePort].FirstMessage = false;
					break;
				default:
					sourceStream.Close();
					info.Remove(sourcePort);
					break;
				}
				
			} else {
				if (info.ContainsKey(sourcePort)) {
					var sockInfo = info[sourcePort];
					if (!sockInfo.DestinationReady) {
						byte[] bytes = new byte[buffer.Length];
						for (int i = 0; i < buffer.Length; i++) {
							bytes[i] = buffer.Bytes[buffer.Position + i];
						}
						sockInfo.Overhead.Enqueue(bytes);
					} else {
						sockInfo.DestinationSocket.GetSocketStream().Write(buffer);
					}
				}
			}
		}
	}
}
