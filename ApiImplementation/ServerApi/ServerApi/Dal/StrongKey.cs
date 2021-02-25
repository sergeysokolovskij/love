using System;
using System.Collections.Generic;
using System.Text;

namespace ServerApi.Dal
{
	public class StrongKey
	{
		public long Id { get; set; }
		public byte[] Cypher { get; set; }
	}
}
