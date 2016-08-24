using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Razor.Compilation.TagHelpers;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;

namespace Microsoft.AspNetCore.Mvc.Razor.Host
{
    public static class ViewComponentTagHelperDescriptorConventions
    {
        public static readonly string ViewComponentProperty = "ViewComponentName";
        public static readonly string ViewComponentTagHelperProperty = "ViewComponentTagHelperName";

        public static readonly string ViewComponentTagHelperPropertyHeader = "__Generated__";
        public static readonly string ViewComponentTagHelperPropertyFooter = "ViewComponentTagHelper";

        public static bool IsViewComponentDescriptor(TagHelperDescriptor descriptor)
        {
            if (!descriptor.PropertyBag.ContainsKey(ViewComponentProperty) ||
                !descriptor.PropertyBag.ContainsKey(ViewComponentTagHelperProperty))
            {
                return false;
            }

            var viewComponentProperty = descriptor.PropertyBag[ViewComponentProperty];
            var viewComponentTagHelperProperty = descriptor.PropertyBag[ViewComponentTagHelperProperty];

            // Checks correctness of view component tag helper property.
            var pattern = $"^{ViewComponentTagHelperPropertyHeader}(.*){ViewComponentTagHelperPropertyFooter}$";
            var regex = new Regex(pattern);
            var match = regex.Match(viewComponentTagHelperProperty);
            if (!match.Success)
            {
                return false;
            }

            // Checks correctness of view component property.
            var minimumLength = ViewComponentTagHelperPropertyHeader.Length +
                ViewComponentTagHelperPropertyFooter.Length;

            if (viewComponentTagHelperProperty.Length <= minimumLength)
            {
                return false;
            }

            var expectedLength = viewComponentTagHelperProperty.Length - minimumLength;
            var expectedProperty = viewComponentTagHelperProperty.Substring(
                ViewComponentTagHelperPropertyHeader.Length, expectedLength);

            return (viewComponentProperty.Equals(expectedProperty));
        }

        public static string GetViewComponentName(TagHelperDescriptor descriptor)
        {
            if (!IsViewComponentDescriptor(descriptor)) return null;

            var viewComponentName = descriptor.PropertyBag[ViewComponentProperty];
            return viewComponentName;
        }

        public static string GetViewComponentTagHelperName(TagHelperDescriptor descriptor)
        {
            if (!IsViewComponentDescriptor(descriptor)) return null;

            var viewComponentTagHelperName = descriptor.PropertyBag[ViewComponentTagHelperProperty];
            return viewComponentTagHelperName;
        }

        public static string GetAssemblyName(ViewComponentDescriptor descriptor) =>
            descriptor.TypeInfo.Assembly.GetName().Name;

        public static string GetTagName(ViewComponentDescriptor descriptor) =>
            $"vc:{TagHelperDescriptorFactory.ToHtmlCase(descriptor.ShortName)}";

        public static string GetTypeName(ViewComponentDescriptor descriptor) =>
            ViewComponentTagHelperDescriptorConventions.ViewComponentTagHelperPropertyHeader
            + descriptor.ShortName
            + ViewComponentTagHelperDescriptorConventions.ViewComponentTagHelperPropertyFooter;
    }
}