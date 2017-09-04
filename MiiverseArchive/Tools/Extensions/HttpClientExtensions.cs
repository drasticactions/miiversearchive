using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace MiiverseArchive.Tools.Extensions
{
	public static class HttpClientExtensions
	{
		public static Task<HttpResponseMessage> HeadAsync(this HttpClient client, string uriString)
			=> client.SendAsync(new HttpRequestMessage(HttpMethod.Head, uriString));

		public static Task<HttpResponseMessage> HeadAsync(this HttpClient client, Uri uri)
			=> client.SendAsync(new HttpRequestMessage(HttpMethod.Head, uri));

		public static Task<Stream> ToTaskOfStream(this Task<HttpResponseMessage> response)
			=> response.ContinueWith(r => r.Result.Content.ReadAsStreamAsync()).Unwrap();

		public static Task<string> ToTaskOfString(this Task<HttpResponseMessage> response)
			=> response.ContinueWith(r => r.Result.Content.ReadAsStringAsync()).Unwrap();
	}
}
