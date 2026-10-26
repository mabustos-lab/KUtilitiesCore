namespace KUtilitiesCore.OrderedInfo
{
    /// <summary>
    /// Estructura que vincula el nombre técnico de una propiedad con el nombre
    /// para mostrar usado en columnas y metadatos ordenables.
    /// </summary>
    /// <param name="technicalName">Nombre técnico de la propiedad.</param>
    /// <param name="displayName">Nombre para mostrar de la propiedad.</param>
    public struct PropertyNameInfo(string technicalName, string displayName)
    {
        /// <summary>Nombre técnico de la propiedad.</summary>
        public string PropertyName { get; set; } = technicalName;
        /// <summary>Nombre para mostrar de la propiedad.</summary>
        public string DisplayName { get; set; } = displayName;
    }
}