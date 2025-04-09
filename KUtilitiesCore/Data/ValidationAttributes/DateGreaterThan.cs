﻿using KUtilitiesCore.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data.ValidationAttributes
{
    /// <summary>
    /// Indica que la propiedad con el atributo debe ser mayor que la otra propiedad
    /// </summary>
    public class DateGreaterThan : ValidationAttribute
    {
        #region Fields

        private bool notAllowEquality;
        private string otherPropertyDisplay;

        #endregion Fields

        #region Constructors

        public DateGreaterThan(string otherPropertyname, bool notAllowEquality = false, bool nullAsMinValue = false)
        {
            if (string.IsNullOrEmpty(otherPropertyname)) throw new ArgumentNullException(nameof(OtherPropertyname));
            this.OtherPropertyname = otherPropertyname;
            this.otherPropertyDisplay = otherPropertyname;
            this.NotAllowEquality = notAllowEquality;
            this.NullAsMinValue = nullAsMinValue;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Establece si permite que los valores sean iguales
        /// </summary>
        public bool NotAllowEquality
        {
            get => notAllowEquality;
            internal set
            {
                notAllowEquality = value;
                this.ErrorMessage = (notAllowEquality ?
                    ValidationAtrributesStrings.ValidationGreaterThanOrEqualToError : ValidationAtrributesStrings.ValidationGreaterThanError);
            }
        }

        /// <summary>
        /// Indica que el valor minimo represente nulo
        /// </summary>
        public bool NullAsMinValue { get; set; }

        /// <summary>
        /// Nombre de la otra propiedad con la que se comparará
        /// </summary>
        public string OtherPropertyname { get; internal set; }
        /// <inheritdoc/>

        public override bool RequiresValidationContext => true;

        #endregion Properties

        #region Methods

        public override string FormatErrorMessage(string name)
        {
            return string.Format(System.Globalization.CultureInfo.CurrentCulture,
                ErrorMessageString,
                otherPropertyDisplay, name);
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (OtherPropertyname.ToLower().Equals(validationContext.MemberName.ToLower()))
                return new ValidationResult(string.Format(ValidationAtrributesStrings.ValidationSamePropertyError, this.GetType().Name));

            PropertyInfo otherPropInfo = validationContext.ObjectType.GetProperty(OtherPropertyname);

            if (otherPropInfo == null)
                return new ValidationResult(string.Format(ValidationAtrributesStrings.ValidatiomPropertyNotFound, OtherPropertyname));

            otherPropertyDisplay = otherPropInfo.DataAnnotationsDisplayName();

            object otherPropertyValue = otherPropInfo.GetValue(validationContext.ObjectInstance, null);

            if (!NullAsMinValue && value == null) return ValidationResult.Success;
            if (otherPropertyValue == null) return ValidationResult.Success;

            if (!DateTime.TryParse(value != null ? value.ToString() : DateTime.MinValue.ToString(), out DateTime dt_Value))
                return new ValidationResult(string.Format(ValidationAtrributesStrings.ValidationIsNotDateTypeError,
                    validationContext.DisplayName));

            if (!DateTime.TryParse(otherPropertyValue != null ? otherPropertyValue.ToString() : "", out DateTime dt_OtherValue))
                return new ValidationResult(string.Format(ValidationAtrributesStrings.ValidationIsNotDateTypeError,
                    otherPropertyDisplay));



            if (NotAllowEquality && dt_Value.Equals(dt_OtherValue))
            {
                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
            }
            else if (dt_Value < dt_OtherValue)
            {
                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
            }

            return ValidationResult.Success;
        }

        #endregion Methods
    }
}
