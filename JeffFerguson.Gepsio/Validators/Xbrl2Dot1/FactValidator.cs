using JeffFerguson.Gepsio;
using JeffFerguson.Gepsio.Xml.Interfaces;
using System.Text;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace JeffFerguson.Gepsio.Validators.Xbrl2Dot1
{
    /// <summary>
    /// A validator for XBRL fact elements, including items and tuples.
    /// </summary>
    /// <remarks>
    /// This validator validates the information within a fact element and
    /// needs no information from any other fragment-level object.
    /// </remarks>
    internal class FactValidator
    {
        private IXbrlFragment validatingFragment;
        private Fact validatingFact;
		private IValidationHandler thisValidationErrors;

		internal void Validate(IXbrlFragment fragment, Fact fact, IValidationHandler validationErrors)
        {
			this.thisValidationErrors = validationErrors;
			this.validatingFragment = fragment;
            this.validatingFact = fact;
            ValidateAttributes();
            if (fact is Item validatingItem)
                ValidateItem(validatingItem);
            else if (fact is Tuple tuple)
                ValidateTuple(tuple);
        }
        private void ValidateTuple(Tuple tuple) {
            Debug.Assert( tuple != null, nameof(tuple) + " != null" );

            /* Attribute uses [XML Schema Structures] (see specifically http://www.w3.org/TR/xmlschema-1/#section-XML-Representation-of-Attribute-Use-Components)
             in Tuple declarations MUST NOT reference attributes from any of the following namespaces: 
             http://www.xbrl.org/2003/instance, http://www.xbrl.org/2003/linkbase, 
             http://www.xbrl.org/2003/XLink, http://www.w3.org/1999/xlink. 
             (See : 104.10 Tuples should not have attributes in XBRL spec defined namespaces.) */
            var rx = new Regex( "^http://(www.xbrl.org/2003/(instance|linkbase|XLink)|www.w3.org/1999/xlink)$" );
            if( tuple.thisFactNode.Attributes.Cast< IAttribute >( ).Any( attr => rx.IsMatch( attr.NamespaceURI ) ) ) {
                this.thisValidationErrors.AddValidationError(new FactValidationError(validatingFact, "Tuples should not have attributes in XBRL spec defined namespaces."));
            }
        }

        /// <summary>
        /// Validates all of the attributes on the fact.
        /// </summary>
        private void ValidateAttributes()
        {
            foreach(IAttribute currentAttribute in validatingFact.thisFactNode.Attributes)
            {
                ValidateAttribute(currentAttribute);
            }
        }

        /// <summary>
        /// Validates one of the attribute on the fact.
        /// </summary>
        /// <param name="currentAttribute">
        /// the attribute to validate.
        /// </param>
        private void ValidateAttribute(IAttribute currentAttribute)
        {
            var attributeType = validatingFragment.Taxonomy.GetAttributeType(currentAttribute);
            if(attributeType != null)
            {
                if(attributeType.CanConvert(currentAttribute.Value) == false)
                {
                    string MessageFormat = AssemblyResources.GetName("AttributeTextNotConvertable");
                    StringBuilder MessageBuilder = new StringBuilder();
                    MessageBuilder.AppendFormat(MessageFormat, validatingFact.Id, currentAttribute.Value);
                    this.thisValidationErrors.AddValidationError(new FactValidationError(validatingFact, MessageBuilder.ToString()));
                }
            }
        }

        //------------------------------------------------------------------------------------
        // Validates an item.
        //
        // If the item is associated with a data type, and it should be, then hand the item
        // off to the data type so that the data type can validate the item. Some data types
        // have specific requirements for items that must be checked. For example, monetary
        // types require that their items have units that are part of the ISO 4217 namespace
        // (http://www.xbrl.org/2003/iso4217). This is checked by the datatype.
        //------------------------------------------------------------------------------------
        private void ValidateItem(Item validatingItem) {
            Debug.Assert( validatingItem != null, nameof(validatingItem) + " != null" );

            if (validatingItem.IsMonetary())
                ValidateMonetaryType(validatingItem);
            else if (validatingItem.IsShares())
                ValidateSharesType(validatingItem);
            else if (validatingItem.IsDecimal())
                ValidateDecimalType(validatingItem);
            else if (validatingItem.IsPure())
            {

                // Pure item types are derived from restriction, so run the pure validation
                // as well as the decimal validation.

                ValidatePureType(validatingItem);
                ValidateDecimalType(validatingItem);
            }
        }

        private void ValidateMonetaryType(Item validatingItem)
        {
            if (validatingItem.UnitRef == null)
                return;

            // According to Table 3 in section 4.8.2 of the XBRL spec, monetary item units cannot use
            // ratios; they must be single measures. This condition is checked by test 304.26 in the
            // XBRL-CONF-CR5-2012-01-24 conformance suite.

            if (validatingItem.UnitRef.Ratio == true)
            {
                StringBuilder MessageBuilder = new StringBuilder();
                string StringFormat = AssemblyResources.GetName("RatioFoundInMonetaryItemUnit");
                MessageBuilder.AppendFormat(StringFormat, validatingItem.Name, validatingItem.UnitRef.Id);
				thisValidationErrors.AddValidationError(new ItemValidationError(validatingItem, MessageBuilder.ToString()));
                return;
            }

            // Validate the unit's measure, if it exists.

            if (validatingItem.UnitRef.MeasureQualifiedNames.Count == 0)
                return;
            if (validatingItem.UnitRef.MeasureQualifiedNames[0] == null)
                return;

            string Uri = validatingItem.UnitRef.MeasureQualifiedNames[0].NamespaceUri;
            if (Uri == null)
            {
                StringBuilder MessageBuilder = new StringBuilder();
                string StringFormat = AssemblyResources.GetName("WrongMeasureNamespaceForMonetaryFact");
                MessageBuilder.AppendFormat(StringFormat, validatingItem.Name, validatingItem.UnitRef.Id, "unspecified");
				thisValidationErrors.AddValidationError(new ItemValidationError(validatingItem, MessageBuilder.ToString()));
                return;
            }
            if ((Uri.Length > 0) && (Uri.Equals(NamespaceUri.XbrlIso4217NamespaceUri) == false))
            {
                StringBuilder MessageBuilder = new StringBuilder();
                string StringFormat = AssemblyResources.GetName("WrongMeasureNamespaceForMonetaryFact");
                MessageBuilder.AppendFormat(StringFormat, validatingItem.Name, validatingItem.UnitRef.Id, validatingItem.UnitRef.MeasureQualifiedNames[0].NamespaceUri);
				thisValidationErrors.AddValidationError(new ItemValidationError(validatingItem, MessageBuilder.ToString()));
                return;
            }
            validatingItem.UnitRef.ValidateISO4217Code(validatingItem.UnitRef.MeasureQualifiedNames[0].LocalName);
            if (validatingItem.UnitRef.IsIso4217CodeValid == false)
            {
                StringBuilder MessageBuilder = new StringBuilder();
                string StringFormat = AssemblyResources.GetName("UnsupportedISO4217CodeForUnitMeasure");
                MessageBuilder.AppendFormat(StringFormat, validatingItem.Name, validatingItem.UnitRef.Id, validatingItem.UnitRef.MeasureQualifiedNames[0].LocalName);
				thisValidationErrors.AddValidationError(new ItemValidationError(validatingItem, MessageBuilder.ToString()));
            }
        }

        /// <summary>
        /// Validate pure item types.
        /// </summary>
        /// <param name="validatingItem"></param>
        private void ValidatePureType(Item validatingItem)
        {
            string UnitMeasureLocalName = string.Empty;
            Unit UnitReference = validatingItem.UnitRef;
            bool PureMeasureFound = true;
            if (UnitReference.MeasureQualifiedNames.Count != 1)
                PureMeasureFound = false;
            if (PureMeasureFound == true)
            {
                UnitMeasureLocalName = UnitReference.MeasureQualifiedNames[0].LocalName;
                PureMeasureFound = UnitMeasureLocalName.Equals("pure");
            }
            if (PureMeasureFound == false)
            {
                StringBuilder MessageBuilder = new StringBuilder();
                string StringFormat = AssemblyResources.GetName("PureItemTypeUnitLocalNameNotPure");
                MessageBuilder.AppendFormat(StringFormat, validatingItem.Name, UnitReference.Id, UnitMeasureLocalName);
				thisValidationErrors.AddValidationError(new ItemValidationError(validatingItem, MessageBuilder.ToString()));
            }
        }

        /// <summary>
        /// Validate shares item types.
        /// </summary>
        /// <param name="validatingItem"></param>
        private void ValidateSharesType(Item validatingItem)
        {
            bool SharesMeasureFound = true;
            string UnitMeasureLocalName = string.Empty;
            Unit UnitReference = validatingItem.UnitRef;
            if (UnitReference.MeasureQualifiedNames.Count != 1)
                SharesMeasureFound = false;
            if (SharesMeasureFound == true)
            {
                UnitMeasureLocalName = UnitReference.MeasureQualifiedNames[0].LocalName;
                SharesMeasureFound = UnitMeasureLocalName.Equals("shares");
            }
            if (SharesMeasureFound == false)
            {
                StringBuilder MessageBuilder = new StringBuilder();
                string StringFormat = AssemblyResources.GetName("SharesItemTypeUnitLocalNameNotShares");
                MessageBuilder.AppendFormat(StringFormat, validatingItem, UnitReference.Id, UnitMeasureLocalName);
				thisValidationErrors.AddValidationError(new ItemValidationError(validatingItem, MessageBuilder.ToString()));
                return;
            }
            var SharesNamespaceCorrect = true;
            string Uri = UnitReference.MeasureQualifiedNames[0].NamespaceUri;
            if (string.IsNullOrEmpty(Uri) == true)
                SharesNamespaceCorrect = false;
            else if (Uri.Equals(NamespaceUri.XbrlNamespaceUri) == false)
                SharesNamespaceCorrect = false;
            if (SharesNamespaceCorrect == false)
            {
                StringBuilder MessageBuilder = new StringBuilder();
                string StringFormat = AssemblyResources.GetName("WrongMeasureNamespaceForSharesFact");
                MessageBuilder.AppendFormat(StringFormat, validatingItem.Name, UnitReference.Id, Uri);
				thisValidationErrors.AddValidationError(new ItemValidationError(validatingItem, MessageBuilder.ToString()));
            }
        }

        /// <summary>
        /// Validate decimal item types.
        /// </summary>
        /// <param name="validatingItem"></param>
        private void ValidateDecimalType(Item validatingItem)
        {
            if (validatingItem.NilSpecified == true)
                ValidateNilDecimalType(validatingItem);
            else
                ValidateNonNilDecimalType(validatingItem);
        }

        private void ValidateNonNilDecimalType(Item validatingItem)
        {
            if ((validatingItem.PrecisionSpecified == false) && (validatingItem.DecimalsSpecified == false))
            {
                string MessageFormat = AssemblyResources.GetName("NumericFactWithoutSpecifiedPrecisionOrDecimals");
                StringBuilder MessageFormatBuilder = new StringBuilder();
                MessageFormatBuilder.AppendFormat(MessageFormat, validatingItem.Name, validatingItem.Id);
				thisValidationErrors.AddValidationError(new ItemValidationError(validatingItem, MessageFormatBuilder.ToString()));
            }
            if ((validatingItem.PrecisionSpecified == true) && (validatingItem.DecimalsSpecified == true))
            {
                string MessageFormat = AssemblyResources.GetName("NumericFactWithSpecifiedPrecisionAndDecimals");
                StringBuilder MessageFormatBuilder = new StringBuilder();
                MessageFormatBuilder.AppendFormat(MessageFormat, validatingItem.Name, validatingItem.Id);
				thisValidationErrors.AddValidationError(new ItemValidationError(validatingItem, MessageFormatBuilder.ToString()));
            }
        }

        private void ValidateNilDecimalType(Item validatingItem)
        {
            if ((validatingItem.PrecisionSpecified == true) || (validatingItem.DecimalsSpecified == true))
            {
                string MessageFormat = AssemblyResources.GetName("NilNumericFactWithSpecifiedPrecisionOrDecimals");
                StringBuilder MessageFormatBuilder = new StringBuilder();
                MessageFormatBuilder.AppendFormat(MessageFormat, validatingItem.Name, validatingItem.Id);
				thisValidationErrors.AddValidationError(new ItemValidationError(validatingItem, MessageFormatBuilder.ToString()));
            }
        }
    }
}
