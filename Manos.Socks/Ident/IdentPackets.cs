using System;
using System.Text;

namespace Manos.Socks
{
	public class IdentRequest
	{
		public string[] SystemNames = new string[] 	{ 
			"AEGIS", "AIX-PS/2", "AIX/370", "APOLLO", "BS-2000", "CEDAR", "CGW", "CHORUS",
			"CHRYSALIS", "CMOS", "CMS", "COS", "CPIX", "CTOS", "CTSS", "DCN", "DDNOS", "DOMAIN",
			"DOS", "EDX", "ELF", "EMBOS", "EMMOS", "EPOS", "FOONEX", "FUZZ", "GCOS", "GPOS",
			"HDOS", "IMAGEN", "IMPRESS", "INTERCOM", "INTERLISP", "IOS", "IRIX", "ISI-68020",
			"ITS", "LISP", "LISPM", "LOCUS", "MACOS", "MINOS", "MOS", "MPE5", "MSDOS", "MULTICS",
			"MUSIC", "MUSIC/SP", "MVS", "MVS/SP", "NEXUS", "NMS", "NONSTOP", "NOS-2", "NTOS", "OS/2",
			"OS/DDP", "OS4", "OS86", "OSX", "PCDOS", "PERQ/OS", "PLI", "PRIMOS", "PSDOS/MIT",
			"RMX/RDOS", "ROS", "RSX11M", "RTE-A", "SATOPS", "SCO-XENIX/386", "SCS", "SIMP", "SUN",
			"SUN OS 3.5", "SUN OS 4.0", "SWIFT", "TAC", "TANDEM", "TENEX", "TOPS10", "TOPS20", "TOS",
			"TP3010", "TRSDOS", "ULTRIX", "UNIX", "UNIX-BSD", "UNIX-PC", "UNIX-V", "UNIX-V.1", "UNIX-V.2",
			"UNIX-V.3", "UNIX-V1AT", "UNKNOWN", "UT2D", "V", "VM", "VM/370", "VM/CMS", "VM/SP", "VMS",
			"VMS/EUNICE", "VRTX", "WAITS", "WANG", "WIN32", "X11R3", "XDE", "XENIX"
		};
		
		public int Outbound { get; set; }
		public int Inbound { get; set; }
		
		public override string ToString()
		{
			return string.Format("{0},{1}", Outbound, Inbound);
		}
		
		public static IdentRequest Parse(string response)
		{
			try {
				string[] res = response.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
				char[] seperators = new char[] { ',', ' ' };
				var ports = res[0].Split(seperators, StringSplitOptions.RemoveEmptyEntries);
				int outbound = int.Parse(ports[0].Trim());
				int inbound  = int.Parse(ports[1].Trim());
				
				if (res.Length == 1) {
					return new IdentRequest() { Outbound = outbound, Inbound = inbound };
				} else if (res.Length == 3) {
					return new IdentResponseError() { Outbound = outbound, Inbound = inbound, ErrorMessage = res[res.Length - 1].Trim() };
				} else if (res.Length == 4) {
					return new IdentResponseUser() {
						Outbound = outbound,
						Inbound = inbound,
						OperatingSystem = res[res.Length - 2].Trim(),
						User = res[res.Length - 1].Trim()
					};
				}
			} catch {
			}
			return null;
		}
		
		public byte[] Serialize(Encoding encoding)
		{
			return encoding.GetBytes(ToString() + "\r\n");
		}
	}
	
	public abstract class IdentResponse : IdentRequest
	{
		public string Type
		{
			get {
				if (this is IdentResponseError) {
					return "ERROR";
				} else if (this is IdentResponseUser) {
					return "USERID";
				}
				throw new Exception();
			}
		}
		
		public override string ToString()
		{
			return string.Format("{0}:{1}", base.ToString(), Type);
		}
	}
	
	public class IdentResponseError : IdentResponse
	{
		public string ErrorMessage { get; set; }
		
		public override string ToString ()
		{
			return string.Format ("{0}:{1}", base.ToString(), ErrorMessage);
		}
	}
	
	public class IdentResponseUser : IdentResponse
	{
		public string OperatingSystem { get; set; }
		public string User { get; set; }
		
		public override string ToString ()
		{
			return string.Format("{0}:{1}:{2}", base.ToString(), OperatingSystem, User);
		}
	}
}

