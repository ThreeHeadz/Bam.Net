using System;

namespace Bam.Net.Schema.Org
{
	///<summary>A work of art that is primarily visual in character.</summary>
	public class VisualArtwork: CreativeWork
	{
		///<summary>The number of copies when multiple copies of a piece of artwork are produced - e.g. for a limited edition of 20 prints, 'artEdition' refers to the total number of copies (in this example "20").</summary>
		public ThisOrThat<Integer , Text> ArtEdition {get; set;}
		///<summary>The material used. (e.g. Oil, Watercolour, Acrylic, Linoprint, Marble, Cyanotype, Digital, Lithograph, DryPoint, Intaglio, Pastel, Woodcut, Pencil, Mixed Media, etc.) Supersedes material.</summary>
		public ThisOrThat<Text , URL> ArtMedium {get; set;}
		///<summary>e.g. Painting, Drawing, Sculpture, Print, Photograph, Assemblage, Collage, etc.</summary>
		public ThisOrThat<Text , URL> Artform {get; set;}
		///<summary>The supporting materials for the artwork, e.g. Canvas, Paper, Wood, Board, etc. Supersedes surface.</summary>
		public ThisOrThat<Text , URL> ArtworkSurface {get; set;}
		///<summary>The depth of the item.</summary>
		public ThisOrThat<Distance , QuantitativeValue> Depth {get; set;}
		///<summary>The height of the item.</summary>
		public ThisOrThat<Distance , QuantitativeValue> Height {get; set;}
		///<summary>The width of the item.</summary>
		public ThisOrThat<Distance , QuantitativeValue> Width {get; set;}
	}
}
