namespace Limbo.Umbraco.Migrations.Constants {

    /// <summary>
    /// Static class with constants for various legacy property editor aliases.
    /// </summary>
    public static class PropertyEditorAliases {

        public const string Archetype = "Imulus.Archetype";

        public static class NuPickers {

            public const string DotNetCheckBoxPicker = "nuPickers.DotNetCheckBoxPicker";

        }

        public static class Skybrud {

            public const string ImagePicker = "Skybrud.ImagePicker";

            public const string LinkPicker = "Skybrud.LinkPicker";

        }

        public static class Umbraco {

            /// <summary>
            /// Gets the alias of the original Umbraco 7 content picker property editor.
            /// </summary>
            public const string ContentPicker = "Umbraco.ContentPickerAlias";

            /// <summary>
            /// Gets the alias of the updated Umbraco 7 content picker property editor.
            /// </summary>
            public const string ContentPicker2 = "Umbraco.ContentPicker2";

            /// <summary>
            /// Gets the alias of the Umbraco Nested Content property editor.
            /// </summary>
            public const string NestedContent = "Umbraco.NestedContent";

        }

    }

}