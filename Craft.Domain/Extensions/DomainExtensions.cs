namespace Craft.Domain;

public static class DomainExtensions
{
    extension(string? value)
    {
        /// <summary>
        /// Converts the stored value to the specified type.
        /// </summary>
        /// <remarks>If the stored value is <see langword="null"/>, the method returns the default value
        /// of  <typeparamref name="TKey"/>. If the conversion fails, the method also returns the default  value of
        /// <typeparamref name="TKey"/>.</remarks>
        /// <typeparam name="TKey">The type to which the value should be converted.</typeparam>
        /// <returns>The converted value of type <typeparamref name="TKey"/> if the conversion is successful;  otherwise, the
        /// default value of <typeparamref name="TKey"/>.</returns>
        public TKey? Parse<TKey>()
        {
            try
            {
                return value != null
                    ? (TKey)Convert.ChangeType(value, typeof(TKey))
                    : default;
            }
            catch (Exception)
            {
                return default;
            }
        }
    }
}
