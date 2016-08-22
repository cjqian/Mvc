using Microsoft.AspNetCore.Razor.Compilation.TagHelpers;

namespace Microsoft.AspNetCore.Mvc.Razor.Host
{
    public static class ViewComponentTagHelperDescriptorConventions
    {
        public static readonly string ViewComponentProperty = "ViewComponentName";
        public static readonly string ViewComponentTagHelperProperty = "ViewComponentTagHelperName";

        public static bool IsViewComponentDescriptor(TagHelperDescriptor descriptor)
        {
            return (descriptor.PropertyBag != null &&
                descriptor.PropertyBag.ContainsKey(ViewComponentProperty) &&
                descriptor.PropertyBag.ContainsKey(ViewComponentTagHelperProperty));
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
    }
}
