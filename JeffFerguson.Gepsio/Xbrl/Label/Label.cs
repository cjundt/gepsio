using JeffFerguson.Gepsio.Xlink;
using JeffFerguson.Gepsio.Xml.Interfaces;
using System.Globalization;

namespace JeffFerguson.Gepsio
{
    /// <summary>
    /// An encapsulation of the XBRL element "label" as defined in the http://www.xbrl.org/2003/linkbase namespace. 
    /// </summary>
    public class Label : XlinkNode
    {
        /// <summary>
        /// A list of possible roles for a label.
        /// </summary>
        /// <remarks>
        /// See Table 8 in section 5.2.2.2.2 in the XBRL Specification for more information about label roles.
        /// </remarks>
        public enum RoleEnum
        {
            /// <summary>
            /// A label with a role URI of http://www.xbrl.org/2003/role/label.
            /// </summary>
            Standard,
            /// <summary>
            /// A label with a role URI of http://www.xbrl.org/2003/role/terseLabel.
            /// </summary>
            Short,
            /// <summary>
            /// A label with a role URI of http://www.xbrl.org/2003/role/verboseLabel.
            /// </summary>
            Verbose,
            /// <summary>
            /// A label with a role URI of http://www.xbrl.org/2003/role/positiveLabel.
            /// </summary>
            StandardPositiveValue,
            /// <summary>
            /// A label with a role URI of http://www.xbrl.org/2003/role/positiveTerseLabel.
            /// </summary>
            ShortPositiveValue,
            /// <summary>
            /// A label with a role URI of http://www.xbrl.org/2003/role/positiveVerboseLabel.
            /// </summary>
            VerbosePositiveValue,
            /// <summary>
            /// A label with a role URI of http://www.xbrl.org/2003/role/negativeLabel.
            /// </summary>
            StandardNegativeValue,
            /// <summary>
            /// A label with a role URI of http://www.xbrl.org/2003/role/negativeTerseLabel.
            /// </summary>
            ShortNegativeValue,
            /// <summary>
            /// A label with a role URI of http://www.xbrl.org/2003/role/negativeVerboseLabel.
            /// </summary>
            VerboseNegativeValue,
            /// <summary>
            /// A label with a role URI of http://www.xbrl.org/2003/role/zeroLabel.
            /// </summary>
            StandardZeroValue,
            /// <summary>
            /// A label with a role URI of http://www.xbrl.org/2003/role/zeroTerseLabel.
            /// </summary>
            ShortZeroValue,
            /// <summary>
            /// A label with a role URI of http://www.xbrl.org/2003/role/zeroVerboseLabel.
            /// </summary>
            VerboseZeroValue,
            /// <summary>
            /// A label with a role URI of http://www.xbrl.org/2003/role/totalLabel.
            /// </summary>
            Total,
            /// <summary>
            /// A label with a role URI of http://www.xbrl.org/2003/role/periodStartLabel.
            /// </summary>
            PeriodStart,
            /// <summary>
            /// A label with a role URI of http://www.xbrl.org/2003/role/periodEndLabel.
            /// </summary>
            PeriodEnd,
            /// <summary>
            /// A label with a role URI of http://www.xbrl.org/2003/role/documentation.
            /// </summary>
            Documentation,
            /// <summary>
            /// A label with a role URI of http://www.xbrl.org/2003/role/definitionGuidance.
            /// </summary>
            DefinitionGuidance,
            /// <summary>
            /// A label with a role URI of http://www.xbrl.org/2003/role/disclosureGuidance.
            /// </summary>
            DisclosureGuidance,
            /// <summary>
            /// A label with a role URI of http://www.xbrl.org/2003/role/presentationGuidance.
            /// </summary>
            PresentationGuidance,
            /// <summary>
            /// A label with a role URI of http://www.xbrl.org/2003/role/measurementGuidance.
            /// </summary>
            MeasurementGuidance,
            /// <summary>
            /// A label with a role URI of http://www.xbrl.org/2003/role/commentaryGuidance.
            /// </summary>
            CommentaryGuidance,
            /// <summary>
            /// A label with a role URI of http://www.xbrl.org/2003/role/exampleGuidance.
            /// </summary>
            ExampleGuidance
        }

        /// <summary>
        /// The role of this label.
        /// </summary>
        public RoleEnum LabelRole { get; private set; }

        /// <summary>
        /// The text of this label.
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// The culture of this label.
        /// </summary>
        public CultureInfo Culture { get; private set; }

        //------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------
        internal Label(INode LabelNode) : base(LabelNode)
        {
            this.Text = LabelNode.InnerText;
            SetRole(this.Role);
            string LanguageValue = LabelNode.GetAttributeValue(NamespaceUri.XmlNamespaceUri, "lang");
            this.Culture = new CultureInfo(LanguageValue);
        }

        //------------------------------------------------------------------------------------
        // See Table 8 in section 5.2.2.2.2 in the XBRL Spec.
        //------------------------------------------------------------------------------------
        private void SetRole(string RoleUri)
        {
            this.LabelRole = RoleEnum.Standard;
            if (RoleUri.Equals(global::RoleUri.XbrlLabelRoleNamespaceUri) == true)
                this.LabelRole = RoleEnum.Standard;
            else if (RoleUri.Equals(global::RoleUri.XbrlTerseLabelRoleNamespaceUri) == true)
                this.LabelRole = RoleEnum.Short;
            else if (RoleUri.Equals(global::RoleUri.XbrlVerboseLabelRoleNamespaceUri) == true)
                this.LabelRole = RoleEnum.Verbose;
            else if (RoleUri.Equals(global::RoleUri.XbrlPositiveLabelRoleNamespaceUri) == true)
                this.LabelRole = RoleEnum.StandardPositiveValue;
            else if (RoleUri.Equals(global::RoleUri.XbrlPositiveTerseLabelRoleNamespaceUri) == true)
                this.LabelRole = RoleEnum.ShortPositiveValue;
            else if (RoleUri.Equals(global::RoleUri.XbrlPositiveVerboseLabelRoleNamespaceUri) == true)
                this.LabelRole = RoleEnum.VerbosePositiveValue;
            else if (RoleUri.Equals(global::RoleUri.XbrlNegativeLabelRoleNamespaceUri) == true)
                this.LabelRole = RoleEnum.StandardNegativeValue;
            else if (RoleUri.Equals(global::RoleUri.XbrlNegativeTerseLabelRoleNamespaceUri) == true)
                this.LabelRole = RoleEnum.ShortNegativeValue;
            else if (RoleUri.Equals(global::RoleUri.XbrlNegativeVerboseLabelRoleNamespaceUri) == true)
                this.LabelRole = RoleEnum.VerboseNegativeValue;
            else if (RoleUri.Equals(global::RoleUri.XbrlZeroLabelRoleNamespaceUri) == true)
                this.LabelRole = RoleEnum.StandardZeroValue;
            else if (RoleUri.Equals(global::RoleUri.XbrlZeroTerseLabelRoleNamespaceUri) == true)
                this.LabelRole = RoleEnum.ShortZeroValue;
            else if (RoleUri.Equals(global::RoleUri.XbrlZeroVerboseLabelRoleNamespaceUri) == true)
                this.LabelRole = RoleEnum.VerboseZeroValue;
            else if (RoleUri.Equals(global::RoleUri.XbrlTotalLabelRoleNamespaceUri) == true)
                this.LabelRole = RoleEnum.Total;
            else if (RoleUri.Equals(global::RoleUri.XbrlPeriodStartLabelRoleNamespaceUri) == true)
                this.LabelRole = RoleEnum.PeriodStart;
            else if (RoleUri.Equals(global::RoleUri.XbrlPeriodEndLabelRoleNamespaceUri) == true)
                this.LabelRole = RoleEnum.PeriodEnd;
            else if (RoleUri.Equals(global::RoleUri.XbrlDocumentationRoleNamespaceUri) == true)
                this.LabelRole = RoleEnum.Documentation;
            else if (RoleUri.Equals(global::RoleUri.XbrlDocumentationGuidanceRoleNamespaceUri) == true)
                this.LabelRole = RoleEnum.DefinitionGuidance;
            else if (RoleUri.Equals(global::RoleUri.XbrlDisclosureGuidanceRoleNamespaceUri) == true)
                this.LabelRole = RoleEnum.DisclosureGuidance;
            else if (RoleUri.Equals(global::RoleUri.XbrlPresentationGuidanceRoleNamespaceUri) == true)
                this.LabelRole = RoleEnum.PresentationGuidance;
            else if (RoleUri.Equals(global::RoleUri.XbrlMeasurementGuidanceRoleNamespaceUri) == true)
                this.LabelRole = RoleEnum.MeasurementGuidance;
            else if (RoleUri.Equals(global::RoleUri.XbrlCommentaryGuidanceRoleNamespaceUri) == true)
                this.LabelRole = RoleEnum.CommentaryGuidance;
            else if (RoleUri.Equals(global::RoleUri.XbrlExampleGuidanceRoleNamespaceUri) == true)
                this.LabelRole = RoleEnum.ExampleGuidance;
        }
    }
}
