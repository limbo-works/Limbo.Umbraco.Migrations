using System;

namespace Limbo.Umbraco.Migrations.Models.Content;

public static class MigrationsContentModelExtensions {

    /// <summary>
    /// Sets the parent key to the specified <paramref name="parentKey"/>. This method may be used to set a different
    /// parent than the page had in the legacy solution.
    /// </summary>
    /// <typeparam name="T">The type of the model.</typeparam>
    /// <param name="model">The model.</param>
    /// <param name="parentKey">The new parent key.</param>
    /// <returns>The model - useful for method chaining.</returns>
    public static T SetParent<T>(this T model, Guid? parentKey) where T : MigrationsContentModel {
        model.ParentKey = parentKey;
        return model;
    }

    /// <summary>
    /// Sets the name of the model. This method may be used to set a different name that the page had in the legacy solution.
    /// </summary>
    /// <typeparam name="T">The type of the model.</typeparam>
    /// <param name="model">The model.</param>
    /// <param name="name">The new name of the page.</param>
    /// <returns>The model - useful for method chaining.</returns>
    public static T SetName<T>(this T model, string name) where T : MigrationsContentModel {
        model.Name = name;
        return model;
    }

    /// <summary>
    /// Sets the value of <paramref name="property"/> to the specified <paramref name="value"/>.
    /// </summary>
    /// <typeparam name="T">The type of the model.</typeparam>
    /// <param name="model">The model.</param>
    /// <param name="property">The alias/name of the property.</param>
    /// <param name="value">The new value of the property.</param>
    /// <returns>The model - useful for method chaining.</returns>
    public static T SetValue<T>(this T model, string property, object? value) where T : MigrationsContentModel {
        model.Properties[property] = value;
        return model;
    }

}