using System;
using System.Collections.Generic;
using System.Text;

namespace ServerApi.Dal
{
	public class Message
	{
		public long Id { get; set; }
		public string ReceiverUser { get; set; }
		public string DecryptedText { get; set; }
	}
}
