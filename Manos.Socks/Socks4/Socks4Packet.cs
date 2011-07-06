using System;
using System.IO;
using System.Text;

namespace Manos.Socks
{
	public class Socks4UserPacket : Socks4Packet
	{
		public string UserId { get; set; }
		
		public Socks4UserPacket()
		{
			UserId = string.Empty;
		}
		
		public Socks4UserPacket(byte[] packet, int startIndex, int count)
			: base(packet, startIndex, count)
		{
			UserId = Encoding.ASCII.GetString(packet, startIndex + 8, count - 9);
		}
		
		public override byte[] Serialize()
		{
			byte[] prefix = base.Serialize();
            byte[] userIdBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(UserId);
			byte[] packet = new byte[prefix.Length + userIdBytes.Length + 1];
            
			prefix.CopyTo(packet, 0);
			
			userIdBytes.CopyTo(packet, 8);
			// null (byte with all zeros) terminator for userId
            packet[prefix.Length + userIdBytes.Length] = 0x00;
			
			return packet;
		}
	}
	
	public class Socks4Packet
	{
		public byte Version { get; set; }
		public byte Command { get; set; }
		public System.Net.IPAddress IPAddress { get; set; }
		public int Port { get; set; }
		
		public bool ValidVersion {
			get {
				return Version == VERSION_NUMBER;
			}
		}
		
		public Socks4Packet()
		{
			IPAddress = System.Net.IPAddress.Any;
		}
		
		public Socks4Packet(byte[] packet, int startIndex, int count)
		{
			Version = packet[startIndex + 0];
			Command = packet[startIndex + 1];
			Port = ((int)(packet[startIndex + 2]) << 8) | packet[startIndex + 3];
			IPAddress = new System.Net.IPAddress(BitConverter.ToInt64(packet, startIndex + 4));
		}
		
		public virtual byte[] Serialize()
		{
			byte[] data = new byte[8];
			data[0] = Version;
			data[1] = Command;
			GetDestinationPortBytes(Port).CopyTo(data, 2);
			IPAddress.GetAddressBytes().CopyTo(data, 4);
			return data;
		}
		
        public const byte VERSION_NUMBER = 4;
		
        public const byte CMD_CONNECT = 0x01;
        public const byte CMD_BIND = 0x02;
		
        public const byte CMD_REPLY_REQUEST_GRANTED = 90;
        public const byte CMD_REPLY_REQUEST_REJECTED_OR_FAILED = 91;
        public const byte CMD_REPLY_REQUEST_REJECTED_CANNOT_CONNECT_TO_IDENTD = 92;
        public const byte CMD_REPLY_REQUEST_REJECTED_DIFFERENT_IDENTD = 93;
		
		public static bool CheckVersion(byte[] packet)
		{
			return CheckVersion(packet, 0);
		}
		
		public static bool CheckVersion(byte[] packet, int position)
		{
			if (packet.Length < position) {
				return false;	
			} else {
				return (packet[position] == VERSION_NUMBER);
			}
		}
		
		public static Socks4Packet GetPacketInfo(byte[] packet, int startIndex, int count)
		{
			if (count == 8) {
				return new Socks4Packet(packet, startIndex, count);
			} else if (count > 0) {
				return GetUserPacketInfo(packet, startIndex, count);
			} else {
				return null;
			}
		}
			
		public static Socks4UserPacket GetUserPacketInfo(byte[] packet, int startIndex, int count)
		{
			return new Socks4UserPacket(packet, startIndex, count);
		}
		
		public static byte[] CreatePacket(byte command) 
		{
			var packet = new Socks4Packet() { Command = command };
			return packet.Serialize();
		}
		
		public static byte[] CreatePacket(byte version, byte command, string host, int port)
		{
			return CreatePacket(version, command, System.Net.IPAddress.Parse(host), port);
		}
		
		public static byte[] CreatePacket(byte version, byte command, System.Net.IPAddress host, int port)
		{
			var packet = new Socks4Packet() { Version = version, Command = command, IPAddress = host, Port = port };
			return packet.Serialize();
		}
		
		public static byte[] CreatePacket(System.Net.IPAddress address, int port, string userId)
		{
			var packet = new Socks4UserPacket() {
				Version = VERSION_NUMBER,
				Command = CMD_CONNECT,
				IPAddress = address,
				Port = port,
				UserId = userId
			};
			return packet.Serialize();
		}
		
		public static byte[] CreatePacket(string host, int port, string userId)
		{
			return CreatePacket(System.Net.IPAddress.Parse(host), port, userId);
		}
		
        private static byte[] GetIPAddressBytes(string destinationHost)
        {
            System.Net.IPAddress ipAddr = null;

            //  if the address doesn't parse then try to resolve with dns
            if (!System.Net.IPAddress.TryParse(destinationHost, out ipAddr)) {
                try {
                    ipAddr = System.Net.Dns.GetHostEntry(destinationHost).AddressList[0];
                } catch (Exception ex) {
                    throw new Exception(String.Format("A error occurred while attempting to DNS resolve the host name {0}.", destinationHost), ex);
                }
            }
           
            // return address bytes
            return ipAddr.GetAddressBytes();            
        }

        /// <summary>
        /// Translate the destination port value to a byte array.
        /// </summary>
        /// <param name="value">Destination port.</param>
        /// <returns>Byte array representing an 16 bit port number as two bytes.</returns>
        private static byte[] GetDestinationPortBytes(int value)
        {
            byte[] array = new byte[2];
            array[0] = Convert.ToByte(value / 256);
            array[1] = Convert.ToByte(value % 256);
            return array;
        }
		
	}
}

