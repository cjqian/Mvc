using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Host;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Razor.Compilation.TagHelpers;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Microsoft.AspNetCore.Mvc.Razor.ViewComponentTagHelpers
{
    public class ViewComponentTagHelperDescriptorFactory 
    {
        private IViewComponentDescriptorProvider _descriptorProvider;

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

                // TODO: Allow customization by user. 
                var viewComponentDescriptorProvider = new DefaultViewComponentDescriptorProvider(partManager);
                return viewComponentDescriptorProvider;
            }
            catch
            {
                return null;
            }
        }

        // Create descriptors for all view components in the descriptor provider.
        public IEnumerable<TagHelperDescriptor> CreateDescriptors()
        {
            var viewComponentDescriptors = _descriptorProvider.GetViewComponents();
            var resolvedDescriptors = ResolveDescriptors(viewComponentDescriptors);
            return resolvedDescriptors;
        }

        // Create descriptors for only the view components in the descriptor provider
        // from the given assembly.
        public IEnumerable<TagHelperDescriptor> CreateDescriptors(string assemblyName)
        {
            var viewComponentDescriptors = GetViewComponentsInAssembly(assemblyName);
            var resolvedDescriptors = ResolveDescriptors(viewComponentDescriptors);
            return resolvedDescriptors;
        }

        // Returns view component descriptors from the descriptor provider in the given assembly.
        private IEnumerable<ViewComponentDescriptor> GetViewComponentsInAssembly(string assemblyName)
        {
            var viewComponents = new List<ViewComponentDescriptor>();
            var providedViewComponents = _descriptorProvider.GetViewComponents();

            foreach (var viewComponent in providedViewComponents)
            {
                var currentAssemblyName = GetAssemblyName(viewComponent);
                if (currentAssemblyName.Equals(assemblyName))
                {
                    viewComponents.Add(viewComponent);
                }
            }

            return viewComponents;
        }

        // Given a list of view component descriptors,
        // returns a list of view component tag helper descriptors.
        private IEnumerable<TagHelperDescriptor> ResolveDescriptors(
            IEnumerable<ViewComponentDescriptor> viewComponentDescriptors)
        {
            var tagHelperDescriptors = new List<TagHelperDescriptor>();
            foreach (var viewComponentDescriptor in viewComponentDescriptors)
            {
                var tagHelperDescriptor = ResolveDescriptor(viewComponentDescriptor);
                tagHelperDescriptors.Add(tagHelperDescriptor);
            }

            return tagHelperDescriptors;
        }

        private TagHelperDescriptor ResolveDescriptor(ViewComponentDescriptor viewComponentDescriptor)
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

            var tagHelperDescriptor = new TagHelperDescriptor
            {
                TagName = GetTagName(viewComponentDescriptor),
                TypeName = GetTypeName(viewComponentDescriptor),
                AssemblyName = GetAssemblyName(viewComponentDescriptor),
                Attributes = attributeDescriptors,
                RequiredAttributes = requiredAttributeDescriptors,
                TagStructure = TagStructure.NormalOrSelfClosing,
            };

            // Add view component properties to the property bag.
            tagHelperDescriptor.PropertyBag.Add(
                ViewComponentTagHelperDescriptorConventions.ViewComponentProperty,
                viewComponentDescriptor.ShortName);
            tagHelperDescriptor.PropertyBag.Add(
                ViewComponentTagHelperDescriptorConventions.ViewComponentTagHelperProperty,
                GetTypeName(viewComponentDescriptor));

            return tagHelperDescriptor;
        }

        private string GetAssemblyName(ViewComponentDescriptor descriptor) =>
            descriptor.TypeInfo.Assembly.GetName().Name;

        private string GetTagName(ViewComponentDescriptor descriptor) =>
            $"vc:{TagHelperDescriptorFactory.ToHtmlCase(descriptor.ShortName)}";

        private string GetTypeName(ViewComponentDescriptor descriptor) =>
            $"__Generated__{descriptor.ShortName}ViewComponentTagHelper";

        // TODO: Add support for customization of HtmlTargetElement, HtmlAttributeName.
        // TODO: Add validation of view component; valid attribute names?
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