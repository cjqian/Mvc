using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Razor.Compilation.TagHelpers;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Razor.Test
{
    public class CompositeTagHelperDescriptorResolverTest
    {
        [Fact]
        public void Resolve_ReturnsCorrectDescriptors()
        {
            // Arrange
            var fooTagHelperDescriptorResolver = new FooTagHelperDescriptorResolver();
            var barTagHelperDescriptorResovler = new BarTagHelperDescriptorResolver();
            var compositeTagHelperDescriptorResolver = new CompositeTagHelperDescriptorResolver();
            compositeTagHelperDescriptorResolver.Resolvers.Add(fooTagHelperDescriptorResolver);
            compositeTagHelperDescriptorResolver.Resolvers.Add(barTagHelperDescriptorResovler);

            var resolutionContext = new TagHelperDescriptorResolutionContext(
                new List<TagHelperDirectiveDescriptor>(), new ErrorSink());

            var expectedDescriptors = DescriptorData.ExpectedTagHelperDescriptors;

            // Act
            var descriptors = compositeTagHelperDescriptorResolver.Resolve(resolutionContext);

            // Assert
            Assert.Equal(expectedDescriptors, descriptors, TagHelperDescriptorComparer.Default);
        }

        private class FooTagHelperDescriptorResolver : TagHelperDescriptorResolver
        {
        //    private bool _hasReturnedResolver;
//
            public FooTagHelperDescriptorResolver()
                : base(designTime: false)
            {
          //      _hasReturnedResolver = false;
            }

            protected override IEnumerable<TagHelperDescriptor> ResolveDescriptorsInAssembly(
                string assemblyName,
                SourceLocation documentLocation,
                ErrorSink errorSink)
            {
                return new List<TagHelperDescriptor> {
                        DescriptorData.DescriptorFoo
                };
            }
        }

        private class BarTagHelperDescriptorResolver : TagHelperDescriptorResolver
        {
           // private bool _hasReturnedResolver;

            public BarTagHelperDescriptorResolver()
                : base(designTime: false)
            {
           //     _hasReturnedResolver = false;
            }

            protected override IEnumerable<TagHelperDescriptor> ResolveDescriptorsInAssembly(
                string assemblyName,
                SourceLocation documentLocation,
                ErrorSink errorSink)
            {
                return new List<TagHelperDescriptor> {
                        DescriptorData.DescriptorBar
                    };
            }
        }

        private static class DescriptorData
        {
            public static TagHelperDescriptor DescriptorFoo = new TagHelperDescriptor
            {
                AssemblyName = "assemblyNameFoo",
                TagName = "tagNameFoo",
                TypeName = "typeNameFoo"
            };

            public static TagHelperDescriptor DescriptorBar = new TagHelperDescriptor
            {
                AssemblyName = "assemblyNameBar",
                TagName = "tagNameBar",
                TypeName = "typeNameBar"
            };

            public static IEnumerable<TagHelperDescriptor> ExpectedTagHelperDescriptors = new List<TagHelperDescriptor>
            {
                DescriptorFoo,
                DescriptorBar
            };
        }
    }
}
