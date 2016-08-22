using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;

namespace Microsoft.AspNetCore.Mvc.Razor.ViewComponentTagHelpers
{
    public class DefaultCompositeTagHelperDescriptorResolver : CompositeTagHelperDescriptorResolver
    {
        public DefaultCompositeTagHelperDescriptorResolver(
            TagHelperDescriptorResolver resolver1,
            ViewComponentTagHelperDescriptorResolver resolver2)
            : base()
        {
            Resolvers.Add(resolver1);
            Resolvers.Add(resolver2);
        }
    }
}
