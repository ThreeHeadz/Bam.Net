using System;

namespace Bam.Net.Schema.Org
{
	///<summary>Event type: Sports event.</summary>
	public class SportsEvent: Event
	{
		///<summary>The away team in a sports event.</summary>
		public OneOfThese<SportsTeam , Person> AwayTeam {get; set;}
		///<summary>A competitor in a sports event.</summary>
		public OneOfThese<SportsTeam , Person> Competitor {get; set;}
		///<summary>The home team in a sports event.</summary>
		public OneOfThese<SportsTeam , Person> HomeTeam {get; set;}
	}
}
