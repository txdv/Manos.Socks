using System;
using System.Text;

using Manos.IO;
using Manos.Socks;

using Manos;
using Manos.Http;

namespace Test
{
	class MainClass
	{
		public static void ChainedWebServer(Context context)
		{
			// this creates a webserver on 1082
			// a socks4 server on 1080
			// and a socket which listens on 1081 and tunnels
			// the socket through the socks4 server to the webserver
			Socks4Server socks4server = new Socks4Server(context, context.CreateSocket());
			
			socks4server.Listen("127.0.0.1", 1080, delegate (Socket clientSocket, Socks4Packet requestPacket) {
				if (requestPacket is Socks4UserPacket) {
					var r = requestPacket as Socks4UserPacket;
					
					return (r.UserId == "bentkus");
				} else {
					return false;
				}
			});
			
			// lets just connect another socket and push stuff through
			var socketListen = context.CreateSocket();
			socketListen.Listen("127.0.0.1", 1081, delegate (Socket socket) {
				
				IdentClient ic = new IdentClient(context, "127.0.0.1", 113);
				ic.Request(socket.Port, 1081, delegate (IdentRequest response) {
					Console.WriteLine(response);
				});
				
				var stream = socket.GetSocketStream();
				
				Socks4Socket socks4socket = new Socks4Socket(context, "127.0.0.1", 1080, "bentkus");
				
				socks4socket.Connect("127.0.0.1", 1082, delegate {
					var socks4stream = socks4socket.GetSocketStream();
					socks4stream.Read(delegate (ByteBuffer buffer) {
						stream.Write(buffer);
					}, delegate (Exception e) {
					}, delegate {
						stream.Close();
					});
					
					stream.Read(delegate (ByteBuffer buffer) {
						socks4stream.Write(buffer);
					}, delegate (Exception e) {
					}, delegate {
					});
					
				});
			});
			
			new HttpServer(context, delegate (IHttpTransaction transaction) {
				var res = transaction.Response;
				res.Headers.SetNormalizedHeader("Content-Type", ManosMimeTypes.GetMimeType("big.txt"));
				res.SendFile("big.txt");
				res.End();
			}, context.CreateSocket(), true).Listen("127.0.0.1", 1082);
		}
		
		public static void IdentTest(Context context)
		{
			string os = Environment.OSVersion.Platform.ToString().ToUpper();
			
			Random r = new Random();
			
			IdentServer ids = new IdentServer(context);

			ids.Listen("127.0.0.1", 1080, delegate (Socket socket, IdentRequest request) {
				
				var response = new IdentResponseError() {
					Outbound = request.Outbound,
					Inbound = request.Inbound,
					ErrorMessage = "NO-USER"
				};
				
				if (r.Next(2) == 0) {
					return response;
				}
				
				
				return new IdentResponseUser() {
					Outbound = request.Outbound,
					Inbound  = request.Inbound,
					OperatingSystem = os,
					User = "username"
				};
			});
			
			IdentClient ic = new IdentClient(context, "127.0.0.1", 1080);
			
			context.CreateTimerWatcher(TimeSpan.Zero, TimeSpan.FromMilliseconds(200), delegate {
				ic.Request(new IdentRequest() { 
					Outbound = r.Next(100), 
					Inbound = 1000 + r.Next(5000)
				}, (response) => {
					Console.WriteLine(response);
				});
			}).Start();
			
			context.Start();
		}
		
		public static void Main(string[] args)
		{
			Context context = Manos.IO.Managed.Context.Create();
			// ChainedWebServer(context);
			IdentTest(context);
			context.Start();
		}
	}
}

