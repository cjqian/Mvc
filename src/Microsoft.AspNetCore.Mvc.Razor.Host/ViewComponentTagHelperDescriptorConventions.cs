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

        private static readonly string _viewComponentTagHelperPropertyHeader = "__Generated__";
        private static readonly string _viewComponentTagHelperPropertyFooter = "ViewComponentTagHelper";

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
            var pattern = $"^{_viewComponentTagHelperPropertyHeader}(.*){_viewComponentTagHelperPropertyFooter}$";
            var regex = new Regex(pattern);
            var match = regex.Match(viewComponentTagHelperProperty);
            if (!match.Success)
            {
                return false;
            }

            // Checks correctness of view component property.
            var minimumLength = _viewComponentTagHelperPropertyHeader.Length +
                _viewComponentTagHelperPropertyFooter.Length;

            if (viewComponentTagHelperProperty.Length <= minimumLength)
            {
                return false;
            }

            var expectedLength = viewComponentTagHelperProperty.Length - minimumLength;
            var expectedProperty = viewComponentTagHelperProperty.Substring(
                _viewComponentTagHelperPropertyHeader.Length, expectedLength);

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
            _viewComponentTagHelperPropertyHeader
            + descriptor.ShortName
            + _viewComponentTagHelperPropertyFooter;
    }
}