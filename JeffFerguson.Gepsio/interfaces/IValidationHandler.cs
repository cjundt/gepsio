using System;
using System.Collections.Generic;
using System.Linq;

namespace JeffFerguson.Gepsio {
	
	/// <summary>
	/// Validation handler interface.
	/// </summary>
	public interface IValidationHandler {
		/// <summary>
		/// Add a validation error to the validation errors list.
		/// </summary>
		/// <param name="validationError"></param>
		void AddValidationError(ValidationError validationError);
	}
}