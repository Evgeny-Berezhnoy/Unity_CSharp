using System;
using System.Collections.Generic;
using System.Linq;

namespace Berezhnoy.Dialogue.Serializable
{

    [Serializable]
    public class ExposedProperty
    {

        #region Fields

        public string PropertyName = "New String";
        public string PropertyValue = "New Value";

        #endregion

        #region Static methods

        // Basing on transmitted exposed property, creates unique property name
        //
        public static string GetUniquePropertyName(List<ExposedProperty> exposedProperties, ExposedProperty exposedProperty)
        {

            return GetUniquePropertyName(exposedProperties, exposedProperty.PropertyName);

        }

        public static string GetUniquePropertyName(List<ExposedProperty> exposedProperties, string PropertyName)
        {

            // Unique property name creation
            var localPropertyName = string.Copy(PropertyName);

            while (exposedProperties.Any(x => x.PropertyName == localPropertyName))
            {

                localPropertyName = $"{localPropertyName}(1)"; // USERNAME(1) || USERNAME(1)(1) ETC

            };

            return localPropertyName;

        }

        #endregion

    }

}