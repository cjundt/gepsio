using System;
using System.Collections.Generic;
using System.Linq;

namespace JeffFerguson.Gepsio {
	public interface IValidationHandler {
		void AddValidationError(ValidationError validationError);
	}
}