﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Razor.ViewComponentTagHelpers;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Razor.Compilation.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Razor.Test.ViewComponentTagHelpers
{
    public class ViewComponentTagHelperDescriptorFactoryTest
    {
        public static TheoryData AssemblyData
        {
            get
            {
                var provider = new TestViewComponentDescriptorProvider();

                var assemblyOne = "Microsoft.AspNetCore.Mvc.Razor";
                var assemblyTwo = "Microsoft.AspNetCore.Mvc.Razor.Test";
                var assemblyNone = string.Empty;

                return new TheoryData<string, IEnumerable<TagHelperDescriptor>>
                {
                    { assemblyOne, new [] { provider.tagHelperDescriptorOne } },
                    { assemblyTwo, new [] { provider.tagHelperDescriptorTwo } },
                    { assemblyNone, new List<TagHelperDescriptor>() }
                };
            }
        }

        [Theory]
        [MemberData(nameof(AssemblyData))]
        public void CreateDescriptors_ReturnsCorrectDescriptors(
            string assemblyName, 
            IEnumerable<TagHelperDescriptor> expectedDescriptors)
        {
            // Arrange
            var provider = new TestViewComponentDescriptorProvider();
            var factory = new ViewComponentTagHelperDescriptorFactory(provider);

            // Act
            var descriptors = factory.CreateDescriptors(assemblyName);

            // Assert
            Assert.Equal(expectedDescriptors, descriptors, TagHelperDescriptorComparer.Default);
        }

        public void TestInvokeOne(string foo, string bar)
        {
        }

        public void TestInvokeTwo(int baz = 5)
        {
        }

        private class TestViewComponentDescriptorProvider : IViewComponentDescriptorProvider
        {
            public readonly TagHelperDescriptor tagHelperDescriptorOne = new TagHelperDescriptor
            {
                TagName = "vc:one",
                TypeName = "__Generated__OneViewComponentTagHelper",
                AssemblyName = "Microsoft.AspNetCore.Mvc.Razor",
                Attributes = new List<TagHelperAttributeDescriptor>
                {
                    new TagHelperAttributeDescriptor
                    {
                        Name = "foo",
                        PropertyName = "foo",
                        TypeName = "System.String",
                        IsStringProperty = true
                    },
                    new TagHelperAttributeDescriptor
                    {
                        Name = "bar",
                        PropertyName = "bar",
                        TypeName = "System.String",
                        IsStringProperty = true
                    }
                },
                RequiredAttributes = new List<TagHelperRequiredAttributeDescriptor>
                {
                    new TagHelperRequiredAttributeDescriptor
                    {
                        Name = "foo"
                    },
                    new TagHelperRequiredAttributeDescriptor
                    {
                        Name = "bar"
                    }
                },
                TagStructure = TagStructure.NormalOrSelfClosing
            };

            public readonly TagHelperDescriptor tagHelperDescriptorTwo = new TagHelperDescriptor
            {
                TagName = "vc:two",
                TypeName = "__Generated__TwoViewComponentTagHelper",
                AssemblyName = "Microsoft.AspNetCore.Mvc.Razor.Test",
                Attributes = new List<TagHelperAttributeDescriptor>
                {
                    new TagHelperAttributeDescriptor
                    {
                        Name = "baz",
                        PropertyName = "baz",
                        TypeName = "System.Int32"
                    }
                },
                RequiredAttributes = new List<TagHelperRequiredAttributeDescriptor>(),
                TagStructure = TagStructure.NormalOrSelfClosing
            };

            private readonly ViewComponentDescriptor _viewComponentDescriptorOne = new ViewComponentDescriptor
            {
                DisplayName = "OneDisplayName",
                FullName = "OneViewComponent",
                ShortName = "One",
                MethodInfo = typeof(ViewComponentTagHelperDescriptorFactoryTest)
                    .GetMethod(nameof(ViewComponentTagHelperDescriptorFactoryTest.TestInvokeOne)),
                TypeInfo = typeof(ViewComponentTagHelperDescriptorFactory).GetTypeInfo()
            };

            private readonly ViewComponentDescriptor _viewComponentDescriptorTwo = new ViewComponentDescriptor
            {
                DisplayName = "TwoDisplayName",
                FullName = "TwoViewComponent",
                ShortName = "Two",
                MethodInfo = typeof(ViewComponentTagHelperDescriptorFactoryTest)
                    .GetMethod(nameof(ViewComponentTagHelperDescriptorFactoryTest.TestInvokeTwo)),
                TypeInfo = typeof(ViewComponentTagHelperDescriptorFactoryTest).GetTypeInfo()
            };

            public IEnumerable<ViewComponentDescriptor> GetViewComponents()
            {
                return new List<ViewComponentDescriptor>
                {
                    _viewComponentDescriptorOne,
                    _viewComponentDescriptorTwo
                };
            }
        }
    }
}
