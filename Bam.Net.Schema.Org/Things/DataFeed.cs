using System;

namespace Bam.Net.Schema.Org
{
	///<summary>A single feed providing structured information about one or more entities or topics.</summary>
	public class DataFeed: Dataset
	{
		///<summary>An item within in a data feed. Data feeds may have many elements.</summary>
		public ThisOrThat<Text , DataFeedItem , Thing> DataFeedElement {get; set;}
	}
}
