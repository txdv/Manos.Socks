using System;
using System.Collections.Generic;
using System.Text;
using Manos.IO;

namespace Manos.Socks
{
	public class Socks4Server
	{
		private Dictionary<int, bool> firstMessage = new Dictionary<int, bool>();
		private Dictionary<int, Socket> sockets = new Dictionary<int, Socket>();
		
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
				firstMessage[sourcePort] = true;
				sourceStream.Read(delegate (ByteBuffer buffer) {
					OnData(sourceSocket, sourceStream, buffer, checkUser);
				}, delegate (Exception exception) { }, delegate {
				});
			});
		}
		
		private void OnData(Socket sourceSocket, Stream sourceStream, ByteBuffer buffer, Func<Socket, Socks4Packet, bool> checkUser)
		{
			var sourcePort = sourceSocket.Port;
			
			if (firstMessage[sourcePort]) {
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
					sockets[sourcePort] = destinationSocket;
					destinationSocket.Connect(data.IPAddress.ToString(), data.Port, delegate {
						var destinationStream = destinationSocket.GetSocketStream();
						destinationStream.Read(delegate (ByteBuffer destinationReadBuffer) {
							sourceStream.Write(destinationReadBuffer);
						}, delegate (Exception exception) { }, delegate {
							sourceSocket.Close();
						});
					});
					
					sourceStream.Write(Socks4Packet.CreatePacket(Socks4Packet.CMD_REPLY_REQUEST_GRANTED));
					
					firstMessage[sourcePort] = false;
					break;
				default:
					sourceStream.Close();
					firstMessage.Remove(sourcePort);
					break;
				}
				
			} else {
				if (sockets.ContainsKey(sourcePort)) {
					var destinationStream = sockets[sourcePort].GetSocketStream();
					if (destinationStream != null && destinationStream.CanWrite) {
						destinationStream.Write(buffer);
					}
				}
			}
		}
	}
}
