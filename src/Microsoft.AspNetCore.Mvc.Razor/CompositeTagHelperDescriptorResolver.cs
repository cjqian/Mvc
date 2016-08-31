using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Razor.Compilation.TagHelpers;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;

namespace Microsoft.AspNetCore.Mvc.Razor
{
    public class CompositeTagHelperDescriptorResolver : ITagHelperDescriptorResolver
    {
        public IList<TagHelperDescriptorResolver> Resolvers { get; set; }
        public CompositeTagHelperDescriptorResolver()
        {
            Resolvers = new List<TagHelperDescriptorResolver>();
        }

        public IEnumerable<TagHelperDescriptor> Resolve(TagHelperDescriptorResolutionContext resolutionContext)
        {
            var descriptors = new List<TagHelperDescriptor>();

            foreach (var resolver in Resolvers)
            {
                var currentDescriptors = resolver.Resolve(resolutionContext);
                descriptors.AddRange(currentDescriptors);
            }

            return descriptors;
        }
    }
}