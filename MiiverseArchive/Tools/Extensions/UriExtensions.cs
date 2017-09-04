using System;
using System.Collections.Generic;
using System.Linq;

namespace MiiverseArchive.Tools.Extensions
{
	public static class UriExtensions
	{
		public static IEnumerable<KeyValuePair<string, string>> QueryToKeyValuePair(this Uri uri)
			=> uri.Query.Substring(1)
				.Split(new char[] { '&' })
				.Select(kv => new KeyValuePair<string, string>(kv.Substring(0, kv.IndexOf('=')), kv.Substring(kv.IndexOf('=') + 1)));
	}
}
