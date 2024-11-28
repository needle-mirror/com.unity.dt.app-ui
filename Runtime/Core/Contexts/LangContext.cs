using System;
using System.Threading.Tasks;
#if UNITY_LOCALIZATION_PRESENT
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
#endif

namespace Unity.AppUI.Core
{
    /// <summary>
    /// The Lang context of the application.
    /// </summary>
    /// <param name="lang"> The language. </param>
    public record LangContext(string lang) : IContext
    {
        /// <summary>
        /// The current language.
        /// </summary>
        public string lang { get; } = lang;

#if UNITY_LOCALIZATION_PRESENT
        /// <summary>
        /// The current locale.
        /// </summary>
        [Obsolete("locale property is obsolete and will be removed in future versions of the package. " +
            "Use lang property to fetch the current Locale via the LocalizationSettings.")]
        public Locale locale => GetLocaleForLang(lang);

        /// <summary>
        /// Get the locale for a language.
        /// </summary>
        /// <param name="lang"> The language. </param>
        /// <returns> The locale. </returns>
        [Obsolete("GetLocaleForLang method is obsolete and will be removed in future versions of the package. " +
            "Use lang property to fetch the current Locale via the LocalizationSettings.")]
        public static Locale GetLocaleForLang(string lang)
        {
            var settings = LocalizationSettings.GetInstanceDontCreateDefault();
            if (!settings)
                return null;

            var globalLocale = settings.GetSelectedLocaleAsync();
            if (!globalLocale.IsDone)
                return null;

            if (string.IsNullOrEmpty(lang))
                return globalLocale.Result;

            var availableLocales = settings.GetAvailableLocales();
            if (availableLocales is LocalesProvider localesProvider &&
                (!localesProvider.PreloadOperation.IsValid() || !localesProvider.PreloadOperation.IsDone))
                return null;

            var scopedLocale = availableLocales.GetLocale(lang);
            return scopedLocale ? scopedLocale : globalLocale.Result;
        }
#endif

        /// <summary>
        /// The delegate to get a localized string asynchronously.
        /// </summary>
        /// <param name="referenceText"> The reference text. </param>
        /// <param name="lang"> The language. </param>
        /// <param name="arguments"> The arguments to format the string. </param>
        /// <returns> The localized string. </returns>
        public delegate Task<string> GetLocalizedStringAsyncDelegate(string referenceText, string lang, params object[] arguments);

        /// <summary>
        /// The delegate to get a localized string.
        /// </summary>
        public GetLocalizedStringAsyncDelegate GetLocalizedStringAsyncFunc { get; set; }

        /// <summary>
        /// Get a localized string asynchronously.
        /// </summary>
        /// <param name="referenceText"> The reference text. </param>
        /// <param name="arguments"> The arguments to format the string. </param>
        /// <returns> The localized string. </returns>
        internal Task<string> GetLocalizedStringAsync(string referenceText, params object[] arguments)
        {
            if (GetLocalizedStringAsyncFunc != null)
                return GetLocalizedStringAsyncFunc.Invoke(referenceText, lang, arguments);
#if UNITY_LOCALIZATION_PRESENT
            return LocalizationUtils.GetLocalizedStringFromLocalizationPackage(referenceText, lang, arguments);
#else
            return Task.FromResult(referenceText);
#endif
        }
    }
}
