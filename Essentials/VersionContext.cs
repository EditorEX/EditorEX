using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterEditor.Essentials
{
	public class VersionContext
	{
		public VersionContext(Version version) 
		{
			Version = version;
		}

		public Version Version { get; }
	}
}
