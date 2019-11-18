using System;
using System.Collections.Generic;
using System.Linq;

namespace JeffFerguson.Gepsio {
	internal interface IXbrlFragment {
		/// <summary>
		/// A collection of <see cref="Context"/> objects representing all contexts found in the fragment.
		/// </summary>
		IEnumerable< Context > Contexts { get; }
		ITaxonomy Taxonomy { get; }
		/// <summary>
		/// A collection of <see cref="Fact"/> objects representing all facts found in the fragment.
		/// </summary>
		IEnumerable< Fact > Facts { get; }
		/// <summary>
		/// A collection of <see cref="FootnoteLink"/> objects representing all footnote links
		/// found in the fragment.
		/// </summary>
		IEnumerable< FootnoteLink > FootnoteLinks { get; }
		/// <summary>
		/// A collection of role references found in the fragment.
		/// </summary>
		IEnumerable< RoleReference > RoleReferences { get; }
		/// <summary>
		/// A collection of arcrole references found in the fragment.
		/// </summary>
		IEnumerable< ArcroleReference > ArcroleReferences { get; }
		/// <summary>
		/// Returns a reference to the context having the supplied context ID.
		/// </summary>
		/// <param name="ContextId">
		/// The ID of the context to return.
		/// </param>
		/// <returns>
		/// A reference to the context having the supplied context ID.
		/// A null is returned if no contexts with the supplied context ID is available.
		/// </returns>
		Context GetContext(string ContextId);
		/// <summary>
		/// Returns a reference to the unit having the supplied unit ID.
		/// </summary>
		/// <param name="UnitId">
		/// The ID of the unit to return.
		/// </param>
		/// <returns>
		/// A reference to the unit having the supplied unit ID.
		/// A null is returned if no units with the supplied unit ID is available.
		/// </returns>
		Unit GetUnit(string UnitId);
		/// <summary>
		/// Returns a reference to the fact having the supplied fact ID.
		/// </summary>
		/// <param name="FactId">
		/// The ID of the fact to return.
		/// </param>
		/// <returns>
		/// A reference to the fact having the supplied fact ID.
		/// A null is returned if no facts with the supplied fact ID is available.
		/// </returns>
		Item GetFact(string FactId);
		/// <summary>
		/// Gets the schema associated with a given namespace prefix.
		/// </summary>
		/// <param name="Prefix">
		/// The namespace prefix whose schema should be returned.
		/// </param>
		/// <returns>
		/// A reference to the XBRL schema containing the specified namespace prefix. A null
		/// is returned if the given namespace prefix is not found in any of the XBRL schemas.
		/// </returns>
		XbrlSchema GetXbrlSchemaForPrefix(string Prefix);
		bool UrlReferencesFragmentDocument(HyperlinkReference LocatorHref);
	}
}