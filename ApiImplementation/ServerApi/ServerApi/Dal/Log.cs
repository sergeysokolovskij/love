using System;
using System.Collections.Generic;
using System.Text;

namespace ServerApi.Dal
{
	public class Log
	{
		public long Id { get; set; }
		public string LogText { get; set; }
		public DateTime Data { get; set; }
	}
}
