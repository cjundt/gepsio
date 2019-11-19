using System;
using System.Collections.Generic;
using System.Linq;

using JeffFerguson.Gepsio.Xsd;

namespace JeffFerguson.Gepsio {
    internal class XbrlSchemaValidator {
        private readonly XbrlSchema thisSchema;
        private readonly IValidationHandler thisValidationHandler;
        public XbrlSchemaValidator(XbrlSchema schema, IValidationHandler validationHandler) {
            this.thisSchema = schema;
            this.thisValidationHandler = validationHandler;
        }
        public void ValidateElement(Element element) {
            /* 105.01 If Element has balance attribute, type MUST be monetaryItemType */
            if( element.SubstitutionGroup == Element.ElementSubstitutionGroup.Item && element.Balance != null && !element.IsMonetaryItemType ) {
                this.thisValidationHandler.AddValidationError( new ElementValidationError( element, "105.01 - If Element has balance attribute, type MUST be monetaryItemType." ) );
            }
            /* 105.02 Elemements where the substitutionGroup attribute value is tuple MUST NOT have a balance attribute. */
            if( element.SubstitutionGroup == Element.ElementSubstitutionGroup.Tuple && element.Balance != null ) {
                this.thisValidationHandler.AddValidationError( new ElementValidationError( element, "105.02 - Elemements where the substitutionGroup attribute value is tuple MUST NOT have a balance attribute." ) );
            }
        }
    }
}