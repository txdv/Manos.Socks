using System;

using Manos.IO;
using System.Text;

namespace Manos.Socks
{
	public class IdentClient
	{
		private Context Context { get; set; }
		
		public string Server { get; set; }
		public int Port { get; set; }
		public Encoding Encoding { get; set; }
		
		public IdentClient(Context context, string server)
			: this(context, server, 113)
		{
		}
		
		public IdentClient(Context context, string server, int port)
		{
			Context = context;
			Server = server;
			Port = port;
			Encoding = Encoding.Default;
		}
		
		public void RawRequest(IdentRequest request, Action<string> response)
		{
			RawRequest(request.Outbound, request.Inbound, response);
		}
			
		public void RawRequest(int outbound, int inbound, Action<string> response)
		{
			var socket = Context.CreateSocket();
			socket.Connect(Server, Port, delegate {
				var stream = socket.GetSocketStream();
				
				stream.Read(delegate (ByteBuffer bytebuffer) {
					response(Encoding.GetString(bytebuffer.Bytes, bytebuffer.Position, bytebuffer.Length));
					socket.Close();
				}, delegate (Exception e) {
				}, delegate {
				});
				
				stream.Write(Encoding.GetBytes(string.Format("{0}, {1}\r\n", outbound, inbound)));
			});
		}
		
		public void Request(IdentRequest request, Action<IdentRequest> response)
		{
			Request(request.Outbound, request.Inbound, response);
		}
		
		public void Request(int outbound, int inbound, Action<IdentRequest> response) {
			RawRequest(outbound, inbound, delegate (string stringResponse) {
				response(IdentResponse.Parse(stringResponse));
			});
		}
	}
}

