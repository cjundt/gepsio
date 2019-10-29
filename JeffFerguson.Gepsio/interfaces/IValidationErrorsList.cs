using System;
using System.Collections.Generic;
using System.Linq;

namespace JeffFerguson.Gepsio {
	public interface IValidationErrorsList {
		void AddValidationError(ValidationError validationError);
	}
}