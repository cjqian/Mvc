﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Host;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Razor.Compilation.TagHelpers;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Microsoft.AspNetCore.Mvc.Razor.ViewComponentTagHelpers
{
    /// <summary>
    /// Provides methods to create tag helper representations of view components.
    /// </summary>
    public class ViewComponentTagHelperDescriptorFactory
    {
        private IViewComponentDescriptorProvider _descriptorProvider;

        /// <summary>
        /// Creates a new ViewComponentTagHelperDescriptorFactory than creates tag helper descriptors for
        /// view components in the given descriptorProvider.
        /// </summary>
        /// <param name="descriptorProvider">The provider of view component descriptors.</param>
        public ViewComponentTagHelperDescriptorFactory(IViewComponentDescriptorProvider descriptorProvider)
        {
            _descriptorProvider = descriptorProvider;
        }


        // Returns a descriptor provider that returns view components from a given assembly, or
        // null if the assembly is invalid. TODO: Allow provider customization by user.
        public static IViewComponentDescriptorProvider CreateDescriptorProvider(string assemblyName)
        {
            try
            {
                var assembly = Assembly.Load(new AssemblyName(assemblyName));

                var partManager = new ApplicationPartManager();
                partManager.ApplicationParts.Add(new AssemblyPart(assembly));
                partManager.FeatureProviders.Add(new ViewComponentFeatureProvider());

                var viewComponentDescriptorProvider = new DefaultViewComponentDescriptorProvider(partManager);
                return viewComponentDescriptorProvider;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Creates <see cref="TagHelperDescriptor"/> representations of view components in a given assembly.
        /// </summary>
        /// <param name="assemblyName">The name of the assembly containing the view components to translate.</param>
        /// <returns>A <see cref="IEnumerable{TagHelperDescriptor}"/>, one for each view component.</returns>
        public IEnumerable<TagHelperDescriptor> CreateDescriptors(string assemblyName)
        {
            var viewComponentDescriptors = _descriptorProvider.GetViewComponents()
                .Where(viewComponent => assemblyName.Equals(
                ViewComponentTagHelperDescriptorConventions.GetAssemblyName(viewComponent)));

            return CreateDescriptors(viewComponentDescriptors);
        }

        private IEnumerable<TagHelperDescriptor> CreateDescriptors(
            IEnumerable<ViewComponentDescriptor> viewComponentDescriptors)
        {
            var tagHelperDescriptors = new List<TagHelperDescriptor>();

            foreach (var viewComponentDescriptor in viewComponentDescriptors)
            {
                var tagHelperDescriptor = CreateDescriptor(viewComponentDescriptor);
                tagHelperDescriptors.Add(tagHelperDescriptor);
            }

            return tagHelperDescriptors;
        }

        private TagHelperDescriptor CreateDescriptor(ViewComponentDescriptor viewComponentDescriptor)
        {
            // Fill in the attribute and required attribute descriptors.
            IEnumerable<TagHelperAttributeDescriptor> attributeDescriptors;
            IEnumerable<TagHelperRequiredAttributeDescriptor> requiredAttributeDescriptors;
            if (!TryGetAttributeDescriptors(viewComponentDescriptor,
                out attributeDescriptors,
                out requiredAttributeDescriptors))
            {
                // After adding view component name validation,
                // this exception will make sense.
                throw new Exception("Unable to resolve view component descriptor to tag helper descriptor.");
            }

            var assemblyName = ViewComponentTagHelperDescriptorConventions.GetAssemblyName(viewComponentDescriptor);
            var tagName = ViewComponentTagHelperDescriptorConventions.GetTagName(viewComponentDescriptor);
            var typeName = ViewComponentTagHelperDescriptorConventions.GetTypeName(viewComponentDescriptor);

            var tagHelperDescriptor = new TagHelperDescriptor
            {
                TagName = tagName,
                TypeName = typeName,
                AssemblyName = assemblyName,
                Attributes = attributeDescriptors,
                RequiredAttributes = requiredAttributeDescriptors,
                TagStructure = TagStructure.NormalOrSelfClosing,
            };

            tagHelperDescriptor.PropertyBag.Add(
                ViewComponentTagHelperDescriptorConventions.ViewComponentProperty, viewComponentDescriptor.ShortName);
            tagHelperDescriptor.PropertyBag.Add(
                ViewComponentTagHelperDescriptorConventions.ViewComponentTagHelperProperty,
                ViewComponentTagHelperDescriptorConventions.GetTypeName(viewComponentDescriptor));

            return tagHelperDescriptor;
        }

        // TODO: Add support for customization of HtmlTargetElement, HtmlAttributeName.
        private bool TryGetAttributeDescriptors(
            ViewComponentDescriptor viewComponentDescriptor,
            out IEnumerable<TagHelperAttributeDescriptor> attributeDescriptors,
            out IEnumerable<TagHelperRequiredAttributeDescriptor> requiredAttributeDescriptors
            )
        {
            var methodParameters = viewComponentDescriptor.MethodInfo.GetParameters();
            var descriptors = new List<TagHelperAttributeDescriptor>();
            var requiredDescriptors = new List<TagHelperRequiredAttributeDescriptor>();

            foreach (var parameter in methodParameters)
            {
                var lowerKebabName = TagHelperDescriptorFactory.ToHtmlCase(parameter.Name);
                var descriptor = new TagHelperAttributeDescriptor
                {
                    Name = lowerKebabName,
                    PropertyName = parameter.Name,
                    TypeName = parameter.ParameterType.FullName
                };

                var tagHelperType = Type.GetType(descriptor.TypeName);
                if (tagHelperType.Equals(typeof(string)))
                {
                    descriptor.IsStringProperty = true;
                }

                descriptors.Add(descriptor);

                if (!parameter.HasDefaultValue)
                {
                    var requiredDescriptor = new TagHelperRequiredAttributeDescriptor
                    {
                        Name = lowerKebabName
                    };

                    requiredDescriptors.Add(requiredDescriptor);
                }
            }

            attributeDescriptors = descriptors;
            requiredAttributeDescriptors = requiredDescriptors;

            return true;
        }
    }
}