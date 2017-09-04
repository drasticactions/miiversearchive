using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace MiiverseArchive.Tools.Extensions
{
	public static class HtmlAgilityExtensions
	{
		public static HtmlNode GetBody(this HtmlDocument document)
			=> document.DocumentNode.GetElementByTagName("html").GetElementByTagName("body");

		public static string[] GetClassName(this HtmlNode node)
			=> node.GetAttributeValue("class", string.Empty).Split(new char[] { ' ' });

		public static bool HasClassName(this HtmlNode node, string className)
			=> node.GetClassName().Contains(className);

		public static IEnumerable<HtmlNode> GetElementsByClassName(this HtmlNodeCollection nodes, string className)
			=> nodes.Where(n => n.GetAttributeValue("class", string.Empty).Split(new char[] { ' ' }).Any(s => s == className));

		public static IEnumerable<HtmlNode> GetElementsByClassName(this HtmlNode node, string className)
			=> node.ChildNodes.GetElementsByClassName(className);

		public static HtmlNode GetElementByClassName(this HtmlNode node, string className)
			=> node.GetElementsByClassName(className).Single();

		public static void MatchClassName(this HtmlNodeCollection nodes, string className, Action<HtmlNode> some = null, Action none = null)
		{
			var elements = nodes.GetElementsByClassName(className);
			if (elements.Count() != 0 && some != null)
			{
				some(elements.Single());
			}
			else if (none != null)
			{
				none();
			}
		}

		public static IEnumerable<HtmlNode> GetElementsByTagName(this HtmlNodeCollection nodes, string tagName)
			=> nodes.Where(n => n.Name == tagName);

		public static IEnumerable<HtmlNode> GetElementsByTagName(this HtmlNode node, string tagName)
			=> node.ChildNodes.GetElementsByTagName(tagName);

		public static HtmlNode GetElementByTagName(this HtmlNode node, string tagName)
			=> node.GetElementsByTagName(tagName).Single();

		public static Uri GetImageSource(this HtmlNode node)
			=> new Uri(node.GetElementByTagName("img").GetAttributeValue("src", string.Empty));

		public static uint GetInnerTextAsUInt32(this HtmlNode node)
			=> Convert.ToUInt32(node.InnerText);
	}
}
