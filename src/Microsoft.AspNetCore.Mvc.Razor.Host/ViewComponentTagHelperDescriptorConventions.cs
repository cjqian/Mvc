﻿using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Razor.Compilation.TagHelpers;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;

namespace Microsoft.AspNetCore.Mvc.Razor.Host
{
    /// <summary>
    /// A library of methods used to generate tag helper descriptors for view components.
    /// </summary>
    public static class ViewComponentTagHelperDescriptorConventions
    {
        /// <summary>
        /// The key in a <see cref="TagHelperDescriptor"/> property bag containing 
        /// the short name of a <see cref="ViewComponent"/>.  
        /// </summary>
        public static readonly string ViewComponentProperty = "ViewComponentName";

        /// <summary>
        /// The key in a <see cref="TagHelperDescriptor"/> property bag containing
        /// a custom type name for a view component's tag helper representation.
        /// </summary>
        public static readonly string ViewComponentTagHelperProperty = "ViewComponentTagHelperName";

        /// <summary>
        /// Each custom type name for a view component's tag helper representation will begin with this header.
        /// </summary>
        public static readonly string ViewComponentTagHelperPropertyHeader = "__Generated__";

        /// <summary>
        /// Each custom type name for a view component's tag helper representation will end with this footer.
        /// </summary>
        public static readonly string ViewComponentTagHelperPropertyFooter = "ViewComponentTagHelper";

        /// <summary>
        /// Verifies whether a <see cref="TagHelperDescriptor"/> represents a <see cref="ViewComponent"/>.  
        /// </summary>
        /// <param name="descriptor">The <see cref="TagHelperDescriptor"/> to check.</param>
        /// <returns>Whether a <see cref="TagHelperDescriptor"/> represents a <see cref="ViewComponent"/>.</returns>
        public static bool IsViewComponentDescriptor(TagHelperDescriptor descriptor)
        {
            return (descriptor.PropertyBag.ContainsKey(ViewComponentProperty)
                && descriptor.PropertyBag.ContainsKey(ViewComponentTagHelperProperty));
        }

        /// <summary>
        /// Retrieves the view component name from the tag helper property bag.
        /// </summary>
        /// <param name="descriptor">The <see cref="TagHelperDescriptor"/>.</param>
        /// <returns>The short name of the <see cref="ViewComponent"/> represented by the <see cref="TagHelperDescriptor"/>,
        /// or null if the tag helper does not represent a view component.</returns>
        public static string GetViewComponentName(TagHelperDescriptor descriptor)
        {
            if (!IsViewComponentDescriptor(descriptor)) return null;

            var viewComponentName = descriptor.PropertyBag[ViewComponentProperty];
            return viewComponentName;
        }

        /// <summary>
        /// Retrieves the view component tag helper name from the tag helper property bag.
        /// </summary>
        /// <param name="descriptor">The <see cref="TagHelperDescriptor"/>.</param>
        /// <returns>The custom type name for a <see cref="ViewComponent"/> represented by the <see cref="TagHelperDescriptor"/>,
        /// or null if the tag helper does not represent a view component.</returns>
        public static string GetViewComponentTagHelperName(TagHelperDescriptor descriptor)
        {
            if (!IsViewComponentDescriptor(descriptor)) return null;

            var viewComponentTagHelperName = descriptor.PropertyBag[ViewComponentTagHelperProperty];
            return viewComponentTagHelperName;
        }

        /// <summary>
        /// Retrieves the assembly name from a view component descriptor.
        /// </summary>
        /// <param name="descriptor">The <see cref="ViewComponentDescriptor"/>.</param>
        /// <returns>The name of the assembly containing the <see cref="ViewComponentDescriptor"/>.</returns>
        public static string GetAssemblyName(ViewComponentDescriptor descriptor) =>
            descriptor.TypeInfo.Assembly.GetName().Name;

        /// <summary>
        /// Creates a custom tag name from a view component descriptor.
        /// </summary>
        /// <param name="descriptor">The <see cref="ViewComponentDescriptor"/>.</param>
        /// <returns>The tag name used to invoke a <see cref="ViewComponent"/> as a tag helper.</returns>
        public static string GetTagName(ViewComponentDescriptor descriptor) =>
            $"vc:{TagHelperDescriptorFactory.ToHtmlCase(descriptor.ShortName)}";

        /// <summary>
        /// Creates a custom type name from a view component descriptor.
        /// </summary>
        /// <param name="descriptor">The <see cref="ViewComponentDescriptor"/>.</param>
        /// <returns>A custom type name for a tag helper representing a view component.</returns>
        public static string GetTypeName(ViewComponentDescriptor descriptor) =>
            ViewComponentTagHelperDescriptorConventions.ViewComponentTagHelperPropertyHeader
            + descriptor.ShortName
            + ViewComponentTagHelperDescriptorConventions.ViewComponentTagHelperPropertyFooter;
    }
}