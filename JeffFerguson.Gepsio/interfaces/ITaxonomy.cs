using System;
using System.Collections.Generic;
using System.Linq;

using JeffFerguson.Gepsio.Xml.Interfaces;
using JeffFerguson.Gepsio.Xsd;

namespace JeffFerguson.Gepsio {
	/// <summary>
	/// Taxonomy interface.
	/// </summary>
	public interface ITaxonomy {
		/// <summary>
		/// List of all calculation linkbases defined in known schemas of current taxonomy.
		/// </summary>
		IEnumerable< CalculationLinkbaseDocument > CalculationLinkbases { get; }
		/// <summary>
		/// List of all definition linkbases defined in known schemas of current taxonomy.
		/// </summary>
		IEnumerable< DefinitionLinkbaseDocument > DefinitionLinkbases { get; }
		/// <summary>
		/// Taxonomy is defined if at least one schema is loaded.
		/// </summary>
		bool IsDefined { get; }
		/// <summary>
		/// Gets the data type for the supplied node.
		/// </summary>
		/// <param name="node">
		/// The node whose data type is returned.
		/// </param>
		/// <returns>
		/// The data type of the supplied node. A null reference will be returned
		/// if no matching element can be found for the supplied node.
		/// </returns>
		AnyType GetNodeType(INode node);
		/// <summary>
		/// Gets the data type for the supplied attribute.
		/// </summary>
		/// <param name="attribute">
		/// The attribute whose data type is returned.
		/// </param>
		/// <returns>
		/// The data type of the supplied attribute. A null reference will be returned
		/// if no matching element can be found for the supplied attribute.
		/// </returns>
		AnyType GetAttributeType(IAttribute attribute);
		/// <summary>
		/// Locates and element using an element locator.
		/// </summary>
		/// <param name="ElementLocator">
		/// A locator for the element to be found.
		/// </param>
		/// <returns>
		/// A reference to the matching element. A null reference will be returned
		/// if no matching element can be found.
		/// </returns>
		Element LocateElement(Locator ElementLocator);
	}
}