using JeffFerguson.Gepsio.IoC;
using JeffFerguson.Gepsio.Validators.Xbrl2Dot1;
using JeffFerguson.Gepsio.Xml.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JeffFerguson.Gepsio
{
	/// <summary>
	/// A fragment of XBRL data. A collection of fragments is available in the <see cref="XbrlDocument"/>
	/// class.
	/// </summary>
	/// <remarks>
	/// <para>
	/// An XBRL fragment is a fragment of XBRL data having an xbrl tag as its root. In the generic case,
	/// an XBRL document will have an xbrl tag as the root tag of the XML document, and, in this case,
	/// the entire XBRL document is one large XBRL fragment. However, section 4.1 of the XBRL 2.1 Specification
	/// makes provisions for multiple XBRL fragments to be stored in a single document:
	/// </para>
	/// <para>
	/// "If multiple 'data islands' of XBRL mark-up are included in a larger document, the xbrl element is
	/// the container for each [fragment]."
	/// </para>
	/// <para>
	/// Gepsio supports this notion by defining an XBRL document containing a collection of one or more
	/// XBRL fragments, as in the following code sample:
	/// </para>
	/// <code>
	/// var myDocument = new XbrlDocument();
	/// myDocument.Load("myxbrldoc.xml");
	/// foreach(var currentFragment in myDocument.XbrlFragments)
	/// {
	///     // XBRL data is available from the "currentFragment" variable
	/// }
	/// </code>
	/// <para>
	/// In the vast majority of cases, an XBRL document will be an XML document with the xbrl tag at its
	/// root, and, as a result, the <see cref="XbrlDocument"/> uses to load the XBRL document will have
	/// a single <see cref="XbrlFragment"/> in the document's fragments container. Consider, however, the
	/// possibility of having more than one fragment in a document, in accordance of the text in section
	/// 4.1 of the XBRL 2.1 Specification.
	/// </para>
	/// </remarks>
	public class XbrlFragment : IXbrlFragment, IValidationHandler {
		private LinkbaseDocumentCollection thisLinkbaseDocuments;

		/// <summary>
		/// The delegate used to handle events fired by the class.
		/// </summary>
		/// <param name="sender">
		/// The sender of the event.
		/// </param>
		/// <param name="e">
		/// Event arguments.
		/// </param>
		public delegate void XbrlEventHandler( object sender, EventArgs e );

		/// <summary>
		/// Event fired after a document has been loaded.
		/// </summary>
		public event XbrlEventHandler Loaded;

		/// <summary>
		/// Event fired after all XBRL validation has been completed.
		/// </summary>
		public event XbrlEventHandler Validated;

		private INamespaceManager thisNamespaceManager;
		private List<Context> _contexts;
		private List< FootnoteLink > _footnoteLinks;
		private List< RoleReference > _roleReferences;
		private List< ArcroleReference > _arcroleReferences;

		/// <summary>
		/// A reference to the <see cref="XbrlDocument"/> instance in which the fragment
		/// was contained.
		/// </summary>
		public XbrlDocument Document { get; private set; }

		/// <summary>
		/// The root XML node for the XBRL fragment.
		/// </summary>
		internal INode XbrlRootNode { get; private set; }

		/// <summary>
		/// A collection of <see cref="Context"/> objects representing all contexts found in the fragment.
		/// </summary>
		public IEnumerable<Context> Contexts  => _contexts;

		/// <summary>
		/// A collection of <see cref="XbrlSchema"/> objects representing all schemas found in the fragment.
		/// </summary>
		public XbrlSchemaCollection Schemas { get; private set; }
		ITaxonomy IXbrlFragment.Taxonomy => this.Schemas;

		/// <summary>
		/// A collection of <see cref="Fact"/> objects representing all facts found in the fragment.
		/// </summary>
		public FactCollection Facts { get; private set; }
		IEnumerable< Fact > IXbrlFragment.Facts => this.Facts;

		/// <summary>
		/// A collection of <see cref="Unit"/> objects representing all units found in the fragment.
		/// </summary>
		public List<Unit> Units { get; private set; }

		/// <summary>
		/// A collection of <see cref="FootnoteLink"/> objects representing all footnote links
		/// found in the fragment.
		/// </summary>
		public IEnumerable< FootnoteLink > FootnoteLinks => this._footnoteLinks;

		/// <summary>
		/// Evaluates to true if the fragment contains no XBRL validation errors. Evaluates to
		/// false if the fragment contains at least one XBRL validation error.
		/// </summary>
		public bool IsValid {
			get {
				if( this.ValidationErrors == null )
					return true;
				if( this.ValidationErrors.Count == 0 )
					return true;
				return false;
			}
		}

		/// <summary>
		/// A collection of all validation errors found while validating the fragment.
		/// </summary>
		public List<ValidationError> ValidationErrors { get; private set; }

		internal INamespaceManager NamespaceManager { get; private set; }

		/// <summary>
		/// A collection of role references found in the fragment.
		/// </summary>
		public IEnumerable< RoleReference > RoleReferences => this._roleReferences;

		/// <summary>
		/// A collection of arcrole references found in the fragment.
		/// </summary>
		public IEnumerable< ArcroleReference > ArcroleReferences => this._arcroleReferences;

		//-------------------------------------------------------------------------------
		//-------------------------------------------------------------------------------
		internal XbrlFragment( XbrlDocument ParentDocument, INamespaceManager namespaceManager, INode XbrlRootNode ) {
			this.Document = ParentDocument;
			this.NamespaceManager = namespaceManager;
			this.XbrlRootNode = XbrlRootNode;
			this.Schemas = new XbrlSchemaCollection( );
			this.ValidationErrors = new List<ValidationError>( );
			CreateNamespaceManager( );
			//---------------------------------------------------------------------------
			// Load.
			//---------------------------------------------------------------------------
			ReadSchemaLocationAttributes( );
			ReadLinkbaseReferences( );
			ReadTaxonomySchemaReferences( );
			ReadRoleReferences( );
			ReadArcroleReferences( );
			ReadContexts( );
			ReadUnits( );
			ReadFacts( );
			ReadFootnoteLinks( );
			Loaded?.Invoke( this, null );
			//---------------------------------------------------------------------------
			// Validate.
			//---------------------------------------------------------------------------
			var validator = new Xbrl2Dot1Validator( );
			validator.Validate( this, this );
			Validated?.Invoke( this, null );
		}

		public void AddValidationError( ValidationError validationError ) {
			this.ValidationErrors.Add( validationError );
		}

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
		public Context GetContext( string ContextId ) {
			foreach( Context CurrentContext in this.Contexts ) {
				if( CurrentContext.Id == ContextId )
					return CurrentContext;
			}
			return null;
		}

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
		public Unit GetUnit( string UnitId ) {
			foreach( Unit CurrentUnit in this.Units ) {
				if( CurrentUnit.Id == UnitId )
					return CurrentUnit;
			}
			return null;
		}

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
		public Item GetFact( string FactId ) {
			var matchingFact = this.Facts.GetFactById( FactId );
			if( matchingFact == null )
				return null;
			return matchingFact as Item;
		}

		/// <summary>
		/// Gets the URI associated with a given namespace prefix.
		/// </summary>
		/// <param name="Prefix">
		/// The namespace prefix whose URI should be returned.
		/// </param>
		/// <returns>
		/// The namespace URI associated with the specified namespace prefix.
		/// </returns>
		public string GetUriForPrefix( string Prefix ) {
			return thisNamespaceManager.LookupNamespace( Prefix );
		}

		/// <summary>
		/// Gets the namespace prefix associated with a given URI.
		/// </summary>
		/// <param name="Uri">
		/// The URI whose namespace prefix should be returned.
		/// </param>
		/// <returns>
		/// The namespace prefix associated with the specified namespace URI.
		/// </returns>
		public string GetPrefixForUri( string Uri ) {
			return thisNamespaceManager.LookupPrefix( Uri );
		}

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
		public XbrlSchema GetXbrlSchemaForPrefix( string Prefix ) {
			string Uri = GetUriForPrefix( Prefix );
			if( Uri == null )
				return null;
			if( Uri.Length == 0 )
				return null;
			return this.Schemas.GetSchemaFromTargetNamespace( Uri, this, this.Document.Path );
		}

		/// <summary>
		/// Finds the <see cref="RoleType"/> object having the given ID.
		/// </summary>
		/// <param name="RoleTypeId">
		/// The ID of the role type to find.
		/// </param>
		/// <returns>
		/// The <see cref="RoleType"/> object having the given ID, or null if no
		/// object can be found.
		/// </returns>
		public RoleType GetRoleType( string RoleTypeId ) {
			return this.Schemas.GetRoleType( RoleTypeId );
		}

        /// <summary>
		/// Read and process any schemaLocation attributes found in the XBRL root node.
		/// </summary>
		private void ReadSchemaLocationAttributes() {
			foreach( IAttribute currentAttribute in this.XbrlRootNode.Attributes ) {
				if( (currentAttribute.NamespaceURI.Equals( NamespaceUri.XmlSchemaInstanceUri ) == true) && (currentAttribute.LocalName.Equals( "schemaLocation" ) == true) ) {
					var attributeValue = currentAttribute.Value.Trim( );
					if( string.IsNullOrEmpty( attributeValue ) == false )
						ProcessSchemaLocationAttributeValue( attributeValue );
				}
			}
		}

		/// <summary>
		/// Read any linkbase references contained directly in the fragment.
		/// </summary>
		/// <remarks>
		/// Linkbase references found as children of XBRL elements is allowed by section
		/// 4.3 of the XBRL specification.
		/// </remarks>
		private void ReadLinkbaseReferences() {
			thisLinkbaseDocuments = new LinkbaseDocumentCollection( );
			thisLinkbaseDocuments.ReadLinkbaseReferences( this.XbrlRootNode.BaseURI, this.XbrlRootNode );
		}

		/// <summary>
		/// Process a value found in a schemaLocation attribute.
		/// </summary>
		/// <remarks>
		/// This string is formatted as a set of whitespace-delimited pairs. The first URI reference in each pair is a namespace name,
		/// and the second is the location of a schema that describes that namespace.
		/// </remarks>
		/// <param name="schemaLocationAttributeValue">
		/// The value of a schemaLocation attribute.
		/// </param>
		private void ProcessSchemaLocationAttributeValue( string schemaLocationAttributeValue ) {
			var NamespacesAndLocations = schemaLocationAttributeValue.Split( new char[0], StringSplitOptions.RemoveEmptyEntries );
			for( var index = 0; index < NamespacesAndLocations.Length; index += 2 ) {
				ProcessSchemaNamespaceAndLocation( NamespacesAndLocations[index], NamespacesAndLocations[index + 1] );
			}
		}

		private void ProcessSchemaNamespaceAndLocation( string schemaNamespace, string schemaLocation ) {
			var newSchema = new XbrlSchema( this.Document.Path, schemaLocation, string.Empty ){ ValidationHandler = this };
			if( newSchema.SchemaRootNode != null )
				AddSchemaToSchemaList( newSchema );
		}

		/// <summary>
		/// Adds a schema to the schema collection.
		/// </summary>
		/// <remarks>
		/// The supplied schema will not be added if its target namespace is already in the list. This will help
		/// with some of the XBRL instance documents in the XBRL Conformance Suite which uses both the "schemaLocation"
		/// attribute as well as a "schemaRef" node to specify the same schema. The "301-01-IdScopeValid.xml"
		/// instance document in the XBRL-CONF-CR5-2012-01-24 suite is one such example.
		/// </remarks>
		/// <param name="schemaToAdd">
		/// The schema to be added.
		/// </param>
		private void AddSchemaToSchemaList( XbrlSchema schemaToAdd ) {
			this.Schemas.Add( schemaToAdd );
		}

		/// <summary>
		/// Read any process any schemaRef nodes beneath the main XBRL node.
		/// </summary>
		private void ReadTaxonomySchemaReferences() {
			string LinkbaseNamespacePrefix = thisNamespaceManager.LookupPrefix( NamespaceUri.XbrlLinkbaseNamespaceUri );
			StringBuilder XPathExpressionBuilder = new StringBuilder( );
			XPathExpressionBuilder.AppendFormat( "//{0}:schemaRef", LinkbaseNamespacePrefix );
			string XPathExpression = XPathExpressionBuilder.ToString( );
			INodeList SchemaRefNodes = this.XbrlRootNode.SelectNodes( XPathExpression, thisNamespaceManager );
			foreach( INode SchemaRefNode in SchemaRefNodes )
				ReadTaxonomySchemaReference( SchemaRefNode );
		}

		//-------------------------------------------------------------------------------
		//-------------------------------------------------------------------------------
		private void ReadRoleReferences() {
			this._roleReferences = new List<RoleReference>( );
			string LinkbaseNamespacePrefix = thisNamespaceManager.LookupPrefix( NamespaceUri.XbrlLinkbaseNamespaceUri );
			StringBuilder XPathExpressionBuilder = new StringBuilder( );
			XPathExpressionBuilder.AppendFormat( "//{0}:roleRef", LinkbaseNamespacePrefix );
			string XPathExpression = XPathExpressionBuilder.ToString( );
			INodeList RoleRefNodes = this.XbrlRootNode.SelectNodes( XPathExpression, thisNamespaceManager );
			foreach( INode RoleRefNode in RoleRefNodes )
				this._roleReferences.Add( new RoleReference( RoleRefNode ) );
		}

		//-------------------------------------------------------------------------------
		//-------------------------------------------------------------------------------
		private void ReadArcroleReferences() {
			this._arcroleReferences = new List<ArcroleReference>( );
			string LinkbaseNamespacePrefix = thisNamespaceManager.LookupPrefix( NamespaceUri.XbrlLinkbaseNamespaceUri );
			StringBuilder XPathExpressionBuilder = new StringBuilder( );
			XPathExpressionBuilder.AppendFormat( "//{0}:arcroleRef", LinkbaseNamespacePrefix );
			string XPathExpression = XPathExpressionBuilder.ToString( );
			INodeList ArcroleRefNodes = this.XbrlRootNode.SelectNodes( XPathExpression, thisNamespaceManager );
			foreach( INode ArcroleRefNode in ArcroleRefNodes )
				this._arcroleReferences.Add( new ArcroleReference( ArcroleRefNode ) );
		}

		//-------------------------------------------------------------------------------
		//-------------------------------------------------------------------------------
		private void ReadTaxonomySchemaReference( INode SchemaRefNode ) {
			string HrefAttributeValue = SchemaRefNode.GetAttributeValue( Xlink.XlinkNode.xlinkNamespace, "href" );
			string Base = SchemaRefNode.GetAttributeValue( NamespaceUri.XmlNamespaceUri, "base" );
			var newSchema = new XbrlSchema( this.Document.Path, HrefAttributeValue, Base ){ ValidationHandler = this };
			if( newSchema.SchemaRootNode != null )
				AddSchemaToSchemaList( newSchema );
		}

		//-------------------------------------------------------------------------------
		//-------------------------------------------------------------------------------
		private void CreateNamespaceManager() {
			thisNamespaceManager = Container.Resolve<INamespaceManager>( );
			thisNamespaceManager.AddNamespace( "instance", this.XbrlRootNode.NamespaceURI );
			foreach( IAttribute CurrentAttribute in this.XbrlRootNode.Attributes ) {
				if( CurrentAttribute.Prefix == "xmlns" )
					thisNamespaceManager.AddNamespace( CurrentAttribute.LocalName, CurrentAttribute.Value );
			}
		}

		//-------------------------------------------------------------------------------
		//-------------------------------------------------------------------------------
		private void ReadContexts() {
			_contexts = new List<Context>( );
			INodeList ContextNodes = this.XbrlRootNode.SelectNodes( "//instance:context", thisNamespaceManager );
			foreach( INode ContextNode in ContextNodes )
				_contexts.Add( new Context( this, ContextNode ) );
		}

		/// <summary>
		/// Reads all of the facts in the XBRL fragment and creates an object for each.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Element instances can be any of the following:
		/// </para>
		/// <para>
		/// <list type="bullet">
		/// <item>
		/// an item, represented in an XBRL schema by an element with a substitution group of "item"
		/// and represented by an <see cref="Item"/> object
		/// </item>
		/// <item>
		/// a tuple, represented in an XBRL schema by an element with a substitution group of "tuple"
		/// and represented by an <see cref="Tuple"/> object
		/// </item>
		/// </list>
		/// </para>
		/// </remarks>
		private void ReadFacts() {
			this.Facts = new FactCollection {
				Capacity = this.XbrlRootNode.ChildNodes.Count
			};
			foreach( INode CurrentChild in this.XbrlRootNode.ChildNodes ) {
				var CurrentFact = Fact.Create( this, CurrentChild );
				if( CurrentFact != null )
					this.Facts.Add( CurrentFact );
			}
			this.Facts.TrimExcess( );
		}

		//-------------------------------------------------------------------------------
		//-------------------------------------------------------------------------------
		private bool IsTaxonomyNamespace( string CandidateNamespace ) {
			var matchingSchema = this.Schemas.GetSchemaFromTargetNamespace( CandidateNamespace, this, this.Document.Path );
			if( matchingSchema == null )
				return false;
			return true;
		}

		//-------------------------------------------------------------------------------
		//-------------------------------------------------------------------------------
		private void ReadUnits() {
			this.Units = new List<Unit>( );
			INodeList UnitNodes = this.XbrlRootNode.SelectNodes( "//instance:unit", thisNamespaceManager );
			foreach( INode UnitNode in UnitNodes )
				this.Units.Add( new Unit( this, UnitNode, thisNamespaceManager ) );
		}

		//-------------------------------------------------------------------------------
		//-------------------------------------------------------------------------------
		private void ReadFootnoteLinks() {
			_footnoteLinks = new List<FootnoteLink>( );
			string LinkbaseNamespacePrefix = thisNamespaceManager.LookupPrefix( NamespaceUri.XbrlLinkbaseNamespaceUri );
			StringBuilder XPathExpressionBuilder = new StringBuilder( );
			XPathExpressionBuilder.AppendFormat( "//{0}:footnoteLink", LinkbaseNamespacePrefix );
			string XPathExpression = XPathExpressionBuilder.ToString( );
			INodeList FootnoteLinkNodes = this.XbrlRootNode.SelectNodes( XPathExpression, thisNamespaceManager );
			foreach( INode FootnoteLinkNode in FootnoteLinkNodes )
				_footnoteLinks.Add( new FootnoteLink( this, FootnoteLinkNode ) );
		}

		//-------------------------------------------------------------------------------
		//-------------------------------------------------------------------------------
		public bool UrlReferencesFragmentDocument( HyperlinkReference Href ) {
			if( Href.UrlSpecified == false )
				return false;
			string DocFullPath = Path.GetFullPath( this.Document.Filename );
			string HrefFullPathString;
			if( Href.Url.IndexOf( Path.DirectorySeparatorChar ) == -1 )
				HrefFullPathString = this.Document.Path + Path.DirectorySeparatorChar + Href.Url;
			else
				HrefFullPathString = Href.Url;
			string HrefFullPath = Path.GetFullPath( HrefFullPathString );
			if( DocFullPath.Equals( HrefFullPath ) == true )
				return true;
			return false;
		}

		//-------------------------------------------------------------------------------
		//-------------------------------------------------------------------------------
		private Item LocateFact( Locator FactLocator ) {
			if( FactLocator == null )
				return null;
			foreach( Item CurrentFact in this.Facts ) {
				if( CurrentFact.Name.Equals( FactLocator.HrefResourceId ) == true )
					return CurrentFact;
			}
			return null;
		}
	}
}
