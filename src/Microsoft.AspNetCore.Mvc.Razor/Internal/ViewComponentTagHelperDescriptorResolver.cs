using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Razor.Compilation.TagHelpers;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;

namespace Microsoft.AspNetCore.Mvc.Razor.Internal
{
    public class ViewComponentTagHelperDescriptorResolver : TagHelperDescriptorResolver
    {
        ViewComponentTagHelperDescriptorFactory _descriptorFactory;

        public ViewComponentTagHelperDescriptorResolver(
            IViewComponentDescriptorProvider viewComponentDescriptorProvider)
            : base(designTime: true)
        {
            _descriptorFactory = new ViewComponentTagHelperDescriptorFactory(viewComponentDescriptorProvider);
        }

        public new IEnumerable<TagHelperDescriptor> Resolve(TagHelperDescriptorResolutionContext resolutionContext)
        {
            return base.Resolve(resolutionContext);
        }

        protected override IEnumerable<TagHelperDescriptor> ResolveDescriptorsInAssembly(
            string assemblyName,
            SourceLocation documentLocation,
            ErrorSink errorSink)
        {
            var tagHelperDescriptors = base.ResolveDescriptorsInAssembly(
                assemblyName,
                documentLocation,
                errorSink);

            var viewComponentTagHelperDescriptors = _descriptorFactory.CreateDescriptors(assemblyName);
            var descriptors = tagHelperDescriptors.Concat(viewComponentTagHelperDescriptors);
            return descriptors;
        }
    }
}