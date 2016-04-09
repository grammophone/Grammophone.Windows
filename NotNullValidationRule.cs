using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace Grammophone.Windows
{
	/// <summary>
	/// Apply to a control's binding to require non-null imput.
	/// </summary>
	public class NotNullValidationRule : ValidationRule
	{
		public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
		{
			if (value != null)
				return ValidationResult.ValidResult;
			else 
				return new ValidationResult(false, "The value must not be null.");
		}
	}
}
