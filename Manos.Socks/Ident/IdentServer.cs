using System;
using System.Text;

using Manos.IO;

namespace Manos.Socks
{
	public class IdentServer
	{
		private Context Context { get; set; }
		public Encoding Encoding { get; set; } 
		
		public IdentServer(Context context)
		{
			Context = context;
			Encoding = Encoding.Default;
		}
		
		public void Listen(string host, int port, Func<Socket, string, string> callback) {
			var listenSocket = Context.CreateSocket();
			listenSocket.Listen(host, port, delegate (Socket clientSocket) {
				var clientStream = clientSocket.GetSocketStream();
				clientStream.Read(delegate (ByteBuffer buffer) {
					string str = Encoding.GetString(buffer.Bytes, buffer.Position, buffer.Length);
					var ret = callback(clientSocket, str);
					clientStream.Write(Encoding.GetBytes(ret));
				}, delegate (Exception exception) {
				}, delegate {
				});
			});
		}
		
		public void Listen(string host, int port, Func<Socket, IdentRequest, IdentResponse> callback) {
			Listen(host, port, delegate (Socket socket, string requestString) {
				var req = IdentRequest.Parse(requestString);
				return callback(socket, req).ToString();
			});
		}
	}
}

