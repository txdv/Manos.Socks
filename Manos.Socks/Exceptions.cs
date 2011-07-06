using System;
using System.Runtime;
using System.Runtime.Serialization;

namespace Manos.Socks
{
	[Serializable]
	public class SocksException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="T:SocksException"/> class
		/// </summary>
		public SocksException()
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="T:SocksException"/> class
		/// </summary>
		/// <param name="message">A <see cref="T:System.String"/> that describes the exception. </param>
		public SocksException(string message)
			: base (message)
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="T:SocksException"/> class
		/// </summary>
		/// <param name="message">A <see cref="T:System.String"/> that describes the exception. </param>
		/// <param name="inner">The exception that is the cause of the current exception. </param>
		public SocksException(string message, Exception inner)
			: base (message, inner)
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="T:SocksException"/> class
		/// </summary>
		/// <param name="context">The contextual information about the source or destination.</param>
		/// <param name="info">The object that holds the serialized object data.</param>
		protected SocksException(SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
	
	
	[Serializable]
	public class Socks4Exception : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="T:Socks4Exception"/> class
		/// </summary>
		public Socks4Exception ()
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="T:Socks4Exception"/> class
		/// </summary>
		/// <param name="message">A <see cref="T:System.String"/> that describes the exception. </param>
		public Socks4Exception (string message) : base (message)
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="T:Socks4Exception"/> class
		/// </summary>
		/// <param name="message">A <see cref="T:System.String"/> that describes the exception. </param>
		/// <param name="inner">The exception that is the cause of the current exception. </param>
		public Socks4Exception (string message, Exception inner) : base (message, inner)
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="T:Socks4Exception"/> class
		/// </summary>
		/// <param name="context">The contextual information about the source or destination.</param>
		/// <param name="info">The object that holds the serialized object data.</param>
		protected Socks4Exception (SerializationInfo info, StreamingContext context) : base (info, context)
		{
		}
	}
}